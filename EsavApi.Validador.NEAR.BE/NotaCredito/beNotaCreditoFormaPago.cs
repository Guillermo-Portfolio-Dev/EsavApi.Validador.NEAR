namespace EsavApi.Validador.NEAR.BE.NotaCredito
{
    public class beNotaCreditoFormaPago
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string FormaPago { get; set; }
        public decimal MontoPendientePago { get; set; }
        public string FechaPago { get; set; }
        public bool PagoCuotas { get; set; }

    }
}
