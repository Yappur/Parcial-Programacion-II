using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BasePage
{
    public FrmVentas() : this(null) { }

    public FrmVentas(User? user)
    {
        InitializeComponent();
        AddPageTitle("Ventas", "Registro y seguimiento de ventas");
        AddEmptyGrid();
    }
}
