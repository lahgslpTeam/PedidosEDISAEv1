using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    class PedidoEDI
    {
        public string NumeroAgencia { get; set; }

        public List<ProductoEDI> Productos { get; set; }

        public PedidoEDI(string numeroAgencia)
        {
            NumeroAgencia = numeroAgencia;
            Productos = new List<ProductoEDI>();
        }

        public void AgregarProducto(string ClaveProducto, int Cantidad, string RAN)
        {
            Productos.Add(new ProductoEDI(ClaveProducto, Cantidad, RAN));
        }
    }
}
