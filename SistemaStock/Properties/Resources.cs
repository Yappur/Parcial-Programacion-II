using System.Drawing;
using System.Reflection;

namespace SistemaStock.Properties;

/// <summary>Acceso fuertemente tipado a los recursos incrustados del proyecto.</summary>
internal static class Resources
{
    private static readonly Lazy<Bitmap> fondoChachin = new(CargarFondoChachin);

    public static Bitmap FondoChachin => fondoChachin.Value;

    private static Bitmap CargarFondoChachin()
    {
        const string resourceName = "SistemaStock.Properties.Resources.fondo-chachin.png";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"No se encontró el recurso incrustado '{resourceName}'.");
        using var bitmap = new Bitmap(stream);
        return new Bitmap(bitmap);
    }
}
