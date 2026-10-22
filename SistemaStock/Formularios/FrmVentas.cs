using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BasePage
{
    private readonly VentaRepositorio repositorio = new();
    private readonly User? usuario;
    private readonly ComboBox cboTipoVenta = new()
    {
        Width = 180,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly DataGridView gridMesas = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = true
    };
    private readonly Button btnAbrirVenta = new()
    {
        Text = "Abrir venta",
        AutoSize = true,
        Enabled = false
    };
    private readonly Label lblAyuda = new()
    {
        AutoSize = true,
        ForeColor = Theme.TextSecondary,
        Padding = new Padding(0, 8, 0, 0)
    };

    public FrmVentas() : this(null) { }

    public FrmVentas(User? user)
    {
        InitializeComponent();
        usuario = user;
        AddPageTitle("Ventas", "Abrí una venta de mesa, mostrador o delivery");

        Theme.StyleGrid(gridMesas);
        gridMesas.SelectionChanged += (_, _) => ActualizarSeleccion();
        gridMesas.CellFormatting += (_, e) =>
        {
            if (gridMesas.Columns[e.ColumnIndex].DataPropertyName == nameof(MesaVenta.Estado) && e.Value is string estado)
                e.CellStyle.ForeColor = estado == "Ocupada" ? Color.FromArgb(255, 145, 130) : Color.FromArgb(140, 220, 160);
        };

        cboTipoVenta.Items.AddRange(new object[]
        {
            new TipoVentaOpcion(TipoVenta.Mesa, "Mesa"),
            new TipoVentaOpcion(TipoVenta.Mostrador, "Mostrador"),
            new TipoVentaOpcion(TipoVenta.Delivery, "Delivery")
        });
        cboTipoVenta.SelectedIndex = 0;
        cboTipoVenta.SelectedIndexChanged += (_, _) => ActualizarTipoVenta();

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
        cboTipoVenta.BackColor = Theme.Surface;
        cboTipoVenta.ForeColor = Theme.TextPrimary;
        barra.Controls.Add(cboTipoVenta);
        barra.Controls.Add(btnAbrirVenta);
        barra.Controls.Add(lblAyuda);

        btnAbrirVenta.FlatStyle = FlatStyle.Flat;
        btnAbrirVenta.FlatAppearance.BorderSize = 0;
        btnAbrirVenta.BackColor = Theme.BlazeOrange;
        btnAbrirVenta.ForeColor = Theme.TextPrimary;
        btnAbrirVenta.Font = Theme.BodyFont;
        btnAbrirVenta.Cursor = Cursors.Hand;
        btnAbrirVenta.EnabledChanged += (_, _) =>
            btnAbrirVenta.ForeColor = btnAbrirVenta.Enabled ? Theme.TextPrimary : SystemColors.GrayText;
        btnAbrirVenta.Click += (_, _) => AbrirVenta();

        PageContent.Controls.Add(gridMesas);
        PageContent.Controls.Add(barra);
        CargarMesas();
        ActualizarTipoVenta();
    }

    private TipoVenta TipoSeleccionado => ((TipoVentaOpcion)cboTipoVenta.SelectedItem!).Tipo;

    private MesaVenta? MesaSeleccionada() => gridMesas.SelectedRows.Count == 0
        ? null
        : gridMesas.SelectedRows[0].DataBoundItem as MesaVenta;

    private void ActualizarTipoVenta()
    {
        bool esMesa = TipoSeleccionado == TipoVenta.Mesa;
        gridMesas.Visible = esMesa;
        lblAyuda.Text = esMesa
            ? "Seleccioná una mesa libre. Las ocupadas no se pueden volver a abrir."
            : "Este tipo de venta no necesita seleccionar una mesa.";
        ActualizarSeleccion();
    }

    private void ActualizarSeleccion()
    {
        bool esMesa = cboTipoVenta.SelectedItem is TipoVentaOpcion opcion && opcion.Tipo == TipoVenta.Mesa;
        MesaVenta? mesa = MesaSeleccionada();
        btnAbrirVenta.Enabled = esMesa
            ? mesa is not null && !mesa.Ocupada
            : cboTipoVenta.SelectedItem is not null;
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
            ActualizarSeleccion();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las mesas. Revisá la conexión a la base de datos.\n" + ex.Message,
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

    private sealed record TipoVentaOpcion(TipoVenta Tipo, string Texto)
    {
        public override string ToString() => Texto;
    }
}
