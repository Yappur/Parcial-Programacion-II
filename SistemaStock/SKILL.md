---
name: winforms-chachin
<<<<<<< HEAD
description: Guía principal del sistema de stock y ventas del restaurante Chachín, hecho en C# Windows Forms (Visual Studio, .NET) con MySQL y MySqlConnector, namespace SistemaStock. Define cómo responder al equipo (principiantes), la arquitectura por carpetas, las reglas de oro de código y UI, y un mapa que indica qué documento de docs/ leer según la tarea (formularios y estilo con BaseForm/Theme/sidebar, tablas y Database.Initialize, repositorios y transacciones, reglas de negocio y flujos de venta, stock por receta, reportes, Git y errores típicos). Usar SIEMPRE que se cree, modifique o depure un formulario, UserControl, botón, DataGridView, menú, consulta SQL, tabla de MySQL, modelo, repositorio, reporte, o cualquier módulo de productos, ingredientes, recetas, mesas, ventas o stock del sistema Chachín, aunque el usuario no mencione esta skill.
---

# Sistema Chachín: guía principal

Este archivo es el punto de entrada: dice **cómo trabajar**, **cómo está armado el proyecto** y **qué reglas no se rompen**. El detalle vive en `docs/`. La sección 2 indica qué documento leer según la tarea: leer solo el que corresponda, no todos a la vez.

## 1. Quién es el usuario y cómo responder

El equipo (tres personas) es **principiante** en C# y Windows Forms. Es un parcial que hay que **defender**, así que importa entender cada cosa.

Reglas para la IA:

1. Responder en español, simple, explicando de a poco y con ejemplos chicos. Dar el *por qué* en una o dos frases.
2. **Un cambio por vez.** No refactorizar cosas que no se pidieron.
3. Entregar **el archivo completo** (no fragmentos sueltos), diciendo el **nombre exacto del archivo** y en qué carpeta va.
4. Si hay que hacer algo en el **diseñador de Visual Studio** (arrastrar un control, cambiar Dock, propiedades), decirlo paso a paso, porque la IA no puede hacerlo por ellos.
5. **No inventar** clases, métodos, columnas, tablas ni namespaces. Si falta información, pedir el archivo o preguntar. Si no se está seguro de algo, decirlo.
6. **No se puede compilar ni ejecutar nada en el chat.** Nunca decir "compila" o "probado" si no se ejecutó. Cerrar cada respuesta con **"Qué revisar después de pegar esto"**: compilar con Ctrl+Shift+B, probar a 850x500 y maximizado, y probar la operación. Si el cambio toca stock o ventas, comprobar además que el stock quedó como se esperaba.
7. **Contradicciones: no decidir solo.** Si el código real contradice la documentación, o dos documentos se contradicen, mostrar la diferencia y preguntar antes de seguir. Mientras tanto:
   - el **código real** es la referencia de cómo está el proyecto hoy (nombres, clases, estructura);
   - `docs/negocio.md` y `docs/esquema.md` son la referencia de cómo debe comportarse y guardarse la información.
8. **Mantener la documentación al día.** Si el equipo decide cambiar el diseño, actualizar en el mismo paso el documento donde vive esa regla (ver sección 6) y avisar cuál se tocó.

## 2. Mapa: qué documento leer según la tarea

| Tarea | Leer |
|---|---|
| Crear o modificar un formulario, `UserControl`, diálogo, menú lateral, estilo, layout | `docs/ui-winforms.md` |
| Crear o modificar una tabla o columna, `Database.Initialize()`, migrar `productos` | `docs/esquema.md` y `docs/codigo-csharp.md` |
| Crear o modificar un modelo o repositorio, usar una transacción | `docs/codigo-csharp.md` |
| Descontar o devolver stock, calcular stock disponible, abrir/agregar/quitar/cerrar/anular una venta | `docs/negocio.md`, `docs/flujos-y-reportes.md` y `docs/codigo-csharp.md` |
| Armar un reporte | `docs/flujos-y-reportes.md` (y `docs/ui-winforms.md` si hay pantalla) |
| Pantalla que mezcla UI y lógica (recetas, ventas de mesa) | `docs/ui-winforms.md` + `docs/negocio.md` + `docs/codigo-csharp.md` |
| Dudas sobre qué debe hacer el sistema | `docs/negocio.md` |
| Git, conflictos, un error al correr el programa | `docs/equipo-y-errores.md` |

## 3. Estado actual del proyecto (verificado)

- **Lenguaje/.NET**: C# moderno con **.NET 10 para Windows Forms**. El proyecto usa `namespace X;`, cadenas `"""` y `net10.0-windows`.
- **Paquete MySQL**: **`MySqlConnector`** (ya en uso). No agregar `MySql.Data`.
- **Namespace raíz**: **`SistemaStock`** (ej. `SistemaStock.Datos`). Usar el mismo prefijo en todo: `SistemaStock.Estilos`, `SistemaStock.Formularios`, `SistemaStock.Modelos`.
- **Carpetas implementadas actualmente**:

```
 Datos/         Database.cs + repositorios implementados
 Estilos/       Theme.cs, BaseForm.cs, BaseFormSidebar.cs
 Formularios/   FrmLogin, FrmDashboard, FrmProductos, FrmUsers, FrmVentas
 Modelos/       Categoria.cs, Producto.cs y Usuarios/User.cs
Recursos/      imágenes e íconos
Program.cs
```

Los repositorios, modelos y formularios de ingredientes, recetas, mesas, ventas y reportes todavía no están implementados. Se agregarán según el orden de trabajo (sección 5). Esta guía vive en `SKILL.md` y los documentos relacionados en `docs/`.

**Arquitectura planificada:** una ventana principal `FrmPrincipal` con `UserControl` por módulo. Esa arquitectura todavía no reemplaza a la actual: hoy la navegación utiliza varios formularios que heredan de `BaseFormSidebar`.
=======
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
>>>>>>> 3162ac05ab8db25ba7eb867cce2fc91761ed396c

- **Acceso a datos**: clase `internal static class Database` con `OpenConnection()` e `Initialize()`. **No crear otra clase de conexión** (nada de `Conexion.cs`).
- **Cadena de conexión**: variable de entorno **`GESTION_VENTAS_CONNECTION`** (si no existe, usa `localhost`, base `gestion_ventas`, usuario `root`). **Nunca** subir contraseñas a GitHub.
- **Base de datos**: `gestion_ventas`, creada por `Database.Initialize()` con `CREATE ... IF NOT EXISTS`.
- **Tablas**: el diseño completo está en `docs/esquema.md`. Según el estado verificado antes de unificar esta guía, existen `usuarios`, `categorias` y `productos` (esta última en su versión anterior, sin `tipo_stock`). Pasarla al diseño nuevo requiere una migración: ver `docs/esquema.md`, sección «Estado actual y migración de `productos`». **`productos` usa `codigo` como clave, no `id`.**
- **Roles observados/documentados**: el código actual usa también `Vendedor`, mientras que el diseño de negocio menciona `Admin`, `Cajero` y `Mozo`. Esta lista debe confirmarse antes de restringir permisos.

<<<<<<< HEAD
## 4. Reglas de oro (código y UI)

1. **Arquitectura actual:** `FrmDashboard`, `FrmProductos`, `FrmUsers` y `FrmVentas` heredan de `BaseFormSidebar` y comparten la navegación. **Arquitectura planificada:** una sola ventana `FrmPrincipal` con un `UserControl` por módulo. No convertir formularios a `UserControl` sin una tarea específica.
2. **Los diálogos** (Nuevo producto, Editar, Confirmar) son `Form` que heredan de **`BaseForm`** y se abren con `ShowDialog(this)`.
3. **Nada de colores, fuentes o tamaños sueltos** en formularios: siempre `Theme.Algo`. Si falta algo, se agrega a `Theme.cs`.
4. **Layout responsive**: `Dock`, `Anchor`, `TableLayoutPanel`, `FlowLayoutPanel`. Prohibido armar pantallas con `Location` en píxeles (salvo dentro de un panel chico y fijo).
5. **SQL solo en `Datos/`** (repositorios), nunca dentro de los eventos de los botones. Siempre **parametrizado**: nunca `"... WHERE x = '" + texto + "'"`. Los formularios validan solo lo básico (campos vacíos, números válidos); las **reglas de negocio** (stock, estados) viven en los repositorios.
6. **Contraseñas**: se guardan con hash (`hash_contrasena`). Nunca texto plano. Mantener el mecanismo del Login actual; no cambiarlo sin avisar.
7. **No duplicar eventos**: si el `Click` ya se conectó en el diseñador, no volver a conectarlo en el constructor.
8. **Antes de escribir una plantilla, revisar si el archivo ya existe.** Si existe, agregar o ajustar solo lo que falta.
=======
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
>>>>>>> 3162ac05ab8db25ba7eb867cce2fc91761ed396c

Las reglas de negocio (stock, transacciones, estados de venta, bajas lógicas) están en `docs/negocio.md` y son de lectura obligatoria antes de tocar stock o ventas.

<<<<<<< HEAD
## 5. Orden de trabajo

Las tablas nuevas **no se crean antes de su paso**: cada una se agrega a `Database.Initialize()` cuando el equipo llega a ese paso, respetando el orden de creación de `docs/esquema.md`.

| Paso | Qué | Tablas | Leer |
|---|---|---|---|
| 1 | Productos y categorías (sumar `tipo_stock`) | ajustar `productos` (migración) | `esquema.md`, `codigo-csharp.md`, `ui-winforms.md` |
| 2 | Ingredientes (alta, baja, modificación, filtro) | `ingredientes` | ídem + `negocio.md` |
| 3 | Recetas (asociar producto con ingredientes y cantidades) | `recetas` | ídem + `negocio.md` |
| 4 | Mesas (ABM simple) | `mesas` | `esquema.md`, `ui-winforms.md` |
| 5 | Ventas: abrir, agregar y quitar líneas, cerrar, anular | `ventas`, `detalle_venta` | `negocio.md`, `flujos-y-reportes.md`, `codigo-csharp.md` |
| 6 | Reportes | ninguna (son consultas) | `flujos-y-reportes.md` |

## 6. Dónde vive cada cosa
=======
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
>>>>>>> 3162ac05ab8db25ba7eb867cce2fc91761ed396c

Cada tema tiene **un solo lugar**. No repetirlo en otro archivo: enlazarlo.

<<<<<<< HEAD
| Tema | Archivo único |
|---|---|
| Reglas de negocio, estados de venta, pendientes con la clienta | `docs/negocio.md` |
| Tablas, columnas, orden de creación, convenciones SQL, migración | `docs/esquema.md` |
| SQL de stock, ventas y reportes | `docs/flujos-y-reportes.md` |
| `Database.cs`, modelos, repositorios, transacciones | `docs/codigo-csharp.md` |
| Paleta, `Theme`, `BaseForm`, sidebar, páginas, diálogos, layout, login | `docs/ui-winforms.md` |
| Git y errores típicos | `docs/equipo-y-errores.md` |
=======
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
>>>>>>> 3162ac05ab8db25ba7eb867cce2fc91761ed396c
