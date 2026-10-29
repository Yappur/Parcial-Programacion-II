using SkiaSharp;

namespace SistemaStock.Estilos;

/// <summary>Colores de las series y ejes de los gráficos de Reportes.</summary>
internal static class ChartPalette
{
    public static readonly SKColor Recaudacion = new(242, 112, 42);
    public static readonly SKColor StockActual = new(242, 112, 42);
    public static SKColor StockBajo => new(Theme.StatusDanger.R, Theme.StatusDanger.G, Theme.StatusDanger.B);
    public static readonly SKColor StockMinimo = new(89, 156, 220);
    public static readonly SKColor Texto = new(245, 245, 245);
    public static readonly SKColor TextoSecundario = new(185, 188, 194);
    public static readonly SKColor Linea = new(58, 60, 66);
    public static readonly SKColor Fondo = new(32, 34, 38);
}
