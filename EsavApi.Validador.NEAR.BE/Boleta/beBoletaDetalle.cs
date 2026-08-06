using System;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaDetalle
    {
        public int accion { get; set; }
        public string IdEmisor { get; set; }
        public string serie { get; set; }
        public string numero { get; set; }
        public string index { get; set; }
        public decimal cantidad { get; set; }
        public string unidadMedida { get; set; }
        public string unidadMedida65 { get; set; }
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
        public string codigoSunat { get; set; }
        public string descripcion { get; set; }
        public string Item_CommodityClassification_ItemClassificationCode { get; set; }
        public string Item_AdditionalItemProperty_Name { get; set; }
        public string Item_AdditionalItemProperty_NameCode { get; set; }
        public string Item_AdditionalItemProperty_Value { get; set; }
        public string Item_AdditionalItemProperty_ValueQualifier { get; set; }
        //public DateTime Item_AdditionalItemProperty_UsabilityPeriod_StartDate { get; set; }
        //public DateTime Item_AdditionalItemProperty_UsabilityPeriod_EndDate { get; set; }
        public int Item_AdditionalItemProperty_UsabilityPeriod_DurationMeasure { get; set; }
        public decimal valorUnitario { get; set; }
        public string Price_PriceAmount_CurrencyID { get; set; }
        public decimal precioUnitario { get; set; }
        public string PricingReference_AlternativeConditionPrice_PriceAmount_CurrencyID { get; set; }
        public string codigoPrecioUnitario { get; set; }
        public decimal baseImponible { get; set; }
        public string LineExtensionAmount_CurrencyID { get; set; }
        public bool AllowanceCharge_ChargeIndicator_Descuento { get; set; }
        public string codigoMotivoDescuento { get; set; }
        public decimal descuento { get; set; }
        public decimal porcentajedescuento { get; set; }
        public string AllowanceCharge_Amount_CurrencyID_Descuento { get; set; }
        public decimal baseDescuentoDetalle { get; set; }
        public decimal pctDescuentoDetalle { get; set; }
        public string AllowanceCharge_BaseAmount_CurrencyID_Descuento { get; set; }
        public bool AllowanceCharge_ChargeIndicator_OtroCargo { get; set; }
        public string codigoMotivoOtrosCargosDetalle { get; set; }
        public decimal otrosCargosDetalle { get; set; }
        public decimal pctOtrosCargosDetalle { get; set; }
        public string AllowanceCharge_Amount_CurrencyID_OtroCargo { get; set; }
        public decimal baseOtrosCargosDetalle { get; set; }
        public string AllowanceCharge_BaseAmount_CurrencyID_OtroCargo { get; set; }
        public int cantidadICBPER { get; set; }
        public decimal perUnitICBPER { get; set; }
        public decimal montoICBPER { get; set; }
        public List<beBoletaDeliveryDetalle> listaPropiedad { get; set; }
        public string UnidadMedidaText { get; set; }
        public string UnidadAbreviatura { get; set; }
        public string BienServicioText { get; set; }
    }
}
