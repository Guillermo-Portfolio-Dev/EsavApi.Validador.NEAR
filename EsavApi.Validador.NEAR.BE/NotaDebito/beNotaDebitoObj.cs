using EsavApi.Validador.NEAR.BE.Commons;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.NotaDebito
{
    public class beNotaDebitoObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beNotaDebito eCabecera { get; set; }
        public beNotaDebitoImpuesto eTotal { get; set; }
        public List<beNotaDebitoDetalle> lDetalle { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
        public List<beNotaDebitoGlobal> lGlobales { get; set; }
    }
}
