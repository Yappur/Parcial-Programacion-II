using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmDashboard : BaseFormSidebar
{
    public FrmDashboard() : this(null) { }

    public FrmDashboard(User? user) : base(PaginaApp.Dashboard, user)
    {
        InitializeComponent();
        AddPageTitle("Dashboard", "Resumen general del sistema");
        AddEmptyGrid();
    }
}
