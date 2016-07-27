using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    class ProductoEDI
    {
        public string ClaveProducto { get; set; }
        public int Cantidad { get; set; }
        public string RAN { get; set; }

        public ProductoEDI(string claveProducto, int cantidad, string ran)
        {
            ClaveProducto = claveProducto;
            Cantidad = cantidad;
            RAN = ran;
        }
    }
}
