namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beEmisorCampoAdicionalRegistro : Helper
    {
        public string IdEmisor { get; set; }
        public int IdRubro { get; set; }
        public string IdCampoAdicional { get; set; }
        public string Ca01_Id { get; set; }
        public string Serie { get; set; }
        public int Numero { get; set; }
        public int Index { get; set; }
        public string Titulo { get; set; }
        public string Valor { get; set; }
        public bool EsDetalle { get; set; }
        public bool EnXML { get; set; }
        public bool EnRepresentacionImpresa { get; set; }
    }
}
