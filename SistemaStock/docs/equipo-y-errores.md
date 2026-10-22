# Trabajo en equipo y errores típicos

Cómo coordinar el trabajo entre tres personas con Git y qué revisar cuando algo falla. Guía general: `../SKILL.md`.

## Contenido

- Trabajo en equipo con Git (3 personas)
- Errores típicos de principiantes

## Trabajo en equipo con Git (3 personas)

1. **Una persona por formulario/UserControl** a la vez. Los archivos `.Designer.cs` se pisan fácil y los conflictos son difíciles de resolver para principiantes.
2. Una rama por integrante o por funcionalidad (`feature/productos`, `feature/ventas`) y `git pull` antes de empezar a trabajar.
3. Archivos compartidos (`Theme.cs`, `BaseForm.cs`, `BaseFormSidebar.cs`, `Database.cs`): avisar al equipo antes de editarlos.
4. Nunca subir contraseñas ni la cadena de conexión real. Cada integrante configura su propia variable `GESTION_VENTAS_CONNECTION`.
5. Commits chicos y con mensaje claro (ej. "Agrega listado de productos").
6. Si se agrega una tabla, se agrega en `Database.Initialize()` para que todos (y el cliente) la tengan automáticamente al abrir la app. Si lo que cambia es una tabla que ya existe (ej. `productos`), `Initialize()` no la modifica: ver la migración en `esquema.md`.

## Errores típicos de principiantes

Revisar esta tabla si algo falla.

| Síntoma | Causa probable |
|---|---|
| El diseñador no abre un formulario heredado | Falta compilar (Ctrl+Shift+B) o `BaseForm` no tiene `InitializeComponent()` |
| El panel de contenido tapa el sidebar | `pnlContenido` no está "enviado al fondo" |
| Un botón ejecuta su acción dos veces | `Click` conectado en el diseñador **y** en el constructor |
| `Unknown column 'id' in 'field list'` | En `productos` la clave se llama `codigo` |
| `Unknown column 'tipo_stock'` | La base tiene la tabla `productos` de antes del cambio: aplicar la migración de `esquema.md` |
| Un documento menciona `FrmPrincipal` o `UserControl` que no existe | Es la arquitectura futura; la navegación actual usa `BaseFormSidebar` y varios formularios |
| No existe una tabla de ingredientes, recetas o ventas | Esos módulos están documentados como próximos pasos, pero todavía no fueron agregados a `Database.Initialize()` |
| `Access denied for user` | Falta o está mal la variable `GESTION_VENTAS_CONNECTION` (reiniciar Visual Studio después de crearla) |
| Los colores de un título no se respetan | Un estilo aplicado por `Theme` lo pisó: revisar `Theme.ApplyForm`, `StyleGrid` y los estilos propios del control |
| Error de `null` / advertencias amarillas | Son advertencias de nulabilidad de .NET moderno: no impiden compilar; usar `string = ""` o `?` según corresponda |
| Se guardó un stock negativo | Una versión de MySQL anterior a 8.0.16 ignora los `CHECK`: la protección real es el `WHERE stock >= @cantidad` del descuento (`flujos-y-reportes.md`) |
