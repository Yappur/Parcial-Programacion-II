# UI en WinForms: estilo, componentes y layout

Todo lo visual del sistema Chachín. Las reglas de oro están en `../SKILL.md`, sección 4. Los repositorios que llaman estas pantallas están en `codigo-csharp.md`.

## Arquitectura real (no inventar otra)

- `Estilos/Theme.cs`: **única fuente** de colores, fuentes y estilos de controles.
- `Estilos/BaseForm.cs`: formulario base (tamaño mínimo, tema). Los diálogos heredan de él.
- `Estilos/BaseFormSidebar.cs`: ventana principal única. Construye el sidebar **por código**, tiene el enum `PaginaApp` y el método `Navegar(PaginaApp)` que crea la página, la acopla a `PageContent` y libera la anterior. También define `BasePage` (`UserControl` con título y `AddEmptyGrid()`).
- Las pantallas (`FrmDashboard`, `FrmProductos`, `FrmIngredientes`, `FrmRecetas`, `FrmMesas`, `FrmUsers`, `FrmVentas`, `FrmHistorialVentas`) son **`UserControl`** (heredan de `BasePage` o de `UserControl`), aunque se llamen `Frm...`. Se muestran dentro de `PageContent`, nunca con `ShowDialog()`.
- Las altas y ediciones son diálogos (`ProductoEditForm`, `UserEditForm`, etc.) que heredan de `BaseForm`. `ShowDialog()` es solo para ellos.
- Para agregar una pantalla: sumar un valor a `PaginaApp`, un botón en `ConstruirSidebar()` y un caso en el `switch` de `Navegar`.

## Reglas de oro

1. **Nada de colores, fuentes ni medidas sueltas en los formularios.** Todo sale de `Theme`. Si falta algo, se agrega a `Theme`.
2. **No duplicar ni reemplazar archivos existentes.** Conservar nombres y propiedades que ya usan los formularios (`ApplyForm`, `StyleNavigationButton`, `EstilizarBoton`, `StyleGrid`, `BodyFont`, `HeadingFont`, etc.). Si se renombra algo, actualizar todos los usos en el mismo cambio.
3. **Probar siempre a 850x500 y maximizado.** Layout con `Dock`, `Anchor`, `TableLayoutPanel` y `FlowLayoutPanel`; sin posiciones ni tamaños fijos (salvo alto/ancho de botones e iconos).
4. **Sin SQL ni reglas de negocio en la UI.** Los datos vienen de los repositorios.
5. **Un solo lenguaje visual:** si un control se ve distinto al resto, se arregla en `Theme`, no en la pantalla.
6. **Antes de tocar `Program.cs` o el flujo de login, pedir el archivo.**

## Paleta (Theme.cs)

| Nombre | RGB | Uso |
|---|---|---|
| ObsidianBlack | 17, 18, 20 | Sidebar, encabezados de grilla |
| DeepCharcoal | 24, 25, 28 | Fondo de formularios y páginas |
| Surface | 32, 34, 38 | Fondo de grillas, inputs y tarjetas |
| SurfaceRaised | 43, 45, 50 | Hover, elementos elevados |
| BlazeOrange | 242, 112, 42 | Acción principal e ítem activo |
| TextPrimary | 245, 245, 245 | Texto principal |
| TextSecondary | 185, 188, 194 | Texto secundario y deshabilitado |
| GridLine | 58, 60, 66 | Bordes sutiles y líneas |

Agregar a `Theme` si faltan: `Danger` (rojo para eliminar/alertas, ej. 229, 72, 77) y `Warning` (ámbar para stock bajo, ej. 245, 166, 35).

## Reglas de diseño

- **Escala de espacio de 8 px:** márgenes y paddings de 8, 16, 24, 32.
- **Fuente:** Segoe UI. Cuerpo 10, subtítulos 12 en negrita, títulos 22 en negrita. Cachear las fuentes en campos `static readonly`; no crear `new Font(...)` en cada llamada.
- **Bordes:** nunca blancos. Los bordes son `GridLine` (sutiles) o `BlazeOrange` (foco/acción). El blanco (`TextPrimary`) es solo para texto e iconos.
- **Una sola acción primaria por pantalla** (botón naranja). El resto son secundarios.
- **Altura estándar:** botones e inputs 36–40 px; ítems del sidebar 48 px.

## Componentes (todos se estilizan desde Theme)

### Botones
- **Primario:** fondo `BlazeOrange`, texto `TextPrimary`, sin borde (`BorderSize = 0`), hover un tono más claro/oscuro.
- **Secundario:** fondo `Surface`, borde 1 px `GridLine`, hover `SurfaceRaised`. **Sin borde blanco.**
- **Peligro:** fondo `Danger` o borde `Danger` con texto `Danger`.
- **Deshabilitado:** texto `TextSecondary`, sin hover.
- `EstilizarBoton` no debe suscribirse a `EnabledChanged` cada vez que se llama (evitar eventos duplicados).
- Con icono: usar `IconButton` (FontAwesome.Sharp), ver Iconos.

### Sidebar
- Ítems con `FlatAppearance.BorderSize = 0`. **Sin borde blanco.**
- Inactivo: fondo `ObsidianBlack`, texto e icono `TextSecondary`.
- Hover: fondo `SurfaceRaised`, texto e icono `TextPrimary`.
- Activo: fondo `BlazeOrange` (o fondo `Surface` con barra naranja de 4 px a la izquierda), texto e icono `TextPrimary`, fuente en negrita.
- Icono a la izquierda, texto alineado a la izquierda, padding izquierdo 16–20 px.
- "Cerrar sesión": estilo secundario o de peligro, con icono; sin borde blanco.
- La navegación y el enum `PaginaApp` no se tocan al cambiar el estilo.

### Iconos (FontAwesome.Sharp)
- Se usan con `IconButton` (botones) o `IconPictureBox` (decoración); no con imágenes sueltas.
- Propiedades: `IconChar`, `IconColor` (de `Theme`), `IconSize` (20–24 en sidebar, 16–18 en botones), `TextImageRelation = ImageBeforeText`, `ImageAlign = MiddleLeft`.
- Mapa sugerido: Inicio `House`, Ventas `CashRegister`, Productos `BoxesStacked`, Ingredientes `Carrot`, Recetas `BookOpen`, Mesas `Chair`, Usuarios `Users`, Historial `ClockRotateLeft`, Cerrar sesión `RightFromBracket`; Nuevo `Plus`, Editar `Pen`, Eliminar `Trash`, Buscar `MagnifyingGlass`, Guardar `FloppyDisk`. Verificar que cada `IconChar` exista en la versión instalada.
- El estilo recursivo de `Theme` **no debe pisar** `IconButton`.

### TextBox
- Fondo `Surface`, texto `TextPrimary`, fuente `BodyFont`, altura ~36 px.
- El borde nativo no se puede colorear: usar `BorderStyle.None` dentro de un panel con borde `GridLine` que cambia a `BlazeOrange` al recibir foco (o `MaterialTextBox2`). Dejar un helper en `Theme` para no repetir el patrón.
- Placeholder con `PlaceholderText` (color gris por defecto) en lugar de etiquetas encima.

### ComboBox
- `DropDownStyle = DropDownList`, `FlatStyle = Flat`, fondo `Surface`, texto `TextPrimary`.
- Para que la lista desplegable y el ítem seleccionado respeten el tema, usar `DrawMode = OwnerDrawFixed` con `DrawItem` (seleccionado `BlazeOrange` suave, resto `Surface`), o `MaterialComboBox`.
- Misma altura y fuente que los TextBox.

### ListView / listas
- Para listas chicas (últimas ventas, stock bajo): `ListView` en `View.Details` con `OwnerDraw = true` (encabezado `ObsidianBlack`, filas alternadas `Surface` / `SurfaceRaised`, selección naranja suave, sin bordes) o `MaterialListView`.
- Para tablas grandes con datos de la base: `DataGridView` con `Theme.StyleGrid`. No migrar las grillas a `ListView`.
- Siempre `DoubleBuffered` en listas y grillas para evitar parpadeo.

### Grillas
- `Theme.StyleGrid` (ya implementado): encabezado `ObsidianBlack`, filas de 36 px, sin bordes de celda, selección naranja suave. Se puede sumar filas alternadas y resaltar con `Warning`/`Danger` el stock bajo.

## MaterialSkin.2: cómo se usa en este proyecto

- **No** usar `MaterialForm`. Se conservan `BaseForm` y `BaseFormSidebar`.
- Usar solo controles sueltos (`MaterialTextBox2`, `MaterialComboBox`, `MaterialListView`, etc.) dentro de las páginas.
- `MaterialSkinManager.Instance` se configura **una sola vez** (en `Theme` o al inicio de la app): tema `DARK` y `ColorScheme` armado con los colores de `Theme`.
- Si un control Material no queda armonioso con el resto, se vuelve al control nativo estilizado desde `Theme`. Primero se prueba en **una** pantalla antes de aplicarlo a todas.

## Diseñador de Visual Studio

- Para ver el Cuadro de herramientas hay que abrir el **diseñador** de una pantalla (`UserControl` o `Form` con `.Designer.cs`): doble clic en el archivo y Ver diseñador (Mayús+F7). `BaseFormSidebar` se construye por código y no tiene superficie de diseño.
- Después de instalar paquetes: compilar (Ctrl+Mayús+B). Si los controles no aparecen: Cuadro de herramientas → clic derecho → *Restablecer* o *Elegir elementos*; si sigue igual, reiniciar Visual Studio.
- Los controles de librerías (`IconButton`, `Material*`) se pueden crear **por código**, sin depender del Cuadro de herramientas.

## Layout: checklist por pantalla

- Título arriba, contenido al centro (`Dock = Fill`), acciones abajo o junto al buscador.
- Lo que tiene `Dock = Fill` va al fondo del orden Z.
- Botones sueltos en una esquina: `Anchor = Bottom | Right`.
- Si algo se corta a 850x500: arreglar con Dock/Anchor o `AutoScroll` antes de achicar fuentes.
- Probar dos tamaños: 850x500 y maximizado.

## Login y cierre de sesión

- Cerrar con la X termina la app; cerrar sesión vuelve al Login sin dejar ventanas ocultas (`DialogResult.Retry`).
- El rol decide qué botones del sidebar se ven (ej. `UserPermissions.CanManageUsers` para Historial de ventas).

## Plantillas de referencia (adaptar a lo existente; NO pegar encima de los archivos actuales)

Son patrones para seguir. Si difieren del código real, **manda el código real**: se integran los métodos nuevos a `Theme.cs` conservando los existentes. Compilar y probar cada pieza antes de aplicarla a todas las pantallas.

### Theme.cs: colores y botones

```csharp
using System.Reflection;
using FontAwesome.Sharp;

// Colores nuevos
public static readonly Color Danger = Color.FromArgb(229, 72, 77);
public static readonly Color Warning = Color.FromArgb(245, 166, 35);
public static readonly Color BlazeOrangeHover = Color.FromArgb(255, 133, 64);
public static readonly Color SelectionSoft = Color.FromArgb(74, 52, 40);
private static readonly Font BodyFontBold = new("Segoe UI", 10F, FontStyle.Bold);

public static void EstilizarBotonPrimario(Button b)
{
    b.FlatStyle = FlatStyle.Flat;
    b.UseVisualStyleBackColor = false;
    b.FlatAppearance.BorderSize = 0;
    b.FlatAppearance.MouseOverBackColor = BlazeOrangeHover;
    b.FlatAppearance.MouseDownBackColor = BlazeOrange;
    b.BackColor = BlazeOrange;
    b.ForeColor = TextPrimary;
    b.Font = BodyFontBold;
    b.Cursor = Cursors.Hand;
    b.Height = 38;
}

// Reemplaza el cuerpo de EstilizarBoton (secundario) conservando el nombre
public static void EstilizarBoton(Button b)
{
    b.FlatStyle = FlatStyle.Flat;
    b.UseVisualStyleBackColor = false;
    b.FlatAppearance.BorderSize = 1;
    b.FlatAppearance.BorderColor = GridLine;          // nunca blanco
    b.FlatAppearance.MouseOverBackColor = SurfaceRaised;
    b.FlatAppearance.MouseDownBackColor = SurfaceRaised;
    b.BackColor = Surface;
    b.Font = BodyFont;
    b.Cursor = Cursors.Hand;
    b.Height = 38;
    ActualizarEstadoBoton(b);
    b.EnabledChanged -= BotonEnabledChanged;          // evita duplicar el evento
    b.EnabledChanged += BotonEnabledChanged;
}

private static void BotonEnabledChanged(object? s, EventArgs e)
{
    if (s is Button b) ActualizarEstadoBoton(b);
}

private static void ActualizarEstadoBoton(Button b)
{
    b.ForeColor = b.Enabled ? TextPrimary : TextSecondary;
    if (b is IconButton ib) ib.IconColor = b.ForeColor;
}

public static void EstilizarBotonPeligro(Button b)
{
    EstilizarBoton(b);
    b.FlatAppearance.BorderColor = Danger;
    b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 30, 32);
}
```

### Theme.cs: ítem del sidebar (con y sin icono)

```csharp
public static void StyleNavigationButton(Button button, bool active)
{
    var fore = active ? TextPrimary : TextSecondary;
    button.FlatStyle = FlatStyle.Flat;
    button.UseVisualStyleBackColor = false;
    button.FlatAppearance.BorderSize = 0;             // sin borde blanco
    button.FlatAppearance.MouseOverBackColor = active ? BlazeOrange : SurfaceRaised;
    button.FlatAppearance.MouseDownBackColor = active ? BlazeOrange : SurfaceRaised;
    button.BackColor = active ? BlazeOrange : ObsidianBlack;
    button.ForeColor = fore;
    button.Font = active ? ActiveNavigationFont : NavigationFont;
    button.TextAlign = ContentAlignment.MiddleLeft;
    button.Padding = new Padding(16, 0, 8, 0);
    button.Cursor = Cursors.Hand;

    if (button is IconButton icon)
    {
        icon.IconColor = fore;
        icon.IconSize = 22;
        icon.TextImageRelation = TextImageRelation.ImageBeforeText;
        icon.ImageAlign = ContentAlignment.MiddleLeft;
    }
}
```

En `BaseFormSidebar`, `CrearBoton` recibe también el icono y crea un `IconButton`:

```csharp
private Button CrearBoton(string texto, PaginaApp destino, IconChar icono)
{
    var boton = new IconButton
    {
        Text = "  " + texto, IconChar = icono,
        Width = 236, Height = 48, Margin = new Padding(0, 0, 0, 8)
    };
    Theme.StyleNavigationButton(boton, false);
    boton.Click += (_, _) => Navegar(destino);
    botones.Add(destino, boton);
    return boton;
}
// uso: navegacion.Controls.Add(CrearBoton("Ventas", PaginaApp.Ventas, IconChar.CashRegister));
```

### Estilos/CampoTexto.cs: TextBox con borde que cambia al enfocar

El borde nativo del `TextBox` no se puede colorear, por eso se envuelve en un panel.

```csharp
namespace SistemaStock.Estilos;

public class CampoTexto : Panel
{
    public TextBox Input { get; } = new();
    private bool enfocado;

    public CampoTexto()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        Height = 38;
        Padding = new Padding(10, 9, 10, 4);
        BackColor = Theme.Surface;

        Input.BorderStyle = BorderStyle.None;
        Input.BackColor = Theme.Surface;
        Input.ForeColor = Theme.TextPrimary;
        Input.Font = Theme.BodyFont;
        Input.Dock = DockStyle.Fill;
        Input.Enter += (_, _) => { enfocado = true; Invalidate(); };
        Input.Leave += (_, _) => { enfocado = false; Invalidate(); };
        Controls.Add(Input);
    }

    public override string Text { get => Input.Text; set => Input.Text = value; }
    public string PlaceholderText { get => Input.PlaceholderText; set => Input.PlaceholderText = value; }
    public bool PasswordMode { get => Input.UseSystemPasswordChar; set => Input.UseSystemPasswordChar = value; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(enfocado ? Theme.BlazeOrange : Theme.GridLine);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
```

Si una pantalla ya usa un `TextBox` con nombre (`txtBuscar`), reemplazarlo por `CampoTexto` y leer `.Text`; revisar los usos de `TextChanged`/`KeyDown` (ahora van en `.Input`).

### Theme.cs: ComboBox

```csharp
public static void EstilizarCombo(ComboBox c)
{
    c.DropDownStyle = ComboBoxStyle.DropDownList;
    c.FlatStyle = FlatStyle.Flat;
    c.BackColor = Surface;
    c.ForeColor = TextPrimary;
    c.Font = BodyFont;
    c.DrawMode = DrawMode.OwnerDrawFixed;
    c.ItemHeight = 28;
    c.DrawItem -= DibujarItemCombo;
    c.DrawItem += DibujarItemCombo;
}

private static void DibujarItemCombo(object? sender, DrawItemEventArgs e)
{
    if (sender is not ComboBox c || e.Index < 0) return;
    bool seleccionado = (e.State & DrawItemState.Selected) != 0;
    using var fondo = new SolidBrush(seleccionado ? SelectionSoft : Surface);
    e.Graphics.FillRectangle(fondo, e.Bounds);
    var area = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
    TextRenderer.DrawText(e.Graphics, c.GetItemText(c.Items[e.Index]), BodyFont, area, TextPrimary,
        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
}
```

### Theme.cs: ListView para listas chicas

```csharp
public static void ActivarDobleBuffer(Control c) =>
    typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
        ?.SetValue(c, true);

public static void EstilizarLista(ListView lv)
{
    lv.View = View.Details;
    lv.FullRowSelect = true;
    lv.MultiSelect = false;
    lv.BorderStyle = BorderStyle.None;
    lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
    lv.OwnerDraw = true;
    lv.BackColor = Surface;
    lv.ForeColor = TextPrimary;
    lv.Font = BodyFont;
    // Alto de fila: un ImageList de 1x36 px fuerza la altura
    lv.SmallImageList = new ImageList { ImageSize = new Size(1, 36) };
    ActivarDobleBuffer(lv);

    lv.DrawColumnHeader += (_, e) =>
    {
        using var fondo = new SolidBrush(ObsidianBlack);
        e.Graphics.FillRectangle(fondo, e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.Header?.Text, BodyFontBold,
            Rectangle.Inflate(e.Bounds, -8, 0), TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    };
    lv.DrawSubItem += (_, e) =>
    {
        bool sel = e.Item?.Selected == true;
        var color = sel ? SelectionSoft : (e.ItemIndex % 2 == 0 ? Surface : SurfaceRaised);
        using var fondo = new SolidBrush(color);
        e.Graphics.FillRectangle(fondo, e.Bounds);
        TextRenderer.DrawText(e.Graphics, e.SubItem?.Text, BodyFont,
            Rectangle.Inflate(e.Bounds, -8, 0), TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    };
}
```

Si el `ListView` muestra un vacío blanco a la derecha de la última columna, ajustar el ancho de la última columna a `-2` (llenar el resto).

### Fila de stock bajo en DataGridView (opcional)

```csharp
grid.CellFormatting += (_, e) =>
{
    // adaptar el nombre real de las columnas y del valor mínimo
    var fila = grid.Rows[e.RowIndex];
    if (fila.Cells["Stock"].Value is int stock && fila.Cells["StockMinimo"].Value is int min && stock <= min)
        e.CellStyle.ForeColor = Theme.Warning;
};
```