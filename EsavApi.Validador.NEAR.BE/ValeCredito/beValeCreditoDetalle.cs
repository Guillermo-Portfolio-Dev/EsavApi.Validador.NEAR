namespace EsavApi.Validador.NEAR.BE.ValeCredito
{
    public class beValeCreditoDetalle
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string index { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public string unidadMedida { get; set; }
        public decimal cantidad { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal igv { get; set; }
        public string graExoIna { get; set; }
        public decimal isc { get; set; }
        public string codigoTipoISC { get; set; }
        public decimal otrosTributosDetalle { get; set; }
        public string TipoOth { get; set; }
        public decimal otrosCargosDetalle { get; set; }
        public decimal Descuento { get; set; }
        public decimal baseImponible { get; set; }
        public decimal importeTotalItem { get; set; }
        public string codigoGravExoIna { get; set; }
        public string unidadMedidaText { get; set; }
        public string BienServicioText
        {
            get { return unidadMedida.ToUpper() == "ZZ" ? "Servicio" : "Bien"; }
        }
    }
}
