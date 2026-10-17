using System.Drawing;
using System.Windows.Forms;
using SistemaStock.Formularios;

namespace SistemaStock.Estilos;

public enum PaginaApp
{
    Dashboard,
    Productos,
    Usuarios
}

/// <summary>Marco compartido para las pantallas principales y su navegación.</summary>
public abstract class BaseFormSidebar : BaseForm
{
    private readonly PaginaApp paginaActiva;
    private readonly User? usuarioActual;
    private readonly Panel pageRoot = new();
    private readonly Panel pageHeader = new();
    protected Panel PageContent { get; } = new();

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
        var marca = new Panel { Dock = DockStyle.Top, Height = 145, BackColor = Theme.ObsidianBlack };
        sidebar.Controls.Add(marca);

        var logo = new PictureBox
        {
            Image = SistemaStock.Properties.Resources.FondoChachin,
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(4, 4),
            Size = new Size(76, 76),
            BackColor = Theme.ObsidianBlack
        };
        marca.Controls.Add(logo);
        marca.Controls.Add(new Label
        {
            Text = "Chachín",
            ForeColor = Theme.TextPrimary,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            Location = new Point(2, 87),
            Size = new Size(200, 38),
            TextAlign = ContentAlignment.MiddleLeft
        });

        var navegacion = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 174,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.ObsidianBlack
        };
        sidebar.Controls.Add(navegacion);
        navegacion.Controls.Add(CrearBoton("Dashboard", PaginaApp.Dashboard));
        navegacion.Controls.Add(CrearBoton("Productos", PaginaApp.Productos));
        navegacion.Controls.Add(CrearBoton("Usuarios", PaginaApp.Usuarios));

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
        var boton = new Button { Text = texto, Width = 205, Height = 48, Margin = new Padding(0, 0, 0, 8) };
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
            _ => throw new ArgumentOutOfRangeException(nameof(destino))
        };

        using (siguiente)
        {
            Hide();
            try
            {
                siguiente.ShowDialog(this);
            }
            finally
            {
                if (!IsDisposed)
                {
                    Show();
                    Activate();
                }
            }
        }
    }
}
