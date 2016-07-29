using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Collections;

namespace PedidosEDISAE
{
    public class ProcesadorPedidosEDISAE
    {
        static public List<string> ProcesaPedidos(List<string> archivosEDI)
        {
            Hashtable TiemposNormativos;
            
            RegistroEjecucionArchivo registrador = new RegistroEjecucionArchivo(ConfigurationManager.AppSettings["ArchivoEjecucionNombre"] + DateTime.Now.ToString(ConfigurationManager.AppSettings["ArchivoEjecucionFormatoFecha"]));
            try
            {
                TiemposNormativos = CargadorTiemposNormativos.ObtenerTiempos();
            }
            catch (Exception e)
            {
                registrador.RegistrarError(e.Message);
                return registrador.Errores();
            }

            registrador.Registrar("Pedidos EDI - SAE. Carga iniciada en " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine);
            int contadorArchivos = 1;
            foreach (string archivo in archivosEDI)
            {
                registrador.Registrar("Archivo " + contadorArchivos++.ToString() + " de " + archivosEDI.Count.ToString() + ": '" + archivo + "'");
                registrador.Registrar("Paso 1. Iniciando lectura de archivo...");

                EDIXmlParser parser = new EDIXmlParser(archivo, registrador);
                if (parser.ProcesaXml() == 0)
                {
                    //No hubo errores
                    EDICargadorBD cargador = new EDICargadorBD(ConfigurationManager.ConnectionStrings["ArtluxSAE"].ConnectionString, registrador);
                    registrador.Registrar("Paso 2. Iniciando carga de archivo a SAE...");
                    if (cargador.CargarArchivoEDIaBD(parser.Archivo, TiemposNormativos) == 0)
                    {
                        registrador.Registrar("Carga a SAE completada exitosamente");
                        //Mover archivo procesado a folder "RutaDestino"
                        string nuevoArchivo = ConfigurationManager.AppSettings["RutaDestino"];
                        if (!nuevoArchivo.EndsWith("\\"))
                        {
                            nuevoArchivo += "\\";
                        }
                        nuevoArchivo += archivo.Substring(archivo.LastIndexOf("\\") + 1);
                        if (archivo != nuevoArchivo)    //Esto en caso de que se haya seleccionado un archivo en folder de Procesados
                        {
                            try
                            {
                                File.Move(archivo, nuevoArchivo);
                                registrador.Registrar("Archivo ha sido movido a carpeta '" + ConfigurationManager.AppSettings["RutaDestino"]  + "' exitosamente");
                            }
                            catch (Exception e)
                            {
                                //Regresar errores de permisos de archivos a la aplicacion
                                registrador.RegistrarError("Imposible mover archivo '" + archivo + "' a '" + nuevoArchivo + "'. Revisar permisos de usuario. " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        //Regresar errores durante la carga de BD a la aplicacion
                        registrador.RegistrarError("Problema en la carga de '" + archivo + "' en SAE. Contactar a soporte de la aplicación: " + ConfigurationManager.AppSettings["CorreoSoporte"]);
                    }
                }
                else
                {
                    //Regresar errores del XML a la aplicacion
                    registrador.RegistrarError("Imposible leer formato de archivo '" + archivo + "'. Verificar si es un archivo XML valido o contactar a soporte de la aplicación: " + ConfigurationManager.AppSettings["CorreoSoporte"]);
                }
                //Separador entre archivo en el log
                registrador.Registrar("");
            }
            return registrador.Errores();
        }
    }
}
