using MySqlConnector;
using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal sealed class VentaRepositorio
{
    public ResumenDashboard ObtenerResumenDashboard()
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT
                (SELECT COALESCE(SUM(total), 0) FROM ventas
                 WHERE estado = 'CERRADA' AND fecha_cierre >= CURDATE() AND fecha_cierre < CURDATE() + INTERVAL 1 DAY) AS ventas_dia,
                (SELECT COUNT(*) FROM ventas
                 WHERE estado = 'CERRADA' AND fecha_cierre >= CURDATE() AND fecha_cierre < CURDATE() + INTERVAL 1 DAY) AS cantidad_ventas,
                (SELECT COUNT(*) FROM productos WHERE activo = TRUE) AS cantidad_productos,
                (SELECT COUNT(*) FROM ingredientes WHERE activo = TRUE AND stock <= stock_minimo) +
                (SELECT COUNT(*) FROM productos WHERE activo = TRUE AND tipo_stock = 'DIRECTO' AND stock <= stock_minimo) AS stock_bajo,
                (SELECT COUNT(*) FROM ingredientes WHERE activo = TRUE) AS cantidad_ingredientes,
                (SELECT COUNT(*) FROM mesas WHERE activo = TRUE AND id IN (SELECT mesa_id FROM ventas WHERE estado = 'PENDIENTE' AND mesa_id IS NOT NULL)) AS mesas_ocupadas,
                (SELECT COUNT(*) FROM mesas WHERE activo = TRUE) AS mesas_activas;
            """, cn);

        ResumenDashboard resumen;
        using (var rd = cmd.ExecuteReader())
        {
            if (!rd.Read())
                throw new InvalidOperationException("No se pudo obtener el resumen del dashboard.");
            resumen = new ResumenDashboard
            {
                VentasDelDia = rd.GetDecimal("ventas_dia"),
                CantidadVentasDelDia = rd.GetInt32("cantidad_ventas"),
                CantidadProductos = rd.GetInt32("cantidad_productos"),
                CantidadStockBajo = rd.GetInt32("stock_bajo"),
                CantidadIngredientes = rd.GetInt32("cantidad_ingredientes"),
                MesasOcupadas = rd.GetInt32("mesas_ocupadas"),
                MesasActivas = rd.GetInt32("mesas_activas")
            };
        }

        var ultimasVentas = new List<VentaDashboard>();
        var stockBajo = new List<StockBajoDashboard>();
        using (var stockCmd = new MySqlCommand("""
            SELECT nombre COLLATE utf8mb4_general_ci AS nombre,
                   CONCAT(stock, ' ', unidad) COLLATE utf8mb4_general_ci AS stock
            FROM ingredientes WHERE activo = TRUE AND stock <= stock_minimo
            UNION ALL
            SELECT nombre COLLATE utf8mb4_general_ci AS nombre,
                   CAST(stock AS CHAR) COLLATE utf8mb4_general_ci AS stock
            FROM productos WHERE activo = TRUE AND tipo_stock = 'DIRECTO' AND stock <= stock_minimo
            ORDER BY nombre LIMIT 8;
            """, cn))
        using (var stockRd = stockCmd.ExecuteReader())
            while (stockRd.Read()) stockBajo.Add(new StockBajoDashboard { Nombre = stockRd.GetString("nombre"), Stock = stockRd.GetString("stock") });
        using (var recientes = new MySqlCommand("""
            SELECT id, COALESCE(fecha_cierre, fecha_hora) AS fecha, tipo_venta, total, estado
            FROM ventas
            ORDER BY COALESCE(fecha_cierre, fecha_hora) DESC, id DESC
            LIMIT 8;
            """, cn))
        using (var rd = recientes.ExecuteReader())
        {
            while (rd.Read())
            {
                string tipo = rd.GetString("tipo_venta") switch
                {
                    "MESA" => "Mesa",
                    "DELIVERY" => "Delivery",
                    "MOSTRADOR" => "Mostrador",
                    var valor => valor
                };
                string estado = rd.GetString("estado") switch
                {
                    "CERRADA" => "Cerrada",
                    "PENDIENTE" => "Pendiente",
                    "ANULADA" => "Anulada",
                    var valor => valor
                };
                ultimasVentas.Add(new VentaDashboard
                {
                    Id = rd.GetInt32("id"),
                    Fecha = rd.GetDateTime("fecha"),
                    Tipo = tipo,
                    Total = rd.GetDecimal("total"),
                    Estado = estado
                });
            }
        }

        return new ResumenDashboard
        {
            VentasDelDia = resumen.VentasDelDia,
            CantidadVentasDelDia = resumen.CantidadVentasDelDia,
            CantidadProductos = resumen.CantidadProductos,
            CantidadStockBajo = resumen.CantidadStockBajo,
            CantidadIngredientes = resumen.CantidadIngredientes,
            MesasOcupadas = resumen.MesasOcupadas,
            MesasActivas = resumen.MesasActivas,
            UltimasVentas = ultimasVentas,
            ProductosStockBajo = stockBajo
        };
    }

    public DateTime FechaMinimaVenta()
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("SELECT MIN(fecha_hora) FROM ventas;", cn);
        object? valor = cmd.ExecuteScalar();
        return valor is null || valor == DBNull.Value ? DateTime.Today : Convert.ToDateTime(valor);
    }

    public List<VentaPendiente> ListarPendientes()
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT id, tipo_venta, mesa_id, total
            FROM ventas
            WHERE estado = 'PENDIENTE'
            ORDER BY fecha_hora, id;
            """, cn);
        using var rd = cmd.ExecuteReader();
        var ventas = new List<VentaPendiente>();
        while (rd.Read())
        {
            var tipo = ConversionesEnum.ATipoVenta(rd.GetString("tipo_venta"));
            string tipoTexto = tipo switch
            {
                TipoVenta.Mesa => $"Mesa {rd.GetInt32("mesa_id")}",
                TipoVenta.Mostrador => "Mostrador",
                TipoVenta.Delivery => "Delivery",
                _ => throw new ArgumentOutOfRangeException()
            };
            ventas.Add(new VentaPendiente
            {
                Id = rd.GetInt32("id"),
                Total = rd.GetDecimal("total"),
                Descripcion = $"Venta #{rd.GetInt32("id")} - {tipoTexto} - {rd.GetDecimal("total"):C2}"
            });
        }
        return ventas;
    }

    public List<DetalleVenta> ListarLineas(int ventaId)
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT d.id, d.venta_id, d.producto_codigo, p.nombre AS producto_nombre,
                   d.cantidad, d.precio_unitario, d.subtotal
            FROM detalle_venta d
            INNER JOIN productos p ON p.codigo = d.producto_codigo
            WHERE d.venta_id = @ventaId
            ORDER BY d.id;
            """, cn);
        cmd.Parameters.AddWithValue("@ventaId", ventaId);
        using var rd = cmd.ExecuteReader();
        var lineas = new List<DetalleVenta>();
        while (rd.Read())
        {
            lineas.Add(new DetalleVenta
            {
                Id = rd.GetInt32("id"),
                VentaId = rd.GetInt32("venta_id"),
                ProductoCodigo = rd.GetInt32("producto_codigo"),
                ProductoNombre = rd.GetString("producto_nombre"),
                Cantidad = rd.GetInt32("cantidad"),
                PrecioUnitario = rd.GetDecimal("precio_unitario"),
                Subtotal = rd.GetDecimal("subtotal")
            });
        }
        return lineas;
    }

    public List<VentaHistorial> ListarHistorial(DateTime desde, DateTime hasta, EstadoVenta? estado)
    {
        if (desde.Date > hasta.Date)
            throw new ArgumentException("La fecha desde no puede ser posterior a la fecha hasta.");

        DateTime hastaExclusivo = hasta.Date.AddDays(1);
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT v.id, v.fecha_hora, v.fecha_cierre, v.tipo_venta, m.numero AS mesa,
                   u.nombre_completo AS atendio, v.total, v.medio_pago, v.estado, v.notas
            FROM ventas v
            LEFT JOIN mesas m ON m.id = v.mesa_id
            JOIN usuarios u ON u.id = v.usuario_id
            WHERE v.fecha_hora >= @desde
              AND v.fecha_hora < @hasta
              AND (@estado IS NULL OR v.estado = @estado)
            ORDER BY v.fecha_hora DESC;
            """, cn);
        cmd.Parameters.AddWithValue("@desde", desde.Date);
        cmd.Parameters.AddWithValue("@hasta", hastaExclusivo);
        cmd.Parameters.AddWithValue("@estado", estado is null ? DBNull.Value : estado.Value.ADb());

        using var rd = cmd.ExecuteReader();
        var ventas = new List<VentaHistorial>();
        while (rd.Read())
        {
            ventas.Add(new VentaHistorial
            {
                Id = rd.GetInt32("id"),
                FechaHora = rd.GetDateTime("fecha_hora"),
                FechaCierre = rd.IsDBNull(rd.GetOrdinal("fecha_cierre"))
                    ? null
                    : rd.GetDateTime("fecha_cierre"),
                TipoVenta = rd.GetString("tipo_venta"),
                Mesa = rd.IsDBNull(rd.GetOrdinal("mesa")) ? null : rd.GetInt32("mesa"),
                Atendio = rd.GetString("atendio"),
                Total = rd.GetDecimal("total"),
                MedioPago = rd.IsDBNull(rd.GetOrdinal("medio_pago"))
                    ? null
                    : rd.GetString("medio_pago"),
                Estado = rd.GetString("estado"),
                Notas = rd.IsDBNull(rd.GetOrdinal("notas")) ? null : rd.GetString("notas")
            });
        }
        return ventas;
    }

    public List<VentaHistorial> ListarVentasCerradasPorPeriodo(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date)
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");

        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT v.id, v.fecha_hora, v.fecha_cierre, v.tipo_venta, m.numero AS mesa,
                   u.nombre_completo AS atendio, v.total, v.medio_pago, v.estado, v.notas
            FROM ventas v
            LEFT JOIN mesas m ON m.id = v.mesa_id
            INNER JOIN usuarios u ON u.id = v.usuario_id
            WHERE v.estado = 'CERRADA'
              AND v.fecha_cierre >= @desde
              AND v.fecha_cierre < @hasta
            ORDER BY v.fecha_cierre DESC, v.id DESC;
            """, cn);
        cmd.Parameters.AddWithValue("@desde", desde.Date);
        cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1));

        using var rd = cmd.ExecuteReader();
        var ventas = new List<VentaHistorial>();
        while (rd.Read())
        {
            ventas.Add(new VentaHistorial
            {
                Id = rd.GetInt32("id"),
                FechaHora = rd.GetDateTime("fecha_hora"),
                FechaCierre = rd.IsDBNull(rd.GetOrdinal("fecha_cierre")) ? null : rd.GetDateTime("fecha_cierre"),
                TipoVenta = rd.GetString("tipo_venta"),
                Mesa = rd.IsDBNull(rd.GetOrdinal("mesa")) ? null : rd.GetInt32("mesa"),
                Atendio = rd.GetString("atendio"),
                Total = rd.GetDecimal("total"),
                MedioPago = rd.IsDBNull(rd.GetOrdinal("medio_pago")) ? null : rd.GetString("medio_pago"),
                Estado = rd.GetString("estado"),
                Notas = rd.IsDBNull(rd.GetOrdinal("notas")) ? null : rd.GetString("notas")
            });
        }
        return ventas;
    }

    public void AgregarLinea(int ventaId, int productoCodigo, int cantidad)
    {
        if (ventaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ventaId), "Elegí una venta pendiente.");
        if (productoCodigo <= 0)
            throw new ArgumentOutOfRangeException(nameof(productoCodigo), "Elegí un producto.");
        if (cantidad <= 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");

        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            ValidarVentaPendiente(cn, tx, ventaId);
            ProductoVenta producto = LeerProductoParaVenta(cn, tx, productoCodigo);

            if (producto.TipoStock == TipoStock.Directo)
                DescontarProductoDirecto(cn, tx, producto, cantidad);
            else
                DescontarIngredientes(cn, tx, producto, cantidad);

            decimal subtotal = producto.Precio * cantidad;
            using (var insertar = new MySqlCommand("""
                INSERT INTO detalle_venta
                    (venta_id, producto_codigo, cantidad, precio_unitario, subtotal)
                VALUES (@ventaId, @productoCodigo, @cantidad, @precio, @subtotal);
                """, cn, tx))
            {
                insertar.Parameters.AddWithValue("@ventaId", ventaId);
                insertar.Parameters.AddWithValue("@productoCodigo", productoCodigo);
                insertar.Parameters.AddWithValue("@cantidad", cantidad);
                insertar.Parameters.AddWithValue("@precio", producto.Precio);
                insertar.Parameters.AddWithValue("@subtotal", subtotal);
                insertar.ExecuteNonQuery();
            }

            RecalcularTotal(cn, tx, ventaId);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void QuitarLinea(int ventaId, int lineaId)
    {
        if (ventaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ventaId), "Elegí una venta pendiente.");
        if (lineaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(lineaId), "Elegí una línea válida.");

        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            ValidarVentaPendiente(cn, tx, ventaId);
            int productoCodigo;
            int cantidad;
            using (var cmd = new MySqlCommand("""
                SELECT producto_codigo, cantidad
                FROM detalle_venta
                WHERE id = @lineaId AND venta_id = @ventaId
                FOR UPDATE;
                """, cn, tx))
            {
                cmd.Parameters.AddWithValue("@lineaId", lineaId);
                cmd.Parameters.AddWithValue("@ventaId", ventaId);
                using var rd = cmd.ExecuteReader();
                if (!rd.Read())
                    throw new InvalidOperationException("La línea ya no existe en esa venta.");
                productoCodigo = rd.GetInt32("producto_codigo");
                cantidad = rd.GetInt32("cantidad");
            }

            ProductoVenta producto = LeerProductoParaVenta(cn, tx, productoCodigo, exigirActivo: false);
            if (producto.TipoStock == TipoStock.Directo)
            {
                using var devolver = new MySqlCommand("""
                    UPDATE productos SET stock = stock + @cantidad WHERE codigo = @codigo;
                    """, cn, tx);
                devolver.Parameters.AddWithValue("@cantidad", cantidad);
                devolver.Parameters.AddWithValue("@codigo", productoCodigo);
                if (devolver.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException($"No se pudo devolver el stock de {producto.Nombre}.");
            }
            else
            {
                DevolverIngredientesVigentes(cn, tx, producto, cantidad);
            }

            using (var borrar = new MySqlCommand(
                "DELETE FROM detalle_venta WHERE id = @lineaId AND venta_id = @ventaId;", cn, tx))
            {
                borrar.Parameters.AddWithValue("@lineaId", lineaId);
                borrar.Parameters.AddWithValue("@ventaId", ventaId);
                if (borrar.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("La línea ya no existe en esa venta.");
            }

            RecalcularTotal(cn, tx, ventaId);
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void Cerrar(int ventaId, MedioPago medioPago, string? notas)
    {
        if (ventaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ventaId), "Elegí una venta pendiente.");
        if (notas?.Length > 500)
            throw new ArgumentException("Las notas no pueden superar los 500 caracteres.", nameof(notas));

        string medioPagoDb = medioPago.ADb();
        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            ValidarVentaPendiente(cn, tx, ventaId);
            using var cmd = new MySqlCommand("""
                UPDATE ventas
                SET estado = 'CERRADA', medio_pago = @medioPago,
                    fecha_cierre = NOW(), notas = @notas
                WHERE id = @ventaId AND estado = 'PENDIENTE';
                """, cn, tx);
            cmd.Parameters.AddWithValue("@medioPago", medioPagoDb);
            cmd.Parameters.AddWithValue("@notas", string.IsNullOrWhiteSpace(notas) ? DBNull.Value : notas.Trim());
            cmd.Parameters.AddWithValue("@ventaId", ventaId);
            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("La venta ya no está pendiente y no se puede cerrar.");

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void Anular(int ventaId)
    {
        if (ventaId <= 0)
            throw new ArgumentOutOfRangeException(nameof(ventaId), "Elegí una venta pendiente.");

        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            ValidarVentaPendiente(cn, tx, ventaId);
            var lineas = new List<(int ProductoCodigo, int Cantidad)>();
            using (var cmd = new MySqlCommand("""
                SELECT producto_codigo, cantidad
                FROM detalle_venta
                WHERE venta_id = @ventaId
                ORDER BY id
                FOR UPDATE;
                """, cn, tx))
            {
                cmd.Parameters.AddWithValue("@ventaId", ventaId);
                using var rd = cmd.ExecuteReader();
                while (rd.Read())
                    lineas.Add((rd.GetInt32("producto_codigo"), rd.GetInt32("cantidad")));
            }

            foreach (var linea in lineas)
            {
                ProductoVenta producto = LeerProductoParaVenta(
                    cn, tx, linea.ProductoCodigo, exigirActivo: false);
                if (producto.TipoStock == TipoStock.Directo)
                {
                    using var devolver = new MySqlCommand("""
                        UPDATE productos SET stock = stock + @cantidad WHERE codigo = @codigo;
                        """, cn, tx);
                    devolver.Parameters.AddWithValue("@cantidad", linea.Cantidad);
                    devolver.Parameters.AddWithValue("@codigo", linea.ProductoCodigo);
                    if (devolver.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException($"No se pudo devolver el stock de {producto.Nombre}.");
                }
                else
                {
                    DevolverIngredientesVigentes(cn, tx, producto, linea.Cantidad);
                }
            }

            using (var cmd = new MySqlCommand("""
                UPDATE ventas SET estado = 'ANULADA'
                WHERE id = @ventaId AND estado = 'PENDIENTE';
                """, cn, tx))
            {
                cmd.Parameters.AddWithValue("@ventaId", ventaId);
                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("La venta ya no está pendiente y no se puede anular.");
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public List<MesaVenta> ListarMesasConEstado()
    {
        using var cn = Database.OpenConnection();
        using var cmd = new MySqlCommand("""
            SELECT m.id, m.numero,
                   EXISTS (SELECT 1 FROM ventas v
                           WHERE v.mesa_id = m.id AND v.estado = 'PENDIENTE') AS ocupada
            FROM mesas m
            WHERE m.activo = TRUE
            ORDER BY m.numero;
            """, cn);
        using var rd = cmd.ExecuteReader();
        var mesas = new List<MesaVenta>();
        while (rd.Read())
        {
            mesas.Add(new MesaVenta
            {
                Id = rd.GetInt32("id"),
                Numero = rd.GetInt32("numero"),
                Ocupada = rd.GetBoolean("ocupada")
            });
        }
        return mesas;
    }

    public int Abrir(TipoVenta tipo, int? mesaId, int usuarioId)
    {
        if (usuarioId <= 0)
            throw new InvalidOperationException("No se encontró el usuario que inició sesión.");

        if (tipo == TipoVenta.Mesa && mesaId is null)
            throw new InvalidOperationException("Seleccioná una mesa para abrir la venta.");

        if (tipo != TipoVenta.Mesa && mesaId is not null)
            throw new InvalidOperationException("Las ventas de delivery y mostrador no llevan mesa.");

        using var cn = Database.OpenConnection();
        using var tx = cn.BeginTransaction();
        try
        {
            if (tipo == TipoVenta.Mesa)
            {
                using var mesaCmd = new MySqlCommand("""
                    SELECT activo FROM mesas WHERE id = @mesaId FOR UPDATE;
                    """, cn, tx);
                mesaCmd.Parameters.AddWithValue("@mesaId", mesaId!.Value);
                var activa = mesaCmd.ExecuteScalar();
                if (activa is null || activa == DBNull.Value || !Convert.ToBoolean(activa))
                    throw new InvalidOperationException("La mesa seleccionada no existe o está inactiva.");

                using var pendienteCmd = new MySqlCommand("""
                    SELECT EXISTS (SELECT 1 FROM ventas
                                   WHERE mesa_id = @mesaId AND estado = 'PENDIENTE');
                    """, cn, tx);
                pendienteCmd.Parameters.AddWithValue("@mesaId", mesaId.Value);
                if (Convert.ToInt32(pendienteCmd.ExecuteScalar()) == 1)
                    throw new InvalidOperationException("Esa mesa ya tiene una venta pendiente.");
            }

            using var cmd = new MySqlCommand("""
                INSERT INTO ventas (fecha_hora, tipo_venta, mesa_id, usuario_id, total, estado)
                VALUES (NOW(), @tipo, @mesaId, @usuarioId, 0, 'PENDIENTE');
                """, cn, tx);
            cmd.Parameters.AddWithValue("@tipo", tipo.ADb());
            cmd.Parameters.AddWithValue("@mesaId", mesaId is null ? DBNull.Value : mesaId.Value);
            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
            cmd.ExecuteNonQuery();
            var id = checked((int)cmd.LastInsertedId);
            tx.Commit();
            return id;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    private static void ValidarVentaPendiente(MySqlConnection cn, MySqlTransaction tx, int ventaId)
    {
        using var cmd = new MySqlCommand(
            "SELECT estado FROM ventas WHERE id = @ventaId FOR UPDATE;", cn, tx);
        cmd.Parameters.AddWithValue("@ventaId", ventaId);
        var estado = cmd.ExecuteScalar() as string;
        if (estado != "PENDIENTE")
            throw new InvalidOperationException("La venta no existe o ya no está pendiente.");
    }

    private static ProductoVenta LeerProductoParaVenta(
        MySqlConnection cn, MySqlTransaction tx, int productoCodigo, bool exigirActivo = true)
    {
        using var cmd = new MySqlCommand("""
            SELECT nombre, precio, tipo_stock, activo
            FROM productos
            WHERE codigo = @codigo
            FOR UPDATE;
            """, cn, tx);
        cmd.Parameters.AddWithValue("@codigo", productoCodigo);
        using var rd = cmd.ExecuteReader();
        if (!rd.Read())
            throw new InvalidOperationException("El producto seleccionado ya no existe.");
        if (exigirActivo && !rd.GetBoolean("activo"))
            throw new InvalidOperationException("El producto seleccionado está inactivo.");

        return new ProductoVenta(
            productoCodigo,
            rd.GetString("nombre"),
            rd.GetDecimal("precio"),
            TipoStockMySql.FromDatabase(rd.GetString("tipo_stock")));
    }

    private static void DescontarProductoDirecto(
        MySqlConnection cn, MySqlTransaction tx, ProductoVenta producto, int cantidad)
    {
        using var cmd = new MySqlCommand("""
            UPDATE productos
            SET stock = stock - @cantidad
            WHERE codigo = @codigo AND stock >= @cantidad;
            """, cn, tx);
        cmd.Parameters.AddWithValue("@cantidad", cantidad);
        cmd.Parameters.AddWithValue("@codigo", producto.Codigo);
        if (cmd.ExecuteNonQuery() == 0)
            throw new InvalidOperationException($"No hay stock suficiente de {producto.Nombre}.");
    }

    private static void DescontarIngredientes(
        MySqlConnection cn, MySqlTransaction tx, ProductoVenta producto, int cantidad)
    {
        var receta = new List<(int IngredienteId, string Nombre, decimal Cantidad)>();
        using (var cmd = new MySqlCommand("""
            SELECT i.id, i.nombre, r.cantidad
            FROM recetas r
            INNER JOIN ingredientes i ON i.id = r.ingrediente_id
            WHERE r.producto_codigo = @codigo
            ORDER BY i.id
            FOR UPDATE;
            """, cn, tx))
        {
            cmd.Parameters.AddWithValue("@codigo", producto.Codigo);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                receta.Add((rd.GetInt32("id"), rd.GetString("nombre"), rd.GetDecimal("cantidad")));
        }

        if (receta.Count == 0)
            throw new InvalidOperationException($"El producto {producto.Nombre} no tiene una receta cargada.");

        foreach (var ingrediente in receta)
        {
            decimal necesario = ingrediente.Cantidad * cantidad;
            using var cmd = new MySqlCommand("""
                UPDATE ingredientes
                SET stock = stock - @necesario
                WHERE id = @id AND stock >= @necesario;
                """, cn, tx);
            cmd.Parameters.AddWithValue("@necesario", necesario);
            cmd.Parameters.AddWithValue("@id", ingrediente.IngredienteId);
            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException(
                    $"No hay stock suficiente de {ingrediente.Nombre} para preparar {producto.Nombre}.");
        }
    }

    private static void DevolverIngredientesVigentes(
        MySqlConnection cn, MySqlTransaction tx, ProductoVenta producto, int cantidad)
    {
        var receta = new List<(int IngredienteId, string Nombre, decimal Cantidad)>();
        using (var cmd = new MySqlCommand("""
            SELECT i.id, i.nombre, r.cantidad
            FROM recetas r
            INNER JOIN ingredientes i ON i.id = r.ingrediente_id
            WHERE r.producto_codigo = @codigo
            ORDER BY i.id
            FOR UPDATE;
            """, cn, tx))
        {
            cmd.Parameters.AddWithValue("@codigo", producto.Codigo);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                receta.Add((rd.GetInt32("id"), rd.GetString("nombre"), rd.GetDecimal("cantidad")));
        }

        if (receta.Count == 0)
            throw new InvalidOperationException(
                $"No se quitó {producto.Nombre}: no tiene una receta vigente para devolver sus ingredientes.");

        foreach (var ingrediente in receta)
        {
            using var cmd = new MySqlCommand("""
                UPDATE ingredientes
                SET stock = stock + @cantidad
                WHERE id = @id;
                """, cn, tx);
            cmd.Parameters.AddWithValue("@cantidad", ingrediente.Cantidad * cantidad);
            cmd.Parameters.AddWithValue("@id", ingrediente.IngredienteId);
            if (cmd.ExecuteNonQuery() == 0)
                throw new InvalidOperationException(
                    $"No se pudo devolver el stock de {ingrediente.Nombre} para {producto.Nombre}.");
        }
    }

    private static void RecalcularTotal(MySqlConnection cn, MySqlTransaction tx, int ventaId)
    {
        using var cmd = new MySqlCommand("""
            UPDATE ventas
            SET total = (SELECT COALESCE(SUM(subtotal), 0)
                         FROM detalle_venta WHERE venta_id = @ventaId)
            WHERE id = @ventaId AND estado = 'PENDIENTE';
            """, cn, tx);
        cmd.Parameters.AddWithValue("@ventaId", ventaId);
        cmd.ExecuteNonQuery();
    }

    private sealed record ProductoVenta(
        int Codigo, string Nombre, decimal Precio, TipoStock TipoStock);
}
