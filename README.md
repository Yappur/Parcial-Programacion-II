# Parcial-Programacion-II

## Control de stock

Chachín es un bar/restaurante que ofrece bebidas y platos, por lo que necesita controlar productos físicos. Desde la sección **Ventas** se puede registrar la cantidad vendida de un producto activo; el sistema descuenta esas unidades del stock dentro de una transacción y rechaza la venta si no alcanza el stock disponible.

La sección **Productos** identifica como **Stock bajo** a los artículos con 5 unidades o menos. Se eligió ese umbral como referencia operativa para el proyecto. Al iniciar la aplicación se crean automáticamente las categorías; los productos y sus cantidades se administran en la tabla `productos`.
