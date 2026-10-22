namespace SistemaStock.Modelos;

public enum TipoVenta
{
    Mesa,
    Delivery,
    Mostrador
}

public enum EstadoVenta
{
    Pendiente,
    Cerrada,
    Anulada
}

public enum MedioPago
{
    Efectivo,
    Transferencia
}

public sealed class Venta
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public DateTime? FechaCierre { get; set; }
    public TipoVenta TipoVenta { get; set; }
    public int? MesaId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Total { get; set; }
    public MedioPago? MedioPago { get; set; }
    public EstadoVenta Estado { get; set; }
    public string? Notas { get; set; }
}

public sealed class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int ProductoCodigo { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class VentaPendiente
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = "";
    public decimal Total { get; set; }

    public override string ToString() => Descripcion;
}

public sealed class MesaVenta
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public bool Ocupada { get; set; }
    public string Estado => Ocupada ? "Ocupada" : "Libre";
}
