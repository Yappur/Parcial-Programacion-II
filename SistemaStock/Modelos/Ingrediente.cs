namespace SistemaStock.Modelos;

public sealed class Ingrediente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Unidad { get; set; } = "un";
    public decimal Stock { get; set; }
    public decimal StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
}
