namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaAnticipada
    {
        public int accion { get; set; }
        public string rucEmisor { get; set; }
        public string tipoDocCliente { get; set; }
        public string docCliente { get; set; }
        public string nombreCliente { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string identificadorPago { get; set; }
        public decimal gravadoAnticipo { get; set; }
        public decimal exoneradoAnticipo { get; set; }
        public decimal inafectoAnticipo { get; set; }
        public decimal iscAnticipo { get; set; }
        public decimal igvAnticipo { get; set; }
        public decimal montoAnticipado { get; set; }
        public string moneda { get; set; }
        public string fechaPago { get; set; }
        public bool externo { get; set; }
    }
}
