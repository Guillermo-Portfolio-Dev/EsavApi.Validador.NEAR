using System;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beRechazo
    {
        public string RUC { get; set; }
        public string Sede { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string CodigoRechazo { get; set; }
        public string Descripcion { get; set; }
        public string Txt { get; set; }
        public string TipoMoneda { get; set; }
        public string TipoDoc { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaTransferencia { get; set; }
        public int Estado { get; set; }
    }
}
