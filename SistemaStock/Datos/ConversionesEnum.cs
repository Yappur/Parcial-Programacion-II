using SistemaStock.Modelos;

namespace SistemaStock.Datos;

internal static class ConversionesEnum
{
    public static string ADb(this TipoVenta valor) => valor switch
    {
        TipoVenta.Mesa => "MESA",
        TipoVenta.Delivery => "DELIVERY",
        TipoVenta.Mostrador => "MOSTRADOR",
        _ => throw new ArgumentOutOfRangeException(nameof(valor))
    };

    public static TipoVenta ATipoVenta(string valor) => valor switch
    {
        "MESA" => TipoVenta.Mesa,
        "DELIVERY" => TipoVenta.Delivery,
        "MOSTRADOR" => TipoVenta.Mostrador,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, "Tipo de venta desconocido.")
    };

    public static string ADb(this EstadoVenta valor) => valor switch
    {
        EstadoVenta.Pendiente => "PENDIENTE",
        EstadoVenta.Cerrada => "CERRADA",
        EstadoVenta.Anulada => "ANULADA",
        _ => throw new ArgumentOutOfRangeException(nameof(valor))
    };

    public static EstadoVenta AEstadoVenta(string valor) => valor switch
    {
        "PENDIENTE" => EstadoVenta.Pendiente,
        "CERRADA" => EstadoVenta.Cerrada,
        "ANULADA" => EstadoVenta.Anulada,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, "Estado de venta desconocido.")
    };

    public static string ADb(this MedioPago valor) => valor switch
    {
        MedioPago.Efectivo => "EFECTIVO",
        MedioPago.Transferencia => "TRANSFERENCIA",
        _ => throw new ArgumentOutOfRangeException(nameof(valor))
    };

    public static MedioPago AMedioPago(string valor) => valor switch
    {
        "EFECTIVO" => MedioPago.Efectivo,
        "TRANSFERENCIA" => MedioPago.Transferencia,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, "Medio de pago desconocido.")
    };
}
