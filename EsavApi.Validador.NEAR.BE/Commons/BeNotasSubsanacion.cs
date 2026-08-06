using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class BeNotasSubsanacion
    {
        public int? Estado { get; set; }
        public string Error { get; set; }
        public string TipoDoc { get; set; }
        public string Serie { get; set; }
        public int? Numero { get; set; }
        public List<string> Motivos { get; set; }
        public string Sustento { get; set; }
        public string Tiket { get; set; }
    }
}
