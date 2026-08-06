using EsavApi.Validador.NEAR.BE.Commons;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Factura
{
    public class beFactura : Emisor
    {
        public int accion { get; set; }
        public string vUbl { get; set; }
        public string vCustomID { get; set; }
        public string IdSucursal { get; set; }
        public string idTipoFactura { get; set; }
        public string idTipoFacturaText { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string fechaEmision { get; set; }
        public string horaEmision { get; set; }
        public string fechaVencimiento { get; set; }
        public string tipoDocEmision { get; set; }
        public string Note { get; set; }
        public string tipoMoneda { get; set; }
        public int LineCountNumeric { get; set; }
        public string OrderReference_ID { get; set; }
        public string AccountingSupplierParty_Party_PartyTaxScheme_RegistrationName { get; set; }
        public string AccountingSupplierParty_Party_PostalAddress_CitySubdivisionName { get; set; }
        public string AccountingSupplierParty_Party_PostalAddress_Country_IdentificationCode { get; set; }
        public string razonSocial { get; set; }
        public string AccountingCustomerParty_Party_PartyTaxScheme_RegistrationName { get; set; }
        public string docIdentidad { get; set; }
        public string tipoDocumento { get; set; }
        public string tipoDocumentoText { get; set; }
        public string AccountingCustomerParty_Party_PartyTaxScheme_RegistrationAddess_AddressTypeCode { get; set; }
        public string ubigeo { get; set; }
        public string direccion { get; set; }
        public string AccountingCustomerParty_Party_PostalAddress_CitySubdivisionName { get; set; }
        public string departamento { get; set; }
        public string provincia { get; set; }
        public string distrito { get; set; }
        public string AccountingCustomerParty_Party_PostalAddress_Country_IdentificationCode { get; set; }
        public bool tieneAsociado { get; set; }
        public string tipoDocumentoAsociado { get; set; }
        public string docIdentidadAsociado { get; set; }
        public string tipoDocumentoTextAsociado { get; set; }
        public string razonSocialAsociado { get; set; }
        public string cuentaDetraccion { get; set; }
        public string codBbSsDetraccion { get; set; }
        public string codBbSsDetraccionText { get; set; }
        public decimal porcentajeDetraccion { get; set; }
        public decimal montoDetraccion { get; set; }
        public string PrepaidPayment_ID { get; set; }
        public string PrepaidPayment_ID_SchemeID { get; set; }
        public int PrepaidPayment_PaidAmount { get; set; }
        public string PrepaidPayment_PaidAmount_CurrencyID { get; set; }
        public string PrepaidPayment_InstructionID { get; set; }
        public string PrepaidPayment_InstructionID_SchemeID { get; set; }
        public bool AllowanceCharge_ChargeIndicator { get; set; }
        public string AllowanceCharge_AllowanceChargeReasonCode { get; set; }
        public decimal AllowanceCharge_MultiplierFactorNumeric { get; set; }
        public decimal AllowanceCharge_Amount { get; set; }
        public string AllowanceCharge_Amount_CurrencyID { get; set; }
        public decimal AllowanceCharge_BaseAmount { get; set; }
        public string AllowanceCharge_BaseAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_LineExtensionAmount { get; set; }
        public string LegalMonetaryTotal_LineExtensionAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_TaxInclusiveAmount { get; set; }
        public string LegalMonetaryTotal_TaxInclusiveAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_AllowanceTotalAmount { get; set; }
        public string LegalMonetaryTotal_AllowanceTotalAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_ChargeTotalAmount { get; set; }
        public string LegalMonetaryTotal_ChargeTotalAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_PrepaidAmount { get; set; }
        public string LegalMonetaryTotal_PrepaidAmount_CurrencyID { get; set; }
        public decimal LegalMonetaryTotal_PayableAmount { get; set; }
        public string LegalMonetaryTotal_PayableAmount_CurrencyID { get; set; }
        public bool anticipo { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ImporteTotalReferencia { get; set; }
        public string observacion { get; set; }
        public string ComentarioLegal { get; set; }
        public string Comentario { get; set; }
        public string CuentaCorriente { get; set; }
        public string CuentaDetraccion { get; set; }
        public string email { get; set; }
        public string IdEmisorTipoOperacion { get; set; }
        public beRubro Rubro { get => _Rubro; set => _Rubro = value; }
        private beRubro _Rubro = new beRubro();
        public List<beFacturaAnticipada> FacturaAnticipada { get; set; }
        public List<beFacturaFormaPagoCuota> Cuotas { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }
        public int Registro { get; set; }
        public decimal tipoCambio { get; set; }
        public string TipoDocumentoDescripcion { get; set; }
        public string tipoMonedaText { get; set; }
        public string tipoMonedaSimbolo { get; set; }
        public string FechaPago { get; set; }
        public decimal MontoPendientePago { get; set; }
        public string FormaPago { get; set; }
        public bool EsExportacion { get; set; }
        public bool esDetraccion { get; set; }
        public string PagoDetraccionCode { get; set; }
        public string PagoDetraccionText { get; set; }
        public string DocumentoNoFiscal { get; set; }
        public Configuracion Configuracion { get; set; }


    }
    public class beFacturaObj
    {
        public string MensajePop { get; set; }
        public string MensajeId { get; set; }
        public beFactura eCabecera { get; set; }
        public List<beFacturaGlobal> lGlobales { get; set; }
        public List<beFacturaDocumentoDespacho> lDocDespacho { get; set; }
        public List<beFacturaDocumentoAdicional> lDocAdicional { get; set; }
        public List<beFacturaOrdenCompra> lOrdenCompra { get; set; }
        public FacturaTotalModel eTotal { get; set; }
        public List<beFacturaDetalle> lDetalle { get; set; }
        public List<beFacturaDetalleTotal> lDetalleTotal { get; set; }
        public List<CampoAdicionalRegistroDetalle> lCampoAdicionalDetalle { get; set; }
        public List<beEmisorCampoAdicionalRegistro> lCampoAdicional { get; set; }
    }
}
