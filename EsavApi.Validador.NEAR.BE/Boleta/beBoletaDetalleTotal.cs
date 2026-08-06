
using EsavApi.Validador.NEAR.BE.Commons;

namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaDetalleTotal : Helper
    {
        public string IdEmisor { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string Tipo { get; set; }
        public string Index { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string TaxTypeCode { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Amount { get; set; }
        public string Amount_CurrencyID { get; set; }
        public string TypeCode { get; set; }
        public string SubID { get; set; }
    }
}
