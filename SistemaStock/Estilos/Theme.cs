using System.Drawing;
using System.Windows.Forms;

namespace SistemaStock.Estilos;

/// <summary>Paleta y estilos compartidos de la aplicación Chachín.</summary>
public static class Theme
{
    public static readonly Color DeepCharcoal = Color.FromArgb(24, 25, 28);
    public static readonly Color ObsidianBlack = Color.FromArgb(17, 18, 20);
    public static readonly Color Surface = Color.FromArgb(32, 34, 38);
    public static readonly Color SurfaceRaised = Color.FromArgb(43, 45, 50);
    public static readonly Color BlazeOrange = Color.FromArgb(242, 112, 42);
    public static readonly Color TextPrimary = Color.FromArgb(245, 245, 245);
    public static readonly Color TextSecondary = Color.FromArgb(185, 188, 194);
    public static readonly Color GridLine = Color.FromArgb(58, 60, 66);

    public static readonly Font BodyFont = new("Segoe UI", 10F, FontStyle.Regular);
    public static readonly Font HeadingFont = new("Segoe UI", 22F, FontStyle.Bold);

    public static void ApplyForm(Form form)
    {
        form.BackColor = DeepCharcoal;
        form.ForeColor = TextPrimary;
        form.Font = BodyFont;
        form.MinimumSize = new Size(850, 500);
        form.StartPosition = FormStartPosition.CenterScreen;
    }

    public static void StyleNavigationButton(Button button, bool active)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = active ? BlazeOrange : ObsidianBlack;
        button.ForeColor = TextPrimary;
        button.Font = new Font("Segoe UI", 10F, active ? FontStyle.Bold : FontStyle.Regular);
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(20, 0, 8, 0);
        button.Cursor = Cursors.Hand;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.GridColor = GridLine;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle.BackColor = ObsidianBlack;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 42;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(74, 52, 40);
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.Font = BodyFont;
        grid.RowHeadersVisible = false;
        grid.RowTemplate.Height = 36;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
    }
}
