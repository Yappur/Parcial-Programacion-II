namespace SistemaStock
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            try
            {
                // Inicializa la base de datos compartida (si corresponde)
                Database.Initialize();
                // Ejecuta el formulario de login consolidado (FrmLogin)
                Application.Run(new Forms.FrmLogin());
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo iniciar la aplicación. Revisá la conexión a la base de datos.\n\n" + ex.Message,
                    "Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
