using System;
using System.Collections.Generic;
using System.Text;

namespace PedidosEDISAE
{
    interface IRegistroEjecucion
    {
        void Registrar(string evento);
        void RegistrarError(string error);
        void RegistrarAdvertencia(string advertencia);
        List<string> Errores();
        List<string> Advertencias();
    }
}
