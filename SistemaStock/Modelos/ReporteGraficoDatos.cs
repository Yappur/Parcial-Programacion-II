namespace SistemaStock.Modelos;

public sealed record RecaudacionDiaria(DateTime Dia, double Total);

public sealed record StockProductoComparacion(string Producto, double StockActual, double StockMinimo)
{
    public double PorcentajeDelMinimo => StockMinimo <= 0 ? 0 : StockActual * 100 / StockMinimo;
}

/// <summary>Prepara para los gráficos datos de las filas ya obtenidas por los repositorios.</summary>
public static class ReporteGraficoDatos
{
    public static List<RecaudacionDiaria> AgruparRecaudacionPorDia(IEnumerable<VentaHistorial> ventas) =>
        ventas
            .Where(venta => venta.FechaCierre.HasValue)
            .GroupBy(venta => venta.FechaCierre!.Value.Date)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo => new RecaudacionDiaria(
                grupo.Key,
                Convert.ToDouble(grupo.Sum(venta => venta.Total))))
            .ToList();

    public static List<StockProductoComparacion> PrepararComparacionStock(IEnumerable<Producto> productos) =>
        productos
            .Where(producto => producto.Stock.HasValue && producto.StockMinimo.HasValue)
            .Select(producto => new StockProductoComparacion(
                producto.Nombre,
                producto.Stock!.Value,
                producto.StockMinimo!.Value))
            .OrderBy(producto => producto.PorcentajeDelMinimo)
            .ToList();
}
