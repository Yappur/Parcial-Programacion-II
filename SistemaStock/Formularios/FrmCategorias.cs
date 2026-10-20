using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos;

namespace SistemaStock.Formularios;

internal sealed class FrmCategorias : BaseForm
{
    private readonly CategoriaRepositorio repositorio;
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
        AllowUserToResizeColumns = false,
        AllowUserToResizeRows = false,
        MultiSelect = false
    };
    private readonly TextBox nombreNuevaCategoria = new() { Width = 250, PlaceholderText = "Nombre de categoría" };
    private readonly Button crear = new() { Text = "Crear categoría", AutoSize = true };
    private readonly Button eliminar = new() { Text = "Eliminar categoría", AutoSize = true, Enabled = false };

    public FrmCategorias(CategoriaRepositorio repositorio)
    {
        this.repositorio = repositorio;
        Text = "Chachín | Categorías";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        ClientSize = new Size(620, 430);
        FormBorderStyle = FormBorderStyle.Sizable;

        Theme.StyleGrid(grid);
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colCodigo", DataPropertyName = nameof(Categoria.Id), HeaderText = "Código",
            Width = 90, ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNombre", DataPropertyName = nameof(Categoria.Nombre), HeaderText = "Nombre",
            Width = 280, ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
        });
        grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "colModificar", HeaderText = "Acción", Text = "Modificar",
            UseColumnTextForButtonValue = true, Width = 110, ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        nombreNuevaCategoria.BackColor = Theme.ObsidianBlack;
        nombreNuevaCategoria.ForeColor = Theme.TextPrimary;
        nombreNuevaCategoria.BorderStyle = BorderStyle.FixedSingle;
        EstilizarBoton(crear, primario: true);
        EstilizarBoton(eliminar, primario: false);

        var titulo = new Label
        {
            Text = "Administrar categorías", Dock = DockStyle.Top, Height = 48,
            Font = Theme.HeadingFont, ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Top, Height = 54, WrapContents = true,
            BackColor = Theme.DeepCharcoal, Padding = new Padding(0, 7, 0, 4)
        };
        acciones.Controls.Add(nombreNuevaCategoria);
        acciones.Controls.Add(crear);
        acciones.Controls.Add(eliminar);
        Controls.Add(grid);
        Controls.Add(acciones);
        Controls.Add(titulo);

        grid.SelectionChanged += (_, _) => eliminar.Enabled = CategoriaSeleccionada() is not null;
        grid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "colModificar")
                ModificarCategoria(grid.Rows[e.RowIndex].DataBoundItem as Categoria);
        };
        crear.Click += (_, _) => CrearCategoria();
        eliminar.Click += (_, _) => EliminarCategoria();
        CargarCategorias();
    }

    private Categoria? CategoriaSeleccionada() => grid.CurrentRow?.DataBoundItem as Categoria;

    private void CargarCategorias()
    {
        grid.DataSource = repositorio.GetAll();
        grid.ClearSelection();
        grid.CurrentCell = null;
        eliminar.Enabled = false;
    }

    private void CrearCategoria()
    {
        var nombre = nombreNuevaCategoria.Text.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
        {
            MessageBox.Show(this, "Escribí el nombre de la categoría.", "Dato obligatorio",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nombreNuevaCategoria.Focus();
            return;
        }

        try
        {
            repositorio.Create(nombre);
            nombreNuevaCategoria.Clear();
            CargarCategorias();
            nombreNuevaCategoria.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo crear la categoría. Verificá que no exista otra con ese nombre.\n" + ex.Message,
                "Error al crear", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ModificarCategoria(Categoria? categoria)
    {
        if (categoria is null) return;
        using var dialogo = new FrmEditarCategoria(categoria.Nombre);
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            categoria.Nombre = dialogo.NombreCategoria;
            repositorio.Update(categoria);
            CargarCategorias();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "No se pudo modificar la categoría. Verificá que no exista otra con ese nombre.\n" + ex.Message,
                "Error al modificar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EliminarCategoria()
    {
        var categoria = CategoriaSeleccionada();
        if (categoria is null)
        {
            MessageBox.Show(this, "Seleccioná una categoría de la lista para eliminarla.", "Categoría no seleccionada",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(this, $"¿Eliminar la categoría «{categoria.Nombre}»?", "Confirmar eliminación",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        try
        {
            repositorio.Delete(categoria.Id);
            CargarCategorias();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "No se pudo eliminar la categoría. Si tiene productos asociados, primero reasignalos a otra categoría.\n" + ex.Message,
                "Categoría en uso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static void EstilizarBoton(Button boton, bool primario)
    {
        boton.FlatStyle = FlatStyle.Flat;
        boton.FlatAppearance.BorderSize = primario ? 0 : 1;
        boton.FlatAppearance.BorderColor = Theme.TextSecondary;
        boton.BackColor = primario ? Theme.BlazeOrange : Theme.Surface;
        boton.ForeColor = Theme.TextPrimary;
        boton.Cursor = Cursors.Hand;
    }
}

internal sealed class FrmEditarCategoria : BaseForm
{
    private readonly TextBox nombre = new() { Width = 280 };
    public string NombreCategoria => nombre.Text.Trim();

    public FrmEditarCategoria(string nombreActual)
    {
        Text = "Modificar categoría";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Normal;
        MinimumSize = Size.Empty;
        ClientSize = new Size(360, 150);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        nombre.Text = nombreActual;
        nombre.BackColor = Theme.ObsidianBlack;
        nombre.ForeColor = Theme.TextPrimary;
        nombre.BorderStyle = BorderStyle.FixedSingle;

        var contenido = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(24),
            FlowDirection = FlowDirection.TopDown, WrapContents = false,
            BackColor = Theme.DeepCharcoal
        };
        contenido.Controls.Add(new Label
        {
            Text = "Nombre de la categoría", AutoSize = true, ForeColor = Theme.TextPrimary
        });
        contenido.Controls.Add(nombre);

        var guardar = new Button
        {
            Text = "Guardar", Width = 100, Height = 34, DialogResult = DialogResult.OK,
            BackColor = Theme.BlazeOrange, ForeColor = Theme.TextPrimary, FlatStyle = FlatStyle.Flat
        };
        guardar.FlatAppearance.BorderSize = 0;
        guardar.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(nombre.Text))
            {
                DialogResult = DialogResult.None;
                MessageBox.Show(this, "Escribí el nombre de la categoría.", "Dato obligatorio",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nombre.Focus();
            }
        };
        contenido.Controls.Add(guardar);
        Controls.Add(contenido);
        AcceptButton = guardar;
    }
}
