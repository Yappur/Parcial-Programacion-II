using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BasePage
{
    private readonly VentaRepositorio repositorio = new();
    private readonly ProductoRepositorio productoRepositorio = new();
    private readonly User? usuario;

    private readonly TabControl pestañas = new() { Dock = DockStyle.Fill };
    private readonly TabPage pestañaAbrir = new("Abrir venta");
    private readonly TabPage pestañaDetalle = new("Agregar o quitar productos");

    private readonly ComboBox cboTipoVenta = new()
    {
        Width = 150,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly DataGridView gridMesas = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = true
    };
    private readonly Button btnAbrirVenta = new() { Text = "Abrir venta", AutoSize = true };
    private readonly Label lblAyudaMesas = new()
    {
        AutoSize = true,
        ForeColor = Theme.TextSecondary,
        Padding = new Padding(0, 8, 0, 0)
    };

    private readonly ComboBox cboVentasPendientes = new()
    {
        Width = 210,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly TextBox txtBuscarProducto = new()
    {
        Width = 210,
        BorderStyle = BorderStyle.FixedSingle
    };
    private readonly NumericUpDown numCantidad = new() { Width = 72, Minimum = 1, Maximum = 100000 };
    private readonly DataGridView gridProductos = new() { Dock = DockStyle.Top, Height = 72, AutoGenerateColumns = true };
    private readonly Label lblSinResultados = new()
    {
        Dock = DockStyle.Top,
        Height = 26,
        ForeColor = Theme.TextSecondary,
        TextAlign = ContentAlignment.MiddleLeft,
        Visible = false
    };
    private readonly DataGridView gridLineas = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly Button btnAgregarLinea = new() { Text = "Agregar producto", AutoSize = true };
    private readonly Button btnQuitarLinea = new() { Text = "Quitar línea", AutoSize = true, Enabled = false };
    private readonly ComboBox cboMedioPago = new() { Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtNotas = new() { Width = 180, MaxLength = 500 };
    private readonly Button btnCerrarVenta = new() { Text = "Cerrar venta", AutoSize = true, Enabled = false };
    private readonly Button btnAnularVenta = new() { Text = "Anular venta", AutoSize = true, Enabled = false };
    private readonly Label lblTotal = new()
    {
        AutoSize = true,
        Font = new Font(Theme.BodyFont, FontStyle.Bold),
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(8, 8, 16, 0),
        Text = "Total: $0,00"
    };
    private List<Producto> productosActivos = [];

    public FrmVentas() : this(null) { }

    public FrmVentas(User? user)
    {
        InitializeComponent();
        usuario = user;
        AddPageTitle("Ventas", "Abrí ventas y administrá sus productos mientras estén pendientes");

        ConstruirPestanaApertura();
        ConstruirPestanaDetalle();
        pestañas.TabPages.Add(pestañaAbrir);
        pestañas.TabPages.Add(pestañaDetalle);
        PageContent.Controls.Add(pestañas);

        CargarMesas();
        CargarProductos();
        CargarVentasPendientes();
        ActualizarEstadoBotones();
    }

    private TipoVenta TipoSeleccionado => ((TipoVentaOpcion)cboTipoVenta.SelectedItem!).Tipo;
    private VentaPendiente? VentaSeleccionada => cboVentasPendientes.SelectedItem as VentaPendiente;
    private Producto? ProductoSeleccionado => gridProductos.SelectedRows.Count == 0
        ? null
        : gridProductos.SelectedRows[0].DataBoundItem as Producto;
    private DetalleVenta? LineaSeleccionada => gridLineas.CurrentRow?.DataBoundItem as DetalleVenta;
    private MedioPago? MedioPagoSeleccionado => (cboMedioPago.SelectedItem as MedioPagoOpcion)?.Medio;

    private void ConstruirPestanaApertura()
    {
        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(0, 8, 0, 12)
        };
        barra.Controls.Add(new Label
        {
            Text = "Tipo de venta",
            AutoSize = true,
            ForeColor = Theme.TextPrimary,
            Padding = new Padding(0, 8, 4, 0)
        });

        cboTipoVenta.Items.AddRange(new object[]
        {
            new TipoVentaOpcion(TipoVenta.Mesa, "Mesa"),
            new TipoVentaOpcion(TipoVenta.Mostrador, "Mostrador"),
            new TipoVentaOpcion(TipoVenta.Delivery, "Delivery")
        });
        cboTipoVenta.SelectedIndex = 0;
        cboTipoVenta.BackColor = Theme.Surface;
        cboTipoVenta.ForeColor = Theme.TextPrimary;
        cboTipoVenta.SelectedIndexChanged += (_, _) => ActualizarTipoVenta();
        barra.Controls.Add(cboTipoVenta);
        EstilizarBoton(btnAbrirVenta, primario: true);
        btnAbrirVenta.Click += (_, _) => AbrirVenta();
        barra.Controls.Add(btnAbrirVenta);
        barra.Controls.Add(lblAyudaMesas);

        Theme.StyleGrid(gridMesas);
        gridMesas.SelectionChanged += (_, _) => ActualizarEstadoBotones();
        gridMesas.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || gridMesas.Columns[e.ColumnIndex].DataPropertyName != nameof(MesaVenta.Estado))
                return;
            if (e.Value is string estado)
                e.CellStyle.ForeColor = estado == "Ocupada" ? Theme.TextSecondary : Theme.TextPrimary;
        };

        pestañaAbrir.Controls.Add(gridMesas);
        pestañaAbrir.Controls.Add(barra);
    }

    private void ConstruirPestanaDetalle()
    {
        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(0, 8, 0, 8)
        };
        barra.Controls.Add(CrearEtiqueta("Venta"));
        cboVentasPendientes.SelectedIndexChanged += VentaPendienteSeleccionada;
        cboVentasPendientes.BackColor = Theme.Surface;
        cboVentasPendientes.ForeColor = Theme.TextPrimary;
        barra.Controls.Add(cboVentasPendientes);

        txtBuscarProducto.PlaceholderText = "Buscar por código o nombre";
        txtBuscarProducto.AccessibleName = "Buscar productos por código o nombre";
        txtBuscarProducto.BackColor = Theme.Surface;
        txtBuscarProducto.ForeColor = Theme.TextPrimary;
        txtBuscarProducto.TextChanged += (_, _) => FiltrarProductos();
        barra.Controls.Add(txtBuscarProducto);

        barra.Controls.Add(CrearEtiqueta("Cantidad"));
        numCantidad.BackColor = Theme.Surface;
        numCantidad.ForeColor = Theme.TextPrimary;
        numCantidad.ValueChanged += (_, _) => ActualizarEstadoBotones();
        barra.Controls.Add(numCantidad);

        EstilizarBoton(btnAgregarLinea, primario: true);
        btnAgregarLinea.Enabled = false;
        btnAgregarLinea.Click += (_, _) => AgregarLinea();
        barra.Controls.Add(btnAgregarLinea);

        Theme.StyleGrid(gridProductos);
        gridProductos.SelectionChanged += (_, _) => ActualizarEstadoBotones();

        var panelBusqueda = new Panel { Dock = DockStyle.Top, Height = 188, BackColor = Theme.DeepCharcoal };
        panelBusqueda.Controls.Add(lblSinResultados);
        panelBusqueda.Controls.Add(gridProductos);
        panelBusqueda.Controls.Add(barra);

        var pie = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            WrapContents = true,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(0, 8, 0, 0)
        };
        EstilizarBoton(btnQuitarLinea, primario: false);
        btnQuitarLinea.Enabled = false;
        btnQuitarLinea.Click += (_, _) => QuitarLinea();
        pie.Controls.Add(btnQuitarLinea);
        pie.Controls.Add(CrearEtiqueta("Medio de pago"));
        cboMedioPago.Items.AddRange(new object[]
        {
            new MedioPagoOpcion(MedioPago.Efectivo, "Efectivo"),
            new MedioPagoOpcion(MedioPago.Transferencia, "Transferencia")
        });
        cboMedioPago.SelectedIndexChanged += (_, _) => ActualizarEstadoBotones();
        cboMedioPago.BackColor = Theme.Surface;
        cboMedioPago.ForeColor = Theme.TextPrimary;
        pie.Controls.Add(cboMedioPago);
        pie.Controls.Add(CrearEtiqueta("Notas"));
        txtNotas.PlaceholderText = "Propina, descuento, pago dividido...";
        txtNotas.BackColor = Theme.Surface;
        txtNotas.ForeColor = Theme.TextPrimary;
        pie.Controls.Add(txtNotas);
        EstilizarBoton(btnCerrarVenta, primario: true);
        btnCerrarVenta.Enabled = false;
        btnCerrarVenta.Click += (_, _) => CerrarVenta();
        pie.Controls.Add(btnCerrarVenta);
        EstilizarBoton(btnAnularVenta, primario: false);
        btnAnularVenta.Enabled = false;
        btnAnularVenta.Click += (_, _) => AnularVenta();
        pie.Controls.Add(btnAnularVenta);
        pie.Controls.Add(lblTotal);

        Theme.StyleGrid(gridLineas);
        gridLineas.SelectionChanged += (_, _) => ActualizarEstadoBotones();

        pestañaDetalle.Controls.Add(gridLineas);
        pestañaDetalle.Controls.Add(pie);
        pestañaDetalle.Controls.Add(panelBusqueda);
    }

    private static Label CrearEtiqueta(string texto) => new()
    {
        Text = texto,
        AutoSize = true,
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(0, 8, 4, 0)
    };

    private static void EstilizarBoton(Button boton, bool primario)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = primario ? 0 : 1;
        boton.BackColor = primario ? Theme.BlazeOrange : Theme.Surface;
        boton.ForeColor = boton.Enabled ? Theme.TextPrimary : SystemColors.GrayText;
        boton.Font = Theme.BodyFont;
        boton.Cursor = Cursors.Hand;
        boton.EnabledChanged += (_, _) =>
            boton.ForeColor = boton.Enabled ? Theme.TextPrimary : SystemColors.GrayText;
    }

    private void ActualizarTipoVenta()
    {
        bool esMesa = TipoSeleccionado == TipoVenta.Mesa;
        gridMesas.Visible = esMesa;
        lblAyudaMesas.Text = esMesa
            ? "Elegí una mesa libre. Las ocupadas no se pueden volver a abrir."
            : "Mostrador y delivery no necesitan seleccionar mesa.";
        ActualizarEstadoBotones();
    }

    private MesaVenta? MesaSeleccionada() => gridMesas.SelectedRows.Count == 0
        ? null
        : gridMesas.SelectedRows[0].DataBoundItem as MesaVenta;

    private void ActualizarEstadoBotones()
    {
        bool esMesa = cboTipoVenta.SelectedItem is TipoVentaOpcion opcion && opcion.Tipo == TipoVenta.Mesa;
        MesaVenta? mesa = MesaSeleccionada();
        btnAbrirVenta.Enabled = usuario is not null && (!esMesa || mesa is not null && !mesa.Ocupada);
        btnAgregarLinea.Enabled = VentaSeleccionada is not null && ProductoSeleccionado is not null && numCantidad.Value > 0;
        btnQuitarLinea.Enabled = VentaSeleccionada is not null && LineaSeleccionada is not null;
        btnCerrarVenta.Enabled = VentaSeleccionada is not null && MedioPagoSeleccionado is not null;
        btnAnularVenta.Enabled = VentaSeleccionada is not null;
    }

    private void CargarMesas()
    {
        try
        {
            gridMesas.DataSource = repositorio.ListarMesasConEstado();
            if (gridMesas.Columns[nameof(MesaVenta.Id)] is { } id) id.Visible = false;
            if (gridMesas.Columns[nameof(MesaVenta.Numero)] is { } numero) numero.HeaderText = "Mesa";
            if (gridMesas.Columns[nameof(MesaVenta.Ocupada)] is { } ocupada) ocupada.Visible = false;
            if (gridMesas.Columns[nameof(MesaVenta.Estado)] is { } estado) estado.HeaderText = "Estado";
            gridMesas.ClearSelection();
            ActualizarEstadoBotones();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las mesas. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarProductos()
    {
        try
        {
            productosActivos = productoRepositorio.GetAll().Where(p => p.Activo).ToList();
            txtBuscarProducto.Clear();
            FiltrarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar los productos. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FiltrarProductos()
    {
        string busqueda = txtBuscarProducto.Text.Trim();
        List<Producto> resultados = productosActivos
            .Where(p => string.IsNullOrEmpty(busqueda)
                || p.Nombre.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                || p.Codigo.ToString().Contains(busqueda, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Nombre)
            .ToList();

        gridProductos.DataSource = resultados;
        if (gridProductos.Columns[nameof(Producto.Codigo)] is { } codigo) codigo.HeaderText = "Código";
        if (gridProductos.Columns[nameof(Producto.Nombre)] is { } nombre) nombre.HeaderText = "Producto";
        if (gridProductos.Columns[nameof(Producto.Precio)] is { } precio)
        {
            precio.HeaderText = "Precio";
            precio.DefaultCellStyle.Format = "C2";
        }
        foreach (string columna in new[]
        {
            nameof(Producto.TipoStock), nameof(Producto.Stock), nameof(Producto.StockMinimo),
            nameof(Producto.StockDisponible), nameof(Producto.Activo), nameof(Producto.CategoriaId),
            nameof(Producto.CategoriaNombre)
        })
            if (gridProductos.Columns[columna] is { } oculta) oculta.Visible = false;

        gridProductos.ClearSelection();
        lblSinResultados.Visible = resultados.Count == 0;
        lblSinResultados.Text = "No se encontraron productos.";
        ActualizarEstadoBotones();
    }

    private void CargarVentasPendientes(int? ventaIdSeleccionada = null)
    {
        try
        {
            var ventas = repositorio.ListarPendientes();
            cboVentasPendientes.DataSource = null;
            cboVentasPendientes.DisplayMember = nameof(VentaPendiente.Descripcion);
            cboVentasPendientes.DataSource = ventas;
            int indice = ventaIdSeleccionada.HasValue
                ? ventas.FindIndex(v => v.Id == ventaIdSeleccionada.Value)
                : -1;
            cboVentasPendientes.SelectedIndex = indice;
            if (indice < 0 && ventas.Count == 1)
                cboVentasPendientes.SelectedIndex = 0;
            CargarLineasSeleccionadas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las ventas pendientes. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarLineasSeleccionadas()
    {
        VentaPendiente? venta = VentaSeleccionada;
        if (venta is null)
        {
            gridLineas.DataSource = null;
            lblTotal.Text = "Total: $0,00";
            ActualizarEstadoBotones();
            return;
        }

        try
        {
            gridLineas.DataSource = repositorio.ListarLineas(venta.Id);
            if (gridLineas.Columns[nameof(DetalleVenta.Id)] is { } id) id.Visible = false;
            if (gridLineas.Columns[nameof(DetalleVenta.VentaId)] is { } ventaId) ventaId.Visible = false;
            if (gridLineas.Columns[nameof(DetalleVenta.ProductoCodigo)] is { } codigo) codigo.Visible = false;
            if (gridLineas.Columns[nameof(DetalleVenta.ProductoNombre)] is { } nombre) nombre.HeaderText = "Producto";
            if (gridLineas.Columns[nameof(DetalleVenta.Cantidad)] is { } cantidad) cantidad.HeaderText = "Cantidad";
            if (gridLineas.Columns[nameof(DetalleVenta.PrecioUnitario)] is { } precio)
            {
                precio.HeaderText = "Precio unitario";
                precio.DefaultCellStyle.Format = "C2";
            }
            if (gridLineas.Columns[nameof(DetalleVenta.Subtotal)] is { } subtotal)
            {
                subtotal.HeaderText = "Subtotal";
                subtotal.DefaultCellStyle.Format = "C2";
            }
            lblTotal.Text = $"Total: {venta.Total:C2}";
            ActualizarEstadoBotones();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las líneas de la venta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AbrirVenta()
    {
        int? mesaId = TipoSeleccionado == TipoVenta.Mesa ? MesaSeleccionada()?.Id : null;
        if (usuario is null)
        {
            MessageBox.Show("No se encontró el usuario que inició sesión.", "No se puede abrir la venta",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            int ventaId = repositorio.Abrir(TipoSeleccionado, mesaId, usuario.Id);
            MessageBox.Show($"Venta #{ventaId} abierta en estado PENDIENTE.", "Venta abierta",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarMesas();
            CargarVentasPendientes(ventaId);
            pestañas.SelectedTab = pestañaDetalle;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se puede abrir la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarMesas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo abrir la venta. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgregarLinea()
    {
        VentaPendiente? venta = VentaSeleccionada;
        Producto? producto = ProductoSeleccionado;
        if (venta is null || producto is null) return;

        try
        {
            repositorio.AgregarLinea(venta.Id, producto.Codigo, Decimal.ToInt32(numCantidad.Value));
            CargarProductos();
            CargarVentasPendientes(venta.Id);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo agregar el producto", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo agregar el producto.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void QuitarLinea()
    {
        VentaPendiente? venta = VentaSeleccionada;
        DetalleVenta? linea = LineaSeleccionada;
        if (venta is null || linea is null) return;

        try
        {
            repositorio.QuitarLinea(venta.Id, linea.Id);
            CargarProductos();
            CargarVentasPendientes(venta.Id);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo quitar la línea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo quitar la línea.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CerrarVenta()
    {
        VentaPendiente? venta = VentaSeleccionada;
        MedioPago? medioPago = MedioPagoSeleccionado;
        if (venta is null) return;
        if (medioPago is null)
        {
            MessageBox.Show("Elegí un medio de pago para cerrar la venta.", "Falta el medio de pago",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            repositorio.Cerrar(venta.Id, medioPago.Value, txtNotas.Text);
            MessageBox.Show($"La venta #{venta.Id} quedó cerrada y ya no se puede editar.", "Venta cerrada",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtNotas.Clear();
            cboMedioPago.SelectedIndex = -1;
            CargarVentasPendientes();
            CargarMesas();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo cerrar la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarVentasPendientes(venta.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo cerrar la venta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AnularVenta()
    {
        VentaPendiente? venta = VentaSeleccionada;
        if (venta is null) return;

        DialogResult confirmacion = MessageBox.Show(
            "¿Anular la venta? Se devolverá el stock",
            "Confirmar anulación",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirmacion != DialogResult.Yes) return;

        try
        {
            repositorio.Anular(venta.Id);
            MessageBox.Show($"La venta #{venta.Id} quedó anulada.", "Venta anulada",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarProductos();
            CargarVentasPendientes();
            CargarMesas();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo anular la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarVentasPendientes(venta.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo anular la venta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed record TipoVentaOpcion(TipoVenta Tipo, string Texto)
    {
        public override string ToString() => Texto;
    }

    private sealed record MedioPagoOpcion(MedioPago Medio, string Texto)
    {
        public override string ToString() => Texto;
    }

    private void VentaPendienteSeleccionada(object? sender, EventArgs e)
    {
        cboMedioPago.SelectedIndex = -1;
        CargarLineasSeleccionadas();
    }
}
