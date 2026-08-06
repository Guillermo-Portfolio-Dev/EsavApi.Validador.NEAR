namespace EsavApi.Validador.NEAR.BE.ResumenDiario
{
    public class beResumenDiarioDetalle
    {
        public int Accion { get; set; }
        public string rucEmisor { get; set; }
        public string serie { get; set; }
        public int NumeroResumen { get; set; }
        public int NroItem { get; set; }
        public string codigoTipoDocElec { get; set; }
        public string tipoDocEmision { get; set; }
        public string fechaEmision { get; set; }
        public string IdSerie { get; set; }
        public int numero { get; set; }
        public string codigoTipoDocElecReferencia { get; set; }
        public string serieReferencia { get; set; }
        public long numeroReferencia { get; set; }
        public string tipoDocCliente { get; set; }
        public string nroDocCliente { get; set; }
        public string Moneda { get; set; }
        public string idGravado { get; set; }
        public decimal gravado { get; set; }
        public string idExonerado { get; set; }
        public decimal exonerado { get; set; }
        public string idInafecto { get; set; }
        public decimal inafecto { get; set; }
        public string idGratuito { get; set; }
        public decimal gratuito { get; set; }
        public string idExportacion { get; set; }
        public decimal exportacion { get; set; }
        public bool indicadorDescuento { get; set; }
        public decimal descuento { get; set; }
        public bool indicadorOtrosCargos { get; set; }
        public decimal otrosCargos { get; set; }
        public string idIgv { get; set; }
        public string nombreIgv { get; set; }
        public string codigoIgv { get; set; }
        public decimal igv { get; set; }
        public string idIsc { get; set; }
        public string nombreIsc { get; set; }
        public string codigoIsc { get; set; }
        public decimal isc { get; set; }
        public string idICBPER { get; set; }
        public string nombreICBPER { get; set; }
        public string codigoICBPER { get; set; }
        public decimal ICBPER { get; set; }
        public string idOth { get; set; }
        public string nombreOth { get; set; }
        public string codigoOth { get; set; }
        public decimal oth { get; set; }
        public string regimen { get; set; }
        public decimal baseImponible { get; set; }
        public decimal importePercepcion { get; set; }
        public decimal importeCobrar { get; set; }
        public decimal importeTotal { get; set; }
        public int estado { get; set; }
    }
}
