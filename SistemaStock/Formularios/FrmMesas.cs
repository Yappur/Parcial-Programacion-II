using System.Globalization;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public sealed class FrmMesas : BasePage
{
    private readonly MesaRepositorio repositorio = new();
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly ComboBox filtroEstado = new()
    {
        Width = 150,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    public FrmMesas() : this(null) { }

    public FrmMesas(User? user)
    {
        Text = "Chachín | Mesas";
        Theme.StyleGrid(grid);
        AddPageTitle("Mesas", "Administración de mesas del local");

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

        if (user is not null && UserPermissions.CanManageUsers(user))
        {
            var add = new Button { Text = "Crear mesa", AutoSize = true };
            add.Click += (_, _) => CrearMesa();
            top.Controls.Add(add);

            var edit = new Button { Text = "Modificar", AutoSize = true };
            edit.Click += (_, _) => ModificarMesa();
            top.Controls.Add(edit);

            var deactivate = new Button { Text = "Desactivar", AutoSize = true };
            deactivate.Click += (_, _) => DesactivarMesa();
            top.Controls.Add(deactivate);

            var activate = new Button { Text = "Activar", AutoSize = true };
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

        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(top);
        CargarMesas();
    }

    private Mesa? MesaSeleccionada() => grid.CurrentRow?.DataBoundItem as Mesa;

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
            grid.DataSource = repositorio.GetAll(filtroActivo);
            if (grid.Columns[nameof(Mesa.Id)] is { } id) id.Visible = false;
            if (grid.Columns[nameof(Mesa.Numero)] is { } numero) numero.HeaderText = "Número";
            if (grid.Columns[nameof(Mesa.Activo)] is { } activeColumn) activeColumn.HeaderText = "Activa";
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar las mesas. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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

        var guardar = new Button { Text = "Guardar", Width = 280, Height = 38 };
        guardar.BackColor = Theme.BlazeOrange;
        guardar.ForeColor = Theme.TextPrimary;
        guardar.FlatStyle = FlatStyle.Flat;
        guardar.FlatAppearance.BorderSize = 0;
        guardar.Click += Guardar;
        panel.Controls.Add(guardar);
        Controls.Add(panel);

        if (mesa is not null)
            numeroBox.Text = mesa.Numero.ToString(CultureInfo.CurrentCulture);
    }

    private static void AgregarCampo(FlowLayoutPanel panel, string texto, Control control)
    {
        panel.Controls.Add(new Label { Text = texto, AutoSize = true, ForeColor = Theme.TextPrimary });
        if (control is TextBox textBox)
        {
            textBox.BackColor = Theme.ObsidianBlack;
            textBox.ForeColor = Theme.TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }
        control.Margin = new Padding(0, 3, 0, 10);
        panel.Controls.Add(control);
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
