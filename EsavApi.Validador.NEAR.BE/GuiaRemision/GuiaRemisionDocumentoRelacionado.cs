namespace EsavApi.Validador.NEAR.BE.GuiaRemision
{
    public class GuiaRemisionDocumentoRelacionado
    {
        public int numero { get; set; }
        public string serie { get; set; }
        public int Accion { get; set; }
        public int itemDocRel { get; set; }
        public string nroDocRel { get; set; }
        public string codigoDocRel { get; set; }
        public string DescripcionRel { get; set; }
        public string docRel { get; set; }
        public string tipoDocEmisor { get; set; }
        public string rucEmisor { get; set; }
        public string rucEmisorDocumentoRelacionado { get; set; }
    }
}
