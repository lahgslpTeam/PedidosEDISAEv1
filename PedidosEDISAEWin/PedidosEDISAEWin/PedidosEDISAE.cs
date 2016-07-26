﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PedidosEDISAE;
using System.Configuration;

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
            CargarRuta(ConfigurationManager.AppSettings["RutaDefault"]);
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
            List<string> errores = ProcesadorPedidosEDISAE.ProcesaPedidos(listaArchivos);

            errores = new List<string>();
            errores.Add("Test1");
            errores.Add("TestX");
            errores.Add("Testfg");

            //Mostrar errores al usuario en ventana especial:
            if (errores.Count > 0)
            {
                DialogoErrores errDialog = new DialogoErrores();
                errDialog.Errores = errores;
                errDialog.ShowDialog();
            }

            //Despues de procesar, refrescar los archivos de la ruta nuevamente para eliminar los movidos por el cargador
            CargarRuta(txtRuta.Text);
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            CargarRuta(txtRuta.Text);
        }
    }
}