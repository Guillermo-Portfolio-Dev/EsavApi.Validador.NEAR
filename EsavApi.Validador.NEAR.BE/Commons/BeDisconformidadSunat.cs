using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class BeDisconformidadSunat
    {
        private DateTime? fecDisconformidad;
        public DateTime? FecDisconformidad { get => fecDisconformidad; set => fecDisconformidad = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc); }
        public List<string> Motivos { get; set; }
        public string Sustento { get; set; }
        public BeSubsanacion Subsanacion { get; set; }
    }
}
