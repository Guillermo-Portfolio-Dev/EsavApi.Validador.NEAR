namespace EsavApi.Validador.NEAR.BE.Factura
{
    public class beFacturaDetalleTransporteCarga
    {
        public int Accion { get; set; }
        public string IdEmisor { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string indexDTC { get; set; }
        public string ubigeoOrigenDTC { get; set; }
        public string ubigeoOrigenTextDTC { get; set; }
        public string direccionOrigenDTC { get; set; }
        public string ubigeoDestinoDTC { get; set; }
        public string ubigeoDestinoTextDTC { get; set; }
        public string direccionDestinoDTC { get; set; }
        public string detalleViajeDTC { get; set; }
        public string tipoValorReferencialSTDTC { get; set; }
        public decimal valorReferencialSTDTC { get; set; }
        public string monedaValorReferencialSTDTC { get; set; }
        public string tipoValorReferencialCEDTC { get; set; }
        public decimal valorReferencialCEDTC { get; set; }
        public string monedaValorReferencialCEDTC { get; set; }
        public string tipoValorReferencialCUNDTC { get; set; }
        public decimal valorReferencialCUNDTC { get; set; }
        public string monedaValorReferencialCUNDTC { get; set; }
    }
}
