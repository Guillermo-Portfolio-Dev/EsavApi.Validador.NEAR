namespace EsavApi.Validador.NEAR.BE.ValeCredito
{
    public class ValeCreditoTotal
    {
        public string nameDescuento { get; set; }
        public string tipoDescuento { get; set; }
        public string codigotExportacion { get; set; }
        public string nameExportacion { get; set; }
        public string tipoExportacion { get; set; }
        public string regimenPercepcion { get; set; }
        public string codigotImportePercepcion { get; set; }
        public string nameImportePercepcion { get; set; }
        public string tipoPercepcion { get; set; }
        public decimal tBaseImponible { get; set; }
        public decimal tSubtotal { get; set; }
        public string codigotDescuento { get; set; }
        public decimal tDescuento { get; set; }
        public decimal tIsc { get; set; }
        public decimal tExonerada { get; set; }
        public decimal tGratuita { get; set; }
        public decimal tGravada { get; set; }
        public decimal tInafecta { get; set; }
        public decimal tOtrosTributos { get; set; }
        public decimal tIcbper { get; set; }
        public decimal tOtrosCargos { get; set; }
        public decimal tExportacion { get; set; }
        public decimal tDescuentoGlobal { get; set; }
        public decimal tImporteTotal { get; set; }
        public decimal tIgv { get; set; }
        public string tipoGratuita { get; set; }
        public string nameGratuita { get; set; }
        public string codigotGratuita { get; set; }
        public string idIGV { get; set; }
        public string codeIGV { get; set; }
        public string nameIGV { get; set; }
        public string tipoIGV { get; set; }
        public string idISC { get; set; }
        public string codeISC { get; set; }
        public string nameISC { get; set; }
        public string tipoISC { get; set; }
        public string idOTH { get; set; }
        public string codeOTH { get; set; }
        public string nameOTH { get; set; }
        public string tipoOTH { get; set; }
        public string idICBPER { get; set; }
        public string codeICBPER { get; set; }
        public string nameICBPER { get; set; }
        public string tipoICBPER { get; set; }
        public string codigotGravada { get; set; }
        public string nameGravada { get; set; }
        public string tipoGravada { get; set; }
        public string codigotInafecta { get; set; }
        public string nameInafecta { get; set; }
        public string tipoInafecta { get; set; }
        public string codigotExonerada { get; set; }
        public string nameExonerada { get; set; }
        public string tipoExonerada { get; set; }
        public decimal tImportePercepcion { get; set; }
        public decimal tImporteCobrar { get; set; }
    }
}
