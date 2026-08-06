using System;

namespace EsavApi.Validador.NEAR.BE.NotaCredito
{
    public class beNotaCreditoDetalle
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public int index { get; set; }
        public decimal cantidad { get; set; }
        public string unidadMedida { get; set; }
        public decimal pctIGV { get; set; }
        public string codeIGV { get; set; }
        public string codigoGravExoIna { get; set; }
        public string graExoIna { get; set; }
        public string idIGV { get; set; }
        public string nameIGV { get; set; }
        public string tipoIGV { get; set; }
        public decimal igv { get; set; }
        public string idISC { get; set; }
        public string codeISC { get; set; }
        public string nameISC { get; set; }
        public string tipoISC { get; set; }
        public string codigoTipoISC { get; set; }
        public decimal pctISC { get; set; }
        public decimal isc { get; set; }
        public string idOTH { get; set; }
        public string codeOTH { get; set; }
        public string nameOTH { get; set; }
        public string tipoOTH { get; set; }
        public decimal pctOTH { get; set; }
        public decimal otrosTributosDetalle { get; set; }
        public string idICBPER { get; set; }
        public string codeICBPER { get; set; }
        public string nameICBPER { get; set; }
        public string tipoICBPER { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public decimal ValorVenta { get; set; }
        public decimal descuento { get; set; }
        public string codigoMotivoDescuento { get; set; }
        public string codigoMotivoOtrosCargosDetalle { get; set; }
        public string Ca02_ValorVenta { get; set; }
        public decimal PrecioVenta { get; set; }
        public string tipoMoneda { get; set; }
        public string codigoPrecioUnitario { get; set; }
        public string Ca02_TaxAmountIgv { get; set; }
        public string Ca02_TaxAmount_TaxSubTotalIgv { get; set; }
        public string Ca02_TaxAmountIsc { get; set; }
        public decimal TaxAmount_TaxSubTotalIsc { get; set; }
        public string Ca02_TaxAmount_TaxSubTotalIsc { get; set; }
        public string Ca02_TaxAmountOth { get; set; }
        public string codigoSunat { get; set; }
        public string AdditionalItemPropertyName { get; set; }
        public string Ca55_AdditionalItemProperty { get; set; }
        public string AdditionalItemPropertyValue { get; set; }
        //public DateTime UsabilityPeriod { get; set; }
        public decimal valorUnitario { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal baseImponible { get; set; }
        public decimal importeTotalItem { get; set; }
        public string Ca02_ValorUnitario { get; set; }
        public int cantidadICBPER { get; set; }
        public decimal perUnitICBPER { get; set; }
        public decimal montoICBPER { get; set; }
        public string Ca05_TaxAmount_TaxSubTotalICBPER { get; set; }
        public string Ca14_TaxAmount_TaxSubTotalIgv { get; set; }
        public string UnidadMedidaText { get; set; }
        public string UnidadAbreviatura { get; set; }
        public string BienServicioText { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }
        public int Registro { get; set; }
        public bool Estado { get; set; }
    }
}
