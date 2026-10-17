using System.Drawing;
using System.Windows.Forms;
using SistemaStock.Formularios;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Estilos;

public enum PaginaApp
{
    Dashboard,
    Productos,
    Usuarios,
    Ventas
}

/// <summary>Marco compartido para las pantallas principales y su navegación.</summary>
public class BaseFormSidebar : BaseForm
{
    private readonly PaginaApp paginaActiva;
    private readonly User? usuarioActual;
    private readonly Panel pageRoot = new();
    private readonly Panel pageHeader = new();
    protected Panel PageContent { get; } = new();

    // WinForms designer needs a concrete base type with a public parameterless constructor.
    public BaseFormSidebar() : this(PaginaApp.Dashboard, null) { }

    protected BaseFormSidebar(PaginaApp paginaActiva, User? usuarioActual)
    {
        this.paginaActiva = paginaActiva;
        this.usuarioActual = usuarioActual;
        ConstruirSidebar();
    }

    private void ConstruirSidebar()
    {
        var sidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 235,
            BackColor = Theme.ObsidianBlack,
            Padding = new Padding(14, 16, 14, 14)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.ObsidianBlack,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        sidebar.Controls.Add(layout);

        var logo = new PictureBox
        {
            Image = SistemaStock.Properties.Resources.FondoChachin,
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            BackColor = Theme.ObsidianBlack,
            Margin = new Padding(0, 0, 0, 12)
        };
        layout.Controls.Add(logo, 0, 0);

        var navegacion = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.ObsidianBlack
        };
        layout.Controls.Add(navegacion, 0, 1);
        navegacion.Controls.Add(CrearBoton("Dashboard", PaginaApp.Dashboard));
        navegacion.Controls.Add(CrearBoton("Productos", PaginaApp.Productos));
        navegacion.Controls.Add(CrearBoton("Usuarios", PaginaApp.Usuarios));
        navegacion.Controls.Add(CrearBoton("Ventas", PaginaApp.Ventas));

        var cerrarSesion = new Button
        {
            Text = "Cerrar sesión",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            Font = Theme.BodyFont,
            Cursor = Cursors.Hand
        };
        cerrarSesion.FlatAppearance.BorderColor = Theme.BlazeOrange;
        cerrarSesion.FlatAppearance.BorderSize = 1;
        cerrarSesion.Click += (_, _) =>
        {
            DialogResult = DialogResult.Retry;
            Close();
        };
        layout.Controls.Add(cerrarSesion, 0, 2);

        pageRoot.Dock = DockStyle.Fill;
        pageRoot.BackColor = Theme.DeepCharcoal;
        PageContent.Dock = DockStyle.Fill;
        PageContent.BackColor = Theme.DeepCharcoal;
        PageContent.Padding = new Padding(34, 14, 34, 28);
        pageRoot.Controls.Add(PageContent);
        pageHeader.Dock = DockStyle.Top;
        pageHeader.Height = 106;
        pageHeader.BackColor = Theme.DeepCharcoal;
        pageHeader.Padding = new Padding(34, 26, 34, 0);
        pageRoot.Controls.Add(pageHeader);
        Controls.Add(pageRoot);
        Controls.Add(sidebar);
    }

    private Button CrearBoton(string texto, PaginaApp destino)
    {
        var boton = new Button { Text = texto, Width = 207, Height = 48, Margin = new Padding(0, 0, 0, 8) };
        Theme.StyleNavigationButton(boton, destino == paginaActiva);
        boton.Click += (_, _) => Navegar(destino);
        return boton;
    }

    protected Label AddPageTitle(string titulo, string? subtitulo = null)
    {
        var title = new Label
        {
            Text = titulo,
            Location = new Point(34, 22),
            Size = new Size(760, 44),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ForeColor = Theme.TextPrimary,
            Font = Theme.HeadingFont,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pageHeader.Controls.Add(title);
        if (!string.IsNullOrWhiteSpace(subtitulo))
        {
            pageHeader.Controls.Add(new Label
            {
                Text = subtitulo,
                Location = new Point(36, 67),
                Size = new Size(760, 27),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Theme.TextSecondary,
                Font = Theme.BodyFont,
                TextAlign = ContentAlignment.MiddleLeft
            });
        }
        return title;
    }

    protected DataGridView AddEmptyGrid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, Name = "gridContenido" };
        Theme.StyleGrid(grid);
        PageContent.Controls.Add(grid);
        grid.BringToFront();
        return grid;
    }

    private void Navegar(PaginaApp destino)
    {
        if (destino == paginaActiva)
            return;

        BaseFormSidebar siguiente = destino switch
        {
            PaginaApp.Dashboard => new FrmDashboard(usuarioActual),
            PaginaApp.Productos => new FrmProductos(usuarioActual),
            PaginaApp.Usuarios => new FrmUsers(usuarioActual),
            PaginaApp.Ventas => new FrmVentas(usuarioActual),
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };

        using (siguiente)
        {
            Hide();
            try
            {
                if (siguiente.ShowDialog(this) == DialogResult.Retry)
                {
                    DialogResult = DialogResult.Retry;
                    Close();
                }
            }
            finally
            {
                if (!IsDisposed && DialogResult != DialogResult.Retry)
                {
                    Show();
                    Activate();
                }
            }
        }
    }
}
