using MySqlConnector;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Datos;

internal sealed class UserRepository
{
    public int Count()
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM usuarios", db);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public User? Login(string username, string password)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("SELECT id, nombre_usuario, nombre_completo, hash_contrasena, rol, activo FROM usuarios WHERE nombre_usuario=@user", db);
        cmd.Parameters.AddWithValue("@user", username);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read() || !reader.GetBoolean("activo") || !PasswordHasher.Verify(password, reader.GetString("hash_contrasena"))) return null;
        return ReadUser(reader);
    }

    public List<User> GetAll(string search = "")
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("SELECT id, nombre_usuario, nombre_completo, rol, activo FROM usuarios WHERE nombre_usuario LIKE @search OR nombre_completo LIKE @search ORDER BY nombre_usuario", db);
        cmd.Parameters.AddWithValue("@search", $"%{search}%");
        using var reader = cmd.ExecuteReader();
        var result = new List<User>();
        while (reader.Read()) result.Add(ReadUser(reader));
        return result;
    }

    public void Create(string username, string fullName, string password, string role)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("INSERT INTO usuarios(nombre_usuario,nombre_completo,hash_contrasena,rol) VALUES(@user,@name,@hash,@role)", db);
        cmd.Parameters.AddWithValue("@user", username);
        cmd.Parameters.AddWithValue("@name", fullName);
        cmd.Parameters.AddWithValue("@hash", PasswordHasher.Hash(password));
        cmd.Parameters.AddWithValue("@role", role);
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("USUARIO", "Agregó un usuario", $"{fullName} · {role}");
    }

    public void CreateFirstAdmin(string username, string fullName, string password) => Create(username, fullName, password, "Administrador");

    public void Update(User user, string? newPassword)
    {
        using var db = Database.OpenConnection();
        string sql = string.IsNullOrWhiteSpace(newPassword)
            ? "UPDATE usuarios SET nombre_usuario=@user,nombre_completo=@name,rol=@role,activo=@active WHERE id=@id"
            : "UPDATE usuarios SET nombre_usuario=@user,nombre_completo=@name,rol=@role,activo=@active,hash_contrasena=@hash WHERE id=@id";
        using var cmd = new MySqlCommand(sql, db);
        cmd.Parameters.AddWithValue("@user", user.Username);
        cmd.Parameters.AddWithValue("@name", user.FullName);
        cmd.Parameters.AddWithValue("@role", user.Role);
        cmd.Parameters.AddWithValue("@active", user.IsActive);
        cmd.Parameters.AddWithValue("@id", user.Id);
        if (!string.IsNullOrWhiteSpace(newPassword)) cmd.Parameters.AddWithValue("@hash", PasswordHasher.Hash(newPassword));
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("USUARIO", "Modificó un usuario", user.FullName);
    }

    public void Deactivate(int id)
    {
        using var db = Database.OpenConnection();
        using var nombreCmd = new MySqlCommand("SELECT nombre_completo FROM usuarios WHERE id = @id;", db);
        nombreCmd.Parameters.AddWithValue("@id", id);
        string nombre = Convert.ToString(nombreCmd.ExecuteScalar()) ?? $"ID {id}";
        using var cmd = new MySqlCommand("UPDATE usuarios SET activo=FALSE WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        ActividadRepositorio.Registrar("USUARIO", "Quitó un usuario", nombre);
    }

    private static User ReadUser(MySqlDataReader reader) => new()
    {
        Id = reader.GetInt32("id"), Username = reader.GetString("nombre_usuario"),
        FullName = reader.GetString("nombre_completo"), Role = reader.GetString("rol"),
        IsActive = reader.GetBoolean("activo")
    };
}

