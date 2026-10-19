using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

/// <summary>Ventana principal que mantiene el sidebar durante toda la sesión.</summary>
public sealed class FrmPrincipal : BaseFormSidebar
{
    public FrmPrincipal() : this(null) { }

    public FrmPrincipal(User? user) : base(PaginaApp.Dashboard, user)
    {
        Text = "Chachín";
    }
}
