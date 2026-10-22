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
        Width = 250,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly ComboBox cboProductos = new()
    {
        Width = 220,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly NumericUpDown numCantidad = new() { Width = 72, Minimum = 1, Maximum = 100000 };
    private readonly DataGridView gridLineas = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly Button btnAgregarLinea = new() { Text = "Agregar producto", AutoSize = true };
    private readonly Button btnQuitarLinea = new() { Text = "Quitar línea", AutoSize = true, Enabled = false };
    private readonly Label lblTotal = new()
    {
        AutoSize = true,
        Font = new Font(Theme.BodyFont, FontStyle.Bold),
        ForeColor = Theme.TextPrimary,
        Padding = new Padding(8, 8, 16, 0),
        Text = "Total: $0,00"
    };

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
    private Producto? ProductoSeleccionado => cboProductos.SelectedItem as Producto;
    private DetalleVenta? LineaSeleccionada => gridLineas.CurrentRow?.DataBoundItem as DetalleVenta;

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
        barra.Controls.Add(CrearEtiqueta("Venta pendiente"));
        cboVentasPendientes.SelectedIndexChanged += VentaPendienteSeleccionada;
        cboVentasPendientes.BackColor = Theme.Surface;
        cboVentasPendientes.ForeColor = Theme.TextPrimary;
        barra.Controls.Add(cboVentasPendientes);

        barra.Controls.Add(CrearEtiqueta("Producto"));
        cboProductos.DisplayMember = nameof(Producto.Nombre);
        cboProductos.SelectedIndexChanged += (_, _) => ActualizarEstadoBotones();
        cboProductos.BackColor = Theme.Surface;
        cboProductos.ForeColor = Theme.TextPrimary;
        barra.Controls.Add(cboProductos);

        barra.Controls.Add(CrearEtiqueta("Cantidad"));
        numCantidad.BackColor = Theme.Surface;
        numCantidad.ForeColor = Theme.TextPrimary;
        numCantidad.ValueChanged += (_, _) => ActualizarEstadoBotones();
        barra.Controls.Add(numCantidad);

        EstilizarBoton(btnAgregarLinea, primario: true);
        btnAgregarLinea.Enabled = false;
        btnAgregarLinea.Click += (_, _) => AgregarLinea();
        barra.Controls.Add(btnAgregarLinea);

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
        pie.Controls.Add(lblTotal);

        Theme.StyleGrid(gridLineas);
        gridLineas.SelectionChanged += (_, _) => ActualizarEstadoBotones();

        pestañaDetalle.Controls.Add(gridLineas);
        pestañaDetalle.Controls.Add(pie);
        pestañaDetalle.Controls.Add(barra);
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
            var productos = productoRepositorio.GetAll().Where(p => p.Activo).ToList();
            cboProductos.DataSource = null;
            cboProductos.DisplayMember = nameof(Producto.Nombre);
            cboProductos.DataSource = productos;
            ActualizarEstadoBotones();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar los productos. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

    private sealed record TipoVentaOpcion(TipoVenta Tipo, string Texto)
    {
        public override string ToString() => Texto;
    }

    private void VentaPendienteSeleccionada(object? sender, EventArgs e) => CargarLineasSeleccionadas();
}
