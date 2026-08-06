using EsavApi.Validador.NEAR.BE.Commons;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.NotaCredito
{
    public class beNotaCreditoObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beNotaCredito eCabecera { get; set; }
        public beNotaCreditoImpuesto eTotal { get; set; }
        public List<beNotaCreditoDetalle> lDetalle { get; set; }
        //public List<beEmisorCampoAdicionalRegistro> lbeEmisorCampoAdicionalRegistro { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
        public List<beNotaCreditoGlobal> lGlobales { get; set; }
    }
}
