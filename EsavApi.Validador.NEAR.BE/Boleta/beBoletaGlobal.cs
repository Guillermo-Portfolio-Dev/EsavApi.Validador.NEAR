using System;

namespace EsavApi.Validador.NEAR.BE.Boleta
{
    public class beBoletaGlobal
    {
        public string IdEmisor { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public Boolean ChargeIndicator { get; set; }
        public string AllowanceChargeReason { get; set; }
        public decimal MultiplierFactor { get; set; }
        public decimal Amount { get; set; }
        public string AmountCurrency { get; set; }
        public decimal BaseAmount { get; set; }
        public string BaseAmountCurrency { get; set; }

    }
}
