using EsavApi.Validador.NEAR.BE.Commons;
using System;

namespace EsavApi.Validador.NEAR.BE.ComunicacionBaja
{
    public class beComunicacionBaja : Emisor
    {
        public int Accion { get; set; }
        public int Id { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string fechaBaja { get; set; }
        public string fechaReferencia { get; set; }
        public string horaBaja { get; set; }
        public string tipoDocCliente { get; set; }
        public string nroDocCliente { get; set; }
        public string razonSocialCliente { get; set; }
        public string codigoTipoDocElec { get; set; }
        public string vUbl { get; set; }
        public string vCustomID { get; set; }
        public string tipoDocEmision { get; set; }
        public string fechaEmision { get; set; }
        public string FechaEmisionDocumentos { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }
        public Configuracion Configuracion { get; set; }

    }
}
