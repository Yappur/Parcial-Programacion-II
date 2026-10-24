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

        using var tableCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS mesas (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                numero INT NOT NULL UNIQUE,
                activo BOOLEAN NOT NULL DEFAULT TRUE
            ) ENGINE=InnoDB;
            """, connection);
        tableCommand.ExecuteNonQuery();

        using var ingredientCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS ingredientes (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre VARCHAR(100) NOT NULL UNIQUE,
                unidad ENUM('un','g','ml') NOT NULL,
                stock DECIMAL(10,3) NOT NULL DEFAULT 0 CHECK (stock >= 0),
                stock_minimo DECIMAL(10,3) NOT NULL DEFAULT 0 CHECK (stock_minimo >= 0),
                activo BOOLEAN NOT NULL DEFAULT TRUE
            ) ENGINE=InnoDB;
            """, connection);
        ingredientCommand.ExecuteNonQuery();
        using var capacityColumnCheck = new MySqlCommand("""
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'mesas'
              AND COLUMN_NAME = 'capacidad';
            """, connection);
        if (Convert.ToInt32(capacityColumnCheck.ExecuteScalar()) > 0)
        {
            using var removeCapacityColumn = new MySqlCommand("ALTER TABLE mesas DROP COLUMN capacidad;", connection);
            removeCapacityColumn.ExecuteNonQuery();
        }

        using var productCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS productos (
                codigo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                nombre VARCHAR(100) NOT NULL,
                precio DECIMAL(10,2) NOT NULL CHECK (precio >= 0),
                categoria_id INT NOT NULL,
                tipo_stock ENUM('DIRECTO','POR_RECETA') NOT NULL,
                stock INT NULL CHECK (stock IS NULL OR stock >= 0),
                stock_minimo INT NULL CHECK (stock_minimo IS NULL OR stock_minimo >= 0),
                activo BOOLEAN NOT NULL DEFAULT TRUE,
                CONSTRAINT fk_productos_categorias FOREIGN KEY (categoria_id) REFERENCES categorias(id)
            ) ENGINE=InnoDB;
            """, connection);
        productCommand.ExecuteNonQuery();

        using var recipeCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS recetas (
                producto_codigo INT NOT NULL,
                ingrediente_id INT NOT NULL,
                cantidad DECIMAL(10,3) NOT NULL CHECK (cantidad > 0),
                PRIMARY KEY (producto_codigo, ingrediente_id),
                CONSTRAINT fk_recetas_productos FOREIGN KEY (producto_codigo) REFERENCES productos(codigo),
                CONSTRAINT fk_recetas_ingredientes FOREIGN KEY (ingrediente_id) REFERENCES ingredientes(id)
            ) ENGINE=InnoDB;
            """, connection);
        recipeCommand.ExecuteNonQuery();

        using var salesCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS ventas (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                fecha_hora DATETIME NOT NULL,
                fecha_cierre DATETIME NULL,
                tipo_venta ENUM('MESA','DELIVERY','MOSTRADOR') NOT NULL,
                mesa_id INT NULL,
                usuario_id INT NOT NULL,
                total DECIMAL(10,2) NOT NULL DEFAULT 0,
                medio_pago ENUM('EFECTIVO','TRANSFERENCIA') NULL,
                estado ENUM('PENDIENTE','CERRADA','ANULADA') NOT NULL DEFAULT 'PENDIENTE',
                notas VARCHAR(500) NULL,
                mesa_pendiente_id INT GENERATED ALWAYS AS (CASE WHEN estado = 'PENDIENTE' THEN mesa_id END) STORED,
                UNIQUE KEY uq_una_pendiente_por_mesa (mesa_pendiente_id),
                CONSTRAINT fk_ventas_mesas FOREIGN KEY (mesa_id) REFERENCES mesas(id),
                CONSTRAINT fk_ventas_usuarios FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
            ) ENGINE=InnoDB;
            """, connection);
        salesCommand.ExecuteNonQuery();

        using var saleDetailsCommand = new MySqlCommand("""
            CREATE TABLE IF NOT EXISTS detalle_venta (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                venta_id INT NOT NULL,
                producto_codigo INT NOT NULL,
                cantidad INT NOT NULL CHECK (cantidad > 0),
                precio_unitario DECIMAL(10,2) NOT NULL,
                subtotal DECIMAL(10,2) NOT NULL,
                CONSTRAINT fk_detalle_ventas FOREIGN KEY (venta_id) REFERENCES ventas(id),
                CONSTRAINT fk_detalle_productos FOREIGN KEY (producto_codigo) REFERENCES productos(codigo)
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
