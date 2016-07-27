using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace PedidosEDISAEWin
{
    public partial class DialogoErrores : Form
    {
        public List<string> Errores { get;  set; }

        public DialogoErrores()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkCarpetaRegistro_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\" + "PedidosEDISAE");
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void DialogoErrores_Load(object sender, EventArgs e)
        {
            foreach (string error in Errores)
            {
                this.txtErrores.Text += error + Environment.NewLine;
            }
        }
    }
}
