using System.Globalization;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;
using FontAwesome.Sharp;

namespace SistemaStock.Formularios;

public sealed class FrmIngredientes : BasePage
{
    private readonly IngredienteRepositorio repositorio = new();
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly TextBox busqueda = new() { PlaceholderText = "Buscar ingrediente", Width = 190 };

    public FrmIngredientes() : this(null) { }

    public FrmIngredientes(User? user)
    {
        Text = "Chachín | Ingredientes";
        Theme.StyleGrid(grid);
        AddPageTitle("Ingredientes", "Administración de ingredientes y existencias");

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal,
            WrapContents = true,
            AutoScroll = true
        };

        if (user is not null && UserPermissions.CanManageUsers(user))
        {
            var add = new IconButton { Text = "Crear", AutoSize = true };
            Theme.SetButtonIcon(add, IconChar.Plus);
            Theme.EstilizarBotonPrimario(add);
            add.Click += (_, _) => CrearIngrediente();
            top.Controls.Add(add);

            var edit = new IconButton { Text = "Modificar", AutoSize = true };
            Theme.SetButtonIcon(edit, IconChar.PenToSquare);
            Theme.EstilizarBotonSecundario(edit);
            edit.Click += (_, _) => ModificarIngrediente();
            top.Controls.Add(edit);

            var deactivate = new IconButton { Text = "Desactivar", AutoSize = true };
            Theme.SetButtonIcon(deactivate, IconChar.TrashCan);
            Theme.EstilizarBotonPeligro(deactivate);
            deactivate.Click += (_, _) => DesactivarIngrediente();
            top.Controls.Add(deactivate);

            top.Controls.Add(busqueda);
            Theme.StyleTextBox(busqueda);
            busqueda.TextChanged += (_, _) => ActualizarListado();
            ActualizarListado();
        }
        else
        {
            top.Controls.Add(new Label
            {
                Text = "Tu rol no permite administrar ingredientes.",
                AutoSize = true,
                Padding = new Padding(8),
                ForeColor = Theme.TextSecondary
            });
        }

        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(top);
    }

    private Ingrediente? IngredienteSeleccionado() => grid.CurrentRow?.DataBoundItem as Ingrediente;

    private void ActualizarListado()
    {
        try
        {
            grid.DataSource = repositorio.GetAll(busqueda.Text);
            if (grid.Columns[nameof(Ingrediente.Id)] is { } id) id.Visible = false;
            if (grid.Columns[nameof(Ingrediente.Nombre)] is { } nombre) nombre.HeaderText = "Nombre";
            if (grid.Columns[nameof(Ingrediente.Unidad)] is { } unidad) unidad.HeaderText = "Unidad";
            if (grid.Columns[nameof(Ingrediente.Stock)] is { } stock)
            {
                stock.HeaderText = "Stock";
                stock.DefaultCellStyle.Format = "0.###";
            }
            if (grid.Columns[nameof(Ingrediente.StockMinimo)] is { } minimo)
            {
                minimo.HeaderText = "Stock mínimo";
                minimo.DefaultCellStyle.Format = "0.###";
            }
            if (grid.Columns[nameof(Ingrediente.Activo)] is { } activo) activo.HeaderText = "Activo";
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudieron cargar los ingredientes. Revisá la conexión a la base de datos.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CrearIngrediente()
    {
        using var form = new IngredienteEditForm();
        if (form.ShowDialog(this.FindForm()) != DialogResult.OK) return;
        try
        {
            repositorio.Create(form.Ingrediente);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo crear el ingrediente. Verificá que el nombre no esté repetido.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ModificarIngrediente()
    {
        var seleccionado = IngredienteSeleccionado();
        if (seleccionado is null) { MessageBox.Show("Seleccioná un ingrediente."); return; }
        using var form = new IngredienteEditForm(seleccionado);
        if (form.ShowDialog(this.FindForm()) != DialogResult.OK) return;
        try
        {
            seleccionado.Nombre = form.Ingrediente.Nombre;
            seleccionado.Unidad = form.Ingrediente.Unidad;
            seleccionado.Stock = form.Ingrediente.Stock;
            seleccionado.StockMinimo = form.Ingrediente.StockMinimo;
            repositorio.Update(seleccionado);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo modificar el ingrediente. Verificá que el nombre no esté repetido.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DesactivarIngrediente()
    {
        var seleccionado = IngredienteSeleccionado();
        if (seleccionado is null) { MessageBox.Show("Seleccioná un ingrediente."); return; }
        if (!seleccionado.Activo) { MessageBox.Show("El ingrediente ya está desactivado."); return; }
        if (MessageBox.Show($"¿Desactivar {seleccionado.Nombre}?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            repositorio.Deactivate(seleccionado.Id);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo desactivar el ingrediente.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class IngredienteEditForm : BaseForm
{
    private readonly TextBox nombreBox = new() { Width = 280 };
    private readonly ComboBox unidadBox = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox stockBox = new() { Width = 280 };
    private readonly TextBox stockMinimoBox = new() { Width = 280 };
    private readonly Ingrediente? ingredienteOriginal;

    public Ingrediente Ingrediente { get; } = new();

    public IngredienteEditForm(Ingrediente? ingrediente = null)
    {
        ingredienteOriginal = ingrediente;
        Text = ingrediente is null ? "Chachín | Crear ingrediente" : "Chachín | Modificar ingrediente";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        ClientSize = new Size(390, 350);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        unidadBox.Items.AddRange(["un", "g", "ml"]);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 20, 28, 20),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.DeepCharcoal
        };
        AgregarCampo(panel, "Nombre", nombreBox);
        AgregarCampo(panel, "Unidad", unidadBox);
        AgregarCampo(panel, "Stock", stockBox);
        AgregarCampo(panel, "Stock mínimo", stockMinimoBox);

        var guardar = new IconButton { Text = "Guardar", Width = 280, Height = 38 };
        Theme.SetButtonIcon(guardar, IconChar.FloppyDisk);
        Theme.EstilizarBotonPrimario(guardar);
        guardar.Click += Guardar;
        panel.Controls.Add(guardar);
        Controls.Add(panel);

        if (ingrediente is not null)
        {
            nombreBox.Text = ingrediente.Nombre;
            unidadBox.SelectedItem = ingrediente.Unidad;
            stockBox.Text = ingrediente.Stock.ToString(CultureInfo.CurrentCulture);
            stockMinimoBox.Text = ingrediente.StockMinimo.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            unidadBox.SelectedItem = "un";
            stockBox.Text = "0";
            stockMinimoBox.Text = "0";
        }
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
        if (string.IsNullOrWhiteSpace(nombreBox.Text))
        {
            MessageBox.Show("El nombre del ingrediente es obligatorio.");
            nombreBox.Focus();
            return;
        }
        if (unidadBox.SelectedItem is not string unidad)
        {
            MessageBox.Show("Elegí una unidad.");
            unidadBox.Focus();
            return;
        }
        if (!decimal.TryParse(stockBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var stock) || stock < 0)
        {
            MessageBox.Show("Ingresá un stock decimal válido mayor o igual a cero.");
            stockBox.Focus();
            return;
        }
        if (!decimal.TryParse(stockMinimoBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var stockMinimo) || stockMinimo < 0)
        {
            MessageBox.Show("Ingresá un stock mínimo decimal válido mayor o igual a cero.");
            stockMinimoBox.Focus();
            return;
        }

        Ingrediente.Id = ingredienteOriginal?.Id ?? 0;
        Ingrediente.Nombre = nombreBox.Text.Trim();
        Ingrediente.Unidad = unidad;
        Ingrediente.Stock = stock;
        Ingrediente.StockMinimo = stockMinimo;
        Ingrediente.Activo = ingredienteOriginal?.Activo ?? true;
        DialogResult = DialogResult.OK;
        Close();
    }
}
