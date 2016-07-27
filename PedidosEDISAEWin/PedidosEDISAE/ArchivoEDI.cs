using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    class ArchivoEDI
    {
        public string NombreArchivo { get; set; }
        public DateTime FechaPedido { get; set; }
        public List<PedidoEDI> Pedidos { get; set; }

        public ArchivoEDI(string nombreArchivo)
        {
            NombreArchivo = nombreArchivo;
            FechaPedido = DateTime.Now;
            Pedidos = new List<PedidoEDI>();
        }

        private PedidoEDI ObtenerPedidoDeAgencia(string NumeroAgencia)
        {
            foreach (PedidoEDI p in Pedidos)
            {
                if (p.NumeroAgencia == NumeroAgencia)
                {
                    return p;
                }
            }
            PedidoEDI n = new PedidoEDI(NumeroAgencia);
            Pedidos.Add(n);
            return n;
        }

        public void AgregaNodoLIN(string NumeroAgencia, string ClaveProducto, int Cantidad, string RAN)
        {
            PedidoEDI pedido = ObtenerPedidoDeAgencia(NumeroAgencia);
            pedido.AgregarProducto(ClaveProducto, Cantidad, RAN);
        }
    }
}
