using System.Globalization;
using FontAwesome.Sharp;
using LiveChartsCore;
using LiveChartsCore.Kernel.Events;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public sealed class FrmReportes : BasePage
{
    private readonly VentaRepositorio ventasRepositorio = new();
    private readonly ProductoRepositorio productosRepositorio = new();
    private readonly DateTimePicker fechaDesde = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly DateTimePicker fechaHasta = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly DataGridView gridVentas = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly DataGridView gridStock = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly CartesianChart chartVentas = CrearChartBase();
    private readonly CartesianChart chartStock = CrearChartBase();
    private readonly Panel chartStockViewport = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        BackColor = Theme.Surface,
        Margin = Padding.Empty,
        Padding = Padding.Empty
    };
    private readonly Label lblResumenVentas = CrearEstado();
    private readonly Label lblEstadoVentas = CrearEstado();
    private readonly Label lblEstadoStock = CrearEstado();
    private bool stockCargado;
    private bool stockCargaEnCurso;
    private int cantidadProductosEnGraficoStock;
    private Panel? paginaVentas;
    private Panel? paginaStock;
    private Button? btnPestanaVentas;
    private Button? btnPestanaStock;

    public FrmReportes(User usuario)
    {
        if (usuario is null || !UserPermissions.CanViewReports(usuario))
            throw new UnauthorizedAccessException("No tenés permiso para acceder a los reportes.");

        AddPageTitle("Reportes", "Consultas de ventas cerradas y productos que necesitan reposición.");
        fechaDesde.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        fechaHasta.Value = DateTime.Today;
        ConstruirInterfaz();
        CargarVentas();
    }

    private void ConstruirInterfaz()
    {
        paginaVentas = CrearPagina();
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
        paginaVentas.Controls.Add(CrearLayoutGraficoTabla(
            CrearPanelGrafico("Recaudación por día", chartVentas), gridVentas));
        paginaVentas.Controls.Add(lblEstadoVentas);
        paginaVentas.Controls.Add(lblResumenVentas);
        paginaVentas.Controls.Add(controlesFecha);

        paginaStock = CrearPagina();
        paginaStock.Visible = false;
        paginaStock.Controls.Add(CrearLayoutGraficoTabla(
            CrearPanelGrafico("Stock actual y mínimo", chartStock, chartStockViewport), gridStock));
        paginaStock.Controls.Add(lblEstadoStock);

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            WrapContents = false,
            BackColor = Theme.DeepCharcoal,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        btnPestanaVentas = new Button { Text = "Ventas por período", Width = 170, Height = 36 };
        btnPestanaStock = new Button { Text = "Productos con bajo stock", Width = 250, Height = 36 };
        Theme.StyleTabButton(btnPestanaVentas, true);
        Theme.StyleTabButton(btnPestanaStock, false);
        btnPestanaVentas.Margin = new Padding(0, 0, 8, 4);
        btnPestanaStock.Margin = new Padding(0, 0, 8, 4);
        btnPestanaVentas.Click += async (_, _) => await MostrarReporteAsync(ventas: true);
        btnPestanaStock.Click += async (_, _) => await MostrarReporteAsync(ventas: false);

        var contenido = new Panel { Dock = DockStyle.Fill, BackColor = Theme.DeepCharcoal };
        contenido.Controls.Add(paginaStock);
        contenido.Controls.Add(paginaVentas);
        PageContent.Controls.Add(contenido);
        PageContent.Controls.Add(barra);
        barra.Controls.Add(btnPestanaVentas);
        barra.Controls.Add(btnPestanaStock);

        Theme.StyleGrid(gridVentas);
        Theme.StyleGrid(gridStock);
        gridStock.CellFormatting += FormatearStockBajo;
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
        chartStockViewport.SizeChanged += (_, _) => AjustarTamanoGraficoStock();
    }

    private async Task MostrarReporteAsync(bool ventas)
    {
        if (paginaVentas is null || paginaStock is null || btnPestanaVentas is null || btnPestanaStock is null)
            return;

        paginaVentas.Visible = ventas;
        paginaStock.Visible = !ventas;
        Theme.StyleTabButton(btnPestanaVentas, ventas);
        Theme.StyleTabButton(btnPestanaStock, !ventas);
        if (!ventas && !stockCargado && !stockCargaEnCurso)
            await CargarStockBajoAsync();
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
            ActualizarGraficoVentas(ReporteGraficoDatos.AgruparRecaudacionPorDia(resultado));
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
            chartVentas.Series = Array.Empty<ISeries>();
            lblResumenVentas.Text = "No se pudieron calcular los totales.";
            lblEstadoVentas.Text = "Error al consultar las ventas. Revisá la conexión a la base de datos.";
            MessageBox.Show($"No se pudo cargar el reporte de ventas.\n{ex.Message}",
                "Error de consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CargarStockBajoAsync()
    {
        stockCargaEnCurso = true;
        lblEstadoStock.Text = "Cargando productos con bajo stock...";
        try
        {
            List<Producto> resultado = await Task.Run(() => productosRepositorio.GetLowStock());
            if (IsDisposed || Disposing)
                return;

            ActualizarGraficoStock(ReporteGraficoDatos.PrepararComparacionStock(resultado));
            gridStock.DataSource = resultado;
            ConfigurarGrillaStock();
            lblEstadoStock.Text = resultado.Count == 0
                ? "No hay productos activos con stock igual o inferior al mínimo."
                : $"Productos que necesitan reposición: {resultado.Count}";
            stockCargado = true;
        }
        catch (Exception ex)
        {
            if (IsDisposed || Disposing)
                return;

            gridStock.DataSource = null;
            chartStock.Series = Array.Empty<ISeries>();
            lblEstadoStock.Text = "Error al consultar el stock. Revisá la conexión a la base de datos.";
            MessageBox.Show($"No se pudo cargar el reporte de bajo stock.\n{ex.Message}",
                "Error de consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            stockCargaEnCurso = false;
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

    private void FormatearStockBajo(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
            gridStock.Columns[e.ColumnIndex].Name == nameof(Producto.Stock) &&
            gridStock.Rows[e.RowIndex].DataBoundItem is Producto)
            e.CellStyle.ForeColor = Theme.StatusDanger;
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
        columna.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        if (formato is not null) columna.DefaultCellStyle.Format = formato;
    }

    private static Panel CrearPagina() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Theme.DeepCharcoal,
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(8)
    };

    private static CartesianChart CrearChartBase() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = Theme.Surface,
        LegendTextPaint = new SolidColorPaint(ChartPalette.Texto),
        LegendBackgroundPaint = new SolidColorPaint(ChartPalette.Fondo),
        TooltipTextPaint = new SolidColorPaint(ChartPalette.Texto),
        TooltipBackgroundPaint = new SolidColorPaint(ChartPalette.Fondo)
    };

    private static TableLayoutPanel CrearLayoutGraficoTabla(Control grafico, DataGridView grid)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Theme.DeepCharcoal,
            Margin = Padding.Empty,
            Padding = new Padding(0, 4, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.Controls.Add(grafico, 0, 0);
        layout.Controls.Add(grid, 0, 1);
        return layout;
    }

    private static Panel CrearPanelGrafico(string titulo, CartesianChart chart, Panel? viewport = null)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Surface,
            Padding = new Padding(8),
            Margin = new Padding(0, 0, 0, 4)
        };
        var encabezado = new Label
        {
            Text = titulo,
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface,
            Font = Theme.BodyFont,
            TextAlign = ContentAlignment.MiddleLeft
        };
        if (viewport is null)
        {
            panel.Controls.Add(chart);
        }
        else
        {
            chart.Dock = DockStyle.None;
            chart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chart.Location = Point.Empty;
            viewport.Controls.Add(chart);
            panel.Controls.Add(viewport);
        }
        panel.Controls.Add(encabezado);
        return panel;
    }

    private void ActualizarGraficoVentas(IReadOnlyList<RecaudacionDiaria> datos)
    {
        chartVentas.LegendPosition = LegendPosition.Hidden;
        chartVentas.Series = datos.Count == 0
            ? Array.Empty<ISeries>()
            : new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Recaudación",
                    Values = datos.Select(dato => dato.Total).ToArray(),
                    Fill = new SolidColorPaint(ChartPalette.Recaudacion),
                    Stroke = null
                }
            };
        chartVentas.XAxes = new Axis[]
        {
            new()
            {
                Labels = datos.Select(dato => dato.Dia.ToString("dd/MM", CultureInfo.CurrentCulture)).ToArray(),
                LabelsPaint = new SolidColorPaint(ChartPalette.TextoSecundario),
                TicksPaint = new SolidColorPaint(ChartPalette.Linea),
                SeparatorsPaint = null
            }
        };
        chartVentas.YAxes = new Axis[]
        {
            new()
            {
                Name = "Importe",
                LabelsPaint = new SolidColorPaint(ChartPalette.TextoSecundario),
                NamePaint = new SolidColorPaint(ChartPalette.Texto),
                TicksPaint = new SolidColorPaint(ChartPalette.Linea),
                SeparatorsPaint = new SolidColorPaint(ChartPalette.Linea),
                Labeler = valor => valor.ToString("C0", CultureInfo.CurrentCulture)
            }
        };
    }

    private void ActualizarGraficoStock(IReadOnlyList<StockProductoComparacion> datos)
    {
        AjustarTamanoGraficoStock(datos.Count);
        chartStock.LegendPosition = LegendPosition.Hidden;
        chartStock.Series = datos.Count == 0
            ? Array.Empty<ISeries>()
            : new ISeries[]
            {
                CrearSerieStockPorEstado(datos)
            };
        chartStock.XAxes = new Axis[]
        {
            new()
            {
                Name = "Stock actual respecto al mínimo",
                MinLimit = 0,
                MaxLimit = 115,
                Labeler = valor => $"{valor:0}%",
                LabelsPaint = new SolidColorPaint(ChartPalette.TextoSecundario),
                NamePaint = new SolidColorPaint(ChartPalette.Texto),
                TicksPaint = new SolidColorPaint(ChartPalette.Linea),
                SeparatorsPaint = new SolidColorPaint(ChartPalette.Linea)
            }
        };
        chartStock.YAxes = new Axis[]
        {
            new()
            {
                Labels = datos.Select(dato => dato.Producto).ToArray(),
                IsInverted = true,
                LabelsPaint = new SolidColorPaint(ChartPalette.TextoSecundario),
                TicksPaint = new SolidColorPaint(ChartPalette.Linea),
                SeparatorsPaint = null
            }
        };
    }

    private void AjustarTamanoGraficoStock(int? cantidadProductos = null)
    {
        if (cantidadProductos.HasValue)
            cantidadProductosEnGraficoStock = cantidadProductos.Value;

        int altoNecesario = Theme.StockChartAxisHeight +
            cantidadProductosEnGraficoStock * Theme.StockChartRowHeight;
        chartStock.Width = Math.Max(0, chartStockViewport.ClientSize.Width);
        chartStock.Height = Math.Max(chartStockViewport.ClientSize.Height, altoNecesario);
    }

    private static RowSeries<double> CrearSerieStockPorEstado(IReadOnlyList<StockProductoComparacion> datos)
    {
        var serie = new RowSeries<double>
        {
            Name = "Stock actual (% del mínimo)",
            Values = datos.Select(dato => dato.PorcentajeDelMinimo).ToArray(),
            Fill = new SolidColorPaint(ChartPalette.StockActual),
            Stroke = null,
            DataLabelsPaint = new SolidColorPaint(ChartPalette.Texto),
            DataLabelsPosition = DataLabelsPosition.End,
            DataLabelsFormatter = punto => $"{punto.Coordinate.PrimaryValue:0}%"
        };

        serie.OnPointMeasured(punto =>
        {
            if (punto.Visual is not null && punto.Index >= 0 && punto.Index < datos.Count)
            {
                var color = datos[punto.Index].PorcentajeDelMinimo <= 45
                    ? ChartPalette.StockBajo
                    : ChartPalette.StockActual;
                punto.Visual.Fill = new SolidColorPaint(color);
            }
        });

        return serie;
    }

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
