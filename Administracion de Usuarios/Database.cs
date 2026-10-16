using MySqlConnector;

namespace GestionVentas;

internal static class Database
{
    // Configurá la variable GESTION_VENTAS_CONNECTION en Windows para evitar subir contraseñas a GitHub.
    private const string DefaultConnectionString = "Server=localhost;Port=3306;Database=gestion_ventas;User ID=root;";

    public static MySqlConnection OpenConnection()
    {
        var connection = new MySqlConnection(Environment.GetEnvironmentVariable("GESTION_VENTAS_CONNECTION") ?? DefaultConnectionString);
        connection.Open();
        return connection;
    }

    public static void Initialize()
    {
        using var connection = OpenConnection();
        using var command = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS usuarios (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
                nombre_completo VARCHAR(100) NOT NULL,
                hash_contrasena VARCHAR(200) NOT NULL,
                rol VARCHAR(20) NOT NULL,
                activo BOOLEAN NOT NULL DEFAULT TRUE,
                creado_en TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """, connection);
        command.ExecuteNonQuery();
    }
}
