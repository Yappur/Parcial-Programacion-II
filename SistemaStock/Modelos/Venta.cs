namespace SistemaStock.Modelos;

public sealed class Venta
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public decimal Total { get; set; }
}

public sealed class VentaDetalle
{
    public int CodigoProducto { get; set; }
    public string NombreProducto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
