using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public sealed class FrmHistorialVentas : BasePage
{
    private readonly VentaRepositorio repositorio = new();
    private readonly DataGridView gridVentas = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly DataGridView gridDetalle = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly DateTimePicker fechaDesde = new()
    {
        Format = DateTimePickerFormat.Short,
        MinDate = new DateTime(2000, 1, 1),
        Value = new DateTime(2000, 1, 1),
        Width = 125
    };
    private readonly DateTimePicker fechaHasta = new()
    {
        Format = DateTimePickerFormat.Short,
        Width = 125
    };
    private readonly ComboBox cboEstado = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 130
    };
    private readonly Label lblDetalle = new()
    {
        Dock = DockStyle.Top,
        Height = 34,
        ForeColor = Theme.TextSecondary,
        TextAlign = ContentAlignment.MiddleLeft
    };

    public FrmHistorialVentas(User usuario)
    {
        if (usuario is null || !UserPermissions.CanManageUsers(usuario))
            throw new UnauthorizedAccessException("El historial de ventas es exclusivo para Administrador.");

        AddPageTitle("Historial de ventas", "Consulta de solo lectura. Los importes corresponden al momento de la venta.");
        fechaHasta.Value = DateTime.Today;
        ConstruirInterfaz();
        CargarVentas();
    }

    private void ConstruirInterfaz()
    {
        var filtros = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(0, 4, 0, 10)
        };
        filtros.Controls.Add(CrearEtiqueta("Desde"));
        filtros.Controls.Add(fechaDesde);
        filtros.Controls.Add(CrearEtiqueta("Hasta"));
        filtros.Controls.Add(fechaHasta);
        filtros.Controls.Add(CrearEtiqueta("Estado"));
        cboEstado.Items.AddRange(new object[]
        {
            new EstadoFiltro(null, "Todas"),
            new EstadoFiltro(EstadoVenta.Pendiente, "Pendiente"),
            new EstadoFiltro(EstadoVenta.Cerrada, "Cerrada"),
            new EstadoFiltro(EstadoVenta.Anulada, "Anulada")
        });
        cboEstado.SelectedIndex = 0;
        cboEstado.BackColor = Theme.Surface;
        cboEstado.ForeColor = Theme.TextPrimary;
        filtros.Controls.Add(cboEstado);

        var btnFiltrar = new Button { Text = "Filtrar", AutoSize = true };
        EstilizarBoton(btnFiltrar);
        btnFiltrar.Click += (_, _) => CargarVentas();
        filtros.Controls.Add(btnFiltrar);

        Theme.StyleGrid(gridVentas);
        Theme.StyleGrid(gridDetalle);
        gridVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridVentas.MultiSelect = false;
        gridVentas.SelectionChanged += (_, _) => CargarDetalleSeleccionado();
        gridDetalle.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridDetalle.MultiSelect = false;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            Size = new Size(900, 500),
            SplitterDistance = 205,
            Panel1MinSize = 90,
            Panel2MinSize = 90,
            BackColor = Theme.DeepCharcoal
        };
        split.Panel1.Controls.Add(gridVentas);
        split.Panel2.Controls.Add(gridDetalle);
        split.Panel2.Controls.Add(lblDetalle);

        PageContent.Controls.Add(split);
        PageContent.Controls.Add(filtros);
    }

    private void CargarVentas()
    {
        if (fechaDesde.Value.Date > fechaHasta.Value.Date)
        {
            MessageBox.Show("La fecha Desde no puede ser posterior a la fecha Hasta.",
                "Rango de fechas inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            EstadoVenta? estado = (cboEstado.SelectedItem as EstadoFiltro)?.Estado;
            gridVentas.DataSource = repositorio.ListarHistorial(fechaDesde.Value, fechaHasta.Value, estado);
            ConfigurarColumnasVentas();
            gridVentas.ClearSelection();
            gridDetalle.DataSource = null;
            lblDetalle.Text = "Seleccioná una venta para ver sus productos.";
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo cargar el historial de ventas. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarColumnasVentas()
    {
        ConfigurarColumna(nameof(VentaHistorial.Id), "N.º venta");
        ConfigurarColumna(nameof(VentaHistorial.FechaHora), "Fecha y hora", "dd/MM/yyyy HH:mm");
        ConfigurarColumna(nameof(VentaHistorial.FechaCierre), "Fecha de cierre", "dd/MM/yyyy HH:mm");
        ConfigurarColumna(nameof(VentaHistorial.TipoVenta), "Tipo");
        ConfigurarColumna(nameof(VentaHistorial.Mesa), "Mesa");
        ConfigurarColumna(nameof(VentaHistorial.Atendio), "Atendió");
        ConfigurarColumna(nameof(VentaHistorial.Total), "Total de venta", "C2");
        ConfigurarColumna(nameof(VentaHistorial.MedioPago), "Medio de pago");
        ConfigurarColumna(nameof(VentaHistorial.Estado), "Estado");
        ConfigurarColumna(nameof(VentaHistorial.Notas), "Notas");
    }

    private void CargarDetalleSeleccionado()
    {
        if (gridVentas.SelectedRows.Count == 0 ||
            gridVentas.SelectedRows[0].DataBoundItem is not VentaHistorial venta)
        {
            gridDetalle.DataSource = null;
            return;
        }

        try
        {
            gridDetalle.DataSource = repositorio.ListarLineas(venta.Id);
            if (gridDetalle.Columns[nameof(DetalleVenta.Id)] is { } id) id.Visible = false;
            if (gridDetalle.Columns[nameof(DetalleVenta.VentaId)] is { } ventaId) ventaId.Visible = false;
            if (gridDetalle.Columns[nameof(DetalleVenta.ProductoCodigo)] is { } codigo) codigo.Visible = false;
            ConfigurarColumnaDetalle(nameof(DetalleVenta.ProductoNombre), "Producto");
            ConfigurarColumnaDetalle(nameof(DetalleVenta.Cantidad), "Cantidad");
            ConfigurarColumnaDetalle(nameof(DetalleVenta.PrecioUnitario), "Precio unitario", "C2");
            ConfigurarColumnaDetalle(nameof(DetalleVenta.Subtotal), "Subtotal", "C2");
            lblDetalle.Text = $"Venta #{venta.Id} · {venta.Estado} · Total de venta: {venta.Total:C2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar los productos de la venta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigurarColumna(string nombre, string encabezado, string? formato = null)
    {
        if (gridVentas.Columns[nombre] is not { } columna) return;
        columna.HeaderText = encabezado;
        if (formato is not null) columna.DefaultCellStyle.Format = formato;
    }

    private void ConfigurarColumnaDetalle(string nombre, string encabezado, string? formato = null)
    {
        if (gridDetalle.Columns[nombre] is not { } columna) return;
        columna.HeaderText = encabezado;
        if (formato is not null) columna.DefaultCellStyle.Format = formato;
    }

    private static Label CrearEtiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(0, 8, 4, 0)
    };

    private static void EstilizarBoton(Button boton)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = 0;
        boton.BackColor = Theme.BlazeOrange;
        boton.ForeColor = Theme.TextPrimary;
        boton.Font = Theme.BodyFont;
        boton.Cursor = Cursors.Hand;
    }

    private sealed record EstadoFiltro(EstadoVenta? Estado, string Texto)
    {
        public override string ToString() => Texto;
    }
}
