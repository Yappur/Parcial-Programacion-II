using System.Drawing;
using System.Reflection;

namespace SistemaStock.Properties;

/// <summary>Acceso fuertemente tipado a los recursos incrustados del proyecto.</summary>
internal static class Resources
{
    private static readonly Lazy<Bitmap> fondoChachin = new(CargarFondoChachin);
    private static readonly Lazy<Bitmap> barChachinLuzAmbar = new(CargarBarChachinLuzAmbar);

    public static Bitmap FondoChachin => fondoChachin.Value;
    public static Bitmap BarChachinLuzAmbar => barChachinLuzAmbar.Value;

    private static Bitmap CargarFondoChachin()
    {
        const string resourceName = "SistemaStock.Properties.Resources.fondo-chachin.png";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"No se encontró el recurso incrustado '{resourceName}'.");
        using var bitmap = new Bitmap(stream);
        return new Bitmap(bitmap);
    }

    private static Bitmap CargarBarChachinLuzAmbar()
    {
        string imagePath = Path.Combine(AppContext.BaseDirectory, "Recursos", "Bar Chachín en Luz Ámbar.png");
        using var bitmap = new Bitmap(imagePath);
        return new Bitmap(bitmap);
    }
}
