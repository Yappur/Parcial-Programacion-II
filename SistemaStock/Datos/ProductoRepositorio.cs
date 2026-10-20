using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class ProductoRepositorio
{
    public List<Producto> GetAll(string busqueda = "", int? categoriaId = null)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.stock, p.activo, p.categoria_id, c.nombre AS categoria_nombre
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

    public void Create(Producto producto)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("INSERT INTO productos(nombre, precio, stock, categoria_id) VALUES(@nombre, @precio, @stock, @categoriaId)", db);
        AgregarParametros(cmd, producto);
        cmd.ExecuteNonQuery();
    }

    public void Update(Producto producto)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE productos SET nombre=@nombre, precio=@precio, stock=@stock, categoria_id=@categoriaId WHERE codigo=@codigo", db);
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

    public List<Producto> GetActivos()
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.stock, p.activo, p.categoria_id, c.nombre AS categoria_nombre
            FROM productos p
            INNER JOIN categorias c ON c.id = p.categoria_id
            WHERE p.activo = TRUE
            ORDER BY p.nombre
            """, db);
        using var reader = cmd.ExecuteReader();
        var productos = new List<Producto>();
        while (reader.Read()) productos.Add(LeerProducto(reader));
        return productos;
    }

    public void RegistrarVenta(int codigo, int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");

        using var db = Database.OpenConnection();
        using var transaction = db.BeginTransaction();
        using var consulta = new MySqlCommand(
            "SELECT stock FROM productos WHERE codigo=@codigo AND activo=TRUE FOR UPDATE", db, transaction);
        consulta.Parameters.AddWithValue("@codigo", codigo);
        var resultado = consulta.ExecuteScalar();
        if (resultado is null)
            throw new InvalidOperationException("El producto no existe o está desactivado.");

        var stockActual = Convert.ToInt32(resultado);
        if (cantidad > stockActual)
            throw new InvalidOperationException($"Stock insuficiente. Hay {stockActual} unidad(es) disponible(s).");

        using var actualizar = new MySqlCommand(
            "UPDATE productos SET stock=@stock WHERE codigo=@codigo", db, transaction);
        actualizar.Parameters.AddWithValue("@stock", stockActual - cantidad);
        actualizar.Parameters.AddWithValue("@codigo", codigo);
        actualizar.ExecuteNonQuery();
        transaction.Commit();
    }

    private static void AgregarParametros(MySqlCommand cmd, Producto producto)
    {
        cmd.Parameters.AddWithValue("@nombre", producto.Nombre.Trim());
        cmd.Parameters.AddWithValue("@precio", producto.Precio);
        cmd.Parameters.AddWithValue("@stock", producto.Stock);
        cmd.Parameters.AddWithValue("@categoriaId", producto.CategoriaId);
    }

    private static Producto LeerProducto(MySqlDataReader reader) => new()
    {
        Codigo = reader.GetInt32("codigo"), Nombre = reader.GetString("nombre"),
        Precio = reader.GetDecimal("precio"), Stock = reader.GetInt32("stock"),
        Activo = reader.GetBoolean("activo"), CategoriaId = reader.GetInt32("categoria_id"),
        CategoriaNombre = reader.GetString("categoria_nombre")
    };
}
