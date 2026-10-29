# Esquema de tablas

Motor InnoDB, `utf8mb4`. Los `CHECK` solo se aplican de verdad desde MySQL 8.0.16 (en versiones anteriores se ignoran: la protección real contra stock negativo es el `WHERE stock >= @cantidad` de `flujos-y-reportes.md`). Reglas de negocio: `negocio.md`. Código C#: `codigo-csharp.md`. Guía general: `../SKILL.md`.

## Contenido

- Convenciones
- Resumen y relaciones
- Orden de creación
- Estado actual y migración de `productos`
- usuarios y categorias (existentes)
- mesas
- ingredientes
- productos
- recetas
- ventas
- detalle_venta
- movimientos_stock (opcional, fase 2)
- Semilla

## Convenciones

- Nombres de tablas y columnas en **minúsculas con guion bajo**.
- `ENGINE=InnoDB`.
- Claves foráneas con nombre `fk_tabla_otra` (ej. `fk_ventas_mesas`) y columnas terminadas en `_id` (ej. `categoria_id`, `mesa_id`, `usuario_id`). Excepción ya existente: `productos` usa `codigo` como clave, y por eso las referencias a producto se llaman `producto_codigo`.
- Toda tabla nueva se agrega en `Database.Initialize()` con `CREATE TABLE IF NOT EXISTS`, en el orden de abajo.
- Si se renombra algo de `productos` (ej. `codigo` → `id`), avisar el impacto en todo el código antes.

## Resumen y relaciones

```
usuarios, categorias           (ya existentes)
actividades                    registro de acciones del sistema
mesas                          numero, capacidad, activo
ingredientes                   nombre, unidad (un/g/ml), stock DECIMAL, stock_minimo, activo
productos                      codigo, nombre, precio, categoria_id, tipo_stock, stock?, stock_minimo?, activo
recetas                        producto_codigo + ingrediente_id (PK compuesta), cantidad
ventas                         fecha_hora, fecha_cierre?, tipo_venta, mesa_id?, usuario_id, total, medio_pago?, estado, notas
detalle_venta                  venta_id, producto_codigo, cantidad, precio_unitario, subtotal
movimientos_stock (opcional)   auditoría de cambios de stock (fase 2)
```

```
categorias 1──N productos 1──N recetas N──1 ingredientes
productos  1──N detalle_venta N──1 ventas N──1 mesas
                                  ventas N──1 usuarios
```

## Orden de creación

Respetar las claves foráneas. Este es el orden de `Initialize()`:

`usuarios`, `actividades`, `categorias`, `mesas`, `ingredientes`, `productos`, `recetas`, `ventas`, `detalle_venta` (y `movimientos_stock` si se implementa).

Cada tabla se agrega cuando el equipo llega a su paso del orden de trabajo (`../SKILL.md`, sección 5), no antes.

## Estado actual y migración de `productos`

Hasta ahora `productos` se creó con: `codigo`, `nombre`, `precio`, `stock` (INT obligatorio, `>= 0`), `activo` y `categoria_id`. El diseño vigente (más abajo) agrega `tipo_stock` y `stock_minimo`, y vuelve `stock` opcional (solo para `DIRECTO`).

`CREATE TABLE IF NOT EXISTS` **no modifica una tabla que ya existe**: en cada base que ya tenga `productos`, el cambio no se aplica solo. Propuesta a confirmar con el equipo:

**A) Bases de desarrollo sin datos que importen** (lo más simple): borrar la base `gestion_ventas` y abrir la app; `Initialize()` la crea de nuevo con el diseño vigente. Cada integrante lo hace en su máquina.

**B) Si hay datos para conservar**: ejecutar **una sola vez por base** (no ponerlo en `Initialize()`: fallaría la segunda vez porque las columnas ya existen). Probarlo antes en una copia: este script no se pudo ejecutar al escribir la guía.

```sql
ALTER TABLE productos
  ADD COLUMN tipo_stock ENUM('DIRECTO','POR_RECETA') NOT NULL DEFAULT 'DIRECTO' AFTER categoria_id,
  ADD COLUMN stock_minimo INT NULL,
  MODIFY COLUMN stock INT NULL;

UPDATE productos SET stock_minimo = 0 WHERE stock_minimo IS NULL;
```

Los productos existentes quedan como `DIRECTO` con su stock actual. El valor por defecto `DIRECTO` queda en la columna migrada; no molesta.

## usuarios y categorias (ya existen)

```sql
CREATE TABLE IF NOT EXISTS usuarios (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre_usuario VARCHAR(50) NOT NULL UNIQUE,
    nombre_completo VARCHAR(100) NOT NULL,
    hash_contrasena VARCHAR(200) NOT NULL,
    rol VARCHAR(20) NOT NULL,                 -- Mozo, Cajero, Admin
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS categorias (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE
) ENGINE=InnoDB;
```

## actividades

```sql
CREATE TABLE IF NOT EXISTS actividades (
    id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    usuario_id INT NULL,
    tipo VARCHAR(20) NOT NULL,
    accion VARCHAR(100) NOT NULL,
    detalle VARCHAR(255) NOT NULL,
    precio DECIMAL(10,2) NULL,
    fecha_hora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX ix_actividades_fecha (fecha_hora),
    CONSTRAINT fk_actividades_usuarios FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;
```

Registra las altas, modificaciones, bajas y operaciones de venta para mostrarlas en “Actividad reciente”. `usuario_id` puede ser nulo para acciones previas a iniciar sesión, como crear el primer administrador. `precio` se completa para operaciones de venta con importe asociado.

## mesas

```sql
CREATE TABLE IF NOT EXISTS mesas (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    numero INT NOT NULL UNIQUE,
    capacidad INT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;
```

No lleva columna de estado: una mesa está ocupada si tiene una venta `PENDIENTE`.

## ingredientes

```sql
CREATE TABLE IF NOT EXISTS ingredientes (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    unidad ENUM('un','g','ml') NOT NULL,
    stock DECIMAL(10,3) NOT NULL DEFAULT 0 CHECK (stock >= 0),
    stock_minimo DECIMAL(10,3) NOT NULL DEFAULT 0 CHECK (stock_minimo >= 0),
    activo BOOLEAN NOT NULL DEFAULT TRUE
) ENGINE=InnoDB;
```

Decimales porque se miden gramos y mililitros. Las cosas contables (pan, huevo, milanesa) usan `un`.

## productos

```sql
CREATE TABLE IF NOT EXISTS productos (
    codigo INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    precio DECIMAL(10,2) NOT NULL CHECK (precio >= 0),
    categoria_id INT NOT NULL,
    tipo_stock ENUM('DIRECTO','POR_RECETA') NOT NULL,
    stock INT NULL CHECK (stock IS NULL OR stock >= 0),            -- solo DIRECTO
    stock_minimo INT NULL CHECK (stock_minimo IS NULL OR stock_minimo >= 0),
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_productos_categorias FOREIGN KEY (categoria_id) REFERENCES categorias(id)
) ENGINE=InnoDB;
```

- `DIRECTO`: `stock` y `stock_minimo` obligatorios (validar en código).
- `POR_RECETA`: `stock` y `stock_minimo` quedan en `NULL`; el stock se calcula (ver `flujos-y-reportes.md`).

## recetas

```sql
CREATE TABLE IF NOT EXISTS recetas (
    producto_codigo INT NOT NULL,
    ingrediente_id INT NOT NULL,
    cantidad DECIMAL(10,3) NOT NULL CHECK (cantidad > 0),
    PRIMARY KEY (producto_codigo, ingrediente_id),
    CONSTRAINT fk_recetas_productos FOREIGN KEY (producto_codigo) REFERENCES productos(codigo),
    CONSTRAINT fk_recetas_ingredientes FOREIGN KEY (ingrediente_id) REFERENCES ingredientes(id)
) ENGINE=InnoDB;
```

Solo tienen filas en esta tabla los productos `POR_RECETA`. La cantidad está en la unidad del ingrediente.

## ventas

```sql
CREATE TABLE IF NOT EXISTS ventas (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    fecha_hora DATETIME NOT NULL,                  -- apertura
    fecha_cierre DATETIME NULL,                    -- momento del pago
    tipo_venta ENUM('MESA','DELIVERY','MOSTRADOR') NOT NULL,
    mesa_id INT NULL,                              -- obligatorio si tipo_venta = 'MESA'
    usuario_id INT NOT NULL,
    total DECIMAL(10,2) NOT NULL DEFAULT 0,
    medio_pago ENUM('EFECTIVO','TRANSFERENCIA') NULL,   -- NULL mientras está PENDIENTE
    estado ENUM('PENDIENTE','CERRADA','ANULADA') NOT NULL DEFAULT 'PENDIENTE',
    notas VARCHAR(500) NULL,                       -- excepciones: pago dividido, propina, descuento, etc.
    CONSTRAINT fk_ventas_mesas FOREIGN KEY (mesa_id) REFERENCES mesas(id),
    CONSTRAINT fk_ventas_usuarios FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
) ENGINE=InnoDB;
```

Reglas que valida el código:

- Si `tipo_venta = 'MESA'`, `mesa_id` es obligatorio; si no, va `NULL`.
- Si `estado = 'CERRADA'`, `medio_pago` y `fecha_cierre` son obligatorios.
- Solo una venta `PENDIENTE` por mesa.

**Opcional**: que la base misma impida dos ventas pendientes en la misma mesa. Como `Initialize()` corre en cada arranque, esto **no** se puede hacer con un `ALTER TABLE` suelto (fallaría la segunda vez, porque la columna ya existe). Si se quiere, agregar estas dos líneas **dentro** del `CREATE TABLE` de `ventas`, después de `notas` y antes de los `CONSTRAINT`:

```sql
    mesa_pendiente_id INT GENERATED ALWAYS AS (CASE WHEN estado = 'PENDIENTE' THEN mesa_id END) STORED,
    UNIQUE KEY uq_una_pendiente_por_mesa (mesa_pendiente_id),
```

(Los `NULL` no chocan entre sí en un `UNIQUE`, por eso solo restringe las pendientes con mesa.) Aunque se use, el código sigue verificando antes de abrir la venta.

## detalle_venta

```sql
CREATE TABLE IF NOT EXISTS detalle_venta (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    venta_id INT NOT NULL,
    producto_codigo INT NOT NULL,
    cantidad INT NOT NULL CHECK (cantidad > 0),
    precio_unitario DECIMAL(10,2) NOT NULL,        -- precio al momento de vender
    subtotal DECIMAL(10,2) NOT NULL,               -- cantidad * precio_unitario
    CONSTRAINT fk_detalle_ventas FOREIGN KEY (venta_id) REFERENCES ventas(id),
    CONSTRAINT fk_detalle_productos FOREIGN KEY (producto_codigo) REFERENCES productos(codigo)
) ENGINE=InnoDB;
```

## movimientos_stock (opcional, fase 2)

```sql
CREATE TABLE IF NOT EXISTS movimientos_stock (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    fecha_hora DATETIME NOT NULL,
    ingrediente_id INT NULL,
    producto_codigo INT NULL,                      -- uno de los dos va con valor
    cantidad DECIMAL(10,3) NOT NULL,               -- negativa si resta
    motivo ENUM('VENTA','COMPRA','AJUSTE','ANULACION') NOT NULL,
    venta_id INT NULL,
    usuario_id INT NOT NULL
) ENGINE=InnoDB;
```

Sirve para auditar por qué cambió un stock. Dejarla para después si hay poco tiempo. No entra en `Initialize()` hasta que el equipo decida implementarla.

## Semilla

```sql
INSERT IGNORE INTO categorias (nombre) VALUES ('Bebidas'), ('Platos'), ('Postres');
```

Usar `INSERT IGNORE` en vez de `ON DUPLICATE KEY UPDATE ... VALUES()`, porque `VALUES()` está deprecado desde MySQL 8.0.20.
