using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class ProductoRepositorio
{
    public List<Producto> GetAll(string busqueda = "", int? categoriaId = null)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.tipo_stock, p.stock, p.stock_minimo, p.activo, p.categoria_id, c.nombre AS categoria_nombre,
                   CASE WHEN p.tipo_stock = 'DIRECTO' THEN p.stock
                        ELSE COALESCE((
                            SELECT MIN(FLOOR(i.stock / r.cantidad))
                            FROM recetas r
                            INNER JOIN ingredientes i ON i.id = r.ingrediente_id
                            WHERE r.producto_codigo = p.codigo
                        ), 0)
                   END AS stock_disponible
            FROM productos p
            INNER JOIN categorias c ON c.id = p.categoria_id
            WHERE p.nombre LIKE @busqueda AND (@categoriaId IS NULL OR p.categoria_id = @categoriaId)
            ORDER BY p.nombre
            """, db);
        cmd.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");
        cmd.Parameters.AddWithValue("@categoriaId", categoriaId.HasValue ? categoriaId.Value : DBNull.Value);
        using var reader = cmd.ExecuteReader();
        var productos = new List<Producto>();
        while (reader.Read()) productos.Add(LeerProducto(reader));
        return productos;
    }

    public List<Producto> GetLowStock()
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.tipo_stock, p.stock, p.stock_minimo,
                   p.activo, p.categoria_id, c.nombre AS categoria_nombre,
                   p.stock AS stock_disponible
            FROM productos p
            INNER JOIN categorias c ON c.id = p.categoria_id
            WHERE p.activo = TRUE
              AND p.tipo_stock = 'DIRECTO'
              AND p.stock <= p.stock_minimo
            ORDER BY p.stock ASC, p.nombre;
            """, db);
        using var reader = cmd.ExecuteReader();
        var productos = new List<Producto>();
        while (reader.Read()) productos.Add(LeerProducto(reader));
        return productos;
    }

    public void Create(Producto producto)
    {
        ValidarStock(producto);
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("INSERT INTO productos(nombre, precio, categoria_id, tipo_stock, stock, stock_minimo, activo) VALUES(@nombre, @precio, @categoriaId, @tipoStock, @stock, @stockMinimo, @activo)", db);
        AgregarParametros(cmd, producto);
        cmd.ExecuteNonQuery();
    }

    public void Update(Producto producto)
    {
        ValidarStock(producto);
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE productos SET nombre=@nombre, precio=@precio, categoria_id=@categoriaId, tipo_stock=@tipoStock, stock=@stock, stock_minimo=@stockMinimo, activo=@activo WHERE codigo=@codigo", db);
        AgregarParametros(cmd, producto);
        cmd.Parameters.AddWithValue("@codigo", producto.Codigo);
        cmd.ExecuteNonQuery();
    }

    public void Deactivate(int codigo)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE productos SET activo=FALSE WHERE codigo=@codigo", db);
        cmd.Parameters.AddWithValue("@codigo", codigo);
        cmd.ExecuteNonQuery();
    }

    private static void AgregarParametros(MySqlCommand cmd, Producto producto)
    {
        cmd.Parameters.AddWithValue("@nombre", producto.Nombre.Trim());
        cmd.Parameters.AddWithValue("@precio", producto.Precio);
        cmd.Parameters.AddWithValue("@tipoStock", TipoStockMySql.ToDatabase(producto.TipoStock));
        object stock = producto.TipoStock == TipoStock.Directo ? producto.Stock!.Value : DBNull.Value;
        object stockMinimo = producto.TipoStock == TipoStock.Directo ? producto.StockMinimo!.Value : DBNull.Value;
        cmd.Parameters.AddWithValue("@stock", stock);
        cmd.Parameters.AddWithValue("@stockMinimo", stockMinimo);
        cmd.Parameters.AddWithValue("@categoriaId", producto.CategoriaId);
        cmd.Parameters.AddWithValue("@activo", producto.Activo);
    }

    private static void ValidarStock(Producto producto)
    {
        if (producto.TipoStock == TipoStock.Directo)
        {
            if (producto.Stock is null or < 0 || producto.StockMinimo is null or < 0)
                throw new ArgumentException("Los productos DIRECTO requieren stock y stock mínimo enteros mayores o iguales a cero.");
        }
        else if (producto.TipoStock == TipoStock.PorReceta)
        {
            producto.Stock = null;
            producto.StockMinimo = null;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(producto), "El tipo de stock no es válido.");
        }
    }

    private static Producto LeerProducto(MySqlDataReader reader) => new()
    {
        Codigo = reader.GetInt32("codigo"), Nombre = reader.GetString("nombre"),
        Precio = reader.GetDecimal("precio"), TipoStock = TipoStockMySql.FromDatabase(reader.GetString("tipo_stock")),
        Stock = reader.IsDBNull(reader.GetOrdinal("stock")) ? null : reader.GetInt32("stock"),
        StockMinimo = reader.IsDBNull(reader.GetOrdinal("stock_minimo")) ? null : reader.GetInt32("stock_minimo"),
        StockDisponible = Convert.ToInt32(reader["stock_disponible"]),
        Activo = reader.GetBoolean("activo"), CategoriaId = reader.GetInt32("categoria_id"),
        CategoriaNombre = reader.GetString("categoria_nombre")
    };
}
