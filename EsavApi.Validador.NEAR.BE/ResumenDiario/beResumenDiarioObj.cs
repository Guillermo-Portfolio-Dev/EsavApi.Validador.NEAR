using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.ResumenDiario
{
    public class beResumenDiarioObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beResumenDiario eCabBaja { get; set; }
        public List<beResumenDiarioDetalle> eDocBaja { get; set; }
    }
}
