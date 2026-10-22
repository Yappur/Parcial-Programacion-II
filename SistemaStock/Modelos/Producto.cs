namespace SistemaStock.Modelos;

public enum TipoStock
{
    Directo,
    PorReceta
}

/// <summary>Convierte TipoStock al valor exacto que se guarda en MySQL y viceversa.</summary>
public static class TipoStockMySql
{
    public static string ToDatabase(TipoStock tipo) => tipo switch
    {
        TipoStock.Directo => "DIRECTO",
        TipoStock.PorReceta => "POR_RECETA",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de stock no válido.")
    };

    public static TipoStock FromDatabase(string valor) => valor switch
    {
        "DIRECTO" => TipoStock.Directo,
        "POR_RECETA" => TipoStock.PorReceta,
        _ => throw new InvalidOperationException($"Tipo de stock desconocido en la base: {valor}.")
    };
}

public sealed class Producto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public TipoStock TipoStock { get; set; } = TipoStock.Directo;
    public int? Stock { get; set; }
    public int? StockMinimo { get; set; }
    /// <summary>Stock para mostrar en el listado; se calcula desde productos o recetas y nunca se guarda.</summary>
    public int StockDisponible { get; set; }
    public bool Activo { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = "";
}
