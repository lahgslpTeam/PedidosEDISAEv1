using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PedidosEDISAE
{
    class RegistroEjecucionArchivo : IRegistroEjecucion
    {
        string NombreArchivo;
        List<string> ListaErrores;

        public RegistroEjecucionArchivo(string nombreArchivo)
        {
            NombreArchivo = nombreArchivo;
            ListaErrores = new List<string>();
        }

        public void Registrar(string evento)
        {
            if (NombreArchivo != String.Empty)
            {
                string folder = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\" + "PedidosEDISAE";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string archivo = folder + "\\" + NombreArchivo + ".txt";
                if (!File.Exists(archivo))
                {
                    //Si archivo no existe, se crea con mensaje inicial de registro:
                    using (StreamWriter archivoRegistroNuevo = File.CreateText(archivo))
                    {
                        archivoRegistroNuevo.WriteLine(evento);
                    }
                }
                else
                {
                    using (StreamWriter archivoRegistro = File.AppendText(archivo))
                    {
                        archivoRegistro.WriteLine(evento);
                    }
                }
            }
        }

        public void RegistrarError(string error)
        {
            string textoError = "ERROR: " + error;
            Registrar(textoError);
            ListaErrores.Add(textoError);
        }

        public List<string> Errores()
        {
            return ListaErrores;
        }
    }
}
