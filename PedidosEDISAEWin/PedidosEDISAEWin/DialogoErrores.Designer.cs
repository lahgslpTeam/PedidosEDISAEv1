namespace PedidosEDISAEWin
{
    partial class DialogoErrores
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtErrores = new System.Windows.Forms.TextBox();
            this.btnAceptar = new System.Windows.Forms.Button();
            this.lblErrores = new System.Windows.Forms.Label();
            this.linkCarpetaRegistro = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.txtErrores);
            this.panel1.Location = new System.Drawing.Point(12, 25);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(482, 141);
            this.panel1.TabIndex = 0;
            // 
            // txtErrores
            // 
            this.txtErrores.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtErrores.Location = new System.Drawing.Point(0, 0);
            this.txtErrores.Multiline = true;
            this.txtErrores.Name = "txtErrores";
            this.txtErrores.Size = new System.Drawing.Size(482, 141);
            this.txtErrores.TabIndex = 1;
            // 
            // btnAceptar
            // 
            this.btnAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAceptar.Location = new System.Drawing.Point(419, 172);
            this.btnAceptar.Name = "btnAceptar";
            this.btnAceptar.Size = new System.Drawing.Size(75, 23);
            this.btnAceptar.TabIndex = 1;
            this.btnAceptar.Text = "&Aceptar";
            this.btnAceptar.UseVisualStyleBackColor = true;
            this.btnAceptar.Click += new System.EventHandler(this.btnAceptar_Click);
            // 
            // lblErrores
            // 
            this.lblErrores.AutoSize = true;
            this.lblErrores.Location = new System.Drawing.Point(12, 9);
            this.lblErrores.Name = "lblErrores";
            this.lblErrores.Size = new System.Drawing.Size(398, 13);
            this.lblErrores.TabIndex = 0;
            this.lblErrores.Text = "Se han encontrado los siguientes errores durante el procesamiento de los archivos" +
                ":";
            // 
            // linkCarpetaRegistro
            // 
            this.linkCarpetaRegistro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkCarpetaRegistro.AutoSize = true;
            this.linkCarpetaRegistro.Location = new System.Drawing.Point(12, 172);
            this.linkCarpetaRegistro.Name = "linkCarpetaRegistro";
            this.linkCarpetaRegistro.Size = new System.Drawing.Size(170, 13);
            this.linkCarpetaRegistro.TabIndex = 2;
            this.linkCarpetaRegistro.TabStop = true;
            this.linkCarpetaRegistro.Text = "Ver carpeta de registro de eventos";
            this.linkCarpetaRegistro.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCarpetaRegistro_LinkClicked);
            // 
            // DialogoErrores
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 207);
            this.Controls.Add(this.linkCarpetaRegistro);
            this.Controls.Add(this.btnAceptar);
            this.Controls.Add(this.lblErrores);
            this.Controls.Add(this.panel1);
            this.Name = "DialogoErrores";
            this.Text = "Errores en la carga de archivos";
            this.Load += new System.EventHandler(this.DialogoErrores_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnAceptar;
        private System.Windows.Forms.TextBox txtErrores;
        private System.Windows.Forms.Label lblErrores;
        private System.Windows.Forms.LinkLabel linkCarpetaRegistro;
    }
}