using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beRUC
    {
        public string RUC { get; set; }
        public string RazonSocial { get; set; }
        public string Estado { get; set; }
        public string Correo { get; set; }
        public string CondicionDomicilio { get; set; }
        public string AgenteRetencion { get; set; }
        public string AgentePercepcion { get; set; }
        public string AgentePercepcionVI { get; set; }
        public string BuenContribuyente { get; set; }
        public List<beDireccion> Direcciones { get; set; }
    }
}
