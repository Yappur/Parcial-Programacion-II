using SistemaStock.Estilos;

namespace SistemaStock.Formularios;

public partial class FrmProductos : BaseFormSidebar
{
    public FrmProductos() : this(null) { }

    public FrmProductos(User? user) : base(PaginaApp.Productos, user)
    {
        InitializeComponent();
        AddPageTitle("Productos", "Listado de productos");
        AddEmptyGrid();
    }
}
