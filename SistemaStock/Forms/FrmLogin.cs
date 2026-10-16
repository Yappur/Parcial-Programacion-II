using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaStock.Forms
{
    public partial class FrmLogin : Form
    {
        private readonly UserRepository repository = new();

        public FrmLogin()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Si no hay usuarios, abrir primer administrador
                if (repository.Count() == 0)
                {
                    using var setup = new FirstAdminForm();
                    if (setup.ShowDialog(this) != DialogResult.OK)
                        return; // no creado
                    MessageBox.Show("Administrador creado. Ahora iniciá sesión.");
                }

                var user = repository.Login(textBox1.Text.Trim(), textBox2.Text);
                if (user is null)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos, o cuenta desactivada.");
                    return;
                }

                Hide();
                using var home = new FrmDashboard(user);
                home.ShowDialog(this);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
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
}
