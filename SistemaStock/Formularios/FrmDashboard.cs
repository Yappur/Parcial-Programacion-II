using FontAwesome.Sharp;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmDashboard : BasePage
{
    private static readonly Font FuenteAdvertencia = new("Segoe UI Symbol", 12F, FontStyle.Bold);
    private static readonly Font FuenteIconoActividad = new("Segoe MDL2 Assets", 14F);
    private readonly VentaRepositorio repositorio = new();
    private readonly DataGridView gridVentas = new();
    private readonly DataGridView gridStock = new();
    private readonly DataGridView gridActividad = new();
    private readonly Label[] valores = Enumerable.Range(0, 4).Select(_ => new Label()).ToArray();

    public FrmDashboard() : this(null) { }

    public FrmDashboard(User? user)
    {
        InitializeComponent();
        AddPageTitle("Inicio", $"Resumen general · Sesión: {(string.IsNullOrWhiteSpace(user?.FullName) ? "Usuario" : user.FullName)}");
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

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 1, RowCount = 1 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var cards = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal, ColumnCount = 4, RowCount = 1, Margin = Padding.Empty };
        for (int i = 0; i < 4; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        cards.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        string[] names = ["Ventas de hoy", "Ventas realizadas", "Stock bajo", "Mesas ocupadas"];
        IconChar[] icons = [IconChar.ChartColumn, IconChar.CartShopping, IconChar.TriangleExclamation, IconChar.Chair];
        for (int i = 0; i < names.Length; i++)
        {
            var tarjeta = CrearTarjeta(names[i], icons[i], valores[i]);
            tarjeta.Margin = new Padding(0, 0, i == names.Length - 1 ? 0 : 8, 0);
            cards.Controls.Add(tarjeta, i, 0);
        }
        header.Controls.Add(cards, 0, 0);
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
        if (compacta)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Icono",
                HeaderText = "",
                FillWeight = 10,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Acción",
                DataPropertyName = "Texto",
                FillWeight = 55,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True,
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Precio",
                DataPropertyName = "Precio",
                FillWeight = 22,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Hora",
                DataPropertyName = "FechaHora",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "HH:mm",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            grid.RowTemplate.Height = 54;
        }
        else
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "N°", DataPropertyName = "Id", FillWeight = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fecha", DataPropertyName = "Fecha", FillWeight = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM HH:mm" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tipo", DataPropertyName = "Tipo", FillWeight = 75 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", FillWeight = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Estado",
                DataPropertyName = "Estado",
                FillWeight = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
        }
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
        gridActividad.DataSource = ActividadRepositorio.ListarRecientes(5);
        gridActividad.CellPainting += (_, e) =>
        {
            if (e.RowIndex < 0 || gridActividad.Columns[e.ColumnIndex].Name != "Icono") return;
            e.PaintBackground(e.ClipBounds, true);
            if (gridActividad.Rows[e.RowIndex].DataBoundItem is ActividadDashboard actividad)
            {
                string simbolo = actividad.Tipo switch
                {
                    "USUARIO" => "\uE77B",
                    "PRODUCTO" or "VENTA" => "\uE7BF",
                    "INGREDIENTE" or "RECETA" => "\uE7B8",
                    _ => "\uE80F"
                };
                TextRenderer.DrawText(e.Graphics, simbolo, FuenteIconoActividad, e.CellBounds,
                    Theme.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
            e.Handled = true;
        };
        gridStock.AutoGenerateColumns = false;
        gridStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Advertencia",
            HeaderText = "",
            FillWeight = 18,
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
        });
        gridStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Producto / ingrediente", DataPropertyName = "Nombre", FillWeight = 80 });
        gridStock.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stock", DataPropertyName = "Stock", FillWeight = 35 });
        Theme.StyleGrid(gridStock);
        gridStock.DataSource = resumen.ProductosStockBajo;
        gridStock.CellFormatting += (_, e) =>
        {
            var columna = gridStock.Columns[e.ColumnIndex];
            if (columna.Name == "Advertencia")
            {
                e.Value = "⚠";
                e.CellStyle.ForeColor = Theme.BlazeOrange;
                e.CellStyle.Font = FuenteAdvertencia;
                e.FormattingApplied = true;
            }
            else if (columna.DataPropertyName == "Stock")
            {
                e.CellStyle.ForeColor = Theme.StatusDanger;
            }
        };
        gridVentas.CellPainting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || gridVentas.Columns[e.ColumnIndex].DataPropertyName != "Estado" || e.Value is not string estado)
                return;

            Color color = estado switch
            {
                "Pendiente" => Theme.BlazeOrange,
                "Anulada" => Theme.StatusDanger,
                "Cerrada" => Theme.StatusSuccess,
                _ => Theme.SurfaceRaised
            };
            e.PaintBackground(e.CellBounds, true);
            Size tamanoTexto = TextRenderer.MeasureText(estado, Theme.BodyFont);
            int anchoEtiqueta = Math.Min(e.CellBounds.Width - 8, tamanoTexto.Width + 16);
            int altoEtiqueta = Math.Min(e.CellBounds.Height - 8, tamanoTexto.Height + 6);
            var etiqueta = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - anchoEtiqueta) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - altoEtiqueta) / 2,
                anchoEtiqueta,
                altoEtiqueta);
            using var ruta = CrearRutaRedondeada(etiqueta, 7);
            using var fondo = new SolidBrush(color);
            var suavizadoAnterior = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillPath(fondo, ruta);
            e.Graphics.SmoothingMode = suavizadoAnterior;
            TextRenderer.DrawText(e.Graphics, estado, Theme.BodyFont, etiqueta, Theme.TextPrimary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.Handled = true;
        };
    }

    private static System.Drawing.Drawing2D.GraphicsPath CrearRutaRedondeada(Rectangle rectangulo, int radio)
    {
        int diametro = radio * 2;
        var ruta = new System.Drawing.Drawing2D.GraphicsPath();
        ruta.AddArc(rectangulo.X, rectangulo.Y, diametro, diametro, 180, 90);
        ruta.AddArc(rectangulo.Right - diametro, rectangulo.Y, diametro, diametro, 270, 90);
        ruta.AddArc(rectangulo.Right - diametro, rectangulo.Bottom - diametro, diametro, diametro, 0, 90);
        ruta.AddArc(rectangulo.X, rectangulo.Bottom - diametro, diametro, diametro, 90, 90);
        ruta.CloseFigure();
        return ruta;
    }

}
