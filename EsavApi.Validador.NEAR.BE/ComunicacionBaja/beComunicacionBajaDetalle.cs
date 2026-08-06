using System;

namespace EsavApi.Validador.NEAR.BE.ComunicacionBaja
{
    public class beComunicacionBajaDetalle
    {
        public int Accion { get; set; }
        public string rucEmisor { get; set; }
        public string Ca01_Id { get; set; }
        public string tipoDocEmision { get; set; }
        public string SerieCabecera { get; set; }
        public string NumeroCabecera { get; set; }
        public string fechaEmision { get; set; }
        public int Id { get; set; }
        public int CBaj_Id { get; set; }
        public string codigoTipoDocElec { get; set; }
        public string serie { get; set; }
        public int numero { get; set; }
        public string motivoBaja { get; set; }
        public string usuario { get; set; }
        public string Fecha { get; set; }
        public string Ip { get; set; }

    }
}
