using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class RecetaRepositorio
{
    public List<Receta> ObtenerPorProducto(int productoCodigo)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT r.producto_codigo, r.ingrediente_id, i.nombre AS ingrediente_nombre,
                   i.unidad, r.cantidad
            FROM recetas r
            INNER JOIN ingredientes i ON i.id = r.ingrediente_id
            WHERE r.producto_codigo = @productoCodigo
            ORDER BY i.nombre;
            """, db);
        cmd.Parameters.AddWithValue("@productoCodigo", productoCodigo);

        using var reader = cmd.ExecuteReader();
        var receta = new List<Receta>();
        while (reader.Read())
        {
            receta.Add(new Receta
            {
                ProductoCodigo = reader.GetInt32("producto_codigo"),
                IngredienteId = reader.GetInt32("ingrediente_id"),
                IngredienteNombre = reader.GetString("ingrediente_nombre"),
                Unidad = reader.GetString("unidad"),
                Cantidad = reader.GetDecimal("cantidad")
            });
        }
        return receta;
    }

    /// <summary>Reemplaza la receta completa. Si falla una fila, se revierte también el borrado y las filas previas.</summary>
    public void Guardar(int productoCodigo, IReadOnlyCollection<Receta> receta)
    {
        ArgumentNullException.ThrowIfNull(receta);
        if (productoCodigo <= 0)
            throw new ArgumentOutOfRangeException(nameof(productoCodigo), "Elegí un producto válido.");

        var ingredientesRepetidos = receta
            .GroupBy(fila => fila.IngredienteId)
            .Any(grupo => grupo.Key <= 0 || grupo.Count() > 1);
        if (ingredientesRepetidos)
            throw new ArgumentException("La receta no puede tener ingredientes repetidos ni IDs inválidos.", nameof(receta));
        if (receta.Any(fila => fila.Cantidad <= 0))
            throw new ArgumentException("Todas las cantidades de la receta deben ser mayores que cero.", nameof(receta));

        using var db = Database.OpenConnection();
        using var transaction = db.BeginTransaction();
        try
        {
            using (var validarProducto = new MySqlCommand(
                "SELECT tipo_stock FROM productos WHERE codigo = @codigo;", db, transaction))
            {
                validarProducto.Parameters.AddWithValue("@codigo", productoCodigo);
                var tipoStock = validarProducto.ExecuteScalar() as string;
                if (tipoStock != "POR_RECETA")
                    throw new InvalidOperationException("Solo se puede guardar una receta para un producto POR_RECETA.");
            }

            using (var borrar = new MySqlCommand(
                "DELETE FROM recetas WHERE producto_codigo = @productoCodigo;", db, transaction))
            {
                borrar.Parameters.AddWithValue("@productoCodigo", productoCodigo);
                borrar.ExecuteNonQuery();
            }

            foreach (var fila in receta)
            {
                using var insertar = new MySqlCommand("""
                    INSERT INTO recetas (producto_codigo, ingrediente_id, cantidad)
                    VALUES (@productoCodigo, @ingredienteId, @cantidad);
                    """, db, transaction);
                insertar.Parameters.AddWithValue("@productoCodigo", productoCodigo);
                insertar.Parameters.AddWithValue("@ingredienteId", fila.IngredienteId);
                insertar.Parameters.AddWithValue("@cantidad", fila.Cantidad);
                insertar.ExecuteNonQuery();
            }

            transaction.Commit();
            ActividadRepositorio.Registrar("RECETA", "Actualizó una receta", $"Producto #{productoCodigo} · {receta.Count} ingredientes");
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
