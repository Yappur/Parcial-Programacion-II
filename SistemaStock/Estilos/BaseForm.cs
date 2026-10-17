using System.Drawing;
using System.Runtime.InteropServices;

namespace SistemaStock.Estilos;

public partial class BaseForm : Form
{
    public BaseForm()
    {
        InitializeComponent();
        Theme.ApplyForm(this);
        ClientSize = new Size(1100, 720);
        Text = "Chachín";
        MinimumSize = new Size(850, 500);
        MaximumSize = Screen.PrimaryScreen?.Bounds.Size ?? Size.Empty;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        Icon = CreateWindowIcon(SistemaStock.Properties.Resources.FondoChachin);
    }

    private static Icon CreateWindowIcon(Image image)
    {
        IntPtr handle = ((Bitmap)image).GetHicon();
        try
        {
            using Icon temporaryIcon = Icon.FromHandle(handle);
            return (Icon)temporaryIcon.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
