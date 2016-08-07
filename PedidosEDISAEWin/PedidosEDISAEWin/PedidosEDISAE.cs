using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PedidosEDISAE;
using System.Configuration;
using System.Diagnostics;

namespace PedidosEDISAEWin
{
    public partial class PedidosEDISAE : Form
    {
        public PedidosEDISAE()
        {
            InitializeComponent();
        }

        private void btnSeleccionRuta_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog selRuta = new FolderBrowserDialog();
            selRuta.SelectedPath = txtRuta.Text;
            if (selRuta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CargarRuta(selRuta.SelectedPath);
            }
        }

        private void PedidosEDISAE_Load(object sender, EventArgs e)
        {
            string ruta = ConfigurationManager.AppSettings["RutaDefault"];
            if (System.IO.Directory.Exists(ruta))
            {
                CargarRuta(ruta);
            }
            else
            {
                MessageBox.Show("Ruta pre-configurada no existe: '" + ruta + "'. Revisar configuracion");
            }
        }

        private void ListToListView(List<string> lista)
        {
            foreach (string s in lista)
            {
                listArchivosXml.Items.Add(s);
            }
        }

        private void listArchivosXml_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            int cuentaSeleccionados = 0;
            foreach (ListViewItem item in listArchivosXml.Items)
            {
                if (item.Selected)
                {
                    cuentaSeleccionados++;
                }
            }
            if (cuentaSeleccionados == 0)
            {
                btnCargar.Enabled = false;
            }
            else
            {
                btnCargar.Enabled = true;
            }
        }

        private void CargarRuta(string Ruta)
        {
            txtRuta.Text = Ruta;
            listArchivosXml.Items.Clear();
            ListToListView(AdminArchivos.ObtenerArchivosXML(Ruta));
            btnCargar.Enabled = false;
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            btnCargar.Enabled = false;

            //Obtener lista de archivos seleccionados por el usuario
            List<string> listaArchivos = new List<string>();
            foreach (ListViewItem item in listArchivosXml.Items)
            {
                if (item.Selected == true)
                {
                    listaArchivos.Add(txtRuta.Text + "\\" + item.Text);
                }
            }

            //Llamada a proceso de carga de archivos
            IRegistroEjecucion registro = ProcesadorPedidosEDISAE.ProcesaPedidos(listaArchivos);

            //Mostrar errores al usuario en ventana especial:
            if (registro.Advertencias().Count > 0 || registro.Errores().Count > 0)
            {
                DialogoErrores errDialog = new DialogoErrores();
                errDialog.Advertencias = registro.Advertencias();
                errDialog.Errores = registro.Errores();
                errDialog.ShowDialog();
            }

            //Despues de procesar, refrescar los archivos de la ruta nuevamente para eliminar los movidos por el cargador
            CargarRuta(txtRuta.Text);
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarRuta(txtRuta.Text);
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
    }
}
