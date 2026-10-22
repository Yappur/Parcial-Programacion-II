# Organización del código C#: datos, modelos y transacciones

Namespace base: `SistemaStock`. Librería: `MySqlConnector`. La estructura de carpetas está en `../SKILL.md` (sección 3). Tablas: `esquema.md`. SQL de cada operación: `flujos-y-reportes.md`. Reglas de negocio: `negocio.md`. Pantallas: `ui-winforms.md`.

Los formularios no contienen SQL: llaman a los repositorios.

## Contenido

- Database.cs: Initialize() dividido
- Modelos y enums
- Repositorios
- Ejemplo: ProductoRepositorio
- Transacciones
- Ejemplo: descontar el stock de una receta

## Database.cs: Initialize() dividido

`Database.cs` **ya existe**: agregar los métodos `CrearTablaXxx` sin cambiar cómo se arma la conexión ni crear otra clase. Un método por tabla, llamados en el orden de dependencias de `esquema.md` (sección «Orden de creación»). Todos reciben la misma conexión abierta.

Cada tabla se agrega cuando el equipo llega a su paso del orden de trabajo (`../SKILL.md`, sección 5). Este es el esquema de cómo queda el método con todas las tablas:

```csharp
public static void Initialize()
{
    // 1) Crear la base si no existe (conexión sin base seleccionada)
    //    El nombre va entre backticks: CREATE DATABASE IF NOT EXISTS `{dbName}` ...

    // 2) Conectar a la base y crear tablas en orden
    using var connection = new MySqlConnection(builder.ConnectionString);
    connection.Open();

    CrearTablaUsuarios(connection);
    CrearTablaCategorias(connection);
    CrearTablaMesas(connection);
    CrearTablaIngredientes(connection);
    CrearTablaProductos(connection);
    CrearTablaRecetas(connection);
    CrearTablaVentas(connection);
    CrearTablaDetalleVenta(connection);

    SembrarCategorias(connection);   // INSERT IGNORE
}

private static void Ejecutar(MySqlConnection connection, string sql)
{
    using var cmd = new MySqlCommand(sql, connection);
    cmd.ExecuteNonQuery();
}

private static void CrearTablaMesas(MySqlConnection connection) =>
    Ejecutar(connection, """
        CREATE TABLE IF NOT EXISTS mesas (
            id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
            numero INT NOT NULL UNIQUE,
            capacidad INT NULL,
            activo BOOLEAN NOT NULL DEFAULT TRUE
        ) ENGINE=InnoDB;
        """);
```

`builder` y `dbName` salen del código que ya tiene `Database.cs`; el bloque de arriba muestra la forma, no reemplaza lo existente. El SQL de cada tabla está en `esquema.md`.

## Modelos y enums

Clases simples con propiedades, en `Modelos/`. Enums para los tipos cerrados:

```csharp
public enum TipoStock { Directo, PorReceta }
public enum TipoVenta { Mesa, Delivery, Mostrador }
public enum EstadoVenta { Pendiente, Cerrada, Anulada }
public enum MedioPago { Efectivo, Transferencia }
```

Al guardar en MySQL, convertir a la cadena del `ENUM` en mayúsculas (`TipoStock.PorReceta` → `'POR_RECETA'`). **Centralizar esa conversión en un solo lugar** para no repetirla. No sirve `ToString().ToUpper()`: daría `PORRECETA`. Ejemplo para `TipoStock` (el nombre de la clase es de ejemplo; los otros enums siguen el mismo patrón):

```csharp
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal static class ConversionesEnum
{
    public static string ADb(this TipoStock tipo) =>
        tipo == TipoStock.PorReceta ? "POR_RECETA" : "DIRECTO";

    public static TipoStock ATipoStock(string valor) =>
        valor == "POR_RECETA" ? TipoStock.PorReceta : TipoStock.Directo;
}
```

`Venta.MedioPago`, `Venta.FechaCierre` y `Venta.MesaId` son nulables (`MedioPago?`, `DateTime?`, `int?`).

`Modelos/Producto.cs`, adaptado a `tipo_stock`:

```csharp
namespace SistemaStock.Modelos;

public class Producto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = "";   // solo para mostrar
    public TipoStock TipoStock { get; set; } = TipoStock.Directo;
    public int? Stock { get; set; }                      // solo DIRECTO; en POR_RECETA es null
    public int? StockMinimo { get; set; }                // solo DIRECTO; en POR_RECETA es null
    public int StockDisponible { get; set; }             // calculado al listar; no se guarda
    public bool Activo { get; set; } = true;
}
```

## Repositorios

- Métodos con parámetros (`cmd.Parameters.AddWithValue`), nunca concatenar texto del usuario en el SQL.
- Cada método abre su conexión con `Database.OpenConnection()` y la cierra con `using`.
- Los métodos que forman parte de una operación mayor (ej. descontar stock dentro de una venta) reciben la `MySqlConnection` y la `MySqlTransaction` desde afuera, para quedar dentro de la misma transacción.
- Las reglas de negocio (stock, estados) se validan acá, no en los formularios.
- En lugar de borrar registros se pone `activo = 0` (ver `negocio.md`).

## Ejemplo: ProductoRepositorio

Consultas parametrizadas. Se actualizó para `tipo_stock`: `stock` y `stock_minimo` pueden ser `NULL`, y el listado trae el stock disponible calculado. Se usa `Convert.ToInt32` en `stock_disponible` porque la consulta mezcla un entero y un resultado de `FLOOR` sobre decimales.

```csharp
using System;
using System.Collections.Generic;
using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal static class ProductoRepositorio
{
    public static List<Producto> Listar(string filtro = "")
    {
        var lista = new List<Producto>();
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.categoria_id, c.nombre AS categoria,
                   p.tipo_stock, p.stock, p.stock_minimo, p.activo,
                   CASE WHEN p.tipo_stock = 'DIRECTO' THEN p.stock
                        ELSE COALESCE((
                          SELECT MIN(FLOOR(i.stock / r.cantidad))
                          FROM recetas r
                          JOIN ingredientes i ON i.id = r.ingrediente_id
                          WHERE r.producto_codigo = p.codigo), 0)
                   END AS stock_disponible
            FROM productos p
            INNER JOIN categorias c ON c.id = p.categoria_id
            WHERE p.nombre LIKE @filtro
            ORDER BY p.nombre;
            """, cn);
        cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            lista.Add(new Producto
            {
                Codigo = rd.GetInt32("codigo"),
                Nombre = rd.GetString("nombre"),
                Precio = rd.GetDecimal("precio"),
                CategoriaId = rd.GetInt32("categoria_id"),
                CategoriaNombre = rd.GetString("categoria"),
                TipoStock = ConversionesEnum.ATipoStock(rd.GetString("tipo_stock")),
                Stock = LeerEnteroNulo(rd, "stock"),
                StockMinimo = LeerEnteroNulo(rd, "stock_minimo"),
                StockDisponible = Convert.ToInt32(rd["stock_disponible"]),
                Activo = rd.GetBoolean("activo")
            });
        }
        return lista;
    }

    public static void Crear(Producto p)
    {
        if (p.TipoStock == TipoStock.Directo && (p.Stock is null || p.StockMinimo is null))
            throw new InvalidOperationException("Un producto directo necesita stock y stock mínimo.");

        // En POR_RECETA el stock no se guarda: se calcula desde los ingredientes.
        object stock = p.TipoStock == TipoStock.Directo ? p.Stock!.Value : DBNull.Value;
        object stockMinimo = p.TipoStock == TipoStock.Directo ? p.StockMinimo!.Value : DBNull.Value;

        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            INSERT INTO productos (nombre, precio, categoria_id, tipo_stock, stock, stock_minimo, activo)
            VALUES (@nombre, @precio, @categoriaId, @tipoStock, @stock, @stockMinimo, @activo);
            """, cn);
        cmd.Parameters.AddWithValue("@nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@precio", p.Precio);
        cmd.Parameters.AddWithValue("@categoriaId", p.CategoriaId);
        cmd.Parameters.AddWithValue("@tipoStock", p.TipoStock.ADb());
        cmd.Parameters.AddWithValue("@stock", stock);
        cmd.Parameters.AddWithValue("@stockMinimo", stockMinimo);
        cmd.Parameters.AddWithValue("@activo", p.Activo);
        cmd.ExecuteNonQuery();
    }

    // Modificar y Desactivar siguen el mismo patrón (UPDATE ... WHERE codigo = @codigo).
    // En lugar de borrar productos se pone activo = 0, para no romper ventas.

    private static int? LeerEnteroNulo(MySqlDataReader rd, string columna)
    {
        int posicion = rd.GetOrdinal(columna);
        return rd.IsDBNull(posicion) ? null : rd.GetInt32(posicion);
    }
}
```

Este ejemplo todavía no se compiló. `Crear` usa `ADb()` de `ConversionesEnum` (arriba). Aplicarlo en el paso 1 del orden de trabajo, junto con la migración de `productos` (`esquema.md`).

## Transacciones

Toda operación que toque stock va dentro de una transacción (regla en `negocio.md`).

```csharp
using var connection = Database.OpenConnection();
using var tx = connection.BeginTransaction();
try
{
    // 1) insertar línea en detalle_venta (usando connection + tx)
    // 2) descontar stock; si RowsAffected == 0 -> throw new InvalidOperationException("Stock insuficiente")
    // 3) recalcular total de la venta
    tx.Commit();
}
catch
{
    tx.Rollback();
    throw;
}
```

Cada `MySqlCommand` dentro de la operación debe usar el mismo `connection` y asignar `Transaction = tx` (o pasarlo en el constructor: `new MySqlCommand(sql, connection, tx)`).

Capturar `InvalidOperationException` en el formulario y mostrar un mensaje claro al usuario ("No hay stock suficiente de ...").

## Ejemplo: descontar el stock de una receta

Ejemplo orientativo, **no compilado**: aplica el bucle por ingrediente descrito en `flujos-y-reportes.md`. Recibe la conexión y la transacción desde afuera, como indica la sección «Repositorios».

```csharp
internal static void DescontarPorReceta(
    MySqlConnection cn, MySqlTransaction tx, int productoCodigo, int cantidad)
{
    var receta = new List<(int IngredienteId, decimal Cantidad)>();

    using (var cmd = new MySqlCommand(
        "SELECT ingrediente_id, cantidad FROM recetas WHERE producto_codigo = @codigo;", cn, tx))
    {
        cmd.Parameters.AddWithValue("@codigo", productoCodigo);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            receta.Add((rd.GetInt32("ingrediente_id"), rd.GetDecimal("cantidad")));
    }   // el lector se cierra acá: no se puede ejecutar otro comando mientras está abierto

    if (receta.Count == 0)
        throw new InvalidOperationException("El producto no tiene receta.");

    foreach (var (ingredienteId, cantidadReceta) in receta)
    {
        using var upd = new MySqlCommand("""
            UPDATE ingredientes SET stock = stock - @necesario
            WHERE id = @id AND stock >= @necesario;
            """, cn, tx);
        upd.Parameters.AddWithValue("@necesario", cantidadReceta * cantidad);
        upd.Parameters.AddWithValue("@id", ingredienteId);

        if (upd.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("No hay stock suficiente de un ingrediente.");
    }
}
```

Mejora posible, sin cambiar el diseño: incluir el nombre del ingrediente en el mensaje de error.
