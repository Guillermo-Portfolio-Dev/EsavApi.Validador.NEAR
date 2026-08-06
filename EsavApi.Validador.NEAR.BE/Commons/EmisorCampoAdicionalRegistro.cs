namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class EmisorCampoAdicionalRegistro
    {
        public int accion { get; set; }
        public int numero { get; set; }
        public string serie { get; set; }
        public string idEmisor { get; set; }
        public int idRubro { get; set; }
        public string idCampoAdicional { get; set; }
        public string tipoDocumento { get; set; }
        public int index { get; set; }
        public string titulo { get; set; }
        public string valor { get; set; }
        public bool esDetalle { get; set; }
        public bool enXML { get; set; }
        public bool enRepresentacionImpresa { get; set; }
        public Configuracion configuracion { get; set; }
    }
}
