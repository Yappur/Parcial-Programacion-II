using MySqlConnector;

namespace SistemaStock.Datos;

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

        using var categoryCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS categorias (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre VARCHAR(100) NOT NULL UNIQUE
            ) ENGINE=InnoDB;
            """, connection);
        categoryCommand.ExecuteNonQuery();

        using var productCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS productos (
                codigo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre VARCHAR(100) NOT NULL,
                precio DECIMAL(10,2) NOT NULL CHECK (precio >= 0),
                stock INT NOT NULL CHECK (stock >= 0),
                activo BOOLEAN NOT NULL DEFAULT TRUE,
                categoria_id INT NOT NULL,
                CONSTRAINT fk_productos_categorias FOREIGN KEY (categoria_id) REFERENCES categorias(id)
            ) ENGINE=InnoDB;
            """, connection);
        productCommand.ExecuteNonQuery();

        using var salesCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS ventas (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                fecha_hora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                id_usuario INT NOT NULL,
                total DECIMAL(10,2) NOT NULL CHECK (total >= 0),
                CONSTRAINT fk_ventas_usuarios FOREIGN KEY (id_usuario) REFERENCES usuarios(id)
            ) ENGINE=InnoDB;
            """, connection);
        salesCommand.ExecuteNonQuery();

        using var saleDetailsCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS detalle_venta (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                id_venta INT NOT NULL,
                codigo_producto INT NOT NULL,
                nombre_producto VARCHAR(100) NOT NULL,
                cantidad INT NOT NULL CHECK (cantidad > 0),
                precio_unitario DECIMAL(10,2) NOT NULL CHECK (precio_unitario >= 0),
                subtotal DECIMAL(10,2) NOT NULL CHECK (subtotal >= 0),
                CONSTRAINT fk_detalle_venta_ventas FOREIGN KEY (id_venta) REFERENCES ventas(id) ON DELETE CASCADE,
                CONSTRAINT fk_detalle_venta_productos FOREIGN KEY (codigo_producto) REFERENCES productos(codigo)
            ) ENGINE=InnoDB;
            """, connection);
        saleDetailsCommand.ExecuteNonQuery();

        using var seedCommand = new MySqlCommand("""
            INSERT INTO categorias (nombre) VALUES ('Bebidas'), ('Platos'), ('Postres')
            ON DUPLICATE KEY UPDATE nombre = VALUES(nombre);
            """, connection);
        seedCommand.ExecuteNonQuery();
    }
}
