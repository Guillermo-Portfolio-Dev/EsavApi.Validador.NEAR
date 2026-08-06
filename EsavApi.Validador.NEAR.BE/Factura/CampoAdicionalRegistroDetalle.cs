namespace EsavApi.Validador.NEAR.BE.Factura
{
    public class CampoAdicionalRegistroDetalle
    {
        public int idRubro { get; set; }
        public string idCampoAdicional { get; set; }
        public int index { get; set; }
        public string Item { get; set; }
        public string titulo { get; set; }
        public string valor { get; set; }
        public bool enXML { get; set; }
        public bool enRepresentacionImpresa { get; set; }
        public string Item_AdditionalItemProperty_NameCode { get; set; }
    }
}
