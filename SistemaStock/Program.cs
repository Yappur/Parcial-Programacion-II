using SistemaStock.Datos;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace SistemaStock
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            LiveCharts.Configure(settings => settings.UseDefaults());
            ApplicationConfiguration.Initialize();

            // Aseguramos que la base de datos y la tabla 'usuarios' existan
            // antes de mostrar el formulario de login.
            try
            {
                Database.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inicializando la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new Formularios.FrmLogin());
        }
    }
}
