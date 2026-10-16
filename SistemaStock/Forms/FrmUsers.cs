using System;
using System.Windows.Forms;

namespace SistemaStock.Forms
{
    internal sealed class FrmUsers : Form
    {
        private readonly User currentUser;
        private readonly UserRepository repository = new();
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly TextBox search = new() { PlaceholderText = "Buscar usuario o nombre", Width = 220 };

        public FrmUsers(User user)
        {
            currentUser = user;
            Text = $"Gestión de usuarios - {user.FullName} ({user.Role})"; Width = 900; Height = 560; StartPosition = FormStartPosition.CenterScreen;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8) };
            top.Controls.Add(new Label { Text = $"Sesión: {user.FullName} | Rol: {user.Role}", AutoSize = true, Padding = new Padding(0, 8, 12, 0) });
            if (UserPermissions.CanManageUsers(user))
            {
                var add = new Button { Text = "Crear usuario" }; add.Click += (_, _) => CreateUser(); top.Controls.Add(add);
                var edit = new Button { Text = "Modificar / contraseña" }; edit.Click += (_, _) => EditUser(); top.Controls.Add(edit);
                var deactivate = new Button { Text = "Desactivar" }; deactivate.Click += (_, _) => DeactivateUser(); top.Controls.Add(deactivate);
                top.Controls.Add(search); search.TextChanged += (_, _) => RefreshUsers();
            }
            else
                top.Controls.Add(new Label { Text = "Tu rol no permite administrar usuarios.", AutoSize = true, Padding = new Padding(8) });
            Controls.Add(grid); Controls.Add(top);
            if (UserPermissions.CanManageUsers(user)) RefreshUsers();
            else grid.Visible = false;
        }

        private User? SelectedUser() => grid.CurrentRow?.DataBoundItem as User;
        private void RefreshUsers() => grid.DataSource = repository.GetAll(search.Text);

        private void CreateUser()
        {
            using var form = new UserEditForm();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                try { repository.Create(form.Username, form.FullName, form.Password, form.Role); RefreshUsers(); }
                catch (Exception ex) { MessageBox.Show("No se pudo crear el usuario. El nombre podría estar repetido.\n" + ex.Message); }
            }
        }

        private void EditUser()
        {
            var user = SelectedUser(); if (user is null) { MessageBox.Show("Seleccioná un usuario."); return; }
            using var form = new UserEditForm(user);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                try { user.Username = form.Username; user.FullName = form.FullName; user.Role = form.Role; user.IsActive = form.IsActive; repository.Update(user, form.Password); RefreshUsers(); }
                catch (Exception ex) { MessageBox.Show("No se pudo modificar el usuario.\n" + ex.Message); }
            }
        }

        private void DeactivateUser()
        {
            var user = SelectedUser(); if (user is null) { MessageBox.Show("Seleccioná un usuario."); return; }
            if (user.Id == currentUser.Id) { MessageBox.Show("No podés desactivar tu propia cuenta mientras estás usando el sistema."); return; }
            if (MessageBox.Show($"¿Desactivar a {user.Username}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { repository.Deactivate(user.Id); RefreshUsers(); }
        }
    }

    internal sealed class UserEditForm : Form
    {
        private readonly TextBox usernameBox = new() { Width = 260 };
        private readonly TextBox fullNameBox = new() { Width = 260 };
        private readonly TextBox passwordBox = new() { Width = 260, UseSystemPasswordChar = true };
        private readonly ComboBox roleBox = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox activeBox = new() { Text = "Cuenta activa", Checked = true, AutoSize = true };
        private readonly bool isNew;
        public string Username => usernameBox.Text.Trim();
        public string FullName => fullNameBox.Text.Trim();
        public string Password => passwordBox.Text;
        public string Role => roleBox.Text;

        public UserEditForm(User? user = null)
        {
            isNew = user is null;
            Text = isNew ? "Crear usuario" : "Modificar usuario"; Width = 380; Height = user is null ? 320 : 360; StartPosition = FormStartPosition.CenterParent;
            roleBox.Items.AddRange(new object[] { "Administrador", "Supervisor", "Vendedor" });
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            panel.Controls.Add(new Label { Text = "Nombre de usuario", AutoSize = true }); panel.Controls.Add(usernameBox);
            panel.Controls.Add(new Label { Text = "Nombre completo", AutoSize = true }); panel.Controls.Add(fullNameBox);
            panel.Controls.Add(new Label { Text = user is null ? "Contraseña" : "Nueva contraseña (vacío = sin cambios)", AutoSize = true }); panel.Controls.Add(passwordBox);
            panel.Controls.Add(new Label { Text = "Rol", AutoSize = true }); panel.Controls.Add(roleBox);
            if (user is not null) panel.Controls.Add(activeBox);
            var save = new Button { Text = "Guardar", Width = 260, Height = 34 }; save.Click += Save; panel.Controls.Add(save); Controls.Add(panel);
            if (user is not null) { usernameBox.Text = user.Username; fullNameBox.Text = user.FullName; roleBox.SelectedItem = user.Role; activeBox.Checked = user.IsActive; }
            else roleBox.SelectedItem = "Vendedor";
        }

        private void Save(object? sender, EventArgs e)
        {
            if (Username.Length < 3 || FullName.Length < 2 || string.IsNullOrWhiteSpace(Role) || (isNew && Password.Length < 8) || (!string.IsNullOrEmpty(Password) && Password.Length < 8))
            { MessageBox.Show("Completá usuario, nombre y rol. La contraseña debe tener al menos 8 caracteres."); return; }
            DialogResult = DialogResult.OK; Close();
        }

        public bool IsActive => activeBox.Checked;
    }
}
