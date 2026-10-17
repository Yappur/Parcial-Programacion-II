using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaStock.Estilos;

namespace SistemaStock.Formularios
{
    public partial class FrmLogin : BaseForm
    {
        private readonly UserRepository repository = new();

        private TextBox txtUsuario = null!;
        private TextBox txtContrasena = null!;
        private Label lblMensaje = null!;

        private readonly Color fondo = Theme.DeepCharcoal;
        private readonly Color panelOscuro = Theme.ObsidianBlack;
        private readonly Color naranja = Theme.BlazeOrange;

        public FrmLogin()
        {
            InitializeComponent();
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            Text = "Chachín";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            ClientSize = new Size(1000, 620);
            BackColor = fondo;
            BackgroundImage = null;

            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            button1.Visible = false;

            Panel panelMarca = new()
            {
                BackColor = Color.FromArgb(170, 17, 18, 20),
                Location = new Point(0, 0),
                Size = new Size(470, 620)
            };
            Controls.Add(panelMarca);

            PictureBox picLogo = new()
            {
                Size = new Size(250, 250),
                Location = new Point(110, 95),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            picLogo.Image = SistemaStock.Properties.Resources.FondoChachin;

            panelMarca.Controls.Add(picLogo);

            Label lblSistema = new()
            {
                Text = "Chachín",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(15, 420),
                Size = new Size(440, 45)
            };
            panelMarca.Controls.Add(lblSistema);

            Panel linea = new()
            {
                BackColor = naranja,
                Location = new Point(190, 450),
                Size = new Size(90, 4)
            };
            panelMarca.Controls.Add(linea);

            Label lblDescripcion = new()
            {
                Text = "Administrá tu negocio de forma simple.",
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 11),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(25, 475),
                Size = new Size(420, 35)
            };
            panelMarca.Controls.Add(lblDescripcion);

            Panel panelLogin = new()
            {
                BackColor = Color.FromArgb(210, 30, 31, 35),
                Location = new Point(525, 65),
                Size = new Size(425, 490)
            };
            Controls.Add(panelLogin);

            Label lblTitulo = new()
            {
                Text = "¡Bienvenido!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                Location = new Point(38, 35),
                Size = new Size(345, 55),
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelLogin.Controls.Add(lblTitulo);

            Label lblSubtitulo = new()
            {
                Text = "Ingresá tus credenciales para continuar",
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 9),
                Location = new Point(38, 100),
                Size = new Size(345, 28),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelLogin.Controls.Add(lblSubtitulo);

            panelLogin.Controls.Add(CrearEtiqueta("USUARIO", 38, 150));

            txtUsuario = new TextBox()
            {
                Location = new Point(38, 180),
                Size = new Size(345, 38),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(43, 44, 49),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelLogin.Controls.Add(txtUsuario);

            panelLogin.Controls.Add(CrearEtiqueta("CONTRASEÑA", 38, 240));

            txtContrasena = new TextBox()
            {
                Location = new Point(38, 270),
                Size = new Size(345, 38),
                Font = new Font("Segoe UI", 12),
                BackColor = Color.FromArgb(43, 44, 49),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            panelLogin.Controls.Add(txtContrasena);

            lblMensaje = new Label()
            {
                Location = new Point(38, 315),
                Size = new Size(345, 35),
                ForeColor = Color.FromArgb(255, 120, 100),
                Font = new Font("Segoe UI", 9)
            };
            panelLogin.Controls.Add(lblMensaje);

            Button btnIngresar = new()
            {
                Text = "INGRESAR",
                Location = new Point(38, 365),
                Size = new Size(345, 48),
                BackColor = naranja,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Click += btnIngresar_Click;
            panelLogin.Controls.Add(btnIngresar);

            AcceptButton = btnIngresar;
        }

        private Label CrearEtiqueta(string texto, int x, int y)
        {
            return new Label()
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            btnIngresar_Click(sender, e);
        }

        private void btnIngresar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsuario.Text) ||
                    string.IsNullOrWhiteSpace(txtContrasena.Text))
                {
                    lblMensaje.Text = "Completá el usuario y la contraseña.";
                    return;
                }

                if (repository.Count() == 0)
                {
                    using FirstAdminForm setup = new();
                    if (setup.ShowDialog(this) != DialogResult.OK)
                        return;

                    MessageBox.Show("Administrador creado. Ahora iniciá sesión.");
                }

                var user = repository.Login(
                    txtUsuario.Text.Trim(),
                    txtContrasena.Text
                );

                if (user is null)
                {
                    lblMensaje.Text = "Usuario o contraseña incorrectos.";
                    return;
                }

                Hide();
                using FrmDashboard home = new(user);
                home.ShowDialog(this);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }

    internal sealed class FirstAdminForm : Form
    {
        private readonly UserRepository repository = new();
        private readonly TextBox txtUsuario = new();
        private readonly TextBox txtNombre = new();
        private readonly TextBox txtPassword = new();

        public FirstAdminForm()
        {
            Text = "Chachín | Crear administrador";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 290);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            AgregarCampo("Usuario", txtUsuario, 20);
            AgregarCampo("Nombre completo", txtNombre, 80);
            AgregarCampo("Contraseña (mínimo 8 caracteres)", txtPassword, 140);
            txtPassword.UseSystemPasswordChar = true;

            Button btnCrear = new()
            {
                Text = "Crear administrador",
                Location = new Point(20, 220),
                Size = new Size(320, 40)
            };
            btnCrear.Click += CrearAdministrador;
            Controls.Add(btnCrear);
        }

        private void AgregarCampo(string titulo, TextBox campo, int y)
        {
            Controls.Add(new Label
            {
                Text = titulo,
                Location = new Point(20, y),
                AutoSize = true
            });

            campo.Location = new Point(20, y + 22);
            campo.Size = new Size(320, 25);
            Controls.Add(campo);
        }

        private void CrearAdministrador(object? sender, EventArgs e)
        {
            if (txtUsuario.Text.Trim().Length < 3 ||
                txtNombre.Text.Trim().Length < 2 ||
                txtPassword.Text.Length < 8)
            {
                MessageBox.Show(
                    "Completá los campos. La contraseña debe tener al menos 8 caracteres."
                );
                return;
            }

            try
            {
                repository.CreateFirstAdmin(
                    txtUsuario.Text.Trim(),
                    txtNombre.Text.Trim(),
                    txtPassword.Text
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}
