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

    public void Create(string nombre)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("INSERT INTO categorias(nombre) VALUES(@nombre)", db);
        cmd.Parameters.AddWithValue("@nombre", nombre.Trim());
        cmd.ExecuteNonQuery();
    }

    public void Update(Categoria categoria)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE categorias SET nombre=@nombre WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@nombre", categoria.Nombre.Trim());
        cmd.Parameters.AddWithValue("@id", categoria.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("DELETE FROM categorias WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
