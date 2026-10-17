namespace SistemaStock.Modelos;

public sealed class Producto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = "";
}
