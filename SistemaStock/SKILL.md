---
name: winforms-chachin
description: Guía y plantillas para el sistema de stock y ventas del restaurante Chachín, hecho en C# Windows Forms (Visual Studio, .NET) con MySQL y MySqlConnector. Cubre BaseForm, Theme (paleta), sidebar con UserControl, acceso a datos con la clase Database, layout responsive y reglas de trabajo en equipo con Git. Usar SIEMPRE que se cree, modifique o depure un formulario, UserControl, botón, DataGridView, panel, menú, consulta SQL, tabla de MySQL o estilo del sistema Chachín, aunque el usuario no mencione esta skill.
---

# Sistema Chachín: WinForms + MySQL

## 0. Quién es el usuario y cómo responder

El equipo (tres personas) es **principiante** en C# y Windows Forms. Es un parcial que hay que **defender**, así que importa entender cada cosa.

Reglas para la IA:

1. Responder en español, simple, y explicar el *por qué* en una o dos frases.
2. **Un cambio por vez.** No refactorizar cosas que no se pidieron.
3. Entregar **el archivo completo** (no fragmentos sueltos), diciendo el **nombre exacto del archivo** y en qué carpeta va.
4. Si hay que hacer algo en el **diseñador de Visual Studio** (arrastrar un control, cambiar Dock, propiedades), decirlo paso a paso, porque la IA no puede hacerlo por ellos.
5. **No inventar** clases, métodos, columnas ni namespaces. Si falta información, pedir el archivo o preguntar. Si no se está seguro de algo, decirlo.
6. **No se puede compilar en el chat.** Nunca decir "compila" o "probado" si no se ejecutó. En su lugar, cerrar cada respuesta con: "Qué revisar después de pegar esto" (compilar con Ctrl+Shift+B, probar a 850x500 y maximizado, probar la operación).
7. Si el código real del proyecto contradice esta skill, **gana el código real**: avisar la diferencia y preguntar antes de cambiar.

## 1. Estado actual del proyecto (verificado)

- Lenguaje/.NET: C# moderno (usa `namespace X;` y cadenas `"""`), por lo tanto **.NET 7 o superior** (probablemente .NET 8). Si hay duda, pedir que lo confirmen en Propiedades del proyecto.
- Paquete MySQL: **`MySqlConnector`** (ya en uso). No agregar `MySql.Data`.
- Namespace raíz: **`SistemaStock`** (ej. `SistemaStock.Datos`). Usar el mismo prefijo en todo: `SistemaStock.Estilos`, `SistemaStock.Formularios`, `SistemaStock.Modelos`.
- Carpetas reales:

```
Datos/         Database.cs (+ repositorios)
Estilos/       Theme.cs, BaseForm.cs
Formularios/   FrmLogin, FrmPrincipal, UcXxx, FrmXxx (diálogos)
Modelos/       Producto.cs, Usuario.cs ...
Recursos/      imágenes e íconos
Program.cs
SKILL.md
```

- Acceso a datos: clase `internal static class Database` con `OpenConnection()` e `Initialize()`. **No crear otra clase de conexión** (nada de `Conexion.cs`).
- Cadena de conexión: variable de entorno **`GESTION_VENTAS_CONNECTION`** (si no existe, usa `localhost`, base `gestion_ventas`, usuario `root`). **Nunca** subir contraseñas a GitHub.
- Base `gestion_ventas`, creada por `Database.Initialize()` con `CREATE ... IF NOT EXISTS`.

### Tablas que existen hoy

| Tabla | Columnas |
|---|---|
| `usuarios` | `id`, `nombre_usuario` (único), `nombre_completo`, `hash_contrasena`, `rol`, `activo`, `creado_en` |
| `categorias` | `id`, `nombre` (único). Semilla: Bebidas, Platos, Postres |
| `productos` | `codigo` (PK autoincremental), `nombre`, `precio` (>= 0), `stock` (>= 0), `activo`, `categoria_id` → `categorias(id)` |

**Usar exactamente estos nombres** (minúsculas, con guion bajo). Nota: `productos` usa `codigo`, no `id`.

### Tablas planificadas (NO crearlas salvo que el equipo lo pida)

`mesas`, `ventas` (cabecera) y `detalle_venta`. Ideas ya habladas, todavía sin definir:

- `mesas`: número, nombre, estado (Libre/Ocupada/Reservada), activa.
- `ventas`: fecha_hora, id_usuario, id_mesa (nulo = delivery/mostrador), tipo_venta (Mesa/Delivery/Mostrador), estado (Pendiente/Cerrada/Anulada), total, observaciones.
- `detalle_venta`: id_venta, codigo_producto, cantidad, **precio_unitario guardado al momento de la venta**, subtotal.
- Roles previstos: Admin, Cajero, Mozo.

Cuando se defina alguna, se agrega en `Database.Initialize()` con `CREATE TABLE IF NOT EXISTS`, **siguiendo el mismo estilo** (nombres en minúsculas con guion bajo, `ENGINE=InnoDB`, claves foráneas con nombre `fk_tabla_otra`). Si se renombra algo de `productos` (ej. `codigo` → `id`), avisar el impacto en todo el código antes.

## 2. Reglas de oro

1. **Una sola ventana principal** (`FrmPrincipal`) con sidebar fijo. Las secciones del menú son **`UserControl`** (`UcVentas`, `UcProductos`...), no formularios.
2. **Los diálogos** (Nuevo producto, Editar, Confirmar) son `Form` que heredan de **`BaseForm`**, y se abren con `ShowDialog(this)`.
3. **Nada de colores, fuentes o tamaños sueltos** en formularios: siempre `Theme.Algo`. Si falta algo, se agrega a `Theme.cs`.
4. **Layout responsive**: `Dock`, `Anchor`, `TableLayoutPanel`, `FlowLayoutPanel`. Prohibido armar pantallas con `Location` en píxeles (salvo dentro de un panel chico y fijo).
5. **SQL solo en `Datos/`** (repositorios), nunca dentro de los eventos de los botones. Siempre **parametrizado**: nunca `"... WHERE x = '" + texto + "'"`.
6. **Contraseñas**: se guardan con hash (`hash_contrasena`). Nunca texto plano. Mantener el mecanismo del Login actual; no cambiarlo sin avisar.
7. **No duplicar eventos**: si el `Click` ya se conectó en el diseñador, no volver a conectarlo en el constructor.

## 3. Paleta (centralizada en Theme.cs)

| Nombre | HEX | Uso |
|---|---|---|
| Obsidian Black | `#0C0C0E` | Sidebar, estructura base, fondo de cajas de texto |
| Deep Charcoal | `#1F2024` | Fondo de formularios, paneles, tarjetas |
| Blaze Orange | `#FF5500` | Acción principal, ítem activo del menú, selección |
| Hazard Yellow | `#FFD000` | Encabezados de tabla, etiquetas, advertencias |
| Chalk White | `#F4F4F6` | Texto principal |
| Zinc Grey | `#8E929C` | Texto secundario, bordes, ítems inactivos |

Ventana: mínimo **850 x 500**; sin `MaximumSize`; la ventana principal arranca maximizada.

## 4. Plantillas (adaptar a lo que ya exista, no duplicar)

Antes de pegar cualquier plantilla, **revisar si el archivo ya existe**. Si existe, agregar/ajustar solo lo que falta.

### 4.1. Estilos/Theme.cs

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

### 4.2. Estilos/BaseForm.cs

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

Para el diseñador de Visual Studio:
- Crear `BaseForm` como **Formulario (Windows Forms)** para que tenga `InitializeComponent()`.
- **Compilar (Ctrl+Shift+B)** antes de crear un "Formulario heredado" de `BaseForm`. Si el diseñador muestra un error, casi siempre se arregla compilando.
- Para convertir un formulario existente: cambiar `: Form` por `: BaseForm` en el archivo `.cs` **y** en el `.Designer.cs`, y agregar `using SistemaStock.Estilos;`.
- **No es suficiente** cambiar `: Form` por `: UserControl` para convertir una página: hay que revisar el `.Designer.cs` (un `UserControl` no tiene `Text`, `FormBorderStyle`, `StartPosition`, `AcceptButton`, etc.). Por eso es mejor crear el `UserControl` nuevo (Agregar > Control de usuario) y pasar los controles uno por uno.

### 4.3. Formularios/FrmPrincipal.cs (sidebar + navegación)

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

### 4.4. Modelos/Producto.cs

```csharp
namespace SistemaStock.Modelos;

public class Producto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = "";   // solo para mostrar
}
```

### 4.5. Datos/ProductoRepositorio.cs (consultas parametrizadas)

```csharp
using System.Collections.Generic;
using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal static class ProductoRepositorio
{
    public static List<Producto> Listar(string filtro = "")
    {
        var lista = new List<Producto>();
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT p.codigo, p.nombre, p.precio, p.stock, p.activo, p.categoria_id, c.nombre AS categoria
            FROM productos p
            INNER JOIN categorias c ON c.id = p.categoria_id
            WHERE p.nombre LIKE @filtro
            ORDER BY p.nombre;
            """, cn);
        cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            lista.Add(new Producto
            {
                Codigo = rd.GetInt32("codigo"),
                Nombre = rd.GetString("nombre"),
                Precio = rd.GetDecimal("precio"),
                Stock = rd.GetInt32("stock"),
                Activo = rd.GetBoolean("activo"),
                CategoriaId = rd.GetInt32("categoria_id"),
                CategoriaNombre = rd.GetString("categoria")
            });
        }
        return lista;
    }

    public static void Crear(Producto p)
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            INSERT INTO productos (nombre, precio, stock, activo, categoria_id)
            VALUES (@nombre, @precio, @stock, @activo, @categoriaId);
            """, cn);
        cmd.Parameters.AddWithValue("@nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@precio", p.Precio);
        cmd.Parameters.AddWithValue("@stock", p.Stock);
        cmd.Parameters.AddWithValue("@activo", p.Activo);
        cmd.Parameters.AddWithValue("@categoriaId", p.CategoriaId);
        cmd.ExecuteNonQuery();
    }

    // Modificar y Desactivar siguen el mismo patrón (UPDATE ... WHERE codigo = @codigo).
    // En lugar de borrar productos se pone activo = 0, para no romper ventas futuras.
}
```

### 4.6. Página interna (UserControl)

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

### 4.7. Diálogo (Form que hereda de BaseForm)

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

## 5. Login y cierre de sesión

- Login → si es válido, se abre `FrmPrincipal` y se guarda el usuario autenticado (nombre y rol) en un lugar al que las páginas puedan acceder.
- El rol (Admin/Cajero/Mozo) decide qué botones del sidebar se ven. Aplicar esto en `FrmPrincipal`, no en cada página.
- **Antes de tocar `Program.cs` o el flujo de login/cierre de sesión, pedir el archivo y leerlo.** No asumir cómo está hecho.
- Cerrar con la X debe terminar la app, sin volver a abrir el Login. Cerrar sesión debe volver al Login sin dejar ventanas ocultas.

## 6. Layout responsive: checklist por pantalla

- Título arriba (`Dock = Top`), contenido al centro (`Dock = Fill`), botones de acción abajo en `FlowLayoutPanel` (`Dock = Bottom`), o arriba junto al buscador.
- Botones sueltos en una esquina: `Anchor = Bottom | Right`.
- Orden de agregado/Z importa: lo que tiene `Dock = Fill` va **al fondo**.
- Si algo se corta a 850x500: arreglar con Dock/Anchor o `AutoScroll = true` en el panel, antes de achicar fuentes.
- Probar **siempre** dos tamaños: 850x500 y maximizado.
- Grillas: `AutoSizeColumnsMode.Fill` por defecto (viene en `Theme`); si hay muchas columnas, cambiar a scroll horizontal.

## 7. Trabajo en equipo con Git (3 personas)

1. **Una persona por formulario/UserControl** a la vez. Los archivos `.Designer.cs` se pisan fácil y los conflictos son difíciles de resolver para principiantes.
2. Una rama por integrante o por funcionalidad (`feature/productos`, `feature/ventas`) y `git pull` antes de empezar a trabajar.
3. Archivos compartidos (`Theme.cs`, `BaseForm.cs`, `Database.cs`, `FrmPrincipal.cs`): avisar al equipo antes de editarlos.
4. Nunca subir contraseñas ni la cadena de conexión real. Cada integrante configura su propia variable `GESTION_VENTAS_CONNECTION`.
5. Commits chicos y con mensaje claro (ej. "Agrega listado de productos").
6. Si se agrega una tabla, se agrega en `Database.Initialize()` para que todos (y el cliente) la tengan automáticamente al abrir la app.

## 8. Errores típicos de principiantes (revisar si algo falla)

| Síntoma | Causa probable |
|---|---|
| El diseñador no abre un formulario heredado | Falta compilar (Ctrl+Shift+B) o `BaseForm` no tiene `InitializeComponent()` |
| El panel de contenido tapa el sidebar | `pnlContenido` no está "enviado al fondo" |
| Un botón ejecuta su acción dos veces | `Click` conectado en el diseñador **y** en el constructor |
| `Unknown column 'id' in 'field list'` | En `productos` la clave se llama `codigo` |
| `Access denied for user` | Falta o está mal la variable `GESTION_VENTAS_CONNECTION` (reiniciar Visual Studio después de crearla) |
| Los colores de un título no se respetan | `Theme.Aplicar` lo pisó: usar `Tag = "propio"` en ese Label |
| Error de `null` / advertencias amarillas | Son advertencias de nulabilidad de .NET moderno: no impiden compilar; usar `string = ""` o `?` según corresponda |