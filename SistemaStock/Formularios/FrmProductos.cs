using System.Globalization;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmProductos : BaseFormSidebar
{
    private readonly ProductoRepositorio repositorio = new();
    private readonly CategoriaRepositorio categoriasRepositorio = new();
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly TextBox busqueda = new() { PlaceholderText = "Buscar producto", Width = 220 };
    private readonly ComboBox filtroCategoria = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };

    public FrmProductos() : this(null) { }

    public FrmProductos(User? user) : base(PaginaApp.Productos, user)
    {
        InitializeComponent();
        Text = "Chachín | Productos";
        Theme.StyleGrid(grid);
        AddPageTitle("Productos", "Administración de productos del sistema");

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 92,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal,
            WrapContents = true
        };

        if (user is not null && UserPermissions.CanManageUsers(user))
        {
            var add = new Button { Text = "Crear producto", AutoSize = true };
            add.Click += (_, _) => CrearProducto();
            top.Controls.Add(add);

            var edit = new Button { Text = "Modificar", AutoSize = true };
            edit.Click += (_, _) => ModificarProducto();
            top.Controls.Add(edit);

            var deactivate = new Button { Text = "Desactivar", AutoSize = true };
            deactivate.Click += (_, _) => DesactivarProducto();
            top.Controls.Add(deactivate);
            top.Controls.Add(busqueda);
            top.Controls.Add(filtroCategoria);
            CargarCategorias();
            busqueda.TextChanged += (_, _) => ActualizarListado();
            filtroCategoria.SelectedIndexChanged += (_, _) => ActualizarListado();
            ActualizarListado();
        }
        else
        {
            top.Controls.Add(new Label
            {
                Text = "Tu rol no permite administrar productos.",
                AutoSize = true,
                Padding = new Padding(8),
                ForeColor = Theme.TextSecondary
            });
        }

        // Keep the same Dock ordering as FrmUsers: the toolbar reserves its row,
        // and the grid fills only the space below it.
        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(top);
    }

    private Producto? ProductoSeleccionado() => grid.CurrentRow?.DataBoundItem as Producto;

    private int? CategoriaSeleccionada => filtroCategoria.SelectedItem is Categoria categoria && categoria.Id > 0
        ? categoria.Id
        : null;

    private void CargarCategorias()
    {
        var categorias = categoriasRepositorio.GetAll();
        categorias.Insert(0, new Categoria { Id = 0, Nombre = "Todas las categorías" });
        filtroCategoria.DisplayMember = nameof(Categoria.Nombre);
        filtroCategoria.ValueMember = nameof(Categoria.Id);
        filtroCategoria.DataSource = categorias;
        filtroCategoria.SelectedIndex = filtroCategoria.Items.Count > 0 ? 0 : -1;
    }

    private void ActualizarListado()
    {
        grid.DataSource = repositorio.GetAll(busqueda.Text, CategoriaSeleccionada);
        if (grid.Columns[nameof(Producto.Codigo)] is { } codigo) codigo.HeaderText = "Código";
        if (grid.Columns[nameof(Producto.Nombre)] is { } nombre) nombre.HeaderText = "Nombre";
        if (grid.Columns[nameof(Producto.Precio)] is { } precio)
        {
            precio.HeaderText = "Precio";
            precio.DefaultCellStyle.Format = "C2";
        }
        if (grid.Columns[nameof(Producto.Stock)] is { } stock) stock.HeaderText = "Stock";
        if (grid.Columns[nameof(Producto.CategoriaNombre)] is { } categoria) categoria.HeaderText = "Categoría";
        if (grid.Columns[nameof(Producto.Activo)] is { } activo) activo.HeaderText = "Activo";
        if (grid.Columns[nameof(Producto.CategoriaId)] is { } categoriaId) categoriaId.Visible = false;
    }

    private void CrearProducto()
    {
        using var form = new ProductoEditForm(categoriasRepositorio.GetAll());
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            repositorio.Create(form.Producto);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo crear el producto.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ModificarProducto()
    {
        var producto = ProductoSeleccionado();
        if (producto is null) { MessageBox.Show("Seleccioná un producto."); return; }
        using var form = new ProductoEditForm(categoriasRepositorio.GetAll(), producto);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            producto.Nombre = form.Producto.Nombre;
            producto.Precio = form.Producto.Precio;
            producto.Stock = form.Producto.Stock;
            producto.CategoriaId = form.Producto.CategoriaId;
            repositorio.Update(producto);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo modificar el producto.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DesactivarProducto()
    {
        var producto = ProductoSeleccionado();
        if (producto is null) { MessageBox.Show("Seleccioná un producto."); return; }
        if (!producto.Activo) { MessageBox.Show("El producto ya está desactivado."); return; }
        if (MessageBox.Show($"¿Desactivar {producto.Nombre}?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        try
        {
            repositorio.Deactivate(producto.Codigo);
            ActualizarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo desactivar el producto.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

internal sealed class ProductoEditForm : BaseForm
{
    private readonly TextBox nombreBox = new() { Width = 280 };
    private readonly TextBox precioBox = new() { Width = 280 };
    private readonly TextBox stockBox = new() { Width = 280 };
    private readonly ComboBox categoriaBox = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Producto? productoOriginal;

    public Producto Producto { get; } = new();

    public ProductoEditForm(List<Categoria> categorias, Producto? producto = null)
    {
        productoOriginal = producto;
        Text = producto is null ? "Chachín | Crear producto" : "Chachín | Modificar producto";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        ClientSize = new Size(390, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        categoriaBox.DisplayMember = nameof(Categoria.Nombre);
        categoriaBox.ValueMember = nameof(Categoria.Id);
        categoriaBox.DataSource = categorias;

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
        AgregarCampo(panel, "Precio", precioBox);
        AgregarCampo(panel, "Stock", stockBox);
        AgregarCampo(panel, "Categoría", categoriaBox);

        var guardar = new Button { Text = "Guardar", Width = 280, Height = 38 };
        guardar.BackColor = Theme.BlazeOrange;
        guardar.ForeColor = Theme.TextPrimary;
        guardar.FlatStyle = FlatStyle.Flat;
        guardar.FlatAppearance.BorderSize = 0;
        guardar.Click += Guardar;
        panel.Controls.Add(guardar);
        Controls.Add(panel);

        if (producto is not null)
        {
            nombreBox.Text = producto.Nombre;
            precioBox.Text = producto.Precio.ToString(CultureInfo.CurrentCulture);
            stockBox.Text = producto.Stock.ToString(CultureInfo.CurrentCulture);
            if (categorias.Any(categoria => categoria.Id == producto.CategoriaId))
                categoriaBox.SelectedValue = producto.CategoriaId;
        }
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
        if (string.IsNullOrWhiteSpace(nombreBox.Text))
        {
            MessageBox.Show("El nombre del producto es obligatorio.");
            nombreBox.Focus();
            return;
        }
        if (!decimal.TryParse(precioBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var precio) || precio < 0)
        {
            MessageBox.Show("Ingresá un precio numérico válido mayor o igual a cero.");
            precioBox.Focus();
            return;
        }
        if (!int.TryParse(stockBox.Text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out var stock) || stock < 0)
        {
            MessageBox.Show("Ingresá un stock entero válido mayor o igual a cero.");
            stockBox.Focus();
            return;
        }
        if (categoriaBox.SelectedItem is not Categoria categoria)
        {
            MessageBox.Show("Elegí una categoría.");
            categoriaBox.Focus();
            return;
        }

        Producto.Codigo = productoOriginal?.Codigo ?? 0;
        Producto.Nombre = nombreBox.Text.Trim();
        Producto.Precio = precio;
        Producto.Stock = stock;
        Producto.CategoriaId = categoria.Id;
        Producto.Activo = productoOriginal?.Activo ?? true;
        DialogResult = DialogResult.OK;
        Close();
    }
}
