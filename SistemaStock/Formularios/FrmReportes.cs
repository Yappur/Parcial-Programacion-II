using FontAwesome.Sharp;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;

namespace SistemaStock.Formularios;

public sealed class FrmReportes : BasePage
{
    private readonly VentaRepositorio ventasRepositorio = new();
    private readonly ProductoRepositorio productosRepositorio = new();
    private readonly DateTimePicker fechaDesde = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly DateTimePicker fechaHasta = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly DataGridView gridVentas = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly DataGridView gridStock = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly Label lblResumenVentas = CrearEstado();
    private readonly Label lblEstadoVentas = CrearEstado();
    private readonly Label lblEstadoStock = CrearEstado();
    private readonly TabControl pestañas = new() { Dock = DockStyle.Fill };
    private bool stockCargado;

    public FrmReportes()
    {
        AddPageTitle("Reportes", "Consultas de ventas cerradas y productos que necesitan reposición.");
        fechaDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        fechaHasta.Value = DateTime.Today;
        ConstruirInterfaz();
        CargarVentas();
    }

    private void ConstruirInterfaz()
    {
        var paginaVentas = CrearPagina("Ventas por período");
        var controlesFecha = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, WrapContents = true,
            BackColor = Theme.DeepCharcoal, Padding = new Padding(8, 4, 8, 10)
        };
        controlesFecha.Controls.Add(CrearEtiqueta("Desde"));
        controlesFecha.Controls.Add(fechaDesde);
        controlesFecha.Controls.Add(CrearEtiqueta("Hasta"));
        controlesFecha.Controls.Add(fechaHasta);
        var btnGenerar = new IconButton { Text = "Generar", AutoSize = true, Height = 36 };
        Theme.SetButtonIcon(btnGenerar, IconChar.Filter);
        Theme.EstilizarBotonPrimario(btnGenerar);
        btnGenerar.Click += (_, _) => CargarVentas();
        controlesFecha.Controls.Add(btnGenerar);
        paginaVentas.Controls.Add(gridVentas);
        paginaVentas.Controls.Add(lblEstadoVentas);
        paginaVentas.Controls.Add(lblResumenVentas);
        paginaVentas.Controls.Add(controlesFecha);

        var paginaStock = CrearPagina("Productos con bajo stock");
        paginaStock.Controls.Add(gridStock);
        paginaStock.Controls.Add(lblEstadoStock);

        pestañas.TabPages.Add(paginaVentas);
        pestañas.TabPages.Add(paginaStock);
        Theme.StyleTabControl(pestañas);
        pestañas.SelectedIndexChanged += (_, _) =>
        {
            if (pestañas.SelectedIndex == 1 && !stockCargado) CargarStockBajo();
        };
        PageContent.Controls.Add(pestañas);

        Theme.StyleGrid(gridVentas);
        Theme.StyleGrid(gridStock);
        gridVentas.ReadOnly = true;
        gridVentas.AllowUserToAddRows = false;
        gridVentas.AllowUserToDeleteRows = false;
        gridVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridVentas.MultiSelect = false;
        gridStock.ReadOnly = true;
        gridStock.AllowUserToAddRows = false;
        gridStock.AllowUserToDeleteRows = false;
        gridStock.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridStock.MultiSelect = false;
        fechaDesde.BackColor = Theme.Surface;
        fechaDesde.ForeColor = Theme.TextPrimary;
        fechaHasta.BackColor = Theme.Surface;
        fechaHasta.ForeColor = Theme.TextPrimary;
    }

    private void CargarVentas()
    {
        if (fechaDesde.Value.Date > fechaHasta.Value.Date)
        {
            MessageBox.Show("La fecha inicial no puede ser posterior a la fecha final.",
                "Rango de fechas inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            List<VentaHistorial> resultado = ventasRepositorio.ListarVentasCerradasPorPeriodo(
                fechaDesde.Value.Date, fechaHasta.Value.Date);
            gridVentas.DataSource = resultado;
            ConfigurarGrillaVentas();
            decimal total = resultado.Sum(v => v.Total);
            lblResumenVentas.Text = $"Ventas: {resultado.Count}     Total vendido: {total:C2}";
            lblEstadoVentas.Text = resultado.Count == 0
                ? "No hay ventas cerradas en el período seleccionado."
                : "";
        }
        catch (Exception ex)
        {
            gridVentas.DataSource = null;
            lblResumenVentas.Text = "No se pudieron calcular los totales.";
            lblEstadoVentas.Text = "Error al consultar las ventas. Revisá la conexión a la base de datos.";
            MessageBox.Show($"No se pudo cargar el reporte de ventas.\n{ex.Message}",
                "Error de consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarStockBajo()
    {
        try
        {
            List<Producto> resultado = productosRepositorio.GetLowStock();
            gridStock.DataSource = resultado;
            ConfigurarGrillaStock();
            lblEstadoStock.Text = resultado.Count == 0
                ? "No hay productos activos con stock igual o inferior al mínimo."
                : $"Productos que necesitan reposición: {resultado.Count}";
            stockCargado = true;
        }
        catch (Exception ex)
        {
            gridStock.DataSource = null;
            lblEstadoStock.Text = "Error al consultar el stock. Revisá la conexión a la base de datos.";
            MessageBox.Show($"No se pudo cargar el reporte de bajo stock.\n{ex.Message}",
                "Error de consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarGrillaVentas()
    {
        OcultarColumnas(gridVentas, nameof(VentaHistorial.FechaHora), nameof(VentaHistorial.Estado), nameof(VentaHistorial.Notas));
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.Id), "N.º", 45);
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.FechaCierre), "Fecha de cierre", 105, "dd/MM/yyyy HH:mm");
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.TipoVenta), "Tipo", 75);
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.Mesa), "Mesa", 45);
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.Atendio), "Atendió", 110);
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.MedioPago), "Pago", 80);
        ConfigurarColumna(gridVentas, nameof(VentaHistorial.Total), "Total", 85, "C2");
    }

    private void ConfigurarGrillaStock()
    {
        OcultarColumnas(gridStock, nameof(Producto.Precio), nameof(Producto.TipoStock), nameof(Producto.Activo),
            nameof(Producto.CategoriaId), nameof(Producto.StockDisponible));
        ConfigurarColumna(gridStock, nameof(Producto.Codigo), "Código", 60);
        ConfigurarColumna(gridStock, nameof(Producto.Nombre), "Producto", 150);
        ConfigurarColumna(gridStock, nameof(Producto.CategoriaNombre), "Categoría", 100);
        ConfigurarColumna(gridStock, nameof(Producto.Stock), "Stock", 65);
        ConfigurarColumna(gridStock, nameof(Producto.StockMinimo), "Mínimo", 65);
    }

    private static void OcultarColumnas(DataGridView grid, params string[] nombres)
    {
        foreach (string nombre in nombres)
            if (grid.Columns[nombre] is { } columna) columna.Visible = false;
    }

    private static void ConfigurarColumna(DataGridView grid, string nombre, string titulo, float peso, string? formato = null)
    {
        if (grid.Columns[nombre] is not { } columna) return;
        columna.HeaderText = titulo;
        columna.MinimumWidth = 40;
        columna.FillWeight = peso;
        columna.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        if (formato is not null) columna.DefaultCellStyle.Format = formato;
    }

    private static TabPage CrearPagina(string texto) => new(texto)
    {
        BackColor = Theme.DeepCharcoal,
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(8)
    };

    private static Label CrearEtiqueta(string texto) => new()
    {
        Text = texto, AutoSize = true, ForeColor = Theme.TextPrimary,
        Padding = new Padding(0, 8, 4, 0)
    };

    private static Label CrearEstado() => new()
    {
        Dock = DockStyle.Top, AutoSize = true, MinimumSize = new Size(0, 32),
        BackColor = Theme.DeepCharcoal, ForeColor = Theme.TextSecondary,
        TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 6, 8, 4)
    };
}
