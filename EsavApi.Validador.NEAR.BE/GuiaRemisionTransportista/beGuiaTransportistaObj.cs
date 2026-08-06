using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.GuiaRemision;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.GuiaRemisionTransportista
{
    public class beGuiaTransportistaObj
    {
        public beGuiaTransportista eTransportista { get; set; }
        public List<GuiaRemisionDocumentoRelacionado> lDocRel { get; set; }
        public List<GuiaRemisionConductor> lConductor { get; set; }
        public List<GuiaRemisionVehiculo> lVehiculo { get; set; }
        public List<GuiaTrasportistaDetalle> lDetalleBien { get; set; }
        public List<EmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
        public List<beGuiaRemisionOrdenCompra> lOrdenCompra { get; set; }
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
    }
}
