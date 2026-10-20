using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class VentaRepositorio
{
    public List<Venta> Buscar(string filtro, DateTime? desde, DateTime? hasta)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT v.id, v.fecha_hora, v.id_usuario, v.total,
                   u.nombre_completo AS usuario_nombre, u.nombre_usuario
            FROM ventas v
            INNER JOIN usuarios u ON u.id = v.id_usuario
            WHERE (@filtro = ''
                   OR CAST(v.id AS CHAR) LIKE @busqueda
                   OR u.nombre_completo LIKE @busqueda
                   OR u.nombre_usuario LIKE @busqueda)
              AND (@desde IS NULL OR v.fecha_hora >= @desde)
              AND (@hasta IS NULL OR v.fecha_hora < @hasta)
            ORDER BY v.fecha_hora DESC, v.id DESC;
            """, db);
        cmd.Parameters.AddWithValue("@filtro", filtro.Trim());
        cmd.Parameters.AddWithValue("@busqueda", $"%{filtro.Trim()}%");
        cmd.Parameters.AddWithValue("@desde", desde.HasValue ? desde.Value.Date : DBNull.Value);
        cmd.Parameters.AddWithValue("@hasta", hasta.HasValue ? hasta.Value.Date.AddDays(1) : DBNull.Value);

        using var reader = cmd.ExecuteReader();
        var ventas = new List<Venta>();
        while (reader.Read())
        {
            ventas.Add(new Venta
            {
                Id = reader.GetInt32("id"),
                FechaHora = reader.GetDateTime("fecha_hora"),
                UsuarioId = reader.GetInt32("id_usuario"),
                Total = reader.GetDecimal("total"),
                UsuarioNombre = reader.GetString("usuario_nombre"),
                NombreUsuario = reader.GetString("nombre_usuario")
            });
        }
        return ventas;
    }

    public List<VentaDetalle> ObtenerDetalle(int ventaId)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT codigo_producto, nombre_producto, cantidad, precio_unitario, subtotal
            FROM detalle_venta
            WHERE id_venta = @ventaId
            ORDER BY id;
            """, db);
        cmd.Parameters.AddWithValue("@ventaId", ventaId);

        using var reader = cmd.ExecuteReader();
        var detalle = new List<VentaDetalle>();
        while (reader.Read())
        {
            detalle.Add(new VentaDetalle
            {
                CodigoProducto = reader.GetInt32("codigo_producto"),
                NombreProducto = reader.GetString("nombre_producto"),
                Cantidad = reader.GetInt32("cantidad"),
                PrecioUnitario = reader.GetDecimal("precio_unitario"),
                Subtotal = reader.GetDecimal("subtotal")
            });
        }
        return detalle;
    }
}
