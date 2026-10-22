namespace SistemaStock.Modelos;

public sealed class Mesa
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public bool Activo { get; set; } = true;
}
