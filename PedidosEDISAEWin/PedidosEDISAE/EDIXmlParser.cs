using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PedidosEDISAE
{
    class EDIXmlParser
    {
        public string NombreArchivo { get; set; }
        public ArchivoEDI Archivo { get; set; }

        private IRegistroEjecucion Registrador;

        public EDIXmlParser(string nombreArchivo, IRegistroEjecucion registrador)
        {
            Registrador = registrador;
            NombreArchivo = nombreArchivo;
            Archivo = new ArchivoEDI(nombreArchivo);
        }

        public int ProcesaXml()
        {
            int errores = 0;
            string RootElementName = "NMEX862R";
            if (NombreArchivo != "")
            {
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(NombreArchivo);
                    XmlElement xelRoot = doc.DocumentElement;
                    XmlNodeList xmlRootNode = xelRoot.SelectNodes("/" + RootElementName);

                    if (xmlRootNode.Count == 0)
                    {
                        Registrador.RegistrarError("Formato de archivo incorrecto, esta aplicacion solo soporta archivos '" + RootElementName + "'. Revisar formato. Formato encontrado: " + doc.DocumentElement.Name);
                        errores++;
                    }
                    else
                    {
                        XmlNodeList xmlNodes = xelRoot.SelectNodes("/" + RootElementName + "/LIN");

                        if (xmlNodes.Count == 0)
                        {
                            Registrador.RegistrarError("No se han encontrado nodos LIN en archivo '" + NombreArchivo + "'. Revisar formato.");
                            errores++;
                        }
                        else
                        {
                            foreach (XmlNode linNode in xmlNodes)
                            {
                                errores += ParseLINNode(linNode);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Registrador.RegistrarError("Problema cargando XML. " + e.Message);
                    errores++;
                }
            }
            else
            {
                Registrador.RegistrarError("Nombre de archivo invalido '" + NombreArchivo + "'");
                errores++;
            }
            return errores;
        }

        private int ParseLINNode(XmlNode linNode)
        {
            int errores = 0;
            XmlNodeList nodosLINFST, nodoSel;

            string ClaveProducto = "";

            //Clave de Producto/Clave alterna de SAE
            nodoSel = linNode.SelectNodes("LIN.LIN/LIN03");
            if (nodoSel.Count == 1)
            {
                ClaveProducto = nodoSel[0].InnerText;
            }
            else
            {
                Registrador.RegistrarError("Problema al obtener clave de producto en nodo LIN. " + linNode.OuterXml);
                errores++;
                return errores;
            }

            nodosLINFST = linNode.SelectNodes("LIN.FST");
            if (nodosLINFST.Count > 0)
            {
                foreach (XmlNode fst in nodosLINFST)
                {
                    string NumeroAgencia = "";
                    string RAN = "";
                    int Cantidad = 0;

                    bool errorEnNodo = false;
                    bool agenciaVacia = false;

                    //Cantidad Solicitada
                    nodoSel = fst.SelectNodes("LIN.FST.FST/FST01");
                    if (nodoSel.Count == 1)
                    {
                        try
                        {
                            Cantidad = Convert.ToInt16(nodoSel[0].InnerText);
                        }
                        catch (Exception e)
                        {
                            Cantidad = 0;
                            errorEnNodo = true;
                            errores++;
                            Registrador.RegistrarError("Formato de número incorrecto en nodo FST01. " + linNode.OuterXml + ". Excepcion: " + e.Message);
                        }
                    }
                    else
                    {
                        errorEnNodo = true;
                        errores++;
                        Registrador.RegistrarError("Problema en nodo FST01. " + fst.OuterXml);
                    }

                    //RAN que se captura en el campo Observaciones de la Partida en SAE
                    nodoSel = fst.SelectNodes("LIN.FST.FST/FST09");
                    if (nodoSel.Count == 1)
                    {
                        RAN = nodoSel[0].InnerText;
                    }
                    else
                    {
                        errorEnNodo = true;
                        errores++;
                        Registrador.RegistrarError("Problema en nodo FST09. " + fst.OuterXml);
                    }

                    //Numero de Agencia
                    nodoSel = fst.SelectNodes("LIN.FST.SDQ/SDQ03");
                    if (nodoSel.Count == 1)
                    {
                        NumeroAgencia = nodoSel[0].InnerText;
                        if (NumeroAgencia.Trim() == String.Empty)
                        {
                            agenciaVacia = true;
                        }
                    }
                    else
                    {
                        agenciaVacia = true;
                    }

                    if (!errorEnNodo)
                    {
                        //Procesar Nodo LIN
                        if (!agenciaVacia)
                        {
                            Registrador.Registrar("Encontrado detalle - NumeroAgencia(" + NumeroAgencia + ");ClaveProducto(" + ClaveProducto + ");Cantidad(" + Cantidad.ToString() + ");RAN(" + RAN + ");");
                            Archivo.AgregaNodoLIN(NumeroAgencia, ClaveProducto, Cantidad, RAN);
                        }
                        else
                        {
                            Registrador.RegistrarAdvertencia("Nodo LIN ignorado debido a que agencia no fue especificada para el nodo con RAN '" + RAN + "'");
                        }
                    }
                }
            }
            else
            {
                Registrador.RegistrarError("Problema al obtener detalles en nodo LIN para producto '" + ClaveProducto + "'. " + linNode.OuterXml);
                errores++;
            }
            return errores;
        }
    }
}
