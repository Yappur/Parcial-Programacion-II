using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class IngredienteRepositorio
{
    private static readonly string[] UnidadesValidas = ["un", "g", "ml"];

    public List<Ingrediente> GetAll(string busqueda = "")
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT id, nombre, unidad, stock, stock_minimo, activo
            FROM ingredientes
            WHERE nombre LIKE @busqueda
            ORDER BY nombre;
            """, db);
        cmd.Parameters.AddWithValue("@busqueda", $"%{busqueda.Trim()}%");

        using var reader = cmd.ExecuteReader();
        var ingredientes = new List<Ingrediente>();
        while (reader.Read())
        {
            ingredientes.Add(new Ingrediente
            {
                Id = reader.GetInt32("id"),
                Nombre = reader.GetString("nombre"),
                Unidad = reader.GetString("unidad"),
                Stock = reader.GetDecimal("stock"),
                StockMinimo = reader.GetDecimal("stock_minimo"),
                Activo = reader.GetBoolean("activo")
            });
        }
        return ingredientes;
    }

    public void Create(Ingrediente ingrediente)
    {
        Validar(ingrediente);
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            INSERT INTO ingredientes (nombre, unidad, stock, stock_minimo, activo)
            VALUES (@nombre, @unidad, @stock, @stockMinimo, @activo);
            """, db);
        AgregarParametros(cmd, ingrediente);
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("INGREDIENTE", "Agregó un ingrediente", ingrediente.Nombre);
    }

    public void Update(Ingrediente ingrediente)
    {
        Validar(ingrediente);
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            UPDATE ingredientes
            SET nombre = @nombre, unidad = @unidad, stock = @stock,
                stock_minimo = @stockMinimo, activo = @activo
            WHERE id = @id;
            """, db);
        AgregarParametros(cmd, ingrediente);
        cmd.Parameters.AddWithValue("@id", ingrediente.Id);
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("INGREDIENTE", "Modificó un ingrediente", ingrediente.Nombre);
    }

    public void Deactivate(int id)
    {
        using var db = Database.OpenConnection();
        using var nombreCmd = new MySqlCommand("SELECT nombre FROM ingredientes WHERE id = @id;", db);
        nombreCmd.Parameters.AddWithValue("@id", id);
        string nombre = Convert.ToString(nombreCmd.ExecuteScalar()) ?? $"ID {id}";
        using var cmd = new MySqlCommand(
            "UPDATE ingredientes SET activo = FALSE WHERE id = @id;", db);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("INGREDIENTE", "Quitó un ingrediente", nombre);
    }

    private static void AgregarParametros(MySqlCommand cmd, Ingrediente ingrediente)
    {
        cmd.Parameters.AddWithValue("@nombre", ingrediente.Nombre.Trim());
        cmd.Parameters.AddWithValue("@unidad", ingrediente.Unidad);
        cmd.Parameters.AddWithValue("@stock", ingrediente.Stock);
        cmd.Parameters.AddWithValue("@stockMinimo", ingrediente.StockMinimo);
        cmd.Parameters.AddWithValue("@activo", ingrediente.Activo);
    }

    private static void Validar(Ingrediente ingrediente)
    {
        if (string.IsNullOrWhiteSpace(ingrediente.Nombre))
            throw new ArgumentException("El nombre del ingrediente es obligatorio.");
        if (!UnidadesValidas.Contains(ingrediente.Unidad))
            throw new ArgumentException("La unidad debe ser un, g o ml.");
        if (ingrediente.Stock < 0)
            throw new ArgumentOutOfRangeException(nameof(ingrediente), "El stock no puede ser negativo.");
        if (ingrediente.StockMinimo < 0)
            throw new ArgumentOutOfRangeException(nameof(ingrediente), "El stock mínimo no puede ser negativo.");
    }
}
