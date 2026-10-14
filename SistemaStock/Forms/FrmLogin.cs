using SistemaStock.Forms;

namespace SistemaStock
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        // Boton que redirecciona al otro formulario
        private void button1_Click(object sender, EventArgs e)
        {
            FrmDashboard dashboard = new FrmDashboard();

            this.Hide();
            dashboard.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
