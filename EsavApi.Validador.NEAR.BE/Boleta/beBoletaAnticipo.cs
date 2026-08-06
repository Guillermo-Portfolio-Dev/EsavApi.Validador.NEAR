namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaAnticipo
    {
        public string IdEmisor { get; set; }
        public string DocCliente { get; set; }
        public string TipoDocCliente { get; set; }
        public string FechaEmision { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal Gravado { get; set; }
        public decimal Exonerado { get; set; }
        public decimal Inafecto { get; set; }
        public decimal Exportacion { get; set; }
        public decimal IGV { get; set; }
        public decimal ISC { get; set; }
        public decimal OtroTributos { get; set; }
        public decimal ImporteTotal { get; set; }
        public string Moneda { get; set; }
    }
}
