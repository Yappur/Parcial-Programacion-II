using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SistemaStock.Forms
{
    public partial class FrmDashboard : Form
    {
        private User? currentUser;

        public FrmDashboard()
        {
            InitializeComponent();
        }

        public FrmDashboard(User user) : this()
        {
            currentUser = user;
            // Añade botón para Gestión de Usuarios si el rol lo permite
            if (UserPermissions.CanManageUsers(user))
            {
                var btn = new Button { Text = "Gestión de Usuarios", Width = 140, Height = 30, Location = new Point(12, 12) };
                btn.Click += (_, _) => { using var form = new FrmUsers(user); form.ShowDialog(this); };
                Controls.Add(btn);
            }
        }
    }
}
