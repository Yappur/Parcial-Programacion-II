using System.Globalization;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;
using FontAwesome.Sharp;

namespace SistemaStock.Formularios;

public sealed class FrmMesas : BasePage
{
    private const int AnchoMinimoTarjeta = 220;
    private const int AltoTarjeta = 176;
    private const int EspacioTarjetas = 16;
    private const int MaximoTarjetasPorFila = 3;

    private readonly MesaRepositorio repositorio = new();
    private readonly VentaRepositorio ventasRepositorio = new();
    private readonly FlowLayoutPanel tarjetasMesas = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        WrapContents = true,
        FlowDirection = FlowDirection.LeftToRight,
        BackColor = Theme.DeepCharcoal,
        Padding = new Padding(EspacioTarjetas / 2),
        Margin = Padding.Empty
    };
    private readonly ComboBox filtroEstado = new()
    {
        Width = 150,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    public FrmMesas() : this(null) { }

    public FrmMesas(User? user)
    {
        Text = "Chachín | Mesas";
        AddPageTitle("Mesas", "Administración de mesas del local");
        tarjetasMesas.SizeChanged += (_, _) => AjustarAnchoTarjetas();

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 92,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal,
            WrapContents = true
        };

        filtroEstado.Items.AddRange(new object[] { "Activas", "Inactivas", "Todas" });
        filtroEstado.SelectedIndex = 0;
        filtroEstado.SelectedIndexChanged += (_, _) => CargarMesas();
        top.Controls.Add(filtroEstado);
        Theme.StyleComboBox(filtroEstado);

        if (user is not null && UserPermissions.CanManageUsers(user))
        {
            var add = new IconButton { Text = "Crear mesa", AutoSize = true };
            Theme.SetButtonIcon(add, IconChar.Plus);
            Theme.EstilizarBotonPrimario(add);
            add.Click += (_, _) => CrearMesa();
            top.Controls.Add(add);

            var edit = new IconButton { Text = "Modificar", AutoSize = true };
            Theme.SetButtonIcon(edit, IconChar.PenToSquare);
            Theme.EstilizarBotonSecundario(edit);
            edit.Click += (_, _) => ModificarMesa();
            top.Controls.Add(edit);

            var deactivate = new IconButton { Text = "Desactivar", AutoSize = true };
            Theme.SetButtonIcon(deactivate, IconChar.TrashCan);
            Theme.EstilizarBotonPeligro(deactivate);
            deactivate.Click += (_, _) => DesactivarMesa();
            top.Controls.Add(deactivate);

            var activate = new IconButton { Text = "Activar", AutoSize = true };
            Theme.SetButtonIcon(activate, IconChar.ArrowRotateLeft);
            Theme.EstilizarBotonAdvertencia(activate);
            activate.Click += (_, _) => ActivarMesa();
            top.Controls.Add(activate);
        }
        else
        {
            top.Controls.Add(new Label
            {
                Text = "Tu rol no permite administrar mesas.",
                AutoSize = true,
                Padding = new Padding(8),
                ForeColor = Theme.TextSecondary
            });
        }

        PageContent.Controls.Add(tarjetasMesas);
        PageContent.Controls.Add(top);
        CargarMesas();
    }

    private Mesa? mesaSeleccionada;

    private Mesa? MesaSeleccionada() => mesaSeleccionada;

    private void CargarMesas()
    {
        try
        {
            bool? filtroActivo = filtroEstado.SelectedIndex switch
            {
                0 => true,
                1 => false,
                _ => null
            };
            List<Mesa> mesas = repositorio.GetAll(filtroActivo)
                .OrderBy(mesa => mesa.Numero)
                .ToList();
            Dictionary<int, bool> ocupacion = ventasRepositorio.ListarMesasConEstado()
                .ToDictionary(mesa => mesa.Id, mesa => mesa.Ocupada);

            int? mesaIdSeleccionada = mesaSeleccionada?.Id;
            tarjetasMesas.SuspendLayout();
            foreach (Control tarjetaAnterior in tarjetasMesas.Controls.Cast<Control>().ToArray())
                tarjetaAnterior.Dispose();
            tarjetasMesas.Controls.Clear();
            foreach (Mesa mesa in mesas)
                tarjetasMesas.Controls.Add(CrearTarjetaMesa(
                    mesa,
                    mesa.Activo && ocupacion.TryGetValue(mesa.Id, out bool ocupada) && ocupada));
            tarjetasMesas.ResumeLayout();

            mesaSeleccionada = mesaIdSeleccionada.HasValue
                ? mesas.FirstOrDefault(mesa => mesa.Id == mesaIdSeleccionada.Value)
                : null;
            ActualizarSeleccionTarjeta();
            if (mesas.Count == 0)
            {
                tarjetasMesas.Controls.Add(new Label
                {
                    Text = "No hay mesas para el filtro seleccionado.",
                    AutoSize = true,
                    ForeColor = Theme.TextSecondary,
                    Padding = new Padding(EspacioTarjetas)
                });
            }
            AjustarAnchoTarjetas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las mesas. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Panel CrearTarjetaMesa(Mesa mesa, bool ocupada)
    {
        var tarjeta = new Panel
        {
            Tag = mesa,
            Height = AltoTarjeta,
            Width = AnchoMinimoTarjeta,
            Margin = new Padding(EspacioTarjetas / 2),
            Padding = new Padding(1),
            BackColor = mesaSeleccionada?.Id == mesa.Id ? Theme.BlazeOrange : Theme.GridLine
        };
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.Surface,
            Padding = new Padding(EspacioTarjetas / 2)
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, EspacioTarjetas * 2));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, EspacioTarjetas * 3));

        var etiquetaEstado = new Label
        {
            Text = ocupada ? "OCUPADA" : mesa.Activo ? "LIBRE" : "INACTIVA",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ocupada ? Theme.Danger : mesa.Activo ? Theme.StatusSuccess : Theme.TextSecondary,
            Font = Theme.BodyFont,
            BackColor = Theme.Surface
        };
        var etiquetaMesa = new Label
        {
            Text = $"MESA {mesa.Numero}",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.TextPrimary,
            Font = Theme.HeadingFont,
            BackColor = Theme.Surface
        };
        var verPedidos = new IconButton { Text = "Ver pedidos", Dock = DockStyle.Fill, Height = 36 };
        Theme.SetButtonIcon(verPedidos, IconChar.Eye);
        Theme.EstilizarBotonSecundario(verPedidos);
        verPedidos.Click += (_, _) => VerPedidos(mesa);
        etiquetaEstado.Click += (_, _) => SeleccionarMesa(mesa);
        etiquetaMesa.Click += (_, _) => SeleccionarMesa(mesa);
        contenido.Click += (_, _) => SeleccionarMesa(mesa);
        contenido.Controls.Add(etiquetaEstado, 0, 0);
        contenido.Controls.Add(etiquetaMesa, 0, 1);
        contenido.Controls.Add(verPedidos, 0, 2);
        tarjeta.Controls.Add(contenido);
        tarjeta.Click += (_, _) => SeleccionarMesa(mesa);
        return tarjeta;
    }

    private void VerPedidos(Mesa mesa)
    {
        SeleccionarMesa(mesa);

        try
        {
            string referenciaMesa = $" - Mesa {mesa.Id} - ";
            VentaPendiente? venta = ventasRepositorio.ListarPendientes()
                .FirstOrDefault(pendiente => pendiente.Descripcion.Contains(
                    referenciaMesa, StringComparison.Ordinal));

            if (venta is null)
            {
                MessageBox.Show($"La mesa {mesa.Numero} no tiene pedidos pendientes.",
                    "Sin pedidos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<DetalleVenta> lineas = ventasRepositorio.ListarLineas(venta.Id);
            using var dialogo = new MesaPedidosForm(mesa.Numero, venta, lineas, ventasRepositorio);
            if (dialogo.ShowDialog(this) == DialogResult.OK)
            {
                mesaSeleccionada = null;
                CargarMesas();
                MessageBox.Show($"Pago registrado. La mesa {mesa.Numero} quedó libre.",
                    "Mesa liberada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar los pedidos de la mesa. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SeleccionarMesa(Mesa mesa)
    {
        mesaSeleccionada = mesa;
        ActualizarSeleccionTarjeta();
    }

    private void ActualizarSeleccionTarjeta()
    {
        foreach (Control control in tarjetasMesas.Controls)
        {
            if (control is Panel tarjeta && tarjeta.Tag is Mesa mesa)
                tarjeta.BackColor = mesaSeleccionada?.Id == mesa.Id ? Theme.BlazeOrange : Theme.GridLine;
        }
    }

    private void AjustarAnchoTarjetas()
    {
        if (tarjetasMesas.ClientSize.Width <= 0)
            return;

        int anchoDisponible = tarjetasMesas.ClientSize.Width - tarjetasMesas.Padding.Horizontal;
        int espacio = EspacioTarjetas;
        int columnas = Math.Clamp(
            anchoDisponible / (AnchoMinimoTarjeta + espacio),
            1,
            MaximoTarjetasPorFila);
        int anchoTarjeta = (anchoDisponible - columnas * espacio) / columnas;

        foreach (Control control in tarjetasMesas.Controls)
            if (control is Panel tarjeta && tarjeta.Tag is Mesa)
                tarjeta.Width = Math.Max(AnchoMinimoTarjeta, anchoTarjeta);
    }

    private void CrearMesa()
    {
        using var form = new MesaEditForm();
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            repositorio.Create(form.Mesa);
            CargarMesas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo crear la mesa. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ModificarMesa()
    {
        var mesa = MesaSeleccionada();
        if (mesa is null) { MessageBox.Show("Seleccioná una mesa."); return; }
        using var form = new MesaEditForm(mesa);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            mesa.Numero = form.Mesa.Numero;
            repositorio.Update(mesa);
            CargarMesas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo modificar la mesa. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DesactivarMesa()
    {
        var mesa = MesaSeleccionada();
        if (mesa is null) { MessageBox.Show("Seleccioná una mesa."); return; }
        if (!mesa.Activo) { MessageBox.Show("La mesa ya está desactivada."); return; }
        if (MessageBox.Show($"¿Desactivar la mesa {mesa.Numero}?", "Confirmar", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            repositorio.Deactivate(mesa.Id);
            CargarMesas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo desactivar la mesa. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActivarMesa()
    {
        var mesa = MesaSeleccionada();
        if (mesa is null) { MessageBox.Show("Seleccioná una mesa."); return; }
        if (mesa.Activo) { MessageBox.Show("La mesa ya está activa."); return; }
        try
        {
            repositorio.Activate(mesa.Id);
            CargarMesas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo activar la mesa. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class MesaPedidosForm : BaseForm
{
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly VentaRepositorio ventasRepositorio;
    private readonly VentaPendiente venta;
    private readonly ComboBox medioPago = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };

    public MesaPedidosForm(
        int numeroMesa,
        VentaPendiente venta,
        List<DetalleVenta> lineas,
        VentaRepositorio ventasRepositorio)
    {
        this.venta = venta;
        this.ventasRepositorio = ventasRepositorio;
        Text = $"Chachín | Pedidos de mesa {numeroMesa}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = Size.Empty;
        MaximumSize = Size.Empty;
        ClientSize = new Size(760, 480);
        FormBorderStyle = FormBorderStyle.Sizable;

        Theme.StyleGrid(grid);
        grid.DataSource = lineas;
        if (grid.Columns[nameof(DetalleVenta.Id)] is { } id) id.Visible = false;
        if (grid.Columns[nameof(DetalleVenta.VentaId)] is { } ventaId) ventaId.Visible = false;
        if (grid.Columns[nameof(DetalleVenta.ProductoCodigo)] is { } codigo) codigo.Visible = false;
        if (grid.Columns[nameof(DetalleVenta.ProductoNombre)] is { } producto)
            producto.HeaderText = "Producto";
        if (grid.Columns[nameof(DetalleVenta.Cantidad)] is { } cantidad)
            cantidad.HeaderText = "Cantidad";
        if (grid.Columns[nameof(DetalleVenta.PrecioUnitario)] is { } precio)
        {
            precio.HeaderText = "Precio unitario";
            precio.DefaultCellStyle.Format = "C2";
        }
        if (grid.Columns[nameof(DetalleVenta.Subtotal)] is { } subtotal)
        {
            subtotal.HeaderText = "Subtotal";
            subtotal.DefaultCellStyle.Format = "C2";
        }

        var encabezado = new Label
        {
            Text = $"Pedidos de mesa {numeroMesa} · Venta #{venta.Id} · Total: {venta.Total:C2}",
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = Theme.TextPrimary,
            Font = Theme.HeadingFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8)
        };
        var pie = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(8)
        };
        var cerrar = new IconButton { Text = "Cerrar", AutoSize = true, Height = 36 };
        Theme.SetButtonIcon(cerrar, IconChar.Xmark);
        Theme.EstilizarBotonSecundario(cerrar);
        cerrar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        medioPago.Items.AddRange(new object[] { MedioPago.Efectivo, MedioPago.Transferencia });
        Theme.StyleComboBox(medioPago);

        var cobrar = new IconButton { Text = "Cobrar y liberar mesa", AutoSize = true, Height = 36 };
        Theme.SetButtonIcon(cobrar, IconChar.Check);
        Theme.EstilizarBotonPrimario(cobrar);
        cobrar.Click += Cobrar;

        pie.Controls.Add(new Label
        {
            Text = "Medio de pago",
            AutoSize = true,
            ForeColor = Theme.TextPrimary,
            Padding = new Padding(4, 8, 4, 0)
        });
        pie.Controls.Add(medioPago);
        pie.Controls.Add(cobrar);
        pie.Controls.Add(cerrar);

        if (lineas.Count == 0)
        {
            pie.Controls.Add(new Label
            {
                Text = "Esta venta pendiente todavía no tiene productos.",
                AutoSize = true,
                ForeColor = Theme.TextSecondary,
                Padding = new Padding(8)
            });
        }

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Theme.DeepCharcoal,
            Padding = new Padding(8)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.Controls.Add(encabezado, 0, 0);
        layout.Controls.Add(grid, 0, 1);
        layout.Controls.Add(pie, 0, 2);
        Controls.Add(layout);
    }

    private void Cobrar(object? sender, EventArgs e)
    {
        if (medioPago.SelectedItem is not MedioPago medioSeleccionado)
        {
            MessageBox.Show("Elegí el medio de pago para cerrar la venta.",
                "Falta medio de pago", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            medioPago.Focus();
            return;
        }

        try
        {
            ventasRepositorio.Cerrar(venta.Id, medioSeleccionado, null);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo registrar el pago ni cerrar la venta.\n" + ex.Message,
                "Error al cobrar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class MesaEditForm : BaseForm
{
    private readonly TextBox numeroBox = new() { Width = 280 };
    private readonly Mesa? mesaOriginal;

    public Mesa Mesa { get; } = new();

    public MesaEditForm(Mesa? mesa = null)
    {
        mesaOriginal = mesa;
        Text = mesa is null ? "Chachín | Crear mesa" : "Chachín | Modificar mesa";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        MaximumSize = Size.Empty;
        ClientSize = new Size(390, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.DeepCharcoal
        };
        AgregarCampo(panel, "Número", numeroBox);

        var guardar = new IconButton { Text = "Guardar", Width = 280, Height = 38 };
        Theme.SetButtonIcon(guardar, IconChar.FloppyDisk);
        Theme.EstilizarBotonPrimario(guardar);
        guardar.Click += Guardar;
        panel.Controls.Add(guardar);
        Controls.Add(panel);

        if (mesa is not null)
            numeroBox.Text = mesa.Numero.ToString(CultureInfo.CurrentCulture);
    }

    private static void AgregarCampo(FlowLayoutPanel panel, string texto, Control control)
    {
        panel.Controls.Add(new Label { Text = texto, AutoSize = true, Margin = new Padding(0, 0, 0, 2), ForeColor = Theme.TextPrimary });
        control.Margin = new Padding(0, 0, 0, 6);
        panel.Controls.Add(control);
        if (control is TextBox textBox)
            Theme.StyleTextBox(textBox);
        else if (control is ComboBox comboBox)
            Theme.StyleComboBox(comboBox);
    }

    private void Guardar(object? sender, EventArgs e)
    {
        if (!int.TryParse(numeroBox.Text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var numero))
        {
            MessageBox.Show("Ingresá un número de mesa entero válido.");
            numeroBox.Focus();
            return;
        }

        Mesa.Id = mesaOriginal?.Id ?? 0;
        Mesa.Numero = numero;
        Mesa.Activo = mesaOriginal?.Activo ?? true;
        DialogResult = DialogResult.OK;
        Close();
    }
}
