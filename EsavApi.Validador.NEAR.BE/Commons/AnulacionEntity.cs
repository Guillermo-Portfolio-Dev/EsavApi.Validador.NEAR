using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EsavApi.Validador.NEAR.BE.Commons
{
    public class AnulacionEntity
    {
        public string Correlativo { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DynamicTableEntity EntidadOriginal { get; set; }
    }
}
