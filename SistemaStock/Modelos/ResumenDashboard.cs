namespace SistemaStock.Modelos;

public sealed class ResumenDashboard
{
    public decimal VentasDelDia { get; init; }
    public int CantidadVentasDelDia { get; init; }
    public int CantidadProductos { get; init; }
    public int CantidadStockBajo { get; init; }
    public int CantidadIngredientes { get; init; }
    public int MesasOcupadas { get; init; }
    public int MesasActivas { get; init; }
    public List<VentaDashboard> UltimasVentas { get; init; } = [];
    public List<StockBajoDashboard> ProductosStockBajo { get; init; } = [];
}

public sealed class StockBajoDashboard
{
    public string Nombre { get; init; } = "";
    public string Stock { get; init; } = "";
}

public sealed class VentaDashboard
{
    public int Id { get; init; }
    public DateTime Fecha { get; init; }
    public string Tipo { get; init; } = "";
    public decimal Total { get; init; }
    public string Estado { get; init; } = "";
    public string Actividad => $"Venta #{Id} · {Estado}{Environment.NewLine}{Tipo} · {Total:C2}";
}
