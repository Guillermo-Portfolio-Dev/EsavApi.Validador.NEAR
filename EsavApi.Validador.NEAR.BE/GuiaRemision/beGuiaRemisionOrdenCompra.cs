using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BE.GuiaRemision
{
    public class beGuiaRemisionOrdenCompra
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string ordenCompra { get; set; }
    }
}
