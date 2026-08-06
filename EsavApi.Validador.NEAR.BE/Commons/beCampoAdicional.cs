namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beCampoAdicional
    {
        public string IdCampoAdicional { get; set; }
        public string Descripcion { get; set; }
        public string Titulo { get; set; }
        public string Placeholder { get; set; }
        public string TipoDato { get; set; }
        public string Icono { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public bool Requerido { get; set; }
        public bool Readonly { get; set; }
        public string ClassNameParent { get; set; }
        public bool EsDetalle { get; set; }
        public bool EnXML { get; set; }
        public bool EnRepresentacionImpresa { get; set; }
    }
}
