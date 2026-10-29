# Flujos y reportes

SQL de stock, de las operaciones de una venta y de los reportes. Qué reglas rigen cada operación: `negocio.md`. Tablas: `esquema.md`. Cómo se ejecuta esto desde C# (conexión, transacción, bucle por ingrediente): `codigo-csharp.md`. Guía general: `../SKILL.md`.

## Contenido

- Stock disponible de productos
- Descontar y devolver stock
- Flujo de venta (abrir, agregar, quitar, cerrar, anular)
- Mesas ocupadas
- Reportes

## Stock disponible de productos

El stock de un `POR_RECETA` nunca se guarda: se calcula.

```sql
SELECT p.codigo, p.nombre,
  CASE WHEN p.tipo_stock = 'DIRECTO' THEN p.stock
       ELSE COALESCE((
         SELECT MIN(FLOOR(i.stock / r.cantidad))
         FROM recetas r
         JOIN ingredientes i ON i.id = r.ingrediente_id
         WHERE r.producto_codigo = p.codigo), 0)
  END AS stock_disponible
FROM productos p
WHERE p.activo = TRUE;
```

Un producto `POR_RECETA` sin filas en `recetas` da 0 (no se puede vender hasta que tenga receta).

`ProductoRepositorio.Listar` (en `codigo-csharp.md`) usa una copia de este bloque `CASE`. Si cambia uno, cambia el otro.

## Descontar y devolver stock

Siempre dentro de una `MySqlTransaction`. Si algún `UPDATE` afecta 0 filas, no hay stock suficiente: rollback y avisar.

**Producto DIRECTO**

```sql
-- descontar
UPDATE productos SET stock = stock - @cant
WHERE codigo = @codigo AND stock >= @cant;

-- devolver
UPDATE productos SET stock = stock + @cant WHERE codigo = @codigo;
```

**Producto POR_RECETA**

Descontar se hace **ingrediente por ingrediente**, en un bucle de C#: leer la receta y después un `UPDATE` por ingrediente. Es lo más simple y claro mientras se aprende.

```sql
-- 1) leer la receta
SELECT ingrediente_id, cantidad FROM recetas WHERE producto_codigo = @codigo;

-- 2) por cada fila, descontar ese ingrediente
UPDATE ingredientes SET stock = stock - (@cantidadReceta * @cant)
WHERE id = @ingredienteId AND stock >= (@cantidadReceta * @cant);
```

Si algún `UPDATE` afecta 0 filas, ese ingrediente no alcanza: rollback de todo. Si la receta no tiene filas, el producto no se puede vender.

No usar un único `UPDATE` con `JOIN` para descontar: solo afecta a los ingredientes que alcanzan, y habría que comparar `RowsAffected` con la cantidad de ingredientes de la receta (`SELECT COUNT(*) FROM recetas WHERE producto_codigo = @codigo`) para detectar el faltante.

Devolver sí puede hacerse de una vez, porque no hay nada que verificar:

```sql
-- devolver
UPDATE ingredientes i
JOIN recetas r ON r.ingrediente_id = i.id
SET i.stock = i.stock + (r.cantidad * @cant)
WHERE r.producto_codigo = @codigo;
```

Ojo: devuelve según la receta *vigente*. Ver el punto abierto sobre recetas modificadas en `negocio.md`.

## Flujo de venta

**1. Abrir**

```sql
INSERT INTO ventas (fecha_hora, tipo_venta, mesa_id, usuario_id, total, estado)
VALUES (NOW(), @tipo, @mesaId, @usuarioId, 0, 'PENDIENTE');
```

Antes, si es de mesa, verificar que no tenga otra venta `PENDIENTE`. `medio_pago` y `fecha_cierre` quedan `NULL`.

**2. Agregar un producto** (transacción)

1. Leer el precio actual del producto.
2. Descontar stock (según `tipo_stock`).
3. `INSERT INTO detalle_venta (venta_id, producto_codigo, cantidad, precio_unitario, subtotal)`.
4. Recalcular el total:

```sql
UPDATE ventas v
SET v.total = (SELECT COALESCE(SUM(subtotal), 0) FROM detalle_venta WHERE venta_id = v.id)
WHERE v.id = @ventaId;
```

Solo se puede agregar si `estado = 'PENDIENTE'`.

**3. Quitar una línea** (transacción, solo si `PENDIENTE`)

1. Leer la línea (producto y cantidad).
2. Devolver el stock.
3. `DELETE FROM detalle_venta WHERE id = @lineaId`.
4. Recalcular `total`.

**4. Cerrar (pagar)**

```sql
UPDATE ventas
SET estado = 'CERRADA', medio_pago = @medioPago, fecha_cierre = NOW(), notas = @notas
WHERE id = @ventaId AND estado = 'PENDIENTE';
```

`notas` lleva las excepciones (pago dividido, propina, descuento). Después de cerrar no se edita.

**5. Anular** (transacción)

Solo se anula una venta `PENDIENTE`. Una venta `CERRADA` no se puede anular.

1. Leer todas las líneas de la venta.
2. Devolver el stock de cada una.
3. `UPDATE ventas SET estado = 'ANULADA' WHERE id = @ventaId AND estado = 'PENDIENTE'`.

Una venta `ANULADA` no cuenta en ningún reporte de recaudación.

## Mesas ocupadas

```sql
SELECT m.id, m.numero,
       EXISTS (SELECT 1 FROM ventas v
               WHERE v.mesa_id = m.id AND v.estado = 'PENDIENTE') AS ocupada
FROM mesas m
WHERE m.activo = TRUE
ORDER BY m.numero;
```

## Reportes

Todos consideran solo ventas `CERRADA`, salvo el historial, que muestra todas con su estado.

**Dashboard**

El resumen del día usa ventas `CERRADA` cuya `fecha_cierre` cae en el día actual. El total es la suma de `total` y la cantidad es el conteo de esas ventas. El conteo de productos incluye solo productos activos. El indicador de stock bajo suma ingredientes activos por debajo de su mínimo y productos activos `DIRECTO` en esa misma condición; el stock `POR_RECETA` se controla mediante sus ingredientes. Las últimas ventas incluyen cualquier estado y se ordenan por fecha descendente, con un máximo de ocho filas. El usuario conectado se toma de la sesión autenticada y no requiere otra consulta.

**Historial de ventas**

```sql
SELECT v.id, v.fecha_hora, v.fecha_cierre, v.tipo_venta, m.numero AS mesa,
       u.nombre_completo AS atendio, v.total, v.medio_pago, v.estado, v.notas
FROM ventas v
LEFT JOIN mesas m ON m.id = v.mesa_id
JOIN usuarios u ON u.id = v.usuario_id
ORDER BY v.fecha_hora DESC;
```

**Ventas y recaudación por día**

```sql
SELECT DATE(fecha_cierre) AS dia, COUNT(*) AS cantidad_ventas, SUM(total) AS recaudado
FROM ventas
WHERE estado = 'CERRADA'
GROUP BY DATE(fecha_cierre)
ORDER BY dia DESC;
```

**Total vendido por mes**

```sql
SELECT YEAR(fecha_cierre) AS anio, MONTH(fecha_cierre) AS mes, SUM(total) AS total_mes
FROM ventas
WHERE estado = 'CERRADA'
GROUP BY YEAR(fecha_cierre), MONTH(fecha_cierre)
ORDER BY anio DESC, mes DESC;
```

**Recaudación por medio de pago** (extra útil)

```sql
SELECT medio_pago, SUM(total) AS total
FROM ventas
WHERE estado = 'CERRADA'
GROUP BY medio_pago;
```

**Bajo stock** (ingredientes y productos directos)

```sql
SELECT 'Ingrediente' AS tipo, nombre, stock, stock_minimo
FROM ingredientes
WHERE activo = TRUE AND stock <= stock_minimo
UNION ALL
SELECT 'Producto', nombre, stock, stock_minimo
FROM productos
WHERE activo = TRUE AND tipo_stock = 'DIRECTO' AND stock <= stock_minimo;
```

**Productos más vendidos**

```sql
SELECT p.codigo, p.nombre, SUM(d.cantidad) AS unidades_vendidas
FROM detalle_venta d
JOIN ventas v ON v.id = d.venta_id AND v.estado = 'CERRADA'
JOIN productos p ON p.codigo = d.producto_codigo
GROUP BY p.codigo, p.nombre
ORDER BY unidades_vendidas DESC
LIMIT 10;
```

Para filtrar por rango de fechas en cualquier reporte, agregar `AND fecha_cierre >= @desde AND fecha_cierre < @hasta` (con `@hasta` = día siguiente al último día incluido).
