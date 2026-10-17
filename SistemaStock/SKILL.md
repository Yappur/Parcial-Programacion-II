---
name: winforms-baseform
description: Guía y plantillas para construir formularios de C# Windows Forms (Visual Studio, .NET) con MySQL que comparten el mismo estilo mediante un BaseForm, una clase Theme con la paleta (Obsidian Black, Deep Charcoal, Blaze Orange, Hazard Yellow, Chalk White, Zinc Grey) y un menú lateral (sidebar). Usar SIEMPRE que el usuario trabaje en el sistema de stock y ventas del restaurante Chachín, cree o modifique un formulario, botón, DataGridView, panel o menú en Windows Forms, hable de tamaño mínimo/máximo de ventana, estilos, colores, diseño responsive (Dock/Anchor), o pida código de C# con WinForms, aunque no mencione BaseForm ni "skill".
---

# Windows Forms + MySQL: BaseForm, Theme y Sidebar (sistema Chachín)

El usuario es principiante en C#/WinForms y trabaja en equipo de tres con Git. Explicá de a poco, en español, y con código que compile. Nunca repitas estilos en cada formulario: todo lo visual vive en `Theme` y `BaseForm`.

## Reglas de oro

1. **Todo formulario hereda de `BaseForm`** (`public partial class FrmVentas : BaseForm`). Nunca de `Form` directamente.
2. **Ningún color, fuente o tamaño suelto** en los formularios. Siempre `Theme.Algo`. Si falta un valor, se agrega a `Theme`.
3. **Tamaño**: mínimo `850 x 500` (lo define `BaseForm`), máximo = pantalla completa. No se setea `MaximumSize`. Arranca en `WindowState.Maximized`.
4. **Responsive**: se usa `Dock`, `Anchor`, `TableLayoutPanel` y `FlowLayoutPanel`. Prohibido posicionar controles con píxeles fijos (`Location`) salvo dentro de un panel pequeño y fijo.
5. **Fondo**: `Deep Charcoal` en todos los formularios y paneles de contenido. El **sidebar** va en `Obsidian Black`.
6. **La conexión a MySQL no va en los formularios.** Va en una clase aparte (`Datos/Conexion.cs`) y las consultas son **parametrizadas** (nunca concatenar texto del usuario en el SQL).

## Paleta

| Nombre | HEX | Uso |
|---|---|---|
| Obsidian Black | #0C0C0E | Sidebar, estructura base |
| Deep Charcoal | #1F2024 | Fondo de formularios, tarjetas, paneles |
| Blaze Orange | #FF5500 | Acento primario, botones de acción (CTA), ítem activo del menú |
| Hazard Yellow | #FFD000 | Acento secundario, etiquetas, resaltados, advertencias |
| Chalk White | #F4F4F6 | Texto principal |
| Zinc Grey | #8E929C | Texto secundario, divisores, íconos inactivos |

## Estructura de carpetas

```
ChachinApp/
├── Estilos/        Theme.cs, BaseForm.cs, BotonSidebar.cs
├── Formularios/    FrmPrincipal.cs, FrmVentas.cs, FrmStock.cs ...
├── Datos/          Conexion.cs, ProductoRepositorio.cs ...
└── Modelos/        Producto.cs, Venta.cs ...
```

Nombres: formularios `FrmXxx`, botones `btnXxx`, etiquetas `lblXxx`, cajas de texto `txtXxx`, grillas `dgvXxx`, paneles `pnlXxx`.

## Plantilla 1: Theme.cs

```csharp
using System.Drawing;
using System.Windows.Forms;

public static class Theme
{
    // Paleta
    public static readonly Color ObsidianBlack = ColorTranslator.FromHtml("#0C0C0E");
    public static readonly Color DeepCharcoal  = ColorTranslator.FromHtml("#1F2024");
    public static readonly Color BlazeOrange   = ColorTranslator.FromHtml("#FF5500");
    public static readonly Color HazardYellow  = ColorTranslator.FromHtml("#FFD000");
    public static readonly Color ChalkWhite    = ColorTranslator.FromHtml("#F4F4F6");
    public static readonly Color ZincGrey      = ColorTranslator.FromHtml("#8E929C");

    // Tamaños de ventana
    public static readonly Size TamanoMinimo = new Size(850, 500);

    // Fuentes
    public static readonly Font FuenteNormal = new Font("Segoe UI", 10f);
    public static readonly Font FuenteTitulo = new Font("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font FuenteMenu   = new Font("Segoe UI Semibold", 10.5f);

    // Botón de acción principal (CTA)
    public static void EstilizarBotonPrimario(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = BlazeOrange;
        b.ForeColor = ChalkWhite;
        b.Font = FuenteNormal;
        b.Cursor = Cursors.Hand;
    }

    // Botón secundario (borde gris, sin relleno)
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

    // Caja de texto
    public static void EstilizarTextBox(TextBox t)
    {
        t.BackColor = ObsidianBlack;
        t.ForeColor = ChalkWhite;
        t.BorderStyle = BorderStyle.FixedSingle;
        t.Font = FuenteNormal;
    }

    // Grilla de datos
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

    // Aplica estilo a todos los controles de un contenedor (recursivo)
    public static void Aplicar(Control raiz)
    {
        foreach (Control c in raiz.Controls)
        {
            switch (c)
            {
                case DataGridView g: EstilizarGrilla(g); break;
                case TextBox t: EstilizarTextBox(t); break;
                case Label l: l.ForeColor = ChalkWhite; l.Font = FuenteNormal; break;
            }
            if (c.HasChildren) Aplicar(c);
        }
    }
}
```

Los botones se estilizan a mano (`Theme.EstilizarBotonPrimario(btnGuardar)`) porque no todos son CTA.

## Plantilla 2: BaseForm.cs

```csharp
using System;
using System.Windows.Forms;

public partial class BaseForm : Form
{
    public BaseForm()
    {
        InitializeComponent();

        AutoScaleMode = AutoScaleMode.Dpi;       // se ve igual en distintas pantallas/escala
        MinimumSize = Theme.TamanoMinimo;        // 850 x 500: no se achica más
        // NO se define MaximumSize: el máximo es la pantalla completa
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        BackColor = Theme.DeepCharcoal;
        ForeColor = Theme.ChalkWhite;
        Font = Theme.FuenteNormal;
        DoubleBuffered = true;                   // menos parpadeo al redimensionar
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode) Theme.Aplicar(this);    // estiliza labels, textbox y grillas
    }
}
```

Importante para el diseñador de Visual Studio: **compilar (Ctrl+Shift+B)** antes de crear un formulario heredado. Si el diseñador falla en `BaseForm`, revisar que `InitializeComponent()` exista (si `BaseForm` se creó como "Formulario", lo tiene).

Para agregar un formulario nuevo: clic derecho en la carpeta > Agregar > Formulario heredado (o crear un Form normal y cambiar `: Form` por `: BaseForm` en ambos archivos `.cs` y `.Designer.cs` si hiciera falta).

## Plantilla 3: Sidebar (FrmPrincipal)

`FrmPrincipal : BaseForm` es el "marco" de la aplicación: sidebar a la izquierda, contenido a la derecha. Las pantallas (Ventas, Stock, etc.) se cargan dentro de `pnlContenido`.

Controles en el diseñador:
- `pnlSidebar`: `Dock = Left`, `Width = 220`, `BackColor = Theme.ObsidianBlack`.
- `pnlContenido`: `Dock = Fill`, `BackColor = Theme.DeepCharcoal`. **Agregar `pnlContenido` primero o ponerlo "Enviar al fondo"** para que el Dock Fill respete el sidebar.
- Dentro de `pnlSidebar`: un `Label lblLogo` arriba (`Dock = Top`) y los botones del menú (`Dock = Top`, alto 48).

```csharp
public partial class FrmPrincipal : BaseForm
{
    private Button _botonActivo;

    public FrmPrincipal()
    {
        InitializeComponent();
        pnlSidebar.BackColor = Theme.ObsidianBlack;
        pnlContenido.BackColor = Theme.DeepCharcoal;

        foreach (Control c in pnlSidebar.Controls)
            if (c is Button b) EstilizarItemMenu(b);

        btnVentas.Click += (s, e) => Navegar(btnVentas, new UcVentas());
        btnStock.Click  += (s, e) => Navegar(btnStock,  new UcStock());
    }

    private void EstilizarItemMenu(Button b)
    {
        b.Dock = DockStyle.Top;
        b.Height = 48;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Theme.DeepCharcoal;
        b.BackColor = Theme.ObsidianBlack;
        b.ForeColor = Theme.ZincGrey;          // inactivo
        b.Font = Theme.FuenteMenu;
        b.TextAlign = ContentAlignment.MiddleLeft;
        b.Padding = new Padding(16, 0, 0, 0);
        b.Cursor = Cursors.Hand;
    }

    private void Navegar(Button boton, UserControl pantalla)
    {
        if (_botonActivo != null) _botonActivo.ForeColor = Theme.ZincGrey;
        _botonActivo = boton;
        boton.ForeColor = Theme.BlazeOrange;   // activo

        pnlContenido.Controls.Clear();
        pantalla.Dock = DockStyle.Fill;
        pantalla.BackColor = Theme.DeepCharcoal;
        pnlContenido.Controls.Add(pantalla);
        Theme.Aplicar(pantalla);
    }
}
```

Las pantallas internas (`UcVentas`, `UcStock`) son **UserControl** (no Form), porque un menú lateral con Forms embebidos da problemas de tamaño y foco. Los formularios secundarios (ej. diálogo "Nuevo producto") sí heredan de `BaseForm`; en esos casos poner `WindowState = Normal`, `StartPosition = CenterParent` y `ShowDialog(this)`.

## Layout responsive: cómo armar una pantalla

- Estructura: `TableLayoutPanel` con filas/columnas en **porcentaje** (`SizeType.Percent`), no en píxeles.
- Título arriba (`Dock = Top`), grilla en el centro (`Dock = Fill`), botones abajo en un `FlowLayoutPanel` (`Dock = Bottom`).
- Botones sueltos que deban quedar en una esquina: `Anchor = Bottom | Right`.
- Probar siempre dos tamaños: el mínimo (850x500) y pantalla completa. Si algo se corta o se superpone en 850x500, corregir con Dock/Anchor o `AutoScroll = true` en el panel contenedor.

## MySQL (resumen)

- Paquete NuGet: `MySql.Data` (o `MySqlConnector`).
- Cadena de conexión en `App.config` (no en el código ni subida con contraseñas reales a GitHub; usar un `App.config.example`).
- Consultas con parámetros:

```csharp
using (var cn = Conexion.Obtener())
using (var cmd = new MySqlCommand("SELECT * FROM productos WHERE id = @id", cn))
{
    cmd.Parameters.AddWithValue("@id", id);
    cn.Open();
    using (var rd = cmd.ExecuteReader()) { /* leer */ }
}
```

## Cómo trabajar con el usuario

1. Si pide un formulario nuevo: dar primero la lista de controles del diseñador (nombre, tipo, Dock/Anchor) y luego el código.
2. Si pide un cambio de estilo: modificarlo en `Theme.cs`, no en cada formulario.
3. Si algo del diseñador de Visual Studio puede confundir (compilar antes, orden de Dock, formulario heredado), decirlo explícitamente.
4. Explicar el *por qué* en una o dos frases; es un parcial que tiene que defender.
5. Antes de cerrar, revisar la lista: ¿hereda de BaseForm? ¿usa Theme? ¿se ve bien a 850x500 y a pantalla completa? ¿sin SQL concatenado?
