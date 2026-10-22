using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class MesaRepositorio
{
    public List<Mesa> GetAll(bool? activo = true)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT id, numero, activo
            FROM mesas
            WHERE @activo IS NULL OR activo = @activo
            ORDER BY numero;
            """, db);
        cmd.Parameters.AddWithValue("@activo", activo.HasValue ? activo.Value : DBNull.Value);
        using var reader = cmd.ExecuteReader();
        var mesas = new List<Mesa>();
        while (reader.Read())
        {
            mesas.Add(new Mesa
            {
                Id = reader.GetInt32("id"),
                Numero = reader.GetInt32("numero"),
                Activo = reader.GetBoolean("activo")
            });
        }
        return mesas;
    }

    public void Create(Mesa mesa)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            INSERT INTO mesas (numero, activo)
            VALUES (@numero, 1);
            """, db);
        AgregarParametros(cmd, mesa);
        EjecutarConNumeroUnico(cmd);
    }

    public void Update(Mesa mesa)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            UPDATE mesas
            SET numero = @numero
            WHERE id = @id;
            """, db);
        AgregarParametros(cmd, mesa);
        cmd.Parameters.AddWithValue("@id", mesa.Id);
        EjecutarConNumeroUnico(cmd);
    }

    public void Deactivate(int id)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE mesas SET activo = 0 WHERE id = @id;", db);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public void Activate(int id)
    {
        using var db = Database.OpenConnection();
        using var cmd = new MySqlCommand("UPDATE mesas SET activo = 1 WHERE id = @id;", db);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static void AgregarParametros(MySqlCommand cmd, Mesa mesa)
    {
        cmd.Parameters.AddWithValue("@numero", mesa.Numero);
    }

    private static void EjecutarConNumeroUnico(MySqlCommand cmd)
    {
        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new InvalidOperationException("Ya existe una mesa con ese número.", ex);
        }
    }
}
