using FontAwesome.Sharp;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmDashboard : BasePage
{
    private readonly VentaRepositorio repositorio = new();
    private readonly DataGridView gridVentas = new();
    private readonly DataGridView gridStock = new();
    private readonly DataGridView gridActividad = new();
    private readonly Label[] valores = Enumerable.Range(0, 4).Select(_ => new Label()).ToArray();
    private readonly Label lblFecha = new();
    private readonly Label lblHora = new();
    private readonly System.Windows.Forms.Timer reloj = new() { Interval = 1000 };

    public FrmDashboard() : this(null) { }

    public FrmDashboard(User? user)
    {
        InitializeComponent();
        AddPageTitle("Dashboard", $"Resumen general · Sesión: {(string.IsNullOrWhiteSpace(user?.FullName) ? "Usuario" : user.FullName)}");
        reloj.Tick += (_, _) => ActualizarReloj();
        reloj.Start();
        Disposed += (_, _) => reloj.Dispose();
        ConstruirContenido();
        CargarResumen();
    }

    private void ConstruirContenido()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 1, RowCount = 2, Padding = new Padding(0, 8, 0, 0) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        PageContent.Controls.Add(root);

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 2, RowCount = 2 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); header.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        lblFecha.Dock = DockStyle.Fill; lblFecha.ForeColor = Theme.TextPrimary; lblFecha.TextAlign = ContentAlignment.MiddleRight;
        lblHora.Dock = DockStyle.Fill; lblHora.ForeColor = Theme.TextPrimary; lblHora.TextAlign = ContentAlignment.MiddleRight;
        header.Controls.Add(lblFecha, 1, 0); header.Controls.Add(lblHora, 1, 1);
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
        for (int i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        string[] names = ["Ventas de hoy", "Ventas realizadas", "Stock bajo", "Mesas ocupadas"];
        IconChar[] icons = [IconChar.ChartColumn, IconChar.CartShopping, IconChar.TriangleExclamation, IconChar.Chair];
        for (int i = 0; i < names.Length; i++) cards.Controls.Add(CrearTarjeta(names[i], icons[i], valores[i]), i, 0);
        header.Controls.Add(cards, 0, 0); header.SetRowSpan(cards, 2);
        root.Controls.Add(header, 0, 0);

        var lower = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 12, 0, 0) };
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        lower.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lower.Controls.Add(CrearPanel("Últimas ventas", gridVentas), 0, 0);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 1, RowCount = 2, Padding = new Padding(12, 0, 0, 0) };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        PrepararVentas(gridActividad, compacta: true);
        right.Controls.Add(CrearPanel("Actividad reciente", gridActividad), 0, 0);
        right.Controls.Add(CrearPanel("Productos con stock bajo", gridStock), 0, 1);
        lower.Controls.Add(right, 1, 0);
        root.Controls.Add(lower, 0, 1);

        PrepararVentas(gridVentas);
    }

    private static Control CrearTarjeta(string titulo, IconChar icono, Label valor)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Margin = new Padding(0, 0, 8, 0), Padding = new Padding(10) };
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Theme.Surface };
        var ico = new IconPictureBox { IconChar = icono, IconColor = Theme.BlazeOrange, IconSize = 24, Size = new Size(32, 30), SizeMode = PictureBoxSizeMode.CenterImage, Dock = DockStyle.Left, BackColor = Theme.Surface };
        encabezado.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Font = Theme.BodyFont, TextAlign = ContentAlignment.MiddleLeft });
        encabezado.Controls.Add(ico);
        valor.Text = "—"; valor.Dock = DockStyle.Fill; valor.ForeColor = Theme.TextPrimary; valor.Font = new Font("Segoe UI", 17F, FontStyle.Bold); valor.TextAlign = ContentAlignment.MiddleLeft;
        panel.Controls.Add(valor); panel.Controls.Add(encabezado);
        return panel;
    }

    private static Control CrearPanel(string titulo, DataGridView grid)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(10), Margin = new Padding(0, 0, 0, 10) };
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Theme.Surface };
        encabezado.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Fill, ForeColor = Theme.TextPrimary, Font = new Font(Theme.BodyFont, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });
        encabezado.Controls.Add(new IconPictureBox { IconChar = titulo.Contains("stock") ? IconChar.TriangleExclamation : IconChar.Clock, IconColor = Theme.BlazeOrange, IconSize = 20, Dock = DockStyle.Left, Size = new Size(30, 30), SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Theme.Surface });
        panel.Controls.Add(grid); panel.Controls.Add(encabezado);
        grid.Dock = DockStyle.Fill;
        return panel;
    }

    private static void PrepararVentas(DataGridView grid, bool compacta = false)
    {
        grid.AutoGenerateColumns = false;
        if (!compacta)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°", DataPropertyName = "Id", FillWeight = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", DataPropertyName = "Fecha", FillWeight = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM HH:mm" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", DataPropertyName = "Tipo", FillWeight = 75 });
        }
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = compacta ? "Actividad" : "Total", DataPropertyName = compacta ? "Actividad" : "Total", FillWeight = 90, DefaultCellStyle = compacta ? new DataGridViewCellStyle() : new DataGridViewCellStyle { Format = "C2" } });
        if (!compacta) grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Estado", DataPropertyName = "Estado", FillWeight = 70 });
        Theme.StyleGrid(grid);
    }

    private void CargarResumen()
    {
        var resumen = repositorio.ObtenerResumenDashboard();
        valores[0].Text = resumen.VentasDelDia.ToString("C0");
        valores[1].Text = resumen.CantidadVentasDelDia.ToString();
        valores[2].Text = resumen.CantidadStockBajo.ToString();
        valores[3].Text = $"{resumen.MesasOcupadas} / {resumen.MesasActivas}";
        gridVentas.DataSource = resumen.UltimasVentas;
        gridActividad.DataSource = resumen.UltimasVentas.Take(5).ToList();
        gridStock.AutoGenerateColumns = false;
        gridStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto / ingrediente", DataPropertyName = "Nombre", FillWeight = 80 });
        gridStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", FillWeight = 35 });
        Theme.StyleGrid(gridStock);
        gridStock.DataSource = resumen.ProductosStockBajo;
        gridStock.CellFormatting += (_, e) => { if (gridStock.Columns[e.ColumnIndex].DataPropertyName == "Stock") e.CellStyle.ForeColor = Theme.StatusDanger; };
        gridVentas.CellFormatting += (_, e) => { if (gridVentas.Columns[e.ColumnIndex].DataPropertyName == "Estado" && e.Value is string estado) e.CellStyle.ForeColor = estado switch { "Pendiente" => Theme.BlazeOrange, "Anulada" => Theme.StatusDanger, "Cerrada" => Theme.StatusSuccess, _ => Theme.TextPrimary }; };
    }

    private void ActualizarReloj()
    {
        var ahora = DateTime.Now;
        lblFecha.Text = ahora.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-AR"));
        lblHora.Text = ahora.ToString("HH:mm");
    }
}
