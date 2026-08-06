using EsavApi.Validador.NEAR.BE.Commons;
using System;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.DocumentoCobranza
{
    public class beDocumentoCobranzaObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public Guid LockToken { get; set; }
        public beDocumentoCobranza eCabecera { get; set; }
        public List<beDocumentoCobranzaDetalle> lDetalle { get; set; }
        public List<beDocumentoCobranzaOrdenCompra> lOrdenCompra { get; set; }
        public List<beDocumentoCobranzaDocumentoDespacho> lDocDespacho { get; set; }
        public DocumentoCobranzaTotal eTotal { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
    }
}
