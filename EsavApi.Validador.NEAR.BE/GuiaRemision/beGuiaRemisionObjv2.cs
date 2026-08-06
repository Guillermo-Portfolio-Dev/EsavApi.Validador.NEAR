using EsavApi.Validador.NEAR.BE.Commons;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.GuiaRemision
{
    public class beGuiaRemisionObjv2
    {
        public beGuiaRemisionv2 eRemitente { get; set; }
        public List<GuiaRemisionDocumentoRelacionado> lDocRel { get; set; }
        public List<beGuiaRemisionOrdenCompra> lOrdenCompra { get; set; }
        public List<GuiaRemisionConductor> lConductor { get; set; }
        public List<GuiaRemisionVehiculo> lVehiculo { get; set; }
        public List<GuiaRemisionDetalle> lDetalleBien { get; set; }
        public List<EmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }

        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
    }
}
