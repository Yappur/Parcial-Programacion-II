using System.ComponentModel;
using System.Globalization;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;
using SistemaStock.Modelos.Usuarios;
using FontAwesome.Sharp;

namespace SistemaStock.Formularios;

/// <summary>Pantalla de administración de recetas para productos POR_RECETA.</summary>
public sealed class FrmRecetas : BasePage
{
    private readonly ProductoRepositorio productosRepositorio = new();
    private readonly IngredienteRepositorio ingredientesRepositorio = new();
    private readonly RecetaRepositorio recetasRepositorio = new();
    private readonly ComboBox productoBox = new() { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox ingredienteBox = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox cantidadBox = new() { Width = 110, PlaceholderText = "Ej. 1 o 50" };
    private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
    private readonly BindingList<Receta> filas = [];

    public FrmRecetas() : this(null) { }

    public FrmRecetas(User? user)
    {
        Text = "Chachín | Recetas";
        Theme.StyleGrid(grid);
        AddPageTitle("Recetas", "Ingredientes y cantidades necesarias por producto");

        var barra = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 112,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = Theme.DeepCharcoal,
            WrapContents = true,
            AutoScroll = true
        };
        PageContent.Controls.Add(grid);
        PageContent.Controls.Add(barra);

        if (user is null || !UserPermissions.CanManageUsers(user))
        {
            barra.Controls.Add(new Label
            {
                Text = "Tu rol no permite administrar recetas.",
                AutoSize = true,
                Padding = new Padding(8),
                ForeColor = Theme.TextSecondary
            });
            grid.Visible = false;
            return;
        }

        var productos = productosRepositorio.GetAll()
            .Where(producto => producto.Activo && producto.TipoStock == TipoStock.PorReceta)
            .OrderBy(producto => producto.Nombre)
            .ToList();
        productoBox.DisplayMember = nameof(Producto.Nombre);
        productoBox.ValueMember = nameof(Producto.Codigo);
        productoBox.DataSource = productos;
        productoBox.SelectedIndexChanged += (_, _) => CargarRecetaSeleccionada();

        var ingredientes = ingredientesRepositorio.GetAll()
            .Where(ingrediente => ingrediente.Activo)
            .OrderBy(ingrediente => ingrediente.Nombre)
            .ToList();
        ingredienteBox.DisplayMember = nameof(Ingrediente.Nombre);
        ingredienteBox.ValueMember = nameof(Ingrediente.Id);
        ingredienteBox.DataSource = ingredientes;

        var agregar = new IconButton { Text = "Agregar ingrediente", AutoSize = true };
        Theme.SetButtonIcon(agregar, IconChar.Plus);
        Theme.EstilizarBotonPrimario(agregar);
        agregar.Click += (_, _) => AgregarIngrediente();
        var quitar = new IconButton { Text = "Quitar seleccionado", AutoSize = true };
        Theme.SetButtonIcon(quitar, IconChar.TrashCan);
        Theme.EstilizarBotonPeligro(quitar);
        quitar.Click += (_, _) => QuitarIngrediente();
        var guardar = new IconButton { Text = "Guardar receta", AutoSize = true };
        Theme.SetButtonIcon(guardar, IconChar.FloppyDisk);
        Theme.EstilizarBotonPrimario(guardar);
        guardar.Click += (_, _) => GuardarReceta();

        barra.Controls.Add(new Label { Text = "Producto", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(productoBox);
        barra.Controls.Add(new Label { Text = "Ingrediente", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(ingredienteBox);
        barra.Controls.Add(new Label { Text = "Cantidad", AutoSize = true, Padding = new Padding(0, 8, 0, 0), ForeColor = Theme.TextPrimary });
        barra.Controls.Add(cantidadBox);
        barra.Controls.Add(agregar);
        barra.Controls.Add(quitar);
        barra.Controls.Add(guardar);

        grid.DataSource = filas;
        ConfigurarColumnas();
        if (productos.Count == 0)
        {
            barra.Controls.Add(new Label
            {
                Text = "No hay productos activos POR_RECETA para configurar.",
                AutoSize = true,
                Padding = new Padding(8),
                ForeColor = Theme.TextSecondary
            });
            productoBox.Enabled = ingredienteBox.Enabled = cantidadBox.Enabled = false;
            agregar.Enabled = quitar.Enabled = guardar.Enabled = false;
        }
        else CargarRecetaSeleccionada();
    }

    private void ConfigurarColumnas()
    {
        if (grid.Columns[nameof(Receta.ProductoCodigo)] is { } productoCodigo) productoCodigo.Visible = false;
        if (grid.Columns[nameof(Receta.IngredienteId)] is { } ingredienteId) ingredienteId.Visible = false;
        if (grid.Columns[nameof(Receta.IngredienteNombre)] is { } nombre) nombre.HeaderText = "Ingrediente";
        if (grid.Columns[nameof(Receta.Unidad)] is { } unidad) unidad.HeaderText = "Unidad";
        if (grid.Columns[nameof(Receta.Cantidad)] is { } cantidad)
        {
            cantidad.HeaderText = "Cantidad por producto";
            cantidad.DefaultCellStyle.Format = "0.###";
        }
    }

    private void CargarRecetaSeleccionada()
    {
        if (productoBox.SelectedItem is not Producto producto) return;
        try
        {
            filas.Clear();
            foreach (var fila in recetasRepositorio.ObtenerPorProducto(producto.Codigo)) filas.Add(fila);
            ConfigurarColumnas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo cargar la receta.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgregarIngrediente()
    {
        if (ingredienteBox.SelectedItem is not Ingrediente ingrediente)
        {
            MessageBox.Show("Elegí un ingrediente.");
            return;
        }
        if (!decimal.TryParse(cantidadBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var cantidad) || cantidad <= 0)
        {
            MessageBox.Show("Ingresá una cantidad válida mayor que cero.");
            cantidadBox.Focus();
            return;
        }
        if (filas.Any(fila => fila.IngredienteId == ingrediente.Id))
        {
            MessageBox.Show("Ese ingrediente ya está en la receta.");
            return;
        }
        if (productoBox.SelectedItem is not Producto producto) return;

        filas.Add(new Receta
        {
            ProductoCodigo = producto.Codigo,
            IngredienteId = ingrediente.Id,
            IngredienteNombre = ingrediente.Nombre,
            Unidad = ingrediente.Unidad,
            Cantidad = cantidad
        });
        cantidadBox.Clear();
        ConfigurarColumnas();
    }

    private void QuitarIngrediente()
    {
        if (grid.CurrentRow?.DataBoundItem is not Receta fila)
        {
            MessageBox.Show("Seleccioná un ingrediente de la receta.");
            return;
        }
        filas.Remove(fila);
    }

    private void GuardarReceta()
    {
        if (productoBox.SelectedItem is not Producto producto)
        {
            MessageBox.Show("Elegí un producto POR_RECETA.");
            return;
        }
        try
        {
            recetasRepositorio.Guardar(producto.Codigo, filas.ToList());
            MessageBox.Show("La receta se guardó correctamente.", "Recetas",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo guardar la receta. No se aplicaron cambios parciales.\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
