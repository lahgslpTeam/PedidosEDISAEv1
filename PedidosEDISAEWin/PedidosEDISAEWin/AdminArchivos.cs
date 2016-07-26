using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PedidosEDISAEWin
{
    class AdminArchivos
    {
        static public List<string> ObtenerArchivosXML(string Ruta)
        {
            string [] archivos = Directory.GetFiles(Ruta, "*.xml");
            List<string> listaArchivos = new List<string>();
            foreach (string s in archivos)
            {
                if (File.Exists(s))
                {
                    FileInfo f = new FileInfo(s);
                    listaArchivos.Add(f.Name);
                }
            }
            return listaArchivos;
        }
    }
}
