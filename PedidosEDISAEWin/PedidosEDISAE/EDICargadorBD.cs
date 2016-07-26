using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    class EDICargadorBD
    {
        private string CadenaConexion;
        private IRegistroEjecucion Registrador;

        public EDICargadorBD(string cadenaConexion, IRegistroEjecucion registrador)
        {
            Registrador = registrador;
            CadenaConexion = cadenaConexion;
        }

        public int CargarArchivoEDIaBD(ArchivoEDI archivo)
        {
            int errores = 0;
            //Recorrer pedidos incluidos en el archivo
            foreach (PedidoEDI pedido in archivo.Pedidos)
            {
                //Recorrer detalles a incluir en el pedido
                //Preparar INSERTS de Pedido
                foreach (ProductoEDI producto in pedido.Productos)
                {
                    //Preparar INSERTS de Productos al Pedido
                }
            }
            //Inicializar Conexion
            //Abrir Transaccion
            //Ejecutar instrucciones SQL
            //Cerrar transaccion
            return errores;
        }
    }
}
