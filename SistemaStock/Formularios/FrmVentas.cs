using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BasePage
{
    private readonly VentaRepositorio repositorio = new();
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly TextBox busqueda = new() { PlaceholderText = "N.º de venta o usuario", Width = 190 };
    private readonly DateTimePicker fechaDesde = new() { Width = 145, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly DateTimePicker fechaHasta = new() { Width = 145, Format = DateTimePickerFormat.Short, ShowCheckBox = true };
    private readonly Button verDetalle = new() { Text = "Ver detalle", AutoSize = true, Enabled = false };

    public FrmVentas() : this(null) { }

    public FrmVentas(User? user)
    {
        InitializeComponent();
        _ = user;
        Theme.StyleGrid(grid);
        AddPageTitle("Historial de ventas", "Buscá ventas, filtrá por fecha y consultá quién las realizó");

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 92, Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal, WrapContents = true
        };
        barra.Controls.Add(new Label { Text = "Buscar", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(busqueda);
        barra.Controls.Add(new Label { Text = "Desde", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(fechaDesde);
        barra.Controls.Add(new Label { Text = "Hasta", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(fechaHasta);

        var buscar = new Button { Text = "Buscar", AutoSize = true };
        EstilizarBoton(buscar);
        buscar.Click += (_, _) => CargarVentas();
        barra.Controls.Add(buscar);
        EstilizarBoton(verDetalle);
        verDetalle.Click += (_, _) => MostrarDetalle();
        barra.Controls.Add(verDetalle);

        grid.SelectionChanged += (_, _) => verDetalle.Enabled = VentaSeleccionada() is not null;
        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(barra);
        CargarVentas();
    }

    private Venta? VentaSeleccionada() => grid.CurrentRow?.DataBoundItem as Venta;

    private void CargarVentas()
    {
        if (fechaDesde.Checked && fechaHasta.Checked && fechaDesde.Value.Date > fechaHasta.Value.Date)
        {
            MessageBox.Show(this, "La fecha inicial no puede ser posterior a la fecha final.", "Rango de fechas inválido",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            grid.DataSource = repositorio.Buscar(busqueda.Text,
                fechaDesde.Checked ? fechaDesde.Value.Date : null,
                fechaHasta.Checked ? fechaHasta.Value.Date : null);
            ConfigurarColumnas();
            verDetalle.Enabled = VentaSeleccionada() is not null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo cargar el historial de ventas.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarColumnas()
    {
        if (grid.Columns[nameof(Venta.Id)] is { } id) id.HeaderText = "N.º venta";
        if (grid.Columns[nameof(Venta.FechaHora)] is { } fecha)
        {
            fecha.HeaderText = "Fecha y hora";
            fecha.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }
        if (grid.Columns[nameof(Venta.UsuarioNombre)] is { } usuario) usuario.HeaderText = "Usuario";
        if (grid.Columns[nameof(Venta.NombreUsuario)] is { } nombreUsuario)
        {
            nombreUsuario.HeaderText = "Nombre de usuario";
            nombreUsuario.Visible = false;
        }
        if (grid.Columns[nameof(Venta.Total)] is { } total)
        {
            total.HeaderText = "Total";
            total.DefaultCellStyle.Format = "C2";
        }
        if (grid.Columns[nameof(Venta.UsuarioId)] is { } usuarioId) usuarioId.Visible = false;
    }

    private void MostrarDetalle()
    {
        var venta = VentaSeleccionada();
        if (venta is null) return;
        try
        {
            using var form = new FrmVentaDetalle(venta, repositorio.ObtenerDetalle(venta.Id));
            form.ShowDialog(this.FindForm());
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo cargar el detalle de la venta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void EstilizarBoton(Button boton)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.BackColor = Theme.BlazeOrange;
        boton.ForeColor = Theme.TextPrimary;
        boton.Cursor = Cursors.Hand;
    }
}

internal sealed class FrmVentaDetalle : BaseForm
{
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };

    public FrmVentaDetalle(Venta venta, List<VentaDetalle> detalle)
    {
        Text = $"Chachín | Venta #{venta.Id}";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        ClientSize = new Size(760, 480);
        Theme.StyleGrid(grid);
        grid.DataSource = detalle;
        if (grid.Columns[nameof(VentaDetalle.CodigoProducto)] is { } codigo) codigo.HeaderText = "Código";
        if (grid.Columns[nameof(VentaDetalle.NombreProducto)] is { } nombre) nombre.HeaderText = "Producto";
        if (grid.Columns[nameof(VentaDetalle.Cantidad)] is { } cantidad) cantidad.HeaderText = "Cantidad";
        if (grid.Columns[nameof(VentaDetalle.PrecioUnitario)] is { } precio)
        {
            precio.HeaderText = "Precio unitario";
            precio.DefaultCellStyle.Format = "C2";
        }
        if (grid.Columns[nameof(VentaDetalle.Subtotal)] is { } subtotal)
        {
            subtotal.HeaderText = "Subtotal";
            subtotal.DefaultCellStyle.Format = "C2";
        }

        var encabezado = new Label
        {
            Text = $"Venta #{venta.Id}  |  {venta.FechaHora:dd/MM/yyyy HH:mm}  |  {venta.UsuarioNombre} ({venta.NombreUsuario})",
            Dock = DockStyle.Top, Height = 48, Font = Theme.BodyFont,
            ForeColor = Theme.TextPrimary, TextAlign = ContentAlignment.MiddleLeft
        };
        var total = new Label
        {
            Text = $"Total: {venta.Total:C2}", Dock = DockStyle.Bottom, Height = 42,
            Font = Theme.BodyFont, ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleRight
        };
        Controls.Add(grid);
        Controls.Add(total);
        Controls.Add(encabezado);
    }
}
