using MySqlConnector;

namespace SistemaStock;

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
        // Usamos MySqlConnectionStringBuilder para separar el servidor del nombre de la base
        var full = Environment.GetEnvironmentVariable("GESTION_VENTAS_CONNECTION") ?? DefaultConnectionString;
        var builder = new MySqlConnectionStringBuilder(full);
        var dbName = string.IsNullOrWhiteSpace(builder.Database) ? "gestion_ventas" : builder.Database;

        // Conectamos al servidor sin seleccionar base de datos para poder crearla si hace falta
        var serverBuilder = new MySqlConnectionStringBuilder(full) { Database = string.Empty };
        using (var serverConn = new MySqlConnection(serverBuilder.ConnectionString))
        {
            serverConn.Open();
            using var cmdCreateDb = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;", serverConn);
            cmdCreateDb.ExecuteNonQuery();
        }

        // Ahora conectamos a la base creada y aseguramos la tabla 'usuarios'
        builder.Database = dbName;
        using var connection = new MySqlConnection(builder.ConnectionString);
        connection.Open();
        using var command = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS usuarios (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
                nombre_completo VARCHAR(100) NOT NULL,
                hash_contrasena VARCHAR(200) NOT NULL,
                rol VARCHAR(20) NOT NULL,
                activo BOOLEAN NOT NULL DEFAULT TRUE,
                creado_en TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            ) ENGINE=InnoDB;
            """, connection);
        command.ExecuteNonQuery();
    }
}
