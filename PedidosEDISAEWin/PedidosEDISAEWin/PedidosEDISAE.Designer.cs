namespace PedidosEDISAEWin
{
    partial class PedidosEDISAE
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PedidosEDISAE));
            this.btnCargar = new System.Windows.Forms.Button();
            this.lblRuta = new System.Windows.Forms.Label();
            this.txtRuta = new System.Windows.Forms.TextBox();
            this.btnSeleccionRuta = new System.Windows.Forms.Button();
            this.grpListaDeArchivos = new System.Windows.Forms.GroupBox();
            this.listArchivosXml = new System.Windows.Forms.ListView();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.grpListaDeArchivos.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCargar
            // 
            this.btnCargar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCargar.Location = new System.Drawing.Point(432, 253);
            this.btnCargar.Name = "btnCargar";
            this.btnCargar.Size = new System.Drawing.Size(115, 23);
            this.btnCargar.TabIndex = 2;
            this.btnCargar.Text = "Cargar a &SAE";
            this.btnCargar.UseVisualStyleBackColor = true;
            this.btnCargar.Click += new System.EventHandler(this.btnCargar_Click);
            // 
            // lblRuta
            // 
            this.lblRuta.AutoSize = true;
            this.lblRuta.Location = new System.Drawing.Point(12, 12);
            this.lblRuta.Name = "lblRuta";
            this.lblRuta.Size = new System.Drawing.Size(33, 13);
            this.lblRuta.TabIndex = 1;
            this.lblRuta.Text = "Ruta:";
            // 
            // txtRuta
            // 
            this.txtRuta.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRuta.Location = new System.Drawing.Point(51, 12);
            this.txtRuta.Name = "txtRuta";
            this.txtRuta.ReadOnly = true;
            this.txtRuta.Size = new System.Drawing.Size(334, 20);
            this.txtRuta.TabIndex = 2;
            // 
            // btnSeleccionRuta
            // 
            this.btnSeleccionRuta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSeleccionRuta.Location = new System.Drawing.Point(391, 12);
            this.btnSeleccionRuta.Name = "btnSeleccionRuta";
            this.btnSeleccionRuta.Size = new System.Drawing.Size(75, 23);
            this.btnSeleccionRuta.TabIndex = 3;
            this.btnSeleccionRuta.Text = "&Cambiar";
            this.btnSeleccionRuta.UseVisualStyleBackColor = true;
            this.btnSeleccionRuta.Click += new System.EventHandler(this.btnSeleccionRuta_Click);
            // 
            // grpListaDeArchivos
            // 
            this.grpListaDeArchivos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpListaDeArchivos.Controls.Add(this.listArchivosXml);
            this.grpListaDeArchivos.Location = new System.Drawing.Point(15, 41);
            this.grpListaDeArchivos.Name = "grpListaDeArchivos";
            this.grpListaDeArchivos.Size = new System.Drawing.Size(532, 206);
            this.grpListaDeArchivos.TabIndex = 1;
            this.grpListaDeArchivos.TabStop = false;
            this.grpListaDeArchivos.Text = "Archivos a procesar:";
            // 
            // listArchivosXml
            // 
            this.listArchivosXml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listArchivosXml.Location = new System.Drawing.Point(3, 16);
            this.listArchivosXml.Name = "listArchivosXml";
            this.listArchivosXml.Size = new System.Drawing.Size(526, 187);
            this.listArchivosXml.TabIndex = 0;
            this.listArchivosXml.UseCompatibleStateImageBehavior = false;
            this.listArchivosXml.View = System.Windows.Forms.View.List;
            this.listArchivosXml.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listArchivosXml_ItemSelectionChanged);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefrescar.Location = new System.Drawing.Point(472, 12);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(75, 23);
            this.btnRefrescar.TabIndex = 4;
            this.btnRefrescar.Text = "&Refrescar";
            this.btnRefrescar.UseVisualStyleBackColor = true;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            // 
            // PedidosEDISAE
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 288);
            this.Controls.Add(this.btnRefrescar);
            this.Controls.Add(this.grpListaDeArchivos);
            this.Controls.Add(this.btnSeleccionRuta);
            this.Controls.Add(this.txtRuta);
            this.Controls.Add(this.lblRuta);
            this.Controls.Add(this.btnCargar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PedidosEDISAE";
            this.Text = "Carga de Pedidos EDI - SAE para Windows - Artlux S.A. de C.V.";
            this.Load += new System.EventHandler(this.PedidosEDISAE_Load);
            this.grpListaDeArchivos.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCargar;
        private System.Windows.Forms.Label lblRuta;
        private System.Windows.Forms.TextBox txtRuta;
        private System.Windows.Forms.Button btnSeleccionRuta;
        private System.Windows.Forms.GroupBox grpListaDeArchivos;
        private System.Windows.Forms.ListView listArchivosXml;
        private System.Windows.Forms.Button btnRefrescar;
    }
}

