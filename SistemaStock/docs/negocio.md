# Reglas de negocio: stock y ventas

Sistema de gestión del local gastronómico Chachín. Este documento dice **cómo debe comportarse** el sistema. Tablas y columnas: `esquema.md`. SQL: `flujos-y-reportes.md`. Código C#: `codigo-csharp.md`. Guía general: `../SKILL.md`.

Si el código o una petición contradicen estas reglas, avisar al usuario antes de seguir.

## Contenido

- Productos, ingredientes y recetas
- Stock
- Ventas
- Estados de una venta
- Fechas de una venta
- Bajas lógicas
- Pendientes de confirmar con la clienta
- Puntos abiertos detectados al unificar la documentación

## Productos, ingredientes y recetas

- **Categorías de productos**: Platos, Bebidas, Postres (tabla `categorias`, con semilla en `Initialize()`).
- **Ingredientes** son una entidad aparte. No son una categoría: se consumen, no se venden.
- Cada producto tiene `tipo_stock`:
  - `DIRECTO`: el stock vive en `productos.stock` (ej. Coca 3l, que se compra y se revende).
  - `POR_RECETA`: el stock **no se guarda**, se calcula desde los ingredientes según su receta (ej. sándwich de milanesa). Tener dos fuentes de verdad desincroniza los datos.
- **Stock disponible de un plato** = el mínimo de `FLOOR(stock_ingrediente / cantidad_en_receta)` entre todos sus ingredientes. Ejemplo: 4 milanesas, 2 panes, 2 huevos, 2 quesos y 2 jamones, con receta de 1 de cada uno, dan 2 sándwiches. La consulta está en `flujos-y-reportes.md`.
- **Recetas**: una pantalla permite definir, por producto `POR_RECETA`, qué ingredientes lleva y en qué cantidad.

## Stock

- Al vender se descuenta el stock directo del producto o, si tiene receta, el de sus ingredientes (cantidad vendida × cantidad de la receta).
- El stock se descuenta **al agregar la línea a la venta**, no al cobrar: reserva el stock real, porque la cocina consume al preparar.
- Toda operación que toque stock va dentro de una transacción (`MySqlTransaction`): o se completa todo o no se guarda nada. Cómo se hace: `codigo-csharp.md`.
- **Nunca permitir stock negativo.** Se descuenta con `UPDATE ... WHERE stock >= @cantidad` y se revisa `RowsAffected`; si es 0, rollback y aviso de que no hay stock suficiente.

## Ventas

- Las ventas de mesa quedan `PENDIENTE`. Se les pueden agregar más productos mientras la mesa sigue ocupada, y pasan a `CERRADA` solo cuando se paga.
- **El precio se congela en la venta**: `detalle_venta.precio_unitario` guarda el precio al momento de vender, así cambiar el precio del producto no altera el historial.
- **Pagos divididos y excepciones**: no hay tabla de pagos. La venta tiene una columna `notas` para registrar cualquier excepción (pago dividido, propina, descuento acordado, etc.). El `medio_pago` se guarda tal cual lo que predomine; el detalle va en `notas`.
- **"Mesa ocupada"** se deduce de si tiene una venta `PENDIENTE`; no se guarda como estado en `mesas`. Solo puede haber una venta `PENDIENTE` por mesa.
- Los **reportes** no tienen tablas propias: salen de consultas sobre `ventas` y `detalle_venta`. Solo cuentan las ventas `CERRADA`: las `ANULADA` y `PENDIENTE` no son recaudación.

## Estados de una venta

| Estado | Cómo se llega | Qué se puede hacer | Cuenta en recaudación |
|---|---|---|---|
| `PENDIENTE` | Al abrir la venta | Agregar líneas, quitar líneas, cerrar, anular | No |
| `CERRADA` | Al pagar | Nada: una venta cerrada ya no se edita | Sí |
| `ANULADA` | Al anular (se devuelve el stock de todas sus líneas) | Nada | No |

Los pasos de cada operación, con su SQL, están en `flujos-y-reportes.md`.

## Fechas de una venta

- `fecha_hora` es la **apertura** de la venta.
- `fecha_cierre` es el **momento del pago**.
- Los reportes de recaudación por día y por mes usan `fecha_cierre`.

## Bajas lógicas

Se usa la columna `activo` en vez de borrar (`usuarios`, `productos`, `ingredientes`, `mesas`), para no romper el historial de ventas.

## Pendientes de confirmar con la clienta

Estos puntos pueden cambiar el diseño. Si el usuario trae respuestas de la entrevista, actualizar este documento y `esquema.md`:

- Si necesita controlar el stock a nivel ingrediente en tiempo real o prefiere solo el de productos.
- Si hay variantes de platos (tamaños, medio plato) o combos y promociones.
- Si el delivery necesita guardar dirección o datos del cliente.
- Si quiere registrar propinas por separado en lugar de en `notas`.

## Puntos abiertos detectados al unificar la documentación

No son reglas: son preguntas que ninguno de los documentos de origen resolvía. Decidirlas con el equipo y, cuando se decida, pasar la respuesta a la sección que corresponda.

1. **¿Se puede anular una venta `CERRADA`?** Los documentos solo definen la anulación y que una venta cerrada no se edita; no dicen si se anula después de cobrada.
2. **Receta modificada con ventas `PENDIENTE`.** Al quitar una línea o anular, el stock de un plato `POR_RECETA` se devuelve usando la receta *vigente*. Si la receta cambió después de agregar la línea, se devolvería una cantidad distinta de la que se descontó. Hay que decidir cómo evitarlo (por ejemplo, restringir la edición de recetas mientras haya ventas pendientes de ese producto).
3. **Mesas "Reservada".** El diseño anterior pensaba un estado Libre/Ocupada/Reservada en `mesas`. El diseño vigente deduce "ocupada" de la venta `PENDIENTE` y no tiene reservas. Si la clienta las necesita, cambia el diseño de `mesas`.
