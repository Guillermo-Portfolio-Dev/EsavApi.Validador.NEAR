using EsavApi.Validador.NEAR.BE.Commons;

namespace EsavApi.Validador.NEAR.BE.ValeCredito
{
    public class beValeCredito : Emisor
    {
        public int accion { get; set; }
        public string vUbl { get; set; }
        public string vCustomID { get; set; }
        public string IdSucursal { get; set; }
        public string tipoDocEmision { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string fechaEmision { get; set; }
        public string horaEmision { get; set; }
        public string fechaVencimiento { get; set; }
        public string observacion { get; set; }
        public string tipoMoneda { get; set; }
        public string tipoMonedaSimbolo { get; set; }
        public string tipoMonedaText { get; set; }
        public string docIdentidad { get; set; }
        public string tipoDocumentoText { get; set; }
        public string direccion { get; set; }
        public string tipoDocumento { get; set; }
        public string razonSocial { get; set; }
        public decimal tIgv { get; set; }
        public string idIGV { get; set; }
        public decimal tIsc { get; set; }
        public string idISC { get; set; }
        public decimal tOtrosTributos { get; set; }
        public string idOTH { get; set; }
        public decimal tOtrosCargos { get; set; }
        public decimal tGravada { get; set; }
        public string codigotGravada { get; set; }
        public decimal tExonerada { get; set; }
        public string codigotExonerada { get; set; }
        public decimal tInafecta { get; set; }
        public string codigotInafecta { get; set; }
        public decimal tGratuita { get; set; }
        public string codigotGratuita { get; set; }
        public decimal tDescuento { get; set; }
        public string codigotDescuento { get; set; }
        public decimal tExportacion { get; set; }
        public string codigotExportacion { get; set; }
        public decimal tBaseImponible { get; set; }
        public decimal tImporteTotal { get; set; }
        public decimal tImporteCobrar { get; set; }
        public bool GeneraXml { get; set; }
        public bool GeneraPdf { get; set; }
        public bool TieneCdrCorrecto { get; set; }
        public bool EnvioCorreo { get; set; }
        public string email { get; set; }
        public string IdUsuario { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public decimal tipoCambio { get; set; }
        public string Ip { get; set; }
        public Configuracion Configuracion { get; set; }
    }
}
