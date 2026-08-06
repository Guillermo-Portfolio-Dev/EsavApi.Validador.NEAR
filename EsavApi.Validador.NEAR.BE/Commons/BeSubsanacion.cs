using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class BeSubsanacion
    {
        public int? Estado { get; set; }
        public int? Indicador { get; set; }
        public List<BeNotasSubsanacion> Notas { get; set; }
    }
}
