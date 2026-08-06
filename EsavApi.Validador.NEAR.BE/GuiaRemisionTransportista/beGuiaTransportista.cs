using EsavApi.Validador.NEAR.BE.Commons;

namespace EsavApi.Validador.NEAR.BE.GuiaRemisionTransportista
{
    public class beGuiaTransportista : Emisor
    {
        public int accion { get; set; }
        public string IdSucursal { get; set; }
        public string vUbl { get; set; }
        public string vCustomID { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string fechaEmision { get; set; }
        public string tipoDocEmision { get; set; }
        public string horaEmision { get; set; }
        public bool tieneDocBaja { get; set; }
        public string serieBaja { get; set; }
        public string numeroBaja { get; set; }
        public string codigoTipoDocElectBaja { get; set; }
        public string tipoDocElectBaja { get; set; }
        public string IdCliente { get; set; }
        public string IdTipoDocumentoCliente { get; set; }
        public string NombresCliente { get; set; }
        public string observaciones { get; set; }
        public string EntidadAutorizadora { get; set; }
        public string tipoDocumentoDestinatario { get; set; }
        public string tipoDocumentoDestinatarioText { get; set; }
        public string nroDocumentoDestinatario { get; set; }
        public string razonSocialDestinatario { get; set; }
        public string tipoDocumentoProveedor { get; set; }
        public string tipoDocumentoProveedorText { get; set; }
        public string nroDocumentoProveedor { get; set; }
        public string razonSocialProveedor { get; set; }
        public string tipoDocumentoComprador { get; set; }
        public string tipoDocumentoCompradorText { get; set; }
        public string nroDocumentoComprador { get; set; }
        public string razonSocialComprador { get; set; }
        public string email { get; set; }
        public string idmotivoTraslado { get; set; }
        public string esTrasladoTotal { get; set; }
        public string motivoTraslado { get; set; }
        public string motivoTrasladoText { get; set; }
        public string motivoTrasladoExtra { get; set; }
        public string unidadPesoBrutoSeleccionados { get; set; }
        public decimal pesoBrutoSeleccionados { get; set; }
        public string sustentoPesoBrutoSeleccionados { get; set; }
        public string unidadPesoBruto { get; set; }
        public decimal pesoBruto { get; set; }
        public int numeroBultos { get; set; }
        public string modalidadTraslado { get; set; }
        public string modalidadTrasladoText { get; set; }
        public string fechaInicioTrasladoPrivado { get; set; }
        public string fechaEntregaBienesEmpresaTransporte { get; set; }
        public string esTransbordoProgramado { get; set; }
        public string tipoDocumentoEmpresaTransporte { get; set; }
        public string nroDocumentoEmpresaTransporte { get; set; }
        public string razonSocialEmpresaTransporte { get; set; }
        public string registroMTC { get; set; }
        public string transportistaNumeroAutorizacion { get; set; }
        public string transportistaCodigoEntidadAutorizadora { get; set; }
        public string ubigeoPuntoPartida { get; set; }
        public string ubigeoPuntoPartidaText { get; set; }
        public string direccionPuntoPartida { get; set; }
        public string rucPuntoPartida { get; set; }
        public string codigoPuntoPartida { get; set; }
        public string latitudPuntoPartida { get; set; }
        public string longitudPuntoPartida { get; set; }
        public string ubigeoPuntoLlegada { get; set; }
        public string ubigeoPuntoLlegadaText { get; set; }
        public string direccionPuntoLlegada { get; set; }
        public string rucPuntoLlegada { get; set; }
        public string codigoPuntoLlegada { get; set; }
        public string latitudPuntoLlegada { get; set; }
        public string longitudPuntoLlegada { get; set; }
        public string tipoPuertoEmbarque { get; set; }
        public string codPuertoEmbarque { get; set; }
        public string PuertoEmbarque { get; set; }
        public string tipoAeroPuertoEmbarque { get; set; }
        public string codAeroPuertoEmbarque { get; set; }
        public string AeroPuertoEmbarque { get; set; }
        public string numeroContenedor { get; set; }
        public string idSubContrata { get; set; }
        public string tipoDocumentSubContrata { get; set; }
        public string tipoDocumentoSubContrataText { get; set; }
        public string nroDocumentoSubContrata { get; set; }
        public string razonSocialSubContrata { get; set; }
        public string esTransporteSubcontratado { get; set; }
        public string esPagadorFlete { get; set; }
        public string comentario { get; set; }
        public string idFormatoRepresentacionImpresa { get; set; }

        public string Ca06_IdRemitente { get; set; }
        public string Remi_idRemitente { get; set; }
        public string RazonSocialRemitente { get; set; }

        public Configuracion configuracion { get; set; }
        public Sucursal sucursal { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }
        public int Registro { get; set; }
    }
}
