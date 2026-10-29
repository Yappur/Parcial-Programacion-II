using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal static class ActividadRepositorio
{
    public static int? UsuarioActualId { get; set; }

    public static void Registrar(string tipo, string accion, string detalle, decimal? precio = null)
    {
        try
        {
            using var db = Database.OpenConnection();
            using var cmd = new MySqlCommand("""
                INSERT INTO actividades (usuario_id, tipo, accion, detalle, precio)
                VALUES (@usuarioId, @tipo, @accion, @detalle, @precio);
                """, db);
            cmd.Parameters.AddWithValue("@usuarioId", UsuarioActualId is int id ? id : DBNull.Value);
            cmd.Parameters.AddWithValue("@tipo", tipo);
            cmd.Parameters.AddWithValue("@accion", accion);
            cmd.Parameters.AddWithValue("@detalle", detalle.Length > 255 ? detalle[..255] : detalle);
            cmd.Parameters.AddWithValue("@precio", precio is decimal monto ? monto : DBNull.Value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo registrar la actividad: {ex.Message}");
        }
    }

    public static List<ActividadDashboard> ListarRecientes(int cantidad = 5)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT id, tipo, accion, detalle, precio, fecha_hora
            FROM actividades
            ORDER BY fecha_hora DESC, id DESC
            LIMIT @cantidad;
            """, db);
        cmd.Parameters.AddWithValue("@cantidad", cantidad);
        using var reader = cmd.ExecuteReader();
        var actividades = new List<ActividadDashboard>();
        while (reader.Read())
        {
            actividades.Add(new ActividadDashboard
            {
                Id = reader.GetInt64("id"),
                Tipo = reader.GetString("tipo"),
                Accion = reader.GetString("accion"),
                Detalle = reader.GetString("detalle"),
                Precio = reader.IsDBNull(reader.GetOrdinal("precio")) ? null : reader.GetDecimal("precio"),
                FechaHora = reader.GetDateTime("fecha_hora")
            });
        }
        return actividades;
    }
}
