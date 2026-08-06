using EsavApi.Validador.NEAR.BE.Commons;
using System;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.ValeCredito
{
    public class beValeCreditoObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public Guid LockToken { get; set; }
        public beValeCredito eCabecera { get; set; }
        public List<beValeCreditoDetalle> lDetalle { get; set; }
        public ValeCreditoTotal eTotal { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
    }
}
