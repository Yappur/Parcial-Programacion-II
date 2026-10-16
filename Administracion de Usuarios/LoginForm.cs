namespace GestionVentas;

internal sealed class LoginForm : Form
{
    private readonly UserRepository repository = new();
    private readonly TextBox username = new() { PlaceholderText = "Usuario" };
    private readonly TextBox password = new() { PlaceholderText = "Contraseña", UseSystemPasswordChar = true };

    public LoginForm()
    {
        Text = "Iniciar sesión - Usuarios";
        ClientSize = new Size(420, 380);
        StartPosition = FormStartPosition.CenterScreen;

        var titulo = new Label
        {
            Text = "CHACHIN",
            Font = new Font("Rye", 30, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(360, 60),
            Location = new Point(30, 40)
        };

        username.Size = new Size(260, 32);
        username.Location = new Point(80, 145);
        username.TextAlign = HorizontalAlignment.Center;

        password.Size = new Size(260, 32);
        password.Location = new Point(80, 195);
        password.TextAlign = HorizontalAlignment.Center;

        var button = new Button
        {
            Text = "Ingresar",
            Size = new Size(260, 44),
            Location = new Point(80, 285)
        };

        button.Click += Login;

        Controls.Add(titulo);
        Controls.Add(username);
        Controls.Add(password);
        Controls.Add(button);
        AcceptButton = button;
        Shown += (_, _) =>
        {
            if (repository.Count() == 0)
            {
                using var setup = new FirstAdminForm();
                if (setup.ShowDialog(this) != DialogResult.OK) Close();
                else MessageBox.Show("Administrador creado. Ahora iniciá sesión.");
            }
        };
    }

    private void Login(object? sender, EventArgs e)
    {
        try
        {
            var user = repository.Login(username.Text.Trim(), password.Text);
            if (user is null) { MessageBox.Show("Usuario o contraseña incorrectos, o cuenta desactivada."); return; }
            Hide();
            using (var home = new MainForm(user)) home.ShowDialog();
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }
}

internal sealed class FirstAdminForm : Form
{
    private readonly TextBox username = new() { PlaceholderText = "Usuario administrador" };
    private readonly TextBox fullName = new() { PlaceholderText = "Nombre completo" };
    private readonly TextBox password = new() { PlaceholderText = "Contraseña (8 caracteres o más)", UseSystemPasswordChar = true };
    public FirstAdminForm()
    {
        Text = "Primera configuración"; Width = 420; Height = 270; StartPosition = FormStartPosition.CenterParent;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), FlowDirection = FlowDirection.TopDown, WrapContents = false };
        panel.Controls.Add(new Label { Text = "Crear el primer administrador", AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) });
        foreach (var box in new[] { username, fullName, password }) { box.Width = 350; panel.Controls.Add(box); }
        var save = new Button { Text = "Crear administrador", Width = 350, Height = 35 };
        save.Click += (_, _) =>
        {
            if (username.Text.Trim().Length < 3 || fullName.Text.Trim().Length < 2 || password.Text.Length < 8) { MessageBox.Show("Completá los campos; la contraseña necesita al menos 8 caracteres."); return; }
            try { new UserRepository().CreateFirstAdmin(username.Text.Trim(), fullName.Text.Trim(), password.Text); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { MessageBox.Show("No se pudo crear el usuario. Revisá si el nombre ya existe.\n" + ex.Message); }
        };
        panel.Controls.Add(save); Controls.Add(panel);
    }
}
