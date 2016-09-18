using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    class PedidoEDI
    {
        public string NumeroAgencia { get; set; }

        public List<ProductoEDI> Partidas { get; set; }

        public PedidoEDI(string numeroAgencia)
        {
            NumeroAgencia = numeroAgencia;
            Partidas = new List<ProductoEDI>();
        }

        public void AgregarPartida(string ClaveProducto, int Cantidad, string RAN)
        {
            Partidas.Add(new ProductoEDI(ClaveProducto, Cantidad, RAN));
        }
    }
}
