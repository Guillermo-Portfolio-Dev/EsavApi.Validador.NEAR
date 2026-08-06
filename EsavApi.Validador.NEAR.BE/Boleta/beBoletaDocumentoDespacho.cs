namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaDocumentoDespacho
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string docRel { get; set; }
        public string idDocRel { get; set; }
        public string DocumentTypeDescripcion { get; set; }
        public string DocumentTypeAbreviatura { get; set; }
    }
}
