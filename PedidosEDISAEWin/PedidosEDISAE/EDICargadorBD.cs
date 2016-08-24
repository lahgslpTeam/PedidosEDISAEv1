using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Data;
using System.Configuration;
using FirebirdSql.Data.FirebirdClient;

namespace PedidosEDISAE
{
    class EDICargadorBD
    {
        private string CadenaConexion;
        private IRegistroEjecucion Registrador;
        private string Num_Empresa = ConfigurationManager.AppSettings["NumEmpresaSAE"];
        private string SeriePedido = ConfigurationManager.AppSettings["SeriePedido"];
        private string ClaveVendedor = ConfigurationManager.AppSettings["ClaveVendedor"];
        private string ClaveCliente = ConfigurationManager.AppSettings["ClaveCliente"];
        private string ClaveUsuarioBitacora = ConfigurationManager.AppSettings["ClaveUsuarioBitacora"];
        private string NombreUsuarioBitacora = ConfigurationManager.AppSettings["NombreUsuarioBitacora"];
        private string Almacen = ConfigurationManager.AppSettings["Almacen"];

        public EDICargadorBD(string cadenaConexion, IRegistroEjecucion registrador)
        {
            Registrador = registrador;
            CadenaConexion = cadenaConexion;
        }

        public int CargarArchivoEDIaBD(ArchivoEDI archivo, Hashtable TiemposNormativos)
        {
            FbConnection fbConexion = null;
            FbTransaction fbTransaccion = null;
            int Registros_afectados = 0;

            int errores = 0;
            try
            {
                //Conectarse a Base de Datos
                fbConexion = new FbConnection(this.CadenaConexion);
                try
                {
                    fbConexion.Open();
                }
                catch (Exception openConnExc)
                {
                    throw new Exception("Error al conectar con Base de Datos. Validar cadena de conexión o contactar a soporte: " + ConfigurationManager.AppSettings["CorreoSoporte"] + ". " + openConnExc.Message);
                }
                //Iniciar Transaccion 
                fbTransaccion = fbConexion.BeginTransaction("Transaccion_InsertaPedidos");

                DataTable dtCliente = this.ObtenCliente(fbConexion, fbTransaccion, this.ClaveCliente);
                DateTime Fecha_Documento = (DateTime)dtCliente.Rows[0]["fecha_Carga"];

                //Recorrer pedidos incluidos en el archivo
                foreach (PedidoEDI pedido in archivo.Pedidos)
                {
                    //Recorrer detalles a incluir en el pedido
                    //Preparar INSERTS de Pedido
                    decimal Subtotal = 0, SubTotal_Impuesto = 0;
                    DataTable dtAgencia = null;
                    string Cve_Doc = "";
                    string MensajeNoProcesado = "";
                    bool procesar = true;

                    try
                    {
                        dtAgencia = this.ObtenCliente(fbConexion, fbTransaccion, pedido.NumeroAgencia);
                    }
                    catch(Exception ex)
                    {
                        procesar = false;
                        MensajeNoProcesado = "El pedido de agencia con clave: '" + pedido.NumeroAgencia.ToString() + "' no procesado debido a '" + ex.Message + "'.";
                    }


                    if (procesar)
                    {
                        //se obtiene el identificador que le sigue y se genera la clave de documento
                        long NuevoIdentificador = this.Nuevo_Identificador(fbConexion, fbTransaccion, ref Cve_Doc);

                        //Se Obtiene los datos de la Agencia, para determinar tiempo de entrega

                        DateTime Fecha_entrega = this.CalcularFechaEntrega((string)dtAgencia.Rows[0]["Cve_Ciudad"], Fecha_Documento, TiemposNormativos);

                        int partida = 1;
                        foreach (ProductoEDI producto in pedido.Productos)
                        {
                            //Preparar INSERTS de Productos al Pedido
                            //Se agrega maestro de partida y partida

                            string ClaveArticulo = "";
                            long id_observacion = this.AgregaObservacion(fbConexion, fbTransaccion, producto.RAN);
                            Registros_afectados += this.AgregaCamposLibresDePartida(fbConexion, fbTransaccion, Cve_Doc, partida);
                            Registros_afectados += this.Agrega_Partida(fbConexion, fbTransaccion, producto, Cve_Doc, partida, ref Subtotal, ref SubTotal_Impuesto
                                                                        , (int)dtCliente.Rows[0]["CvePrecio"], ref ClaveArticulo, id_observacion);

                            //Se actualiza el campo 'pendientes por surtir' en el inventario
                            Registros_afectados += this.Actualiza_Inventario(fbConexion, fbTransaccion, producto, ClaveArticulo, producto.Cantidad);

                            partida++;
                        }
                        //se agrega registra la informacion de envio
                        long id_InformacionEnvio = this.Agrega_Informacion_Envio(fbConexion, fbTransaccion, dtAgencia, Fecha_Documento);
                        Registros_afectados += id_InformacionEnvio > 0 ? 1 : 0;

                        //Se registra el movimiento en la Bitacora
                        long Id_Bitacora = this.Agrega_registroBitacora(fbConexion, fbTransaccion, Cve_Doc, Subtotal + SubTotal_Impuesto);
                        Registros_afectados += Id_Bitacora > 0 ? 1 : 0;

                        //No puedo seguir tu recomendacion de insertar al inicio el pedido, es necesario insertar aqui el pedido despues insertar las partidas y la bitacora
                        Registros_afectados += this.AgregaCampoLibresDePedido(fbConexion, fbTransaccion, Cve_Doc);
                        Registros_afectados += this.Agrega_Pedido(fbConexion, fbTransaccion, pedido, Cve_Doc, Subtotal, SubTotal_Impuesto, NuevoIdentificador, Id_Bitacora
                                                                  , (string)dtCliente.Rows[0]["RFC"], Fecha_Documento, Fecha_entrega, id_InformacionEnvio);
                    }
                    else {
                        string RANs = "";
                        foreach (ProductoEDI producto in pedido.Productos)
                        {
                            RANs += producto.RAN + ",";
                        }
                        MensajeNoProcesado += " Números RAN no procesados: " + RANs + ".";
                        Registrador.RegistrarAdvertencia(MensajeNoProcesado);
                    }

                }
                //Inicializar Conexion
                //Abrir Transaccion
                //Ejecutar instrucciones SQL
                //Cerrar transaccion

            }
            catch (Exception ex)
            {
                errores++;
                Registrador.RegistrarError("Problema al cargar pedidos de archivo XML: " + archivo.NombreArchivo + Environment.NewLine + ex.Message);
            }
            finally
            {
                bool CancelarTransaccion = errores > 0 ? true : false;
                try
                {
                    if (fbTransaccion != null)
                    {
                        if (CancelarTransaccion)
                            fbTransaccion.Rollback(); //Deshacer los cambios efectuados en la BD
                        else
                            fbTransaccion.Commit(); //Se realizan los cambios efectuados
                    }
                }
                catch (Exception ex)
                {
                    errores++;
                    Registrador.RegistrarError("Problema finalizar la transacción al cargar el archivo XML: " + archivo.NombreArchivo + Environment.NewLine + ex.Message);
                }

                //Cerramos la conexion a la Base de Datos
                if (fbConexion != null)
                    fbConexion.Close();
            }

            //Inicializar Conexion
            //Abrir Transaccion
            //Ejecutar instrucciones SQL
            //Cerrar transaccion    

            return errores;
        }

        private DateTime CalcularFechaEntrega(string ClaveCiudad, DateTime fecha_Documento, Hashtable TiempoNormativo)
        {
            int DiasDefaultEntrega = Convert.ToInt16(ConfigurationManager.AppSettings["DiasDefaultEntrega"]);
            DateTime fechaEntrega = fecha_Documento.AddDays(DiasDefaultEntrega); //Por default son 2 dias de tiempo de entrega, sacar de settings

            if (!string.IsNullOrEmpty(ClaveCiudad))
            {
                if (TiempoNormativo.Contains(ClaveCiudad))
                {
                    int Dias = (int)TiempoNormativo[ClaveCiudad];

                    fechaEntrega = fechaEntrega.AddDays(Dias);
                    //Si la fecha de entrega es en fin de semana se ajusta para entregar en Lunes.
                    if (fechaEntrega.DayOfWeek == DayOfWeek.Saturday)
                        fechaEntrega = fechaEntrega.AddDays(2);
                    else if (fechaEntrega.DayOfWeek == DayOfWeek.Sunday)
                        fechaEntrega = fechaEntrega.AddDays(1);
                }
                else
                {
                    throw new Exception("Clave de Ciudad: '" + ClaveCiudad + "' no existe en archivo '" + System.IO.Directory.GetCurrentDirectory()
                        + "\\TiemposNormativos.XML'. Favor de actualizar archivo con este dato o contactar a " + ConfigurationManager.AppSettings["CorreoSoporte"]);
                }
            }
            else
            {
                throw new Exception("Clave Ciudad no encontrado: '" + ClaveCiudad + "'. Validar campo libre de cliente en la tabla Clientes referente a 'Clave Ciudad' para determinar la fecha de entrega.");
            }

            return fechaEntrega;
        }

        //private long Nuevo_Identificador(FbConnection fbConexion,FbTransaction fbTransaccion) {
        //    object identificador = 1;
        //    string Texto_sql = "select coalesce(max(CAST(trim(CLAVE_DOC) AS bigint)) + 1,1) NuevoId from factp_clib01 where trim(CLAVE_DOC) SIMILAR TO \'[0-9]+\'";

        //    fbComando.CommandText = Texto_sql;
        //    fbComando.CommandType = CommandType.Text;
        //    identificador = fbComando.ExecuteScalar();

        //    return identificador == DBNull.Value ? 1:(long)identificador;
        //}

        private long Nuevo_Identificador(FbConnection fbConexion, FbTransaction fbTransaccion, ref string CveDocumento)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            object identificador = null;

            //revisar consulta poara determinar el folio siguiente
            string Texto_sql = "select max(ULT_DOC) + 1 NuevoId from foliosf" + this.Num_Empresa + " where trim(serie) = @serie AND TIP_DOC = 'P'";

            fbComando.CommandText = Texto_sql;
            fbComando.CommandType = CommandType.Text;
            fbComando.Parameters.AddWithValue("@serie", this.SeriePedido);

            identificador = fbComando.ExecuteScalar();

            if (identificador == DBNull.Value)
                throw new Exception("No se localizo la serie: '" + this.SeriePedido + "' en folios para pedidos.");

            //si no existe mandar exception y revisar la consulta
            string Formato = ConfigurationManager.AppSettings["Formato_IdClaveDocumento"];
            CveDocumento = this.SeriePedido + ((long)identificador).ToString(Formato); ///Sacar el formato de settings

            //Se actualiza la tabla de folios, con el nuevo Identificador
            this.Actualiza_Folios(fbConexion, fbTransaccion, (long)identificador);

            return (long)identificador;
        }

        private DataTable ObtenCliente(FbConnection fbConexion, FbTransaction fbTransaccion, string ClaveCliente)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            DataTable dtCliente = new DataTable();
            FbDataAdapter fbDataAdaptador = new FbDataAdapter();
            string Texto_sql = "";

            //Texto_sql += " select CLAVE,NOMBRE,RFC,coalesce(trim(libre.CAMPLIB9), '') Cve_Ciudad,coalesce(lista_prec, 1) CvePrecio,cast('Now' as date) fecha_Carga ";
            //Lista_prec siempre va a ser 1, y el campo libre a Usarse es CAMPLIB8
            Texto_sql += " select CLAVE,NOMBRE,RFC,coalesce(trim(libre.CAMPLIB8), '') Cve_Ciudad,1 CvePrecio,cast('Now' as date) fecha_Carga ";
            Texto_sql += "     ,coalesce(CALLE,'') CALLE,coalesce(NUMINT,'') NUMINT,coalesce(NUMEXT,'') NUMEXT,coalesce(CRUZAMIENTOS,'') CRUZAMIENTOS,coalesce(CRUZAMIENTOS2,'') CRUZAMIENTOS2,coalesce(LOCALIDAD,'') POB, CURP, coalesce(REFERDIR,'') REFERDIR, coalesce(CVE_ZONA,'') CVE_ZONA";
            Texto_sql += "     ,'' STRNOGUIA,'' STRMODOENV, '' NOMBRE_RECEP,'' NO_RECEP, NULL FECHA_RECEP,coalesce(COLONIA,'') COLONIA";
            Texto_sql += "     ,coalesce(CODIGO,'') CODIGO,coalesce(ESTADO,'') ESTADO,coalesce(PAIS,'') PAIS,coalesce(MUNICIPIO,'') MUNICIPIO";
            Texto_sql += " from clie" + this.Num_Empresa + " clie ";
            Texto_sql += "     left join clie_clib" + this.Num_Empresa + " libre on clie.clave = libre.cve_clie";
            Texto_sql += " where trim(CLAVE) = trim(@Clave)";

            fbComando.CommandText = Texto_sql;
            fbComando.Parameters.AddWithValue("@Clave", ClaveCliente);

            fbDataAdaptador.SelectCommand = fbComando;
            fbDataAdaptador.Fill(dtCliente);

            if (dtCliente.Rows.Count <= 0) throw new Exception("No se localizo un cliente con la clave: " + ClaveCliente + ".");

            return dtCliente;
        }

        private DataTable ObtenArticulo(FbConnection fbConexion, FbTransaction fbTransaccion, ProductoEDI producto, int ClavePrecio)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            DataTable dtArticulo = new DataTable();
            FbDataAdapter fbDataAdaptador = new FbDataAdapter();
            string Texto_sql = "";

            //Texto_sql += " select alternas.CVE_ART,precio,ult_costo,impuesto.impuesto4,imp4Aplica";
            //Texto_sql += "        ," + producto.Cantidad.ToString() + " * precio * (impuesto4 / 100) TotImp4,apart,uni_med,tipo_ele";
            //Texto_sql += "        , " + producto.Cantidad.ToString() + " * precio Tot_Partida";
            //Texto_sql += "        ,apl_man_imp,cuota_ieps,apl_man_ieps,inventario.cve_esqimpu cve_esq";
            //Texto_sql += "        , descr, exist, cve_alter";
            //Texto_sql += " from cves_alter" + this.Num_Empresa + " as alternas";
            //Texto_sql += "        inner join inve" + this.Num_Empresa + " inventario on alternas.cve_art = inventario.cve_art";
            //Texto_sql += "        inner join precio_x_prod" + this.Num_Empresa + " precio on alternas.cve_art = precio.cve_art";
            //Texto_sql += "        inner join impu" + this.Num_Empresa + " impuesto on inventario.cve_esqimpu = impuesto.cve_esqimpu";
            //Texto_sql += " where trim(alternas.CVE_ALTER) = @cve_art and cve_precio = 1";

            Texto_sql += " select alternas.CVE_ART, case when precioCliente.precio=0 then precioPublico.precio else precioCliente.precio end Precio";
            Texto_sql += "        ,ult_costo,impuesto.impuesto4,imp4Aplica";
            Texto_sql += "        , " + producto.Cantidad.ToString() + " * case when precioCliente.precio = 0 then precioPublico.precio else precioCliente.precio end *(impuesto4 / 100) TotImp4";
            Texto_sql += "        ,apart,uni_med,tipo_ele";
            Texto_sql += "        , " + producto.Cantidad.ToString() + " * case when precioCliente.precio = 0 then precioPublico.precio else precioCliente.precio end Tot_Partida";
            Texto_sql += "        ,apl_man_imp,coalesce(cuota_ieps,0) cuota_ieps,apl_man_ieps,inventario.cve_esqimpu cve_esq";
            Texto_sql += "        , descr, exist, cve_alter";
            Texto_sql += " from cves_alter" + this.Num_Empresa + " as alternas";
            Texto_sql += "      inner join inve" + this.Num_Empresa + " inventario on alternas.cve_art = inventario.cve_art";
            Texto_sql += "      left join precio_x_prod" + this.Num_Empresa + " precioCliente on alternas.cve_art = precioCliente.cve_art and precioCliente.cve_precio = @Clave_Precio";
            Texto_sql += "      left join precio_x_prod" + this.Num_Empresa + " precioPublico on alternas.cve_art = precioPublico.cve_art and precioPublico.cve_precio = 1";
            Texto_sql += "      inner join impu" + this.Num_Empresa + " impuesto on inventario.cve_esqimpu = impuesto.cve_esqimpu";
            Texto_sql += " where trim(alternas.CVE_ALTER) =  @cve_art";


            fbComando.CommandText = Texto_sql;
            fbComando.Parameters.AddWithValue("@cve_art", producto.ClaveProducto);
            fbComando.Parameters.AddWithValue("@Clave_Precio", ClavePrecio);
            //fbComando.Parameters.AddWithValue("@Cantidad", producto.Cantidad );

            fbDataAdaptador.SelectCommand = fbComando;
            fbDataAdaptador.Fill(dtArticulo);

            if (dtArticulo.Rows.Count <= 0) throw new Exception("No se localizo un producto con la clave: '" + producto.ClaveProducto + "'.");

            if (dtArticulo.Rows[0]["Precio"] == DBNull.Value) throw new Exception("Producto con clave: '" + producto.ClaveProducto + "' no tieneprecio definido, favor de definir precio.");

            if ((double)dtArticulo.Rows[0]["Precio"] == 0) throw new Exception("Producto con clave: " + producto.ClaveProducto + "' tiene precio definido en cero, favor de verificar.");

            return dtArticulo;
        }

        private int Actualiza_Inventario(FbConnection fbConexion, FbTransaction fbTransaccion, ProductoEDI producto, string Clave_Articulo, int Cantidad)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            string Texto_sql = "";

            Texto_sql += " Update inve" + this.Num_Empresa + " set PEND_SURT = PEND_SURT + @Cantidad where CVE_ART = @cve_art";

            fbComando.CommandText = Texto_sql;
            fbComando.Parameters.AddWithValue("@cve_art", Clave_Articulo);
            fbComando.Parameters.AddWithValue("@Cantidad", Cantidad);

            Registros_afectados = fbComando.ExecuteNonQuery();

            if (Registros_afectados <= 0)
                throw new Exception("No se actualizo 'pendientes por surtir' para el Articulo con clave: " + producto.ClaveProducto + ".");
            else if (Registros_afectados > 1)
                throw new Exception("Se actualizo mas de una vez 'pendientes por surtir' para el Articulo con clave: " + producto.ClaveProducto + ".");

            return Registros_afectados;
        }

        private int AgregaCampoLibresDePedido(FbConnection fbConexion, FbTransaction fbTransaccion, string Clave_Doc)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            string Texto_sql = "INSERT INTO  FACTP_CLIB" + this.Num_Empresa + " (CLAVE_DOC,CAMPLIB6,CAMPLIB7,CAMPLIB8) VALUES(@CLAVE_DOC, NULL, NULL, NULL) ";

            fbComando.CommandText = Texto_sql;
            fbComando.Parameters.AddWithValue("@CLAVE_DOC", Clave_Doc);

            Registros_afectados = fbComando.ExecuteNonQuery();

            return Registros_afectados;
        }

        private int Agrega_Pedido(FbConnection fbConexion, FbTransaction fbTransaccion, PedidoEDI pedido, string Cve_Doc, decimal Subtotal, decimal SubTotal_impuesto, long Identificador
                                , long idBitacora, string RFC_Cliente, DateTime Fecha_Doc, DateTime Fecha_Entrega, long ClaveEnvio)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            string Texto_sql = " INSERT INTO FACTP" + this.Num_Empresa + " (";

            Texto_sql += " TIP_DOC,CVE_DOC,CVE_CLPV,STATUS,DAT_MOSTR,CVE_VEND,CVE_PEDI,FECHA_DOC,FECHA_ENT";
            Texto_sql += " ,FECHA_VEN,FECHA_CANCELA,CAN_TOT,IMP_TOT1,IMP_TOT2,IMP_TOT3,IMP_TOT4,DES_TOT,DES_FIN";
            Texto_sql += " ,COM_TOT,CONDICION,CVE_OBS,NUM_ALMA,ACT_CXC,ACT_COI,ENLAZADO,TIP_DOC_E,NUM_MONED,TIPCAMB";
            Texto_sql += " ,NUM_PAGOS,FECHAELAB,PRIMERPAGO,RFC,CTLPOL,ESCFD,AUTORIZA,SERIE,FOLIO,AUTOANIO,DAT_ENVIO";
            Texto_sql += " ,CONTADO,CVE_BITA,BLOQ,FORMAENVIO,DES_FIN_PORC,DES_TOT_PORC,IMPORTE,COM_TOT_PORC,METODODEPAGO";
            Texto_sql += " ,NUMCTAPAGO,TIP_DOC_ANT,DOC_ANT,TIP_DOC_SIG,DOC_SIG,UUID,VERSION_SINC";
            Texto_sql += " ) VALUES (";
            //Texto_sql += "  'P',@CVE_DOC,@CVE_CLPV,'O',0,@CVE_VEND,NULL,cast('Now' as date),cast('Now' as date)";
            //Texto_sql += " ,cast('Now' as date),NULL,@CAN_TOT,0,0,0,@IMP_TOT4,@DES_TOT,@DES_FIN";
            Texto_sql += "  'P',@CVE_DOC,@CVE_CLPV,'O',0,@CVE_VEND,@CVE_PEDI,@FECHA_DOC,@FECHA_ENT";
            Texto_sql += " ,@FECHA_ENT,NULL,@CAN_TOT,0,0,0,@IMP_TOT4,0,0";
            Texto_sql += " ,0,NULL,@CVE_OBS,1,'S','N','O','O',1,1";
            Texto_sql += " ,1,cast('Now' as timestamp),0,@RFC,0,'N',0,@SERIE,@FOLIO,'',@DAT_ENVIO";
            Texto_sql += " ,'N',@CVE_BITA,'N',NULL,0,0,@IMPORTE,0,NULL";
            Texto_sql += " ,NULL,'','',NULL,NULL,UUID_TO_CHAR(gen_uuid()),cast('Now' as timestamp)";
            Texto_sql += " )";

            fbComando.Parameters.AddWithValue("@CVE_DOC", Cve_Doc);
            fbComando.Parameters.AddWithValue("@CVE_CLPV", this.ClaveCliente);
            fbComando.Parameters.AddWithValue("@CVE_VEND", this.ClaveVendedor);
            fbComando.Parameters.AddWithValue("@CVE_PEDI", this.Almacen + "-" + pedido.NumeroAgencia);    //Por definir David, si lleva algo en la 'Clave Pedido' ==> definido Almacen-<man02>
            fbComando.Parameters.AddWithValue("@FECHA_DOC", Fecha_Doc);
            fbComando.Parameters.AddWithValue("@FECHA_ENT", Fecha_Entrega);
            fbComando.Parameters.AddWithValue("@CAN_TOT", Subtotal);
            fbComando.Parameters.AddWithValue("@IMP_TOT4", SubTotal_impuesto);
            fbComando.Parameters.AddWithValue("@CVE_OBS", 0);                   //Por default, el encabezado de los pedidos no lleva RAN/Observaciones
            fbComando.Parameters.AddWithValue("@RFC", RFC_Cliente);
            fbComando.Parameters.AddWithValue("@SERIE", this.SeriePedido);
            fbComando.Parameters.AddWithValue("@FOLIO", Identificador);
            fbComando.Parameters.AddWithValue("@DAT_ENVIO", ClaveEnvio);
            fbComando.Parameters.AddWithValue("@CVE_BITA", idBitacora);
            fbComando.Parameters.AddWithValue("@IMPORTE", Subtotal + SubTotal_impuesto);

            fbComando.CommandText = Texto_sql;

            Registros_afectados = fbComando.ExecuteNonQuery();

            return Registros_afectados;
        }

        private int AgregaCamposLibresDePartida(FbConnection fbConexion, FbTransaction fbTransaccion, string Cve_Doc, int partida)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            string Texto_sql = "INSERT INTO  PAR_FACTP_CLIB" + this.Num_Empresa + " (CLAVE_DOC,NUM_PART,CAMPLIB1,CAMPLIB2,CAMPLIB3) VALUES(@CLAVE_DOC, @NUM_PART, NULL, NULL,NULL) ";

            fbComando.Parameters.AddWithValue("@CLAVE_DOC", Cve_Doc);
            fbComando.Parameters.AddWithValue("@NUM_PART", partida);

            fbComando.CommandText = Texto_sql;

            Registros_afectados = fbComando.ExecuteNonQuery();

            return Registros_afectados;
        }

        private int Agrega_Partida(FbConnection fbConexion, FbTransaction fbTransaccion, ProductoEDI producto, string Cve_Doc, int partida, ref decimal subTotal, ref decimal subTotal_Impuesto
                                   , int ClavePrecio, ref string ClaveArticulo, long id_observacion)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            DataTable dtArticulo = this.ObtenArticulo(fbConexion, fbTransaccion, producto, ClavePrecio);

            string Texto_sql = " INSERT INTO PAR_FACTP" + this.Num_Empresa + " (";
            Texto_sql += " CVE_DOC,NUM_PAR,CVE_ART,CANT,PXS,PREC,COST,IMPU1,IMPU2,IMPU3,IMPU4,IMP1APLA";
            Texto_sql += " ,IMP2APLA,IMP3APLA,IMP4APLA,TOTIMP1,TOTIMP2,TOTIMP3,TOTIMP4,DESC1,DESC2,DESC3";
            Texto_sql += " ,COMI,APAR,ACT_INV,NUM_ALM,POLIT_APLI,TIP_CAM,UNI_VENTA,TIPO_PROD,CVE_OBS";
            Texto_sql += " ,REG_SERIE,E_LTPD,TIPO_ELEM,NUM_MOV,TOT_PARTIDA,IMPRIMIR,MAN_IEPS,APL_MAN_IMP";
            Texto_sql += " ,CUOTA_IEPS,APL_MAN_IEPS,MTO_PORC,MTO_CUOTA,CVE_ESQ,UUID,VERSION_SINC";
            Texto_sql += " ) VALUES (";
            Texto_sql += " @CVE_DOC,@NUM_PAR,@CVE_ART,@CANT,@CANT,@PREC,@COST,0,0,0,@IMPU4,0";
            Texto_sql += " ,0,0,@IMP4APLA,0,0,0,@TOTIMP4,0,0,0";
            Texto_sql += " ,0,@APAR,'N',1,'',1,@UNI_VENTA,@TIPO_PROD,@CVE_OBS";
            Texto_sql += " ,0,0,'N',0,@TOT_PARTIDA,'S','N',@APL_MAN_IMP";
            Texto_sql += " ,@CUOTA_IEPS,@APL_MAN_IEPS,0,0,@CVE_ESQ,UUID_TO_CHAR(gen_uuid()),cast('Now' as timestamp)";
            Texto_sql += " )";

            fbComando.Parameters.AddWithValue("@CVE_DOC", Cve_Doc);
            fbComando.Parameters.AddWithValue("@NUM_PAR", partida);
            fbComando.Parameters.AddWithValue("@CVE_ART", dtArticulo.Rows[0]["CVE_ART"]);
            fbComando.Parameters.AddWithValue("@CANT", producto.Cantidad);
            fbComando.Parameters.AddWithValue("@PREC", dtArticulo.Rows[0]["precio"]);
            fbComando.Parameters.AddWithValue("@COST", dtArticulo.Rows[0]["ult_costo"]);
            fbComando.Parameters.AddWithValue("@IMPU4", dtArticulo.Rows[0]["impuesto4"]);
            fbComando.Parameters.AddWithValue("@IMP4APLA", dtArticulo.Rows[0]["imp4Aplica"]);
            fbComando.Parameters.AddWithValue("@TOTIMP4", dtArticulo.Rows[0]["TotImp4"]);
            fbComando.Parameters.AddWithValue("@APAR", dtArticulo.Rows[0]["apart"]);
            fbComando.Parameters.AddWithValue("@UNI_VENTA", dtArticulo.Rows[0]["uni_med"]);
            fbComando.Parameters.AddWithValue("@TIPO_PROD", dtArticulo.Rows[0]["tipo_ele"]);
            fbComando.Parameters.AddWithValue("@CVE_OBS", id_observacion);
            fbComando.Parameters.AddWithValue("@TOT_PARTIDA", dtArticulo.Rows[0]["Tot_Partida"]);
            fbComando.Parameters.AddWithValue("@APL_MAN_IMP", dtArticulo.Rows[0]["apl_man_imp"]);
            fbComando.Parameters.AddWithValue("@CUOTA_IEPS", dtArticulo.Rows[0]["cuota_ieps"]);
            fbComando.Parameters.AddWithValue("@APL_MAN_IEPS", dtArticulo.Rows[0]["apl_man_ieps"]);
            fbComando.Parameters.AddWithValue("@CVE_ESQ", dtArticulo.Rows[0]["cve_esq"]);

            subTotal += Convert.ToDecimal(dtArticulo.Rows[0]["Tot_Partida"]);
            subTotal_Impuesto += Convert.ToDecimal(dtArticulo.Rows[0]["TotImp4"]);

            ClaveArticulo = (string)dtArticulo.Rows[0]["CVE_ART"];

            fbComando.CommandText = Texto_sql;

            Registros_afectados = fbComando.ExecuteNonQuery();

            return Registros_afectados;
        }

        private long Agrega_registroBitacora(FbConnection fbConexion, FbTransaction fbTransaccion, string Cve_Doc, decimal MontoTotal)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Cve_Usuario = -1;
            string Observacion = "No. [ " + Cve_Doc + " ] $ " + MontoTotal.ToString("0.00000");
            string Texto_sql = "select max(CVE_BITA) + 1 NuevoId from BITA" + this.Num_Empresa + " ";

            fbComando.CommandText = Texto_sql;
            object Nuevo_idBitacora = fbComando.ExecuteScalar();
            Nuevo_idBitacora = Nuevo_idBitacora == DBNull.Value ? 1 : (long)Nuevo_idBitacora;

            Texto_sql = " INSERT INTO BITA" + this.Num_Empresa + " (";
            Texto_sql += " CVE_BITA,CVE_CLIE,CVE_CAMPANIA,CVE_ACTIVIDAD,FECHAHORA,CVE_USUARIO,OBSERVACIONES";
            Texto_sql += " ,STATUS,NOM_USUARIO";
            Texto_sql += " ) VALUES (";
            Texto_sql += " @CVE_BITA,@CVE_CLIE,@CVE_CAMPANIA,@CVE_ACTIVIDAD,cast('Now' as timestamp),@CVE_USUARIO,@OBSERVACIONES,'F',@NOM_USUARIO";
            Texto_sql += " )";

            fbComando.Parameters.AddWithValue("@CVE_BITA", Nuevo_idBitacora);
            fbComando.Parameters.AddWithValue("@CVE_CLIE", this.ClaveCliente);
            fbComando.Parameters.AddWithValue("@CVE_CAMPANIA", "_SAE_");
            fbComando.Parameters.AddWithValue("@CVE_ACTIVIDAD", "    4");

            if(int.TryParse(this.ClaveUsuarioBitacora,out Cve_Usuario))
                fbComando.Parameters.AddWithValue("@CVE_USUARIO", Cve_Usuario);
            else
                throw new Exception("La clave de usuario : '" + this.ClaveUsuarioBitacora  + "' para bitácora no es un valor entero, favor de verificar.");

            fbComando.Parameters.AddWithValue("@OBSERVACIONES", Observacion);
            fbComando.Parameters.AddWithValue("@NOM_USUARIO", this.NombreUsuarioBitacora );

            fbComando.CommandText = Texto_sql;
            fbComando.ExecuteNonQuery();

            this.Actualiza_Control_tablas(fbConexion, fbTransaccion, 62, (long)Nuevo_idBitacora);//Clave 62 para Bita01

            return (long)Nuevo_idBitacora;
        }

        private int Actualiza_Folios(FbConnection fbConexion, FbTransaction fbTransaccion, long Identificador)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_Afectados = 0;
            string Texto_sql = "update foliosf" + this.Num_Empresa + " set ULT_DOC = @NuevoIdentificador, fech_ult_doc= cast('Now' as timestamp) where trim(serie) = '" + this.SeriePedido + "' AND TIP_DOC = 'P' ";

            fbComando.Parameters.AddWithValue("@NuevoIdentificador", Identificador);

            fbComando.CommandText = Texto_sql;
            Registros_Afectados = fbComando.ExecuteNonQuery();

            if (Registros_Afectados <= 0)
                throw new Exception("No se actualizo el registro de Folios de la serie 'O', Tipo Documento 'P' con el folio: " + Identificador.ToString() + ".");
            else if (Registros_Afectados > 1)
                throw new Exception("Se actualizo mas de un registro en Folios de la serie 'O', Tipo Documento 'P' con el folio: " + Identificador.ToString() + ".");

            return Registros_Afectados;
        }

        private int Actualiza_Control_tablas(FbConnection fbConexion, FbTransaction fbTransaccion, int idTabla, long Folio)
        {
            FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
            int Registros_afectados = 0;
            string Texto_sql = " Update TBLCONTROL" + this.Num_Empresa + " set ULT_CVE = @Folio where ID_TABLA = @idTabla";

            fbComando.CommandText = Texto_sql;
            fbComando.Parameters.AddWithValue("@idTabla", idTabla);
            fbComando.Parameters.AddWithValue("@Folio", Folio);

            Registros_afectados = fbComando.ExecuteNonQuery();

            if (Registros_afectados <= 0)
                throw new Exception("No se actualizo 'Ultimo Folio' en el Control de Folios de Tablas con folio: " + Folio.ToString() + ".");
            else if (Registros_afectados > 1)
                throw new Exception("Se actualizo mas de una vez 'Ultimo Folio' en el Control de Folios de Tablas con folio: " + Folio.ToString() + ".");

            return Registros_afectados;
        }

        private long AgregaObservacion(FbConnection fbConexion, FbTransaction fbTransaccion, string Observacion)
        {

            if (string.IsNullOrEmpty(Observacion))
            {
                return 0;
            }
            else
            {
                FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
                string Texto_sql = "select max(Cve_obs)+ 1 from obs_docf" + this.Num_Empresa + " ";

                fbComando.CommandText = Texto_sql;
                object Nuevo_idObservacion = fbComando.ExecuteScalar();
                Nuevo_idObservacion = Nuevo_idObservacion == DBNull.Value ? 1 : (long)Nuevo_idObservacion;

                Texto_sql = " insert into obs_docf" + this.Num_Empresa + " (CVE_OBS,STR_OBS) values (@Cve_Obs,@Str_Obs)";

                fbComando.Parameters.AddWithValue("@Cve_Obs", Nuevo_idObservacion);
                fbComando.Parameters.AddWithValue("@Str_Obs", Observacion);

                fbComando.CommandText = Texto_sql;
                fbComando.ExecuteNonQuery();

                this.Actualiza_Control_tablas(fbConexion, fbTransaccion, 56, (long)Nuevo_idObservacion);//Clave 56 para obs_docf01

                return (long)Nuevo_idObservacion;
            }

            //llamada a funcion de actualizado control de tablas
        }

        private long Agrega_Informacion_Envio(FbConnection fbConexion, FbTransaction fbTransaccion, DataTable dtCliente, DateTime FechaEnvio)
        {
            if (dtCliente == null || dtCliente.Rows.Count <= 0)
            {
                return 0;
            }
            else
            {
                FbCommand fbComando = new FbCommand("", fbConexion, fbTransaccion);
                string Texto_sql = "select max(CVE_INFO)+ 1 from INFENVIO" + this.Num_Empresa + " ";

                fbComando.CommandText = Texto_sql;
                object Nuevo_id_InfoEnvio = fbComando.ExecuteScalar();
                Nuevo_id_InfoEnvio = Nuevo_id_InfoEnvio == DBNull.Value ? 1 : (long)Nuevo_id_InfoEnvio;

                Texto_sql = "";
                Texto_sql += " insert into INFENVIO" + this.Num_Empresa + " ( ";
                Texto_sql += "   CVE_INFO,CVE_CONS,NOMBRE,CALLE,NUMINT,NUMEXT,CRUZAMIENTOS,CRUZAMIENTOS2,POB,CURP,REFERDIR,CVE_ZONA,CVE_OBS,STRNOGUIA,STRMODOENV";
                Texto_sql += "   ,FECHA_ENV,NOMBRE_RECEP,NO_RECEP,FECHA_RECEP,COLONIA,CODIGO,ESTADO,PAIS,MUNICIPIO";
                Texto_sql += " ) values (";
                Texto_sql += "    @CVE_INFO,@CVE_CONS,@NOMBRE,@CALLE,@NUMINT,@NUMEXT,@CRUZAMIENTOS,@CRUZAMIENTOS2,@POB,@CURP,@REFERDIR,@CVE_ZONA,@CVE_OBS,@STRNOGUIA,@STRMODOENV";
                Texto_sql += "   ,@FECHA_ENV,@NOMBRE_RECEP,@NO_RECEP,@FECHA_RECEP,@COLONIA,@CODIGO,@ESTADO,@PAIS,@MUNICIPIO";
                Texto_sql += " )";

                fbComando.Parameters.AddWithValue("@CVE_INFO", Nuevo_id_InfoEnvio);
                fbComando.Parameters.AddWithValue("@CVE_CONS", dtCliente.Rows[0]["CLAVE"]);
                fbComando.Parameters.AddWithValue("@NOMBRE", dtCliente.Rows[0]["NOMBRE"]);
                fbComando.Parameters.AddWithValue("@CALLE", dtCliente.Rows[0]["CALLE"]);
                fbComando.Parameters.AddWithValue("@NUMINT", dtCliente.Rows[0]["NUMINT"]);
                fbComando.Parameters.AddWithValue("@NUMEXT", dtCliente.Rows[0]["NUMEXT"]);
                fbComando.Parameters.AddWithValue("@CRUZAMIENTOS", dtCliente.Rows[0]["CRUZAMIENTOS"]);
                fbComando.Parameters.AddWithValue("@CRUZAMIENTOS2", dtCliente.Rows[0]["CRUZAMIENTOS2"]);
                fbComando.Parameters.AddWithValue("@POB", dtCliente.Rows[0]["POB"]);
                fbComando.Parameters.AddWithValue("@CURP", dtCliente.Rows[0]["CURP"]);
                fbComando.Parameters.AddWithValue("@REFERDIR", dtCliente.Rows[0]["REFERDIR"]);
                fbComando.Parameters.AddWithValue("@CVE_ZONA", dtCliente.Rows[0]["CVE_ZONA"]);
                fbComando.Parameters.AddWithValue("@CVE_OBS", DBNull.Value);
                fbComando.Parameters.AddWithValue("@STRNOGUIA", dtCliente.Rows[0]["STRNOGUIA"]);
                fbComando.Parameters.AddWithValue("@STRMODOENV", dtCliente.Rows[0]["STRMODOENV"]);
                fbComando.Parameters.AddWithValue("@FECHA_ENV", FechaEnvio);
                //fbComando.Parameters.AddWithValue("@FECHA_ENV", DBNull.Value);
                fbComando.Parameters.AddWithValue("@NOMBRE_RECEP", dtCliente.Rows[0]["NOMBRE_RECEP"]);
                fbComando.Parameters.AddWithValue("@NO_RECEP", dtCliente.Rows[0]["NO_RECEP"]);
                fbComando.Parameters.AddWithValue("@FECHA_RECEP", dtCliente.Rows[0]["FECHA_RECEP"]);
                fbComando.Parameters.AddWithValue("@COLONIA", dtCliente.Rows[0]["COLONIA"]);
                fbComando.Parameters.AddWithValue("@CODIGO", dtCliente.Rows[0]["CODIGO"]);
                fbComando.Parameters.AddWithValue("@ESTADO", dtCliente.Rows[0]["ESTADO"]);
                fbComando.Parameters.AddWithValue("@PAIS", dtCliente.Rows[0]["PAIS"]);
                fbComando.Parameters.AddWithValue("@MUNICIPIO", dtCliente.Rows[0]["MUNICIPIO"]);


                fbComando.CommandText = Texto_sql;
                fbComando.ExecuteNonQuery();

                this.Actualiza_Control_tablas(fbConexion, fbTransaccion, 70, (long)Nuevo_id_InfoEnvio);//Clave 70 para Infoenvio01

                return (long)Nuevo_id_InfoEnvio;
            }
        }

    }
}
