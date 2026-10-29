using System.Drawing;
using FontAwesome.Sharp;
using SistemaStock.Formularios;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Estilos;

public enum PaginaApp
{
    Dashboard,
    Productos,
    Ingredientes,
    Recetas,
    Mesas,
    Usuarios,
    Ventas,
    HistorialVentas,
    Reportes
}

/// <summary>Ventana principal única: conserva el sidebar y aloja una página a la vez.</summary>
public class BaseFormSidebar : BaseForm
{
    private readonly User usuarioActual;
    protected Panel PageContent { get; } = new();
    private readonly Dictionary<PaginaApp, Button> botones = new();
    private UserControl? paginaActual;
    private PaginaApp? paginaActiva;

    // WinForms designer needs a public parameterless constructor.
    public BaseFormSidebar() : this(PaginaApp.Dashboard, null) { }

    protected BaseFormSidebar(PaginaApp paginaInicial, User? usuario)
    {
        usuarioActual = usuario ?? new User();
        ConstruirSidebar();
        Navegar(paginaInicial);
    }

    private void ConstruirSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left, Width = 260, BackColor = Theme.ObsidianBlack,
            Padding = new Padding(8, 16, 8, 14)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            BackColor = Theme.ObsidianBlack, Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        sidebar.Controls.Add(layout);
        layout.Controls.Add(new PictureBox
        {
            Image = SistemaStock.Properties.Resources.FondoChachin,
            SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill,
            BackColor = Theme.ObsidianBlack, Margin = new Padding(0, 0, 0, 12)
        }, 0, 0);

        var navegacion = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
            WrapContents = false, AutoScroll = true, BackColor = Theme.ObsidianBlack
        };
        navegacion.HorizontalScroll.Enabled = false;
        navegacion.HorizontalScroll.Visible = false;
        layout.Controls.Add(navegacion, 0, 1);
        navegacion.Controls.Add(CrearBoton("Inicio", PaginaApp.Dashboard));
        navegacion.Controls.Add(CrearBoton("Ventas", PaginaApp.Ventas));
        navegacion.Controls.Add(CrearBoton("Productos", PaginaApp.Productos));
        navegacion.Controls.Add(CrearBoton("Ingredientes", PaginaApp.Ingredientes));
        navegacion.Controls.Add(CrearBoton("Recetas", PaginaApp.Recetas));
        navegacion.Controls.Add(CrearBoton("Mesas", PaginaApp.Mesas));
        navegacion.Controls.Add(CrearBoton("Usuarios", PaginaApp.Usuarios));
        navegacion.Controls.Add(CrearBoton("Reportes", PaginaApp.Reportes));
        if (UserPermissions.CanManageUsers(usuarioActual))
            navegacion.Controls.Add(CrearBoton("Historial de ventas", PaginaApp.HistorialVentas));

        var cerrarSesion = new IconButton
        {
            Text = "Cerrar sesión", Dock = DockStyle.Fill, Margin = Padding.Empty
        };
        Theme.SetButtonIcon(cerrarSesion, IconChar.RightFromBracket, 22);
        Theme.EstilizarBotonCerrarSesion(cerrarSesion);
        cerrarSesion.Click += (_, _) => { DialogResult = DialogResult.Retry; Close(); };
        layout.Controls.Add(cerrarSesion, 0, 2);

        var pageRoot = new Panel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal };
        PageContent.Dock = DockStyle.Fill;
        PageContent.BackColor = Theme.DeepCharcoal;
        PageContent.Padding = new Padding(34, 14, 34, 28);
        pageRoot.Controls.Add(PageContent);
        Controls.Add(pageRoot);
        Controls.Add(sidebar);
    }

    private IconButton CrearBoton(string texto, PaginaApp destino)
    {
        IconChar icono = destino switch
        {
            PaginaApp.Dashboard => IconChar.House,
            PaginaApp.Ventas => IconChar.CashRegister,
            PaginaApp.Productos => IconChar.BoxesStacked,
            PaginaApp.Ingredientes => IconChar.Carrot,
            PaginaApp.Recetas => IconChar.BookOpen,
            PaginaApp.Mesas => IconChar.Chair,
            PaginaApp.Usuarios => IconChar.Users,
            PaginaApp.HistorialVentas => IconChar.ClockRotateLeft,
            PaginaApp.Reportes => IconChar.ChartColumn,
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };
        var boton = new IconButton { Text = texto, Width = 236, Height = 48, Margin = new Padding(0, 0, 0, 8) };
        Theme.SetButtonIcon(boton, icono, 22);
        Theme.StyleNavigationButton(boton, false);
        boton.Click += (_, _) => Navegar(destino);
        botones.Add(destino, boton);
        return boton;
    }

    protected void Navegar(PaginaApp destino)
    {
        if (destino == PaginaApp.HistorialVentas && !UserPermissions.CanManageUsers(usuarioActual))
            return;

        if (paginaActiva == destino)
            return;

        UserControl siguiente = destino switch
        {
            PaginaApp.Dashboard => new FrmDashboard(usuarioActual),
            PaginaApp.Productos => new FrmProductos(usuarioActual),
            PaginaApp.Ingredientes => new FrmIngredientes(usuarioActual),
            PaginaApp.Recetas => new FrmRecetas(usuarioActual),
            PaginaApp.Mesas => new FrmMesas(usuarioActual),
            PaginaApp.Usuarios => new FrmUsers(usuarioActual),
            PaginaApp.Ventas => new FrmVentas(usuarioActual),
            PaginaApp.HistorialVentas => new FrmHistorialVentas(usuarioActual),
            PaginaApp.Reportes => new FrmReportes(),
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };

        foreach (var item in botones)
            Theme.StyleNavigationButton(item.Value, item.Key == destino);

        PageContent.SuspendLayout();
        try
        {
            if (paginaActual is not null)
            {
                PageContent.Controls.Remove(paginaActual);
                paginaActual.Dispose();
            }
            PageContent.Controls.Clear();
            paginaActual = siguiente;
            paginaActiva = destino;
            siguiente.Dock = DockStyle.Fill;
            PageContent.Controls.Add(siguiente);
        }
        catch
        {
            siguiente.Dispose();
            throw;
        }
        finally { PageContent.ResumeLayout(true); }
    }
}

/// <summary>Base de las secciones embebidas en PageContent.</summary>
public class BasePage : UserControl
{
    private readonly Panel header = new();
    protected Panel PageContent { get; } = new();

    public BasePage()
    {
        BackColor = Theme.DeepCharcoal;
        header.Dock = DockStyle.Top;
        header.Height = 64;
        header.BackColor = Theme.DeepCharcoal;
        Controls.Add(PageContent);
        PageContent.Dock = DockStyle.Fill;
        PageContent.BackColor = Theme.DeepCharcoal;
        Controls.Add(header);
    }

    protected void AddPageTitle(string titulo, string? subtitulo = null)
    {
        if (!string.IsNullOrWhiteSpace(subtitulo))
            header.Controls.Add(new Label
            {
                Text = subtitulo, Dock = DockStyle.Top, Height = 24,
                ForeColor = Theme.TextSecondary, Font = Theme.BodyFont,
                TextAlign = ContentAlignment.MiddleLeft
            });
        header.Controls.Add(new Label
        {
            Text = titulo, Dock = DockStyle.Top, Height = 38,
            ForeColor = Theme.TextPrimary, Font = Theme.HeadingFont,
            TextAlign = ContentAlignment.MiddleLeft
        });
    }

    protected DataGridView AddEmptyGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, Name = "gridContenido" };
        Theme.StyleGrid(grid);
        PageContent.Controls.Add(grid);
        return grid;
    }
}
