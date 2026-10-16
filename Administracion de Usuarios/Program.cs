namespace GestionVentas;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Database.Initialize();
            Application.Run(new LoginForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo iniciar la aplicación. Revisá la conexión a MySQL.\n\n" + ex.Message,
                "Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
