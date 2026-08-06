using EsavApi.Validador.NEAR.BE.Commons;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beBoleta eCabecera { get; set; }
        public List<beBoletaGlobal> lGlobales { get; set; }
        public List<beBoletaDocumentoDespacho> lDocDespacho { get; set; }
        public List<beBoletaDocumentoAdicional> lDocAdicional { get; set; }
        public List<beBoletaOrdenCompra> lOrdenCompra { get; set; }
        public BoletaTotalModel eTotal { get; set; }
        public List<beBoletaDetalle> lDetalle { get; set; }
        public List<beBoletaDetalleTotal> lDetalleTotal { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
    }
}
