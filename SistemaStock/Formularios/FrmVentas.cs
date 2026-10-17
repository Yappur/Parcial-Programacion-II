using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BaseFormSidebar
{
    public FrmVentas() : this(null) { }

    public FrmVentas(User? user) : base(PaginaApp.Ventas, user)
    {
        InitializeComponent();
        AddPageTitle("Ventas", "Registro y seguimiento de ventas");
        AddEmptyGrid();
    }
}
