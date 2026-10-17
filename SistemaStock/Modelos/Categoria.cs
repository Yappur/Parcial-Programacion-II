namespace SistemaStock.Modelos;

public sealed class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";

    public override string ToString() => Nombre;
}
