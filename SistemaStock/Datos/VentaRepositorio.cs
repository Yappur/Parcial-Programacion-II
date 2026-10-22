using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class VentaRepositorio
{
    public List<MesaVenta> ListarMesasConEstado()
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT m.id, m.numero,
                   EXISTS (SELECT 1 FROM ventas v
                           WHERE v.mesa_id = m.id AND v.estado = 'PENDIENTE') AS ocupada
            FROM mesas m
            WHERE m.activo = TRUE
            ORDER BY m.numero;
            """, cn);
        using var rd = cmd.ExecuteReader();
        var mesas = new List<MesaVenta>();
        while (rd.Read())
        {
            mesas.Add(new MesaVenta
            {
                Id = rd.GetInt32("id"),
                Numero = rd.GetInt32("numero"),
                Ocupada = rd.GetBoolean("ocupada")
            });
        }
        return mesas;
    }

    public int Abrir(TipoVenta tipo, int? mesaId, int usuarioId)
    {
        if (usuarioId <= 0)
            throw new InvalidOperationException("No se encontró el usuario que inició sesión.");

        if (tipo == TipoVenta.Mesa && mesaId is null)
            throw new InvalidOperationException("Seleccioná una mesa para abrir la venta.");

        if (tipo != TipoVenta.Mesa && mesaId is not null)
            throw new InvalidOperationException("Las ventas de delivery y mostrador no llevan mesa.");

        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            if (tipo == TipoVenta.Mesa)
            {
                using var mesaCmd = new MySqlCommand("""
                    SELECT activo FROM mesas WHERE id = @mesaId FOR UPDATE;
                    """, cn, tx);
                mesaCmd.Parameters.AddWithValue("@mesaId", mesaId!.Value);
                var activa = mesaCmd.ExecuteScalar();
                if (activa is null || activa == DBNull.Value || !Convert.ToBoolean(activa))
                    throw new InvalidOperationException("La mesa seleccionada no existe o está inactiva.");

                using var pendienteCmd = new MySqlCommand("""
                    SELECT EXISTS (SELECT 1 FROM ventas
                                   WHERE mesa_id = @mesaId AND estado = 'PENDIENTE');
                    """, cn, tx);
                pendienteCmd.Parameters.AddWithValue("@mesaId", mesaId.Value);
                if (Convert.ToInt32(pendienteCmd.ExecuteScalar()) == 1)
                    throw new InvalidOperationException("Esa mesa ya tiene una venta pendiente.");
            }

            using var cmd = new MySqlCommand("""
                INSERT INTO ventas (fecha_hora, tipo_venta, mesa_id, usuario_id, total, estado)
                VALUES (NOW(), @tipo, @mesaId, @usuarioId, 0, 'PENDIENTE');
                """, cn, tx);
            cmd.Parameters.AddWithValue("@tipo", tipo.ADb());
            cmd.Parameters.AddWithValue("@mesaId", mesaId is null ? DBNull.Value : mesaId.Value);
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
            cmd.ExecuteNonQuery();
            var id = checked((int)cmd.LastInsertedId);
            tx.Commit();
            return id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
