using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.ComunicacionBaja
{
    public class beComunicacionBajaObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beComunicacionBaja eCabBaja { get; set; }
        public List<beComunicacionBajaDetalle> eDocBaja { get; set; }
    }
}
