using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class CategoriaRepositorio
{
    public List<Categoria> GetAll()
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("SELECT id, nombre FROM categorias ORDER BY nombre", db);
        using var reader = cmd.ExecuteReader();
        var categorias = new List<Categoria>();
        while (reader.Read())
            categorias.Add(new Categoria { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre") });
        return categorias;
    }
}
