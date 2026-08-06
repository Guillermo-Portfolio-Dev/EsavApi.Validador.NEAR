namespace EsavApi.Validador.NEAR.BE.Factura
{
    public class beFacturaFormaPagoCuota
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string IdCuota { get; set; }
        public decimal MontoPagoCuota { get; set; }
        public string FechaPagoCuota { get; set; }
    }
}
