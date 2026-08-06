namespace EsavApi.Validador.NEAR.BE.GuiaRemision
{
    public class GuiaRemisionDetalle
    {
        public string IdEmisor { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public int itemBien { get; set; }
        public string itemRef { get; set; }
        public decimal cantidadBien { get; set; }
        public string codigoSUNAT { get; set; }
        public string codigoBien { get; set; }
        public string descripcionBien { get; set; }
        public string unidadMedida { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal valorVenta { get; set; }
        public string afectacionIgv { get; set; }
        public string codigoconcepto { get; set; }
        public string nombreconcepto { get; set; }
        public string partidaArancelaria { get; set; }
        public string numeroDAM { get; set; }
        public string serieDAM { get; set; }
        public bool bienNormalizado { get; set; }
        public string unidadMedidaText { get; set; }
        public string UnidadAbreviatura { get; set; }
        public string descripcionExtendida { get; set; }
        public decimal pesototal { get; set; }
        public decimal pesounitario { get; set; }
    }
}
