namespace SistemaStock.Modelos;

/// <summary>Una fila de la receta de un producto.</summary>
public sealed class Receta
{
    public int ProductoCodigo { get; set; }
    public int IngredienteId { get; set; }
    public string IngredienteNombre { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Cantidad { get; set; }
}
