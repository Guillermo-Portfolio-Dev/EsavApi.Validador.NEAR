namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class Emisor
    {
        public string tipoDocEmisor { get; set; }
        public string rucEmisor { get; set; }
        public string razonsocialEmisor { get; set; }
        public string direccionEmisor { get; set; }
        public string emailEmisor { get; set; }
        public string telefonoEmisor { get; set; }
        public string faxEmisor { get; set; }
        public string distritoIdEmisor { get; set; }
        public string distritoEmisor { get; set; }
        public string provinciaIdEmisor { get; set; }
        public string provinciaEmisor { get; set; }
        public string departamentoIdEmisor { get; set; }
        public string departamentoEmisor { get; set; }
        public string paisIdEmisor { get; set; }
        public string paisEmisor { get; set; }
        public string rubroEmisor { get; set; }
        public Sucursal sucursalEmisor { get; set; }
        public short isProduccionEmisor { get; set; }
        public bool isAgenteRetencion { get; set; }
        public string rsAgenteRetencion { get; set; }
        public bool isAgentePercepcion { get; set; }
        public string rsAgentePercepcion { get; set; }
        public bool isAgentePercepcionVI { get; set; }
        public string rsAgentePercepcionVI { get; set; }
        public bool isBuenContribuyente { get; set; }
        public string rsBuenContribuyente { get; set; }
        public string rutapfxEmisor { get; set; }
        public string clavepfxEmisor { get; set; }
        public string usuarioSunatEmisor { get; set; }
        public string claveSunatEmisor { get; set; }
        public string valida { get; set; }
        public string fileLogoEmisor { get; set; }
        public string fileLogoPDFEmisor { get; set; }
        public string digestValue { get; set; }
        public bool esEsavquery { get; set; }
        public string BC_NroResolucion { get; set; }
        public string BC_Fecha { get; set; }
    }
}
