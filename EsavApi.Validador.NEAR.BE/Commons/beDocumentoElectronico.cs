using System;
using System.Collections.Generic;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beDocumentoElectronico
    {
        public int Id { get; set; }
        public String Serie { get; set; }
        public int Numero { get; set; }
        public String FechaEmision { get; set; }
        public String FechaVencimiento { get; set; }
        public String Moneda { get; set; }
        public String ClienteRazonSocial { get; set; }
        public String ClienteDocumento { get; set; }
        public String ClienteTipoDocumento { get; set; }
        public String ClienteUbigeo { get; set; }
        public String ClienteDireccion { get; set; }
        public double ValorVenta { get; set; }
        public double PrecioVenta { get; set; }
        public double Descuento { get; set; }
        public double OtroCargo { get; set; }
        public double Anticipo { get; set; }
        public double ImporteTotal { get; set; }
        public double Gravado { get; set; }
        public String GravadoCodigo { get; set; }
        public double Inafecto { get; set; }
        public String InafectoCodigo { get; set; }
        public double Exonerado { get; set; }
        public String ExoneradoCodigo { get; set; }
        public double Gratuito { get; set; }
        public String GratuitoCodigo { get; set; }
        public double Exportacion { get; set; }
        public String ExportacionCodigo { get; set; }
        public double Percepcion { get; set; }
        public String PercepcionCodigo { get; set; }
        public double Detraccion { get; set; }
        public String DetraccionCodigo { get; set; }
        public double IGV { get; set; }
        public String IGVCodigo { get; set; }
        public double ISC { get; set; }
        public String ISCCodigo { get; set; }
        public double OTH { get; set; }
        public String OTHCodigo { get; set; }
        public double ICBPER { get; set; }
        public String ICBPERCodigo { get; set; }
        public String CodigoOtroCargoGlobal { get; set; }
        public String CodigoDescuentoGlobal { get; set; }
        public String CodigoAnticipoGlobal { get; set; }
        public decimal PorcentajeOtroCargoGlobal { get; set; }
        public decimal PorcentajeDescuentoGlobal { get; set; }
        public decimal PorcentajeAnticipoGlobal { get; set; }
        public decimal MontoOtroCargoGlobal { get; set; }
        public decimal MontoDescuentoGlobal { get; set; }
        public decimal MontoAnticipoGlobal { get; set; }
        public decimal MontoBaseOtroCargoGlobal { get; set; }
        public decimal MontoBaseDescuentoGlobal { get; set; }
        public decimal MontoBaseAnticipoGlobal { get; set; }
        public List<beDocumentoElectronicoDetalle> Detalle { get; set; }
        public bool EstadoComprobante { get; set; }
        //public List<beDocumentoElectronicoCargosDescuentosGlobales> OtrosCargosDescuentosGlobales { get; set; }
        //  public List<beDocumentoElectronicoTotal> Totales { get; set;}
    }

    public class beDocumentoElectronicoDetalle
    {
        public int ID { get; set; }
        public decimal Cantidad { get; set; }
        public String Unidad { get; set; }
        public String DescripcionUnidad { get; set; }
        public double PrecioVenta { get; set; }
        public double PrecioVentaUnitario { get; set; }
        public String TipoPrecio { get; set; }
        public String Descripcion { get; set; }
        public String CodigoProducto { get; set; }
        public String CodigoProductoSunat { get; set; }
        public String ConceptoTributario { get; set; }
        public String CodigoConceptoTributario { get; set; }
        public String CodigoDescuento { get; set; }
        public String unidadMedida65 { get; set; }
        public String BienServicioDetraccion { get; set; }
        public String FechaInicio { get; set; }
        public double ValorUnitario { get; set; }
        public double IGV { get; set; }
        public String IGVCodigo { get; set; }
        public double ISC { get; set; }
        public String ISCCodigo { get; set; }
        public double OTH { get; set; }
        public String OTHCodigo { get; set; }
        public double ICBPER { get; set; }
        public String ICBPERCodigo { get; set; }
        public double Descuento { get; set; }
        public double MontoDescuento { get; set; }
        public double OtroCargo { get; set; }
        public String AfectacionIGV { get; set; }
        public String SubAfectacionIGV { get; set; }
        public String TipoISC { get; set; }
        public String CodigoOtroCargo { get; set; }
        public decimal PorcentajeOtroCargo { get; set; }
        public decimal PrecioUnitarioIcbper { get; set; }
        public decimal PorcentajeIgv { get; set; }
        public decimal PorcentajeIsc { get; set; }

    }

    public class beDocumentoElectronicoTotal
    {
        public String Codigo { get; set; }
        public String Descripcion { get; set; }
        public double Monto { get; set; }
        public String Moneda { get; set; }
        public String Tipo { get; set; }
    }
    public class beDocumentoElectronicoDetalleTotal
    {
        public int Item { get; set; }
        public double Monto { get; set; }
        public String Moneda { get; set; }
        public String CodigoTributo { get; set; }
        public String Afectacion { get; set; }
        public String DetalleAfectacion { get; set; }
        public decimal ProcentajeImpuesto { get; set; }
        public string NombreImpuesto { get; set; }
        public string CodigoTipoImpuesto { get; set; }
    }
    public class beDocumentoElectronicoCargosDescuentosGlobales
    {
        public bool Indicador { get; set; }
        public String CodigoCargo { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Monto { get; set; }
        public String Moneda { get; set; }
        public decimal BaseMonto { get; set; }

    }
}
