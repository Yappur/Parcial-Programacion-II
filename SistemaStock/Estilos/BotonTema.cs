using System.Drawing;
using System.Windows.Forms;

namespace SistemaStock.Estilos;

/// <summary>Botón plano que conserva texto y borde legibles también deshabilitado.</summary>
public sealed class BotonTema : Button
{
    private bool mouseSobre;

    public BotonTema()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        mouseSobre = true;
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        mouseSobre = false;
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Color fondo = mouseSobre && Enabled ? FlatAppearance.MouseOverBackColor : BackColor;
        Color texto = Enabled ? Theme.TextPrimary : Theme.TextSecondary;
        using (var brush = new SolidBrush(fondo)) e.Graphics.FillRectangle(brush, ClientRectangle);
        if (FlatAppearance.BorderSize > 0)
            using (var pen = new Pen(FlatAppearance.BorderColor, FlatAppearance.BorderSize))
                e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
        TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, texto,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (Focused && ShowFocusCues)
        {
            Rectangle focus = Rectangle.Inflate(ClientRectangle, -4, -4);
            ControlPaint.DrawFocusRectangle(e.Graphics, focus, texto, fondo);
        }
    }
}
