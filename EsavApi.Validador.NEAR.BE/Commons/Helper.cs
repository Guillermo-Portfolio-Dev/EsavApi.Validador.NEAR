using System;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class Helper
    {
        public int Accion { get; set; }
        public string UBLVersionID { get; set; }
        public string CustomizationID { get; set; }
        public string Usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }
        public int Registro { get; set; }
        public bool Estado { get; set; }
        public decimal Tipocambio { get; set; }
        public string TipoDocumentoDescripcion { get; set; }
        public string MonedaDescripcion { get; set; }
        public string MonedaSimbolo { get; set; }
        public string DigestValue { get; set; }
        public Configuracion Configuracion { get; set; }
    }
}
