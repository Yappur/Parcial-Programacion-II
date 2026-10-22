---
name: winforms-chachin
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

- **Acceso a datos**: clase `internal static class Database` con `OpenConnection()` e `Initialize()`. **No crear otra clase de conexión** (nada de `Conexion.cs`).
- **Cadena de conexión**: variable de entorno **`GESTION_VENTAS_CONNECTION`** (si no existe, usa `localhost`, base `gestion_ventas`, usuario `root`). **Nunca** subir contraseñas a GitHub.
- **Base de datos**: `gestion_ventas`, creada por `Database.Initialize()` con `CREATE ... IF NOT EXISTS`.
- **Tablas**: el diseño completo está en `docs/esquema.md`. Según el estado verificado antes de unificar esta guía, existen `usuarios`, `categorias` y `productos` (esta última en su versión anterior, sin `tipo_stock`). Pasarla al diseño nuevo requiere una migración: ver `docs/esquema.md`, sección «Estado actual y migración de `productos`». **`productos` usa `codigo` como clave, no `id`.**
- **Roles observados/documentados**: el código actual usa también `Vendedor`, mientras que el diseño de negocio menciona `Admin`, `Cajero` y `Mozo`. Esta lista debe confirmarse antes de restringir permisos.

## 4. Reglas de oro (código y UI)

1. **Arquitectura actual:** `FrmDashboard`, `FrmProductos`, `FrmUsers` y `FrmVentas` heredan de `BaseFormSidebar` y comparten la navegación. **Arquitectura planificada:** una sola ventana `FrmPrincipal` con un `UserControl` por módulo. No convertir formularios a `UserControl` sin una tarea específica.
2. **Los diálogos** (Nuevo producto, Editar, Confirmar) son `Form` que heredan de **`BaseForm`** y se abren con `ShowDialog(this)`.
3. **Nada de colores, fuentes o tamaños sueltos** en formularios: siempre `Theme.Algo`. Si falta algo, se agrega a `Theme.cs`.
4. **Layout responsive**: `Dock`, `Anchor`, `TableLayoutPanel`, `FlowLayoutPanel`. Prohibido armar pantallas con `Location` en píxeles (salvo dentro de un panel chico y fijo).
5. **SQL solo en `Datos/`** (repositorios), nunca dentro de los eventos de los botones. Siempre **parametrizado**: nunca `"... WHERE x = '" + texto + "'"`. Los formularios validan solo lo básico (campos vacíos, números válidos); las **reglas de negocio** (stock, estados) viven en los repositorios.
6. **Contraseñas**: se guardan con hash (`hash_contrasena`). Nunca texto plano. Mantener el mecanismo del Login actual; no cambiarlo sin avisar.
7. **No duplicar eventos**: si el `Click` ya se conectó en el diseñador, no volver a conectarlo en el constructor.
8. **Antes de escribir una plantilla, revisar si el archivo ya existe.** Si existe, agregar o ajustar solo lo que falta.

Las reglas de negocio (stock, transacciones, estados de venta, bajas lógicas) están en `docs/negocio.md` y son de lectura obligatoria antes de tocar stock o ventas.

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

Cada tema tiene **un solo lugar**. No repetirlo en otro archivo: enlazarlo.

| Tema | Archivo único |
|---|---|
| Reglas de negocio, estados de venta, pendientes con la clienta | `docs/negocio.md` |
| Tablas, columnas, orden de creación, convenciones SQL, migración | `docs/esquema.md` |
| SQL de stock, ventas y reportes | `docs/flujos-y-reportes.md` |
| `Database.cs`, modelos, repositorios, transacciones | `docs/codigo-csharp.md` |
| Paleta, `Theme`, `BaseForm`, sidebar, páginas, diálogos, layout, login | `docs/ui-winforms.md` |
| Git y errores típicos | `docs/equipo-y-errores.md` |