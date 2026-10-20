using System;
using System.Windows.Forms;
using SistemaStock.Datos;
using SistemaStock.Estilos;
using SistemaStock.Modelos.Usuarios;

namespace SistemaStock.Formularios
{
    public sealed class FrmUsers : BasePage
    {
        private readonly User? currentUser;
        private readonly UserRepository repository = new();
        private readonly DataGridView grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly TextBox search = new() { PlaceholderText = "Buscar usuario o nombre", Width = 220 };

        public FrmUsers() : this(null) { }

        public FrmUsers(User? user)
        {
            currentUser = user;
            Theme.StyleGrid(grid);
            AddPageTitle("Usuarios", "Administración de usuarios del sistema");
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 92,
                Padding = new Padding(0, 8, 0, 8),
                BackColor = Theme.DeepCharcoal,
                WrapContents = true
            };
            if (user is not null)
                top.Controls.Add(new Label { Text = $"Sesión: {user.FullName} | Rol: {user.Role}", AutoSize = true, Padding = new Padding(0, 8, 12, 0), ForeColor = Theme.TextSecondary });
            if (user is not null && UserPermissions.CanManageUsers(user))
            {
                var add = new Button { Text = "Crear usuario", AutoSize = true }; add.Click += (_, _) => CreateUser(); top.Controls.Add(add);
                var edit = new Button { Text = "Modificar / contraseña", AutoSize = true }; edit.Click += (_, _) => EditUser(); top.Controls.Add(edit);
                var deactivate = new Button { Text = "Desactivar", AutoSize = true }; deactivate.Click += (_, _) => DeactivateUser(); top.Controls.Add(deactivate);
                var delete = new Button { Text = "Eliminar usuario", AutoSize = true }; delete.Click += (_, _) => DeleteUser(); top.Controls.Add(delete);
                top.Controls.Add(search); search.TextChanged += (_, _) => RefreshUsers();
            }
            else
                top.Controls.Add(new Label { Text = "Tu rol no permite administrar usuarios.", AutoSize = true, Padding = new Padding(8), ForeColor = Theme.TextSecondary });
            PageContent.Controls.Add(grid);
            PageContent.Controls.Add(top);
            if (user is not null && UserPermissions.CanManageUsers(user)) RefreshUsers();
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
            if (currentUser is not null && user.Id == currentUser.Id) { MessageBox.Show("No podés desactivar tu propia cuenta mientras estás usando el sistema."); return; }
            if (MessageBox.Show($"¿Desactivar a {user.Username}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            { repository.Deactivate(user.Id); RefreshUsers(); }
        }

        private void DeleteUser()
        {
            var user = SelectedUser(); if (user is null) { MessageBox.Show("Seleccioná un usuario."); return; }
            if (currentUser is not null && user.Id == currentUser.Id)
            {
                MessageBox.Show("No podés eliminar tu propia cuenta mientras estás usando el sistema.");
                return;
            }
            if (MessageBox.Show($"¿Eliminar definitivamente al usuario {user.Username}? Esta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            try
            {
                repository.Delete(user.Id);
                RefreshUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar el usuario. Puede estar asociado a registros del sistema.\n" + ex.Message,
                    "Error al eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal sealed class UserEditForm : Form
    {
        private readonly TextBox usernameBox = new() { Width = 280 };
        private readonly TextBox fullNameBox = new() { Width = 280 };
        private readonly TextBox passwordBox = new() { Width = 280, UseSystemPasswordChar = true };
        private readonly ComboBox roleBox = new() { Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox activeBox = new() { Text = "Cuenta activa", Checked = true, AutoSize = true };
        private readonly bool isNew;
        public string Username => usernameBox.Text.Trim();
        public string FullName => fullNameBox.Text.Trim();
        public string Password => passwordBox.Text;
        public string Role => roleBox.Text;

        public UserEditForm(User? user = null)
        {
            isNew = user is null;
            Text = isNew ? "Chachín | Crear usuario" : "Chachín | Modificar usuario";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(390, 380);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            BackColor = Theme.DeepCharcoal;
            ForeColor = Theme.TextPrimary;
            Font = Theme.BodyFont;
            roleBox.Items.AddRange(new object[] { "Administrador", "Supervisor", "Vendedor" });
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 20, 28, 20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Theme.DeepCharcoal
            };
            AgregarCampo(panel, "Nombre de usuario", usernameBox);
            AgregarCampo(panel, "Nombre completo", fullNameBox);
            AgregarCampo(panel, user is null ? "Contraseña" : "Nueva contraseña (vacío = sin cambios)", passwordBox);
            AgregarCampo(panel, "Rol", roleBox);
            if (user is not null)
            {
                activeBox.ForeColor = Theme.TextPrimary;
                activeBox.Margin = new Padding(0, 3, 0, 10);
                panel.Controls.Add(activeBox);
            }
            var save = new Button { Text = "Guardar", Width = 280, Height = 38 };
            save.BackColor = Theme.BlazeOrange;
            save.ForeColor = Theme.TextPrimary;
            save.FlatStyle = FlatStyle.Flat;
            save.FlatAppearance.BorderSize = 0;
            save.Click += Save;
            panel.Controls.Add(save);
            Controls.Add(panel);
            if (user is not null) { usernameBox.Text = user.Username; fullNameBox.Text = user.FullName; roleBox.SelectedItem = user.Role; activeBox.Checked = user.IsActive; }
            else roleBox.SelectedItem = "Vendedor";
        }

        private static void AgregarCampo(FlowLayoutPanel panel, string texto, Control control)
        {
            panel.Controls.Add(new Label { Text = texto, AutoSize = true, ForeColor = Theme.TextPrimary });
            if (control is TextBox textBox)
            {
                textBox.BackColor = Theme.ObsidianBlack;
                textBox.ForeColor = Theme.TextPrimary;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = Theme.ObsidianBlack;
                comboBox.ForeColor = Theme.TextPrimary;
                comboBox.FlatStyle = FlatStyle.Flat;
            }
            control.Margin = new Padding(0, 3, 0, 10);
            panel.Controls.Add(control);
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
