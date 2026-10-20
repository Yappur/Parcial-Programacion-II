using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios;

public partial class FrmVentas : BasePage
{
    private readonly ProductoRepositorio repositorio = new();
    private readonly ComboBox productoBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 270 };
    private readonly NumericUpDown cantidadBox = new() { Minimum = 1, Maximum = 1000000, Width = 100 };
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
    private readonly Label estadoLabel = new() { AutoSize = true, Padding = new Padding(8) };

    public FrmVentas() : this(null) { }

    public FrmVentas(User? user)
    {
        InitializeComponent();
        AddPageTitle("Ventas", "Registrá ventas y descontá automáticamente las unidades del inventario");
        Theme.StyleGrid(grid);
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Producto.Codigo), HeaderText = "Código" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Producto.Nombre), HeaderText = "Producto" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Producto.CategoriaNombre), HeaderText = "Categoría" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Producto.Precio), HeaderText = "Precio", DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" } });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Producto.Stock), HeaderText = "Stock disponible" });

        var registrar = new Button { Text = "Registrar venta", AutoSize = true, Height = 34 };
        registrar.BackColor = Theme.BlazeOrange;
        registrar.ForeColor = Theme.TextPrimary;
        registrar.FlatStyle = FlatStyle.Flat;
        registrar.FlatAppearance.BorderSize = 0;
        registrar.Click += (_, _) => RegistrarVenta();

        var actualizar = new Button { Text = "Actualizar productos", AutoSize = true, Height = 34 };
        actualizar.BackColor = Theme.SurfaceRaised;
        actualizar.ForeColor = Theme.TextPrimary;
        actualizar.FlatStyle = FlatStyle.Flat;
        actualizar.FlatAppearance.BorderColor = Theme.GridLine;
        actualizar.Click += (_, _) => CargarProductos();

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal,
            WrapContents = true
        };
        barra.Controls.Add(new Label { Text = "Producto", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        barra.Controls.Add(productoBox);
        barra.Controls.Add(new Label { Text = "Cantidad", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        barra.Controls.Add(cantidadBox);
        barra.Controls.Add(registrar);
        barra.Controls.Add(actualizar);
        barra.Controls.Add(estadoLabel);

        productoBox.SelectedIndexChanged += (_, _) => MostrarStockSeleccionado();
        grid.SelectionChanged += (_, _) => SeleccionarProductoDeGrilla();
        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(barra);
        CargarProductos();
    }

    private void CargarProductos()
    {
        try
        {
            var productos = repositorio.GetActivos();
            productoBox.DisplayMember = nameof(Producto.Nombre);
            productoBox.ValueMember = nameof(Producto.Codigo);
            productoBox.DataSource = productos;
            grid.DataSource = productos.Select(producto => new Producto
            {
                Codigo = producto.Codigo, Nombre = producto.Nombre, CategoriaNombre = producto.CategoriaNombre,
                Precio = producto.Precio, Stock = producto.Stock, Activo = producto.Activo,
                CategoriaId = producto.CategoriaId
            }).ToList();
            estadoLabel.Text = productos.Count == 0 ? "No hay productos activos para vender." : "";
            MostrarStockSeleccionado();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron cargar los productos.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RegistrarVenta()
    {
        if (productoBox.SelectedItem is not Producto producto)
        {
            MessageBox.Show("Elegí un producto para registrar la venta.", "Ventas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            repositorio.RegistrarVenta(producto.Codigo, (int)cantidadBox.Value);
            CargarProductos();
            estadoLabel.Text = "Venta registrada; el stock fue actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "No se pudo registrar la venta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo registrar la venta.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MostrarStockSeleccionado()
    {
        if (productoBox.SelectedItem is Producto producto)
            estadoLabel.Text = $"Disponibles: {producto.Stock}";
    }

    private void SeleccionarProductoDeGrilla()
    {
        if (grid.CurrentRow?.DataBoundItem is not Producto producto) return;
        productoBox.SelectedValue = producto.Codigo;
    }
}
