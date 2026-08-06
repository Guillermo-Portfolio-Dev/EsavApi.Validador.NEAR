namespace EsavApi.Validador.NEAR.BE.Factura
{
    public class beFacturaDocumentoDespacho
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string fechaEmision { get; set; }
        //public int IdFactura { get; set; }
        public string docRel { get; set; }
        public string idDocRel { get; set; }
        public string DocumentTypeDescripcion { get; set; }
        public string DocumentTypeAbreviatura { get; set; }
    }
}
