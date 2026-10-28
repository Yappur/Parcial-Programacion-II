using System.Drawing;
using System.Runtime.CompilerServices;
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
    private static readonly Color InputSelection = Color.FromArgb(74, 52, 40);
    private static readonly Color GridRowAlternate = Color.FromArgb(48, 52, 59);
    private static readonly Color GridRowHover = Color.FromArgb(87, 59, 43);
    private const int InputHeight = 28;
    private static readonly ConditionalWeakTable<DataGridView, GridHoverState> GridHoverStates = new();
    private static readonly ConditionalWeakTable<Control, ThemedInputBorder> ThemedInputBorders = new();

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
        StyleActionButton(button, Surface, SurfaceRaised, 1);
        button.FlatAppearance.BorderColor = BlazeOrange;
        if (button is IconButton iconButton)
            CenterIconButtonContents(iconButton);
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = Surface;
        textBox.ForeColor = TextPrimary;
        textBox.Font = BodyFont;
        textBox.AutoSize = false;
        textBox.Multiline = false;
        textBox.Height = InputHeight;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        ApplyThemedInputBorder(textBox);
    }

    public static void StyleComboBox(ComboBox comboBox, bool keepEditable = false)
    {
        comboBox.DropDownStyle = keepEditable ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Surface;
        comboBox.ForeColor = TextPrimary;
        comboBox.Font = BodyFont;
        comboBox.Height = InputHeight;
        ApplyThemedInputBorder(comboBox);
        comboBox.DrawItem -= DrawThemedComboItem;
        if (keepEditable)
        {
            comboBox.DrawMode = DrawMode.Normal;
        }
        else
        {
            comboBox.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox.ItemHeight = 22;
            comboBox.DrawItem += DrawThemedComboItem;
        }
    }

    private static void DrawThemedComboItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ComboBox comboBox || e.Index < 0)
            return;

        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color background = selected ? InputSelection : Surface;
        using (var brush = new SolidBrush(background))
            e.Graphics.FillRectangle(brush, e.Bounds);

        TextRenderer.DrawText(e.Graphics, comboBox.GetItemText(comboBox.Items[e.Index]), comboBox.Font,
            Rectangle.Inflate(e.Bounds, -8, 0), TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
            e.DrawFocusRectangle();
    }

    public static void SetButtonIcon(IconButton button, IconChar icon, int size = 18)
    {
        button.IconChar = icon;
        button.IconSize = size;
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextAlign = ContentAlignment.MiddleCenter;
    }

    public static void CenterIconButtonContents(IconButton button)
    {
        button.SizeChanged -= CenterIconButtonContentsChanged;
        button.SizeChanged += CenterIconButtonContentsChanged;
        button.TextChanged -= CenterIconButtonContentsChanged;
        button.TextChanged += CenterIconButtonContentsChanged;
        button.FontChanged -= CenterIconButtonContentsChanged;
        button.FontChanged += CenterIconButtonContentsChanged;
        ApplyCenteredIconButtonLayout(button);
    }

    private static void CenterIconButtonContentsChanged(object? sender, EventArgs e)
    {
        if (sender is IconButton button)
            ApplyCenteredIconButtonLayout(button);
    }

    private static void ApplyCenteredIconButtonLayout(IconButton button)
    {
        int textWidth = TextRenderer.MeasureText(button.Text, button.Font).Width;
        int groupWidth = button.IconSize + 4 + textWidth;
        int sidePadding = Math.Max(0, (button.ClientSize.Width - groupWidth) / 2);
        // Use only left padding: symmetric padding leaves too little room for the text
        // in IconButton's ImageBeforeText layout and can make it wrap to a second line.
        button.Padding = new Padding(sidePadding, 0, 0, 0);
        button.TextImageRelation = TextImageRelation.ImageBeforeText;
        button.ImageAlign = ContentAlignment.MiddleLeft;
        button.TextAlign = ContentAlignment.MiddleLeft;
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
        grid.AlternatingRowsDefaultCellStyle.BackColor = GridRowAlternate;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.SelectionBackColor = InputSelection;
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
        _ = GridHoverStates.GetValue(grid, _ => new GridHoverState());
        grid.MouseMove -= GridMouseMove;
        grid.MouseMove += GridMouseMove;
        grid.MouseLeave -= GridMouseLeave;
        grid.MouseLeave += GridMouseLeave;
        grid.RowPrePaint -= GridRowPrePaint;
        grid.RowPrePaint += GridRowPrePaint;
        grid.Invalidate();
    }

    private static void GridMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not DataGridView grid || !GridHoverStates.TryGetValue(grid, out GridHoverState state))
            return;

        int previousRow = state.HoveredRow;
        state.HoveredRow = grid.HitTest(e.X, e.Y).RowIndex;
        if (previousRow >= 0 && previousRow < grid.Rows.Count)
            grid.InvalidateRow(previousRow);
        if (state.HoveredRow >= 0 && state.HoveredRow < grid.Rows.Count)
            grid.InvalidateRow(state.HoveredRow);
    }

    private static void GridMouseLeave(object? sender, EventArgs e)
    {
        if (sender is not DataGridView grid || !GridHoverStates.TryGetValue(grid, out GridHoverState state))
            return;

        int previousRow = state.HoveredRow;
        state.HoveredRow = -1;
        if (previousRow >= 0 && previousRow < grid.Rows.Count)
            grid.InvalidateRow(previousRow);
    }

    private static void GridRowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 ||
            !GridHoverStates.TryGetValue(grid, out GridHoverState state))
            return;

        e.InheritedRowStyle.BackColor = e.RowIndex == state.HoveredRow
            ? GridRowHover
            : e.RowIndex % 2 == 0 ? Surface : GridRowAlternate;
    }

    private sealed class GridHoverState
    {
        public int HoveredRow { get; set; } = -1;
    }

    private static void ApplyThemedInputBorder(Control control)
    {
        _ = ThemedInputBorders.GetValue(control, input => new ThemedInputBorder(input));
    }

    private sealed class ThemedInputBorder : NativeWindow
    {
        private const int WmPaint = 0x000F;
        private const int WmNcPaint = 0x0085;
        private readonly Control control;

        public ThemedInputBorder(Control control)
        {
            this.control = control;
            control.HandleCreated += ControlHandleCreated;
            control.HandleDestroyed += ControlHandleDestroyed;
            control.Disposed += ControlDisposed;
            if (control.IsHandleCreated)
                AssignHandle(control.Handle);
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            if ((message.Msg == WmPaint || message.Msg == WmNcPaint) &&
                !control.IsDisposed && control.IsHandleCreated && control.Width > 1 && control.Height > 1)
            {
                using Graphics graphics = Graphics.FromHwnd(Handle);
                using Pen border = new(GridLine, 1F);
                graphics.DrawRectangle(border, 0, 0, control.Width - 1, control.Height - 1);
            }
        }

        private void ControlHandleCreated(object? sender, EventArgs e)
        {
            if (control.IsHandleCreated && Handle == IntPtr.Zero)
                AssignHandle(control.Handle);
        }

        private void ControlHandleDestroyed(object? sender, EventArgs e)
        {
            if (Handle != IntPtr.Zero)
                ReleaseHandle();
        }

        private void ControlDisposed(object? sender, EventArgs e)
        {
            control.HandleCreated -= ControlHandleCreated;
            control.HandleDestroyed -= ControlHandleDestroyed;
            control.Disposed -= ControlDisposed;
            if (Handle != IntPtr.Zero)
                ReleaseHandle();
        }
    }
}
