using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmDashboard : BasePage
{
    public FrmDashboard() : this(null) { }

    public FrmDashboard(User? user)
    {
        InitializeComponent();
        AddPageTitle("Dashboard", "Resumen general del sistema");
        AddEmptyGrid();
    }
}
