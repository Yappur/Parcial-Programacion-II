using System.Drawing;
using System.Windows.Forms;
using FontAwesome.Sharp;

namespace SistemaStock.Estilos;

/// <summary>Paleta y estilos compartidos de la aplicación Chachín.</summary>
public static class Theme
{
    public static readonly Color DeepCharcoal = Color.FromArgb(24, 25, 28);
    public static readonly Color ObsidianBlack = Color.FromArgb(17, 18, 20);
    public static readonly Color Surface = Color.FromArgb(32, 34, 38);
    public static readonly Color SurfaceRaised = Color.FromArgb(43, 45, 50);
    public static readonly Color BlazeOrange = Color.FromArgb(242, 112, 42);
    public static readonly Color Danger = Color.FromArgb(190, 55, 48);
    public static readonly Color Warning = Color.FromArgb(225, 168, 54);
    public static readonly Color TextPrimary = Color.FromArgb(245, 245, 245);
    public static readonly Color TextSecondary = Color.FromArgb(185, 188, 194);
    public static readonly Color GridLine = Color.FromArgb(58, 60, 66);

    public static readonly Font BodyFont = new("Segoe UI", 10F, FontStyle.Regular);
    public static readonly Font HeadingFont = new("Segoe UI", 22F, FontStyle.Bold);
    private static readonly Font NavigationFont = new("Segoe UI", 10F, FontStyle.Regular);
    private static readonly Font ActiveNavigationFont = new("Segoe UI", 10F, FontStyle.Bold);
    private static readonly Font GridHeaderFont = new("Segoe UI", 9F, FontStyle.Bold);
    private static readonly Color PrimaryHover = Color.FromArgb(255, 137, 76);
    private static readonly Color DangerHover = Color.FromArgb(215, 75, 66);
    private static readonly Color WarningHover = Color.FromArgb(239, 187, 76);

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
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = 0;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(20, 0, 8, 0);
        button.Cursor = Cursors.Hand;
        button.Tag = active;
        button.MouseEnter -= NavigationButtonMouseEnter;
        button.MouseEnter += NavigationButtonMouseEnter;
        button.MouseLeave -= NavigationButtonMouseLeave;
        button.MouseLeave += NavigationButtonMouseLeave;
        ApplyNavigationButtonState(button, active, false);
    }

    private static void NavigationButtonMouseEnter(object? sender, EventArgs e)
    {
        if (sender is Button button)
            ApplyNavigationButtonState(button, button.Tag is true, true);
    }

    private static void NavigationButtonMouseLeave(object? sender, EventArgs e)
    {
        if (sender is Button button)
            ApplyNavigationButtonState(button, button.Tag is true, false);
    }

    private static void ApplyNavigationButtonState(Button button, bool active, bool hovered)
    {
        button.FlatAppearance.MouseOverBackColor = active ? PrimaryHover : SurfaceRaised;
        button.FlatAppearance.MouseDownBackColor = active ? PrimaryHover : SurfaceRaised;
        button.BackColor = active ? (hovered ? PrimaryHover : BlazeOrange) : (hovered ? SurfaceRaised : ObsidianBlack);
        button.ForeColor = active || hovered ? TextPrimary : TextSecondary;
        button.Font = active ? ActiveNavigationFont : NavigationFont;
        if (button is IconButton iconButton)
            iconButton.IconColor = active || hovered ? TextPrimary : TextSecondary;
    }

    public static void EstilizarBoton(Button button)
    {
        EstilizarBotonSecundario(button);
    }

    public static void EstilizarBotonPrimario(Button button)
    {
        StyleActionButton(button, BlazeOrange, PrimaryHover, 0);
    }

    public static void EstilizarBotonSecundario(Button button)
    {
        StyleActionButton(button, Surface, SurfaceRaised, 1);
    }

    public static void EstilizarBotonPeligro(Button button)
    {
        StyleActionButton(button, Danger, DangerHover, 0);
    }

    public static void EstilizarBotonAdvertencia(Button button)
    {
        StyleActionButton(button, Warning, WarningHover, 0);
    }

    public static void EstilizarBotonCerrarSesion(Button button)
    {
        StyleActionButton(button, BlazeOrange, PrimaryHover, 1);
        button.FlatAppearance.BorderColor = GridLine;
    }

    public static void SetButtonIcon(IconButton button, IconChar icon, int size = 18)
    {
        button.IconChar = icon;
        button.IconSize = size;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextAlign = ContentAlignment.MiddleCenter;
    }

    private static void StyleActionButton(Button button, Color background, Color hover, int borderSize)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;
        button.FlatAppearance.BorderSize = borderSize;
        button.FlatAppearance.BorderColor = GridLine;
        button.FlatAppearance.MouseOverBackColor = hover;
        button.FlatAppearance.MouseDownBackColor = hover;
        button.BackColor = background;
        button.ForeColor = TextPrimary;
        button.Font = BodyFont;
        button.Cursor = Cursors.Hand;
        if (button is IconButton iconButton)
        {
            iconButton.IconColor = TextPrimary;
            iconButton.IconSize = 18;
            iconButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton.TextAlign = ContentAlignment.MiddleCenter;
        }
        button.EnabledChanged -= UpdateButtonEnabledStyle;
        button.EnabledChanged += UpdateButtonEnabledStyle;
        UpdateButtonEnabledStyle(button, EventArgs.Empty);
    }

    private static void UpdateButtonEnabledStyle(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.ForeColor = button.Enabled ? TextPrimary : TextSecondary;
            if (button is IconButton iconButton)
                iconButton.IconColor = button.Enabled ? TextPrimary : TextSecondary;
        }
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
        grid.ColumnHeadersDefaultCellStyle.Font = GridHeaderFont;
        grid.ColumnHeadersVisible = true;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
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
