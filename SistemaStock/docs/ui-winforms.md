# UI en WinForms: estilo, plantillas y layout

Todo lo visual del sistema Chachín: paleta, `Theme`, `BaseForm`, sidebar, formularios, páginas (`UserControl`), diálogos y layout. La implementación actual usa `BaseFormSidebar` y varios formularios (`FrmDashboard`, `FrmProductos`, `FrmUsers` y `FrmVentas`). La arquitectura de una ventana principal con `UserControl` por módulo queda como objetivo futuro. Las reglas de oro están en `../SKILL.md`, sección 4. Los repositorios que llaman estas pantallas están documentados en `codigo-csharp.md`.

## Contenido

- Paleta (centralizada en Theme.cs)
- Plantillas (adaptar a lo que ya exista, no duplicar)
  - Estilos/Theme.cs (implementado)
  - Estilos/BaseForm.cs (implementado)
  - Estilos/BaseFormSidebar.cs (sidebar y navegación actual)
  - Formularios/FrmDashboard.cs, FrmProductos.cs, FrmUsers.cs y FrmVentas.cs
  - Página interna (UserControl)
  - Diálogo (Form que hereda de BaseForm)
- Pantallas por módulo
- Login y cierre de sesión
- Layout responsive: checklist por pantalla

## Paleta actual (centralizada en Theme.cs)

| Nombre | HEX | Uso |
|---|---|---|
| Obsidian Black | `#111214` | Sidebar, estructura base, fondo oscuro |
| Deep Charcoal | `#18191C` | Fondo de formularios y paneles |
| Surface | `#202226` | Fondo de grillas y superficies |
| Blaze Orange | `#F2702A` | Acción principal e ítem activo del menú |
| Text Primary | `#F5F5F5` | Texto principal |
| Text Secondary | `#B9BCC2` | Texto secundario |

Ventana: mínimo **850 x 500**. `BaseForm` establece un tamaño inicial de `1100 x 720`; el login es una excepción fija de `1000 x 620`.

## Implementación actual

`Theme.cs` expone actualmente estos métodos principales:

- `ApplyForm(Form form)` para el estilo base de un formulario.
- `StyleNavigationButton(Button button, bool active)` para el menú lateral.
- `StyleGrid(DataGridView grid)` para las grillas.

`BaseForm` hereda de `Form`, aplica `Theme.ApplyForm(this)` y configura el tamaño mínimo, el ícono y el comportamiento general. `BaseFormSidebar` construye el sidebar y navega entre los formularios actuales. Los diálogos actuales son clases internas como `ProductoEditForm`, que hereda de `BaseForm`, y `UserEditForm`, que actualmente hereda directamente de `Form`.

La plantilla de `Theme` que aparece debajo es una referencia de diseño y no debe pegarse encima del archivo actual. Si se modifica el tema, hay que conservar los nombres y propiedades que ya utilizan los formularios.

## Plantillas (adaptar a lo que ya exista, no duplicar)

Antes de pegar cualquier plantilla, **revisar si el archivo ya existe**. Si existe, agregar o ajustar solo lo que falta.

### Estilos/Theme.cs (referencia futura)

```csharp
using System.Drawing;
using System.Windows.Forms;

namespace SistemaStock.Estilos;

public static class Theme
{
    // Paleta
    public static readonly Color ObsidianBlack = ColorTranslator.FromHtml("#0C0C0E");
    public static readonly Color DeepCharcoal  = ColorTranslator.FromHtml("#1F2024");
    public static readonly Color BlazeOrange   = ColorTranslator.FromHtml("#FF5500");
    public static readonly Color HazardYellow  = ColorTranslator.FromHtml("#FFD000");
    public static readonly Color ChalkWhite    = ColorTranslator.FromHtml("#F4F4F6");
    public static readonly Color ZincGrey      = ColorTranslator.FromHtml("#8E929C");

    public static readonly Size TamanoMinimo = new Size(850, 500);

    // Fuentes
    public static readonly Font FuenteNormal = new Font("Segoe UI", 10f);
    public static readonly Font FuenteTitulo = new Font("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font FuenteMenu   = new Font("Segoe UI Semibold", 10.5f);

    public static void EstilizarBotonPrimario(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = BlazeOrange;
        b.ForeColor = ChalkWhite;
        b.Font = FuenteNormal;
        b.Cursor = Cursors.Hand;
    }

    public static void EstilizarBotonSecundario(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = ZincGrey;
        b.FlatAppearance.BorderSize = 1;
        b.BackColor = DeepCharcoal;
        b.ForeColor = ChalkWhite;
        b.Font = FuenteNormal;
        b.Cursor = Cursors.Hand;
    }

    public static void EstilizarTextBox(TextBox t)
    {
        t.BackColor = ObsidianBlack;
        t.ForeColor = ChalkWhite;
        t.BorderStyle = BorderStyle.FixedSingle;
        t.Font = FuenteNormal;
    }

    public static void EstilizarGrilla(DataGridView g)
    {
        g.BackgroundColor = DeepCharcoal;
        g.BorderStyle = BorderStyle.None;
        g.EnableHeadersVisualStyles = false;
        g.GridColor = ZincGrey;
        g.RowHeadersVisible = false;
        g.AllowUserToAddRows = false;
        g.ReadOnly = true;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        g.ColumnHeadersDefaultCellStyle.BackColor = ObsidianBlack;
        g.ColumnHeadersDefaultCellStyle.ForeColor = HazardYellow;
        g.ColumnHeadersDefaultCellStyle.Font = FuenteMenu;

        g.DefaultCellStyle.BackColor = DeepCharcoal;
        g.DefaultCellStyle.ForeColor = ChalkWhite;
        g.DefaultCellStyle.SelectionBackColor = BlazeOrange;
        g.DefaultCellStyle.SelectionForeColor = ChalkWhite;
    }

    // Estiliza labels, textbox y grillas de un contenedor (recursivo).
    // Un Label con Tag = "propio" NO se toca (para títulos o etiquetas con color especial).
    // Los botones NO se tocan: se estilizan a mano con EstilizarBotonPrimario/Secundario.
    public static void Aplicar(Control raiz)
    {
        foreach (Control c in raiz.Controls)
        {
            switch (c)
            {
                case DataGridView g: EstilizarGrilla(g); break;
                case TextBox t: EstilizarTextBox(t); break;
                case Label l when (l.Tag as string) != "propio":
                    l.ForeColor = ChalkWhite;
                    l.Font = FuenteNormal;
                    break;
            }
            if (c.HasChildren) Aplicar(c);
        }
    }
}
```

### Estilos/BaseForm.cs (referencia futura)

```csharp
using System;
using System.Windows.Forms;

namespace SistemaStock.Estilos;

public partial class BaseForm : Form
{
    // Los diálogos ponen esto en false dentro de su constructor.
    protected bool AbrirMaximizado { get; set; } = true;

    public BaseForm()
    {
        InitializeComponent();

        AutoScaleMode = AutoScaleMode.Dpi;
        MinimumSize = Theme.TamanoMinimo;      // 850 x 500; NO se define MaximumSize
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Theme.DeepCharcoal;
        ForeColor = Theme.ChalkWhite;
        Font = Theme.FuenteNormal;
        DoubleBuffered = true;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;

        WindowState = AbrirMaximizado ? FormWindowState.Maximized : FormWindowState.Normal;
        Theme.Aplicar(this);
    }
}
```

Para el diseñador de Visual Studio y para el código actual:

- Crear `BaseForm` como **Formulario (Windows Forms)** para que tenga `InitializeComponent()`.
- **Compilar (Ctrl+Shift+B)** antes de crear un "Formulario heredado" de `BaseForm`. Si el diseñador muestra un error, casi siempre se arregla compilando.
- Para convertir un formulario existente: cambiar `: Form` por `: BaseForm` en el archivo `.cs` **y** en el `.Designer.cs`, y agregar `using SistemaStock.Estilos;`.
- **No es suficiente** cambiar `: Form` por `: UserControl` para convertir una página: hay que revisar el `.Designer.cs` (un `UserControl` no tiene `Text`, `FormBorderStyle`, `StartPosition`, `AcceptButton`, etc.). Por eso es mejor crear el `UserControl` nuevo (Agregar > Control de usuario) y pasar los controles uno por uno.

### Formularios/FrmPrincipal.cs (arquitectura futura)

Es el único marco de la app. Diseñador:

- `pnlSidebar`: `Dock = Left`, `Width = 220`.
- `pnlContenido`: `Dock = Fill`. Debe quedar **al fondo del orden Z**: clic derecho > "Enviar al fondo". Si no, tapa al sidebar.
- Dentro de `pnlSidebar`: botones (`btnVentas`, `btnProductos`, `btnUsuarios`...) con `Dock = Top`.

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaStock.Estilos;

namespace SistemaStock.Formularios;

public partial class FrmPrincipal : BaseForm
{
    private Button? _botonActivo;

    public FrmPrincipal()
    {
        InitializeComponent();

        pnlSidebar.BackColor = Theme.ObsidianBlack;
        pnlContenido.BackColor = Theme.DeepCharcoal;

        foreach (Control c in pnlSidebar.Controls)
            if (c is Button b) EstilizarItemMenu(b);

        btnProductos.Click += (s, e) => Navegar(btnProductos, () => new UcProductos());
        // Agregar una línea por cada sección nueva.
    }

    private static void EstilizarItemMenu(Button b)
    {
        b.Dock = DockStyle.Top;
        b.Height = 48;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Theme.DeepCharcoal;
        b.BackColor = Theme.ObsidianBlack;
        b.ForeColor = Theme.ZincGrey;               // inactivo
        b.Font = Theme.FuenteMenu;
        b.TextAlign = ContentAlignment.MiddleLeft;
        b.Padding = new Padding(16, 0, 0, 0);
        b.Cursor = Cursors.Hand;
    }

    // Recibe una función que crea la página, así solo se crea al hacer clic.
    private void Navegar(Button boton, Func<UserControl> crearPagina)
    {
        if (_botonActivo == boton) return;          // ya está en esa página

        if (_botonActivo != null) _botonActivo.ForeColor = Theme.ZincGrey;
        _botonActivo = boton;
        boton.ForeColor = Theme.BlazeOrange;        // activo

        // Liberar la página anterior (Controls.Clear solo NO libera memoria)
        var anteriores = new Control[pnlContenido.Controls.Count];
        pnlContenido.Controls.CopyTo(anteriores, 0);
        pnlContenido.Controls.Clear();
        foreach (var c in anteriores) c.Dispose();

        var pagina = crearPagina();
        pagina.Dock = DockStyle.Fill;
        pagina.BackColor = Theme.DeepCharcoal;
        pnlContenido.Controls.Add(pagina);
        Theme.Aplicar(pagina);
    }
}
```

Esto significa que al cambiar de sección **se pierden** filtros y búsquedas de la página anterior. Para el parcial está bien; si se necesita conservarlos, se decide caso por caso.

Si ya existe una navegación con `PaginaApp` o `BaseFormSidebar` que funciona, **no reemplazarla** sin avisar: mostrar la diferencia y preguntar.

### Página interna (UserControl)

Crear con Agregar > Control de usuario. Diseñador: un `TableLayoutPanel` (`Dock = Fill`) con filas en **porcentaje o AutoSize** (título arriba, buscador + botones, grilla en el centro `Dock = Fill`).

```csharp
using System;
using System.Windows.Forms;
using SistemaStock.Datos;
using SistemaStock.Estilos;

namespace SistemaStock.Formularios;

public partial class UcProductos : UserControl
{
    public UcProductos()
    {
        InitializeComponent();
        Theme.EstilizarBotonPrimario(btnNuevo);
        Load += (s, e) => { if (!DesignMode) CargarGrilla(); };
    }

    private void CargarGrilla()
    {
        try
        {
            dgvProductos.DataSource = ProductoRepositorio.Listar(txtBuscar.Text.Trim());
        }
        catch (Exception)
        {
            MessageBox.Show("No se pudieron cargar los productos. Revisá la conexión a la base de datos.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

Un `UserControl` **no** define `WindowState`, `FormBorderStyle` ni `MinimumSize` de ventana: se adapta al contenedor.

### Diálogo (Form que hereda de BaseForm)

```csharp
public partial class FrmProductoEditar : BaseForm
{
    public FrmProductoEditar()
    {
        InitializeComponent();
        AbrirMaximizado = false;                    // los diálogos no se maximizan
        StartPosition = FormStartPosition.CenterParent;
    }
}

// Para abrirlo, desde la página:
using var dlg = new FrmProductoEditar();
if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
    CargarGrilla();
```

`ShowDialog()` es solo para diálogos, **nunca** para navegar entre secciones del menú.

## Pantallas por módulo

Cada módulo es una página `UcXxx` del sidebar (`UcProductos`, `UcIngredientes`, `UcRecetas`, `UcMesas`, `UcVentas`, `UcReportes`); las altas y ediciones van en diálogos que heredan de `BaseForm`. En todas: validar en la pantalla solo lo básico (campos vacíos, números válidos) y dejar las reglas de negocio (stock, estados) en los repositorios (`codigo-csharp.md`).

- **Recetas**: permite elegir un producto `POR_RECETA` y agregar o quitar filas ingrediente + cantidad.
- **Ventas de mesa**: muestra primero las mesas (libres u ocupadas según su venta `PENDIENTE`); al elegir una ocupada se abre su venta para agregar o quitar productos, o cerrarla.
- Las pantallas que capturan un `InvalidOperationException` de un repositorio muestran un mensaje claro al usuario (ej. "No hay stock suficiente de ...").

## Login y cierre de sesión

- Login actual → si es válido, se abre el formulario correspondiente del dashboard y se pasa el usuario autenticado a los formularios que lo necesitan.
- Arquitectura futura → el rol (`Admin`, `Cajero`, `Mozo` o el valor definitivo que confirme el equipo) decidirá qué botones del sidebar se ven en `FrmPrincipal`, no en cada página.
- **Antes de tocar `Program.cs` o el flujo de login/cierre de sesión, pedir el archivo y leerlo.** No asumir cómo está hecho.
- Cerrar con la X debe terminar la app, sin volver a abrir el Login. Cerrar sesión debe volver al Login sin dejar ventanas ocultas.

## Layout responsive: checklist por pantalla

- Título arriba (`Dock = Top`), contenido al centro (`Dock = Fill`), botones de acción abajo en `FlowLayoutPanel` (`Dock = Bottom`), o arriba junto al buscador.
- Botones sueltos en una esquina: `Anchor = Bottom | Right`.
- Orden de agregado/Z importa: lo que tiene `Dock = Fill` va **al fondo**.
- Si algo se corta a 850x500: arreglar con Dock/Anchor o `AutoScroll = true` en el panel, antes de achicar fuentes.
- Probar **siempre** dos tamaños: 850x500 y maximizado.
- Grillas: `AutoSizeColumnsMode.Fill` por defecto (viene en `Theme`); si hay muchas columnas, cambiar a scroll horizontal.
