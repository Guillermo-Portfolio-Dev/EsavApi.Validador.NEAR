using EsavApi.Validador.NEAR.BE.Commons;

namespace EsavApi.Validador.NEAR.BE.NotaDebito
{
    public class beNotaDebito : Emisor
    {
        public int accion { get; set; }
        public string vUbl { get; set; }
        public string vCustomID { get; set; }
        public string IdSucursal { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string tipoDocEmision { get; set; }
        public string fechaEmision { get; set; }
        public string fechaVencimiento { get; set; }
        public string horaEmision { get; set; }
        public string Ca52_Id { get; set; }
        public string tipoMoneda { get; set; }
        public string serieDocReferencia { get; set; }
        public string nroDocReferencia { get; set; }
        public string tipoNotaDebito { get; set; }
        public string motivoEmision { get; set; }
        public string fechaEmisionDocReferencia { get; set; }
        public string tipoDocReferencia { get; set; }
        public string serieGuiaRemision { get; set; }
        public string Numero_GuiaRemision { get; set; }
        public string Ca01_Id_GuiaRemision { get; set; }
        public string Serie_Referencia { get; set; }
        public string NroGuiaRemision { get; set; }
        public string Ca12_Id_Referencia { get; set; }
        public string IdEmisor { get; set; }
        public string CodigoLocalSunat { get; set; }
        public string tipoDocumento { get; set; }
        public string docIdentidad { get; set; }
        public string RazonSocialEmisor { get; set; }
        public string razonSocial { get; set; }
        public string distritoId { get; set; }
        public string direccion { get; set; }
        public decimal tDescuento { get; set; }
        public decimal tOtrosCargos { get; set; }
        public string Ca02_ChargeTotalAmount { get; set; }
        public decimal tAnticipo { get; set; }
        public string Ca02_PrepaidTotalAmount { get; set; }
        public decimal tImporteTotal { get; set; }
        public decimal tValorVenta { get; set; }
        public string tipoMonedaText { get; set; }
        public string tipoMonedaSimbolo { get; set; }
        public string email { get; set; }
        public string idEmisorTipoOperacion { get; set; }
        public string Ca09_Descripcion { get; set; }
        public string ComentarioLegal { get; set; }
        public string Comentario { get; set; }
        public string CuentaCorriente { get; set; }
        public decimal TipoCambio { get; set; }
        public decimal tipoCambio { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public bool Estado { get; set; }
        public string Ip { get; set; }
        public int Registro { get; set; }
        public string observacion { get; set; }
        public string tipoDocumentoText { get; set; }
        public Configuracion Configuracion { get; set; }
    }
}
