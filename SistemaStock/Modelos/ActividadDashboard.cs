namespace SistemaStock.Modelos;

public sealed class ActividadDashboard
{
    public long Id { get; init; }
    public string Tipo { get; init; } = "";
    public string Accion { get; init; } = "";
    public string Detalle { get; init; } = "";
    public decimal? Precio { get; init; }
    public DateTime FechaHora { get; init; }
    public string Texto => $"{Accion}{Environment.NewLine}{Detalle}";
}
