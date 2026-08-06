using System;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class beDocumentoAdicional : beDocumento
    {
        public String Codigo { get; set; }
        public String CodigoTipoDocumento { get; set; }
        public String CodigoTipoDocumentoReferencia { get; set; }
        public String SerieReferencia { get; set; }
        public long NumeroReferencia { get; set; }
        public String TipoDocumentoIdentidadCliente { get; set; }
        public String NroDocumentoIdentidadCliente { get; set; }
        public String RazónSocialCliente { get; set; }
        public String FormaPago { get; set; }
        public String Moneda { get; set; }
        public string IdGravado { get; set; }
        public decimal Gravado { get; set; }
        public string IdInafecto { get; set; }
        public decimal Inafecto { get; set; }
        public string IdExonerado { get; set; }
        public decimal Exonerado { get; set; }
        public string IdGratuito { get; set; }
        public decimal Gratuito { get; set; }
        public string IdExportacion { get; set; }
        public decimal Exportacion { get; set; }
        public bool IndicadorOtrosCargos { get; set; }
        public decimal OtrosCargos { get; set; }
        public bool IndicadorDescuento { get; set; }
        public decimal Descuento { get; set; }
        public string IdIgv { get; set; }
        public string NombreIgv { get; set; }
        public string CodigoIgv { get; set; }
        public decimal Igv { get; set; }
        public string IdIsc { get; set; }
        public string CodigoIsc { get; set; }
        public string NombreIsc { get; set; }
        public decimal Isc { get; set; }
        public string IdOth { get; set; }
        public string NombreOth { get; set; }
        public string CodigoOth { get; set; }
        public decimal Oth { get; set; }//
        public string IdIcbper { get; set; }
        public string NombreIcbper { get; set; }
        public string CodigoIcbper { get; set; }
        public decimal Icbper { get; set; }//
        public double ImporteTotal { get; set; }
        public string Regimen { get; set; }
        public decimal PorcentajePercepcion { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ImportePercepcion { get; set; }
        public decimal ImporteCobrar { get; set; }
        public bool Estado { get; set; }
        public string IdCuentaContable { get; set; }
        public string DescripcionCuentaContable { get; set; }
        public String BajaSerie { get; set; }
        public String BajaTipo { get; set; }
        public String BajaFecha { get; set; }
        public int BajaNumero { get; set; }
        public String ResumenSerie { get; set; }
        public String ResumenTipo { get; set; }
        public String ResumenFecha { get; set; }
        public int ResumenNumero { get; set; }
        public string CorreoCliente { get; set; }
        public string CodigoCDR { get; set; }
        public string DescripcionCDR { get; set; }
        public string OSEBalanceado { get; set; }
        public string OSEBalanceadoRazonSocial { get; set; }
        public int EstadoCDR { get; set; }
        public BeDisconformidadSunat DisconformidadSunat { get; set; }
        public string Plataforma { get; set; }
    }
}
