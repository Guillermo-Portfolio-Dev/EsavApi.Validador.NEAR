namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaDeliveryDetalle
    {
        public int Accion { get; set; }
        public string IdEmisor { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string IdDetalle { get; set; }
        public string idPropiedad { get; set; }
        public string descripcionPropiedad { get; set; }
        public string valorPropiedad { get; set; }
        public bool enXML { get; set; }
        public bool enRepresentacionImpresa { get; set; }
        public string Item { get; set; }
        public int index { get; set; }
    }
}
