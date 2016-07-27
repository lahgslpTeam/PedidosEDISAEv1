using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.Configuration;

namespace PedidosEDISAE
{
    class CargadorTiemposNormativos
    {
        static public Hashtable ObtenerTiempos()
        {
            Hashtable tiempos = new Hashtable();
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load("TiemposNormativos.xml");
            }
            catch (Exception e)
            {
                throw new Exception("No se ha encontrado el archivo 'TiemposNormativos.xml', favor de reinstalar aplicacion o contactar a " + ConfigurationManager.AppSettings["CorreoSoporte"] + ". " + e.Message);
            }
            XmlElement xelRoot = doc.DocumentElement;
            XmlNodeList xmlNodes = xelRoot.SelectNodes("/TiemposNormativos/Ciudad");
            foreach (XmlNode ciudad in xmlNodes)
            {
                int dias = 0;
                try
                {
                    dias = Convert.ToInt32(ciudad["TiempoLogistica"].InnerText);
                }
                catch (Exception e)
                {
                    throw new Exception("Formato incorrecto en archivo 'TiemposNormativos.xml', favor de validar nodo '" + ciudad.OuterXml + "' o contactar a " + ConfigurationManager.AppSettings["CorreoSoporte"] + ". " + e.Message);
                }
                try
                {
                    tiempos[ciudad["Clave"].InnerText] = dias;
                }
                catch (Exception e)
                {
                    throw new Exception("Clave de Ciudad duplicada en archivo 'TiemposNormativos.xml', favor de validar nodo '" + ciudad.OuterXml + "' o contactar a " + ConfigurationManager.AppSettings["CorreoSoporte"] + ". " + e.Message);
                }
            }
            return tiempos;
        }
    }
}
