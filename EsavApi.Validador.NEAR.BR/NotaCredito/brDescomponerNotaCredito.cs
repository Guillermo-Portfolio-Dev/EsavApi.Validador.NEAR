using EsavApi.Validador.BR;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.NotaCredito;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.NotaCredito
{
    public class brDescomponerNotaCredito : brGenerico
    {
        public async Task<beNotaCreditoObj> DescomponerNotaCredito(string[] lineas)
        {
            beNotaCreditoObj oNotaCredito = new beNotaCreditoObj();
            List<beNotaCreditoDetalle> Detalle = new List<beNotaCreditoDetalle>();
            List<beNotaCreditoFormaPagoCuota> Cuotas = new List<beNotaCreditoFormaPagoCuota>();
            List<beEmisorCampoAdicionalRegistro> oCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();

            try
            {
                int i = 0;
                string[] eCabecera = lineas[0].Split('|');
                var configuracion = await new brConfiguracion().Consultar(eCabecera[4].ToString(), eCabecera[5].ToString());
                var DocReferencia = eCabecera[18].Trim() != "05" ? new brConsultar().ConsultarDocReferencia(
                    eCabecera[4], eCabecera[5].Trim(), eCabecera[19], eCabecera[20], eCabecera[18]) : new DateTime();
                var FechaTipoCambio = eCabecera[6].Substring(0, 10).Split('/');
                var _DataDocReferencia = eCabecera[17].Trim() == "1" && eCabecera[18].Trim() != "05" ?
                        new brConsultar().ObtenerDocElectronicoForNC(eCabecera[4].Trim(), eCabecera[18].Trim(), eCabecera[19].Trim(), eCabecera[20].Trim()) : null;
                int dex = 1;

                decimal tGravada = 0;
                decimal tInafecta = 0;
                decimal tExonerada = 0;
                decimal tGratuita = 0;
                decimal tExportacion = 0;
                decimal tOtrosTributos = 0;
                decimal tOtrosCargos = 0;

                decimal sumbaseImponibleDetalle = 0;
                decimal sumbiconOTributos = 0;
                decimal sumbisinOTributos = 0;
                decimal sumIgvDetalle = 0;


                var ICBPER = (eCabecera.Length > 49 && !string.IsNullOrEmpty(eCabecera[49]))
                                    ? Convert.ToDecimal(eCabecera[49])
                                    : 0m;
                var ITEMOTROSCARGOS = lineas.Where(x =>
                {
                    var partes = x.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return partes.Length > 1 && partes[0].ToUpper() == "ITEM" && partes[1].ToUpper() == "OTROSCARGOS";
                }).ToList();

                var cuotas = lineas.Where(x => x.ToUpper().StartsWith("CUOTAS")).ToList();
                var anticipo = lineas.Where(x => x.ToUpper().StartsWith("ANTICIPO")).ToList();
                var Fecha = lineas.Where(x => x.ToUpper().StartsWith("FECHA")).ToList();
                var clienteLine = lineas.FirstOrDefault(x => x.ToUpper().StartsWith("CLIENTE"));
                var IGV = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[13]) ? Convert.ToDecimal(eCabecera[13]) : 0);
                var IMPORTETOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[16]) ? Convert.ToDecimal(eCabecera[16]) : 0);
                var BASEIMPONIBLE = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[11]) ? Convert.ToDecimal(eCabecera[11]) : 0);
                var OCTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[14]) ? Convert.ToDecimal(eCabecera[14]) : 0);
                var OTTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[15]) ? Convert.ToDecimal(eCabecera[15]) : 0);
                var ISCTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[12]) ? Convert.ToDecimal(eCabecera[12]) : 0);

                for (int j = 0; j < lineas.Length; j++)
                {
                    var line = lineas[j].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    #region CABECERA

                    if (line[0] == "210" && line.Length > 20 &&
                        (line[1].ToUpper() != "BIEN" && line[1].ToUpper() != "SERVICIO"))
                    {
                        DateTime fechaBoletoAerero = DateTime.MinValue;
                        if (Fecha.Count > 0)
                        {

                            var partes = Fecha.First().Split('|');
                            if (partes.Length > 1 && DateTime.TryParse(partes[1], out var fechaParseada))
                                fechaBoletoAerero = fechaParseada;
                        }
                        oNotaCredito.eCabecera = new beNotaCredito
                        {
                            accion = 1,
                            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                            numero = int.Parse(eCabecera[22]) == 0 ? 0 : int.Parse(eCabecera[22]),
                            email = line[i] == "Cliente" ? line[9] : "",
                            fechaEmision = eCabecera[6].ToString().Trim(),
                            horaEmision = DateTime.Parse(eCabecera[6].ToString()).ToString("HH:mm:ss"),
                            fechaVencimiento = eCabecera[7].ToString().Trim(),
                            tipoMoneda = eCabecera[8].ToString().Trim(),
                            tipoMonedaText = eCabecera[8].ToString() == "USD" ? "DÓLARES AMERICANOS" : "SOLES",
                            tipoMonedaSimbolo = eCabecera[8].ToString() == "USD" ? "$" : "S/",
                            tipoDocReferencia = eCabecera[18].ToString().Trim(),
                            serieDocReferencia = eCabecera[19].ToString().Trim(),
                            nroDocReferencia = (eCabecera.Length > 20 && long.TryParse(eCabecera[20]?.ToString().Trim(), out long nro)) ? nro.ToString() : "",
                            fechaEmisionDocReferencia = Fecha.Count > 0 ? null : DocReferencia.ToString("dd/MM/yyyy").Trim(),
                            tipoNotaCredito = eCabecera[3].ToString().Trim(),
                            IdSucursal = configuracion.Sucu_IdSucursal,
                            tImporteTotal = Decimal.Parse(eCabecera[16]),
                            observacion = eCabecera[24],
                            docIdentidad = line[0] == "Cliente" ? line[2].Trim() : "",
                            razonSocial = line[0] == "Cliente" ? line[3].Trim() : "",
                            tipoDocumento = line[0] == "Cliente" ? line[1].Trim() : "",
                            ChargeTotalAmount = eCabecera[14] == "" ? 0 : Convert.ToDecimal(eCabecera[14]),
                            tDescuento = eCabecera[10] == "" ? 0 : Convert.ToDecimal(eCabecera[10]),
                            tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[0]),

                            tipoCambio =
                            eCabecera[8].ToString() == "USD" && eCabecera[18].Trim() != "05" && !string.IsNullOrEmpty(eCabecera[19]) && !string.IsNullOrEmpty(eCabecera[20]) ?
                            new brObtenerTipoCambio().Obtener($"{DocReferencia.Year}-{DocReferencia.Month:D2}-{DocReferencia.Day:D2}", eCabecera[8].ToString()) :
                            eCabecera[8].ToString() == "USD" && string.IsNullOrEmpty(eCabecera[19]) && string.IsNullOrEmpty(eCabecera[20]) ?
                            new brObtenerTipoCambio().Obtener($"{FechaTipoCambio[2]}-{FechaTipoCambio[1]}-{FechaTipoCambio[0]}", eCabecera[8].ToString()) : eCabecera[8].ToString() == "USD" && eCabecera[18].Trim() == "05" ?
                            new brObtenerTipoCambio().Obtener($"{fechaBoletoAerero.Year}-{fechaBoletoAerero.Month:D2}-{fechaBoletoAerero.Day:D2}", eCabecera[8].ToString()) : 1,
                            Configuracion = new Configuracion
                            {
                                Anotacion = configuracion.Anotacion,
                                CantidadDecimal = configuracion.CSuc_CantidadDecimal,
                                CantidadDecimalDetalle = configuracion.CEmi_CantidadDecimalDetalle,
                                ColorCss = configuracion.CSuc_ColorCss,
                                CuentaDetraccion = configuracion.CSuc_CuentaDetraccion,
                                EstiloCss = configuracion.CSuc_EstiloCss,
                                FormatoNumerico = configuracion.CSuc_FormatoNumerico,
                                FormatoNumericoDetalle = configuracion.CSuc_FormatoNumericoDetalle,
                                NombreFuncionDll = configuracion.CSuc_NombreFuncionDll,
                                NroResolucion = configuracion.CSuc_NroResolucion,
                                VistaPdf = configuracion.CSuc_VistaPdf + "NotaCredito",
                                Detra027OrigDestPdf = configuracion.CSuc_Detra027OrigDestPdf,
                                ComentarioLegalExportacion = configuracion.CSuc_ComentarioLegalExportacion,
                                PorcentajeIGV = configuracion.CSuc_PorcentajeIGV,
                                LogoPDF = configuracion.Form_Icono,
                                ComentarioLegal = configuracion.CSuc_ComentarioLegal,
                                CuentaCorriente = configuracion.CSuc_CuentaCorriente,
                            },
                            tipoDocEmisor = "6",
                            rucEmisor = eCabecera[4].ToString().Trim(),
                            razonsocialEmisor = configuracion.Emis_RazonSocial,
                            RazonSocialEmisor = configuracion.Emis_RazonSocial,
                            direccionEmisor = configuracion.Emis_Direccion,
                            emailEmisor = configuracion.Emis_Correo,
                            telefonoEmisor = configuracion.Emis_Telefono,
                            faxEmisor = null,
                            distritoIdEmisor = null,
                            distritoEmisor = configuracion.Dist_Descripcion,
                            provinciaIdEmisor = null,
                            provinciaEmisor = configuracion.Prov_Descripcion,
                            departamentoIdEmisor = null,
                            departamentoEmisor = configuracion.Depa_Descripcion,
                            paisIdEmisor = "PE",
                            paisEmisor = "PERU",
                            rubroEmisor = configuracion.Rubr_IdRubro.ToString(),
                            sucursalEmisor = new Sucursal
                            {
                                codSucursal = configuracion.Sucu_IdSucursal,
                                codLocalSunat = configuracion.Sucu_CodigoLocalSunat,
                                nombreSucursal = configuracion.Sucu_Nombre,
                                direccionSucursal = configuracion.Sucu_Direccion,
                                correoSucursal = configuracion.Sucu_Correo,
                                telefonoSucursal = configuracion.Sucu_Telefono,
                                webSucursal = configuracion.Sucu_Web,
                                ubigeo = configuracion.Sucu_Ubigeo,
                                departamento = configuracion.Depa_Descripcion,
                                provincia = configuracion.Prov_Descripcion,
                                distrito = configuracion.Dist_Descripcion,
                                pais = null,
                                idRubro = 0,
                                tipoSucursal = null,
                            },
                            rutapfxEmisor = configuracion.Emis_RutaPFX,
                            clavepfxEmisor = configuracion.Emis_ClavePFX,
                            usuarioSunatEmisor = configuracion.Emis_UsuarioSunat,
                            claveSunatEmisor = configuracion.Emis_ClaveSunat,
                            valida = configuracion.Emis_OSEBalanceado,
                            fileLogoPDFEmisor = configuracion.Form_Icono,
                            vUbl = "2.1",
                            vCustomID = "2.0",
                            tipoDocEmision = eCabecera[2].ToString().Trim(),
                            usuario = eCabecera[23].ToString().Trim(),
                            Fecha = DateTime.Now.ToString(),
                            Ca02_AllowanceTotalAmount = eCabecera[8].ToString().Trim(),
                            Ca02_ChargeTotalAmount = eCabecera[8].ToString().Trim(),
                            tValorVenta = BASEIMPONIBLE

                        };
                        if (eCabecera.Length > 52)
                        {
                            if (eCabecera[52].Trim() != "")
                            {
                                oNotaCredito.eCabecera.motivoEmision = eCabecera[52].Trim();
                            }
                            else
                            {
                                oNotaCredito.eCabecera.motivoEmision = UTilidades.ObtenerMotivoEmision(eCabecera[3].Trim());
                            }
                        }
                        else
                        {
                            oNotaCredito.eCabecera.motivoEmision = UTilidades.ObtenerMotivoEmision(eCabecera[3].Trim());
                        }
                    }

                    if (line[0].ToUpper() == "FECHA")
                    {
                        oNotaCredito.eCabecera.FechaEmision_Modifica = line[1].Trim();
                        oNotaCredito.eCabecera.fechaEmisionDocReferencia =
                            DateTime.ParseExact(
                                oNotaCredito.eCabecera.FechaEmision_Modifica.ToString(),
                                "dd/MM/yyyy",
                                CultureInfo.GetCultureInfo("es-PE")
                            ).ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-PE"));
                    }
                    if (line[0].ToUpper() == "CLIENTE")
                    {
                        oNotaCredito.eCabecera.docIdentidad = line[2].Trim();
                        oNotaCredito.eCabecera.razonSocial = line[3].Trim();
                        oNotaCredito.eCabecera.ubigeo = string.IsNullOrEmpty(line[6].Trim()) ? "" : new brConsultar().ObtenerUbigeo(line[6].Trim());
                        oNotaCredito.eCabecera.email = (line.Length > 9 &&
                            !string.IsNullOrWhiteSpace(line[9]) &&
                            !new[] { "-", ",", ".", ";", "null", "N/A" }.Contains(line[9].Trim()))
                            ? string.Join(";", line[9].Replace(",", ";").Split(';')
                                .Select(e => e.Trim())
                                .Where(e => { try { return new MailAddress(e).Address == e; } catch { return false; } }))
                            : "";
                        oNotaCredito.eCabecera.direccion = line[7].Trim();
                        oNotaCredito.eCabecera.tipoDocumento = line[1].Trim();
                        oNotaCredito.eCabecera.tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[1]);
                    }
                    if (line[0].ToUpper() == "FORMAPAGO")
                    {
                        oNotaCredito.eCabecera.FormaPago = cuotas.Count > 0 ? "Credito" : "Contado";
                        oNotaCredito.eCabecera.MontoPendientePago = Decimal.Parse(line[2]);
                        oNotaCredito.eCabecera.FechaPago = line[3];

                        if (oNotaCredito.eCabecera.FormaPago.ToUpper() == "CREDITO")
                        {
                            if (cuotas.Count > 0)
                            {
                                foreach (var item in cuotas)
                                {
                                    var idcuota = item.Split('|')[1];
                                    var montocuota = item.Split('|')[2];
                                    var fechapagocuota = item.Split('|')[3];
                                    Cuotas.Add(new beNotaCreditoFormaPagoCuota
                                    {
                                        accion = 1,
                                        IdCuota = Regex.Replace(idcuota, @"(\D+)(\d+)", m =>
                                            char.ToUpper(m.Groups[1].Value[0]) + m.Groups[1].Value.Substring(1).ToLower() + m.Groups[2].Value),
                                        MontoPagoCuota = Decimal.Parse(montocuota),
                                        FechaPagoCuota = fechapagocuota
                                    });
                                }
                            }

                        }
                    }

                    oNotaCredito.eCabecera.Cuotas = Cuotas;

                    #endregion

                    #region DETALLE
                    if (line[1].ToUpper() == "BIEN" || line[1].ToUpper() == "SERVICIO")
                    {
                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(line[3]);
                        tGravada += line[2] == "10" ? Convert.ToDecimal(line[11]) : 0;
                        tInafecta += line[2] == "30" ? Convert.ToDecimal(line[11]) : 0;
                        tExonerada += line[2] == "20" ? Convert.ToDecimal(line[11]) : 0;
                        tGratuita += UTilidades.EsCodigoGratuito(line[2]) ? Convert.ToDecimal(line[11]) : 0;
                        tExportacion += line[2] == "40" ? Convert.ToDecimal(line[11]) : 0;





                        decimal bi_detalle = decimal.TryParse(line[11], out var bi) ? bi : 0;
                        decimal vu_detalle = decimal.TryParse(line[8], out var vu) ? vu : 0;
                        decimal cantidad_detalle = decimal.TryParse(line[7], out var cantidad) ? cantidad : 0;
                        decimal pctdescuento_detalle = decimal.TryParse(line[10], out var pctdescuento_) ? pctdescuento_ : 0;
                        decimal montodescuento_detalle = decimal.TryParse(line[9], out var montodescuento) ? montodescuento : 0;
                        decimal igv_detalle = decimal.TryParse(line[12], out var igv_) ? igv_ : 0;
                        decimal importe_detalle =
                            decimal.TryParse(line[19], out var imported) && UTilidades.EsCodigoGratuitoGravado(line[2]) ? bi : imported;
                        decimal oc_detalle = decimal.TryParse(line[15], out var oc) ? oc : 0;
                        decimal pctOtrosCargos_detalle = decimal.TryParse(line[16], out var pctOtrosCargos) ? pctOtrosCargos : 0;
                        decimal otrosTributos_detalle = decimal.TryParse(line[17], out var otrosTributos) ? otrosTributos : 0;
                        decimal pctOTH_detalle = decimal.TryParse(line[18], out var pctOTH) ? pctOTH * 100 : 0;
                        decimal isc_detalle = decimal.TryParse(line[13], out var isc) ? isc : 0;
                        decimal pctISC_detalle = decimal.TryParse(line[14], out decimal pctISC)
                                    ? (eCabecera[3].Trim() == "01" ? pctISC : pctISC * 100)
                                    : 0;

                        sumbaseImponibleDetalle += !UTilidades.EsCodigoGratuito(line[2]) && line[2].Trim() != "21" ? Convert.ToDecimal(line[11]) : 0;
                        sumbiconOTributos += otrosTributos_detalle != 0 ? Convert.ToDecimal(line[11]) : 0;
                        sumbisinOTributos += otrosTributos_detalle == 0 ? Convert.ToDecimal(line[11]) : 0;
                        sumIgvDetalle += (line[12] == "0" || line[12] == "") ? 0 : line[2].Trim() == "10" ? Convert.ToDecimal(line[12]) : 0;
                        tOtrosTributos += otrosTributos_detalle;
                        tOtrosCargos += oc_detalle;

                        #region CALCULOS_ANTICIPOS
                        //if (_DataDocReferencia.Detalle.Count > 0 && _DataDocReferencia.MontoAnticipoGlobal > 0)
                        //{
                        //    var GlobalAnticipoValor = _DataDocReferencia.MontoAnticipoGlobal;
                        //    var GlobalAnticipoBase = _DataDocReferencia.MontoBaseAnticipoGlobal;
                        //    var GlobalAnticipoCodigo = _DataDocReferencia.CodigoAnticipoGlobal;
                        //    var PctDescuentoAnticipo = Math.Round(GlobalAnticipoValor, 2) / Math.Round((decimal)_DataDocReferencia.ValorVenta, 2);

                        //    var GlobalDescuentoPCT = _DataDocReferencia.PorcentajeDescuentoGlobal;

                        //    var baseImponible = (decimal)_DataDocReferencia.Detalle[dex].ValorUnitario * (decimal)_DataDocReferencia.Detalle[dex].Cantidad +
                        //        (_DataDocReferencia.Detalle[dex].CodigoOtroCargo == "47" ? (decimal)_DataDocReferencia.Detalle[dex].OtroCargo : 0) - (_DataDocReferencia.Detalle[dex].CodigoDescuento == "00" ? (decimal)_DataDocReferencia.Detalle[dex].Descuento : 0);

                        //    Detalle.Add(new beNotaCreditoDetalle()
                        //    {
                        //        accion = 1,
                        //        index = dex,
                        //        cantidad = line[7] == "" ? 0 : Convert.ToDecimal(line[7]),
                        //        codigoSunat = line[5].Trim(),
                        //        descripcion = line[6].Trim(),
                        //        unidadMedida = line[3].Trim(),
                        //        UnidadMedidaText = UnidadMedidaText,
                        //        codigo = line[4].Trim(),
                        //        codigoPrecioUnitario = UTilidades.EsCodigoGratuito(line[2]) ? "02" : "01",
                        //        valorUnitario = (decimal)_DataDocReferencia.Detalle[dex].ValorUnitario * (1 - PctDescuentoAnticipo),
                        //        ValorVenta = (decimal)_DataDocReferencia.Detalle[dex].PrecioVenta,
                        //        PrecioVenta = (decimal)_DataDocReferencia.Detalle[dex].PrecioVenta,
                        //        precioUnitario = (decimal)_DataDocReferencia.Detalle[dex].PrecioVentaUnitario * (1 - PctDescuentoAnticipo),
                        //        idIGV = "1000",
                        //        codigoGravExoIna = line[2],
                        //        graExoIna = line[2] == "10" ? "1001" : line[2] == "30" ? "1002" : line[2] == "20" ? "1003" : line[2] == "40" ? "1000" : "1004",
                        //        pctIGV = configuracion.CSuc_PorcentajeIGV * 100,
                        //        igv = (decimal)_DataDocReferencia.Detalle[dex].IGV * (1 - PctDescuentoAnticipo),
                        //        idISC = "2000",
                        //        codigoTipoISC = "01",
                        //        tipoISC = null,
                        //        pctISC = pctISC_detalle,
                        //        isc = isc_detalle,
                        //        idOTH = "9999",
                        //        codeOTH = "OTH",
                        //        nameOTH = "OTROS",
                        //        tipoOTH = "I",
                        //        pctOTH = pctOTH_detalle,
                        //        idICBPER = "7152",
                        //        otrosTributosDetalle = otrosTributos_detalle,
                        //        descuento = montodescuento_detalle,
                        //        baseImponible = baseImponible * (1 - PctDescuentoAnticipo),
                        //        importeTotalItem = (decimal)_DataDocReferencia.Detalle[dex].PrecioVenta * (1 - PctDescuentoAnticipo),
                        //        codeIGV = "VAT",
                        //        nameIGV = "IGV",
                        //        codeISC = "EXC",
                        //        nameISC = "ISC",
                        //        codigoMotivoDescuento = null,
                        //        codigoMotivoOtrosCargosDetalle = "48",
                        //        cantidadICBPER = 0,
                        //        perUnitICBPER = 0,
                        //        montoICBPER = 0,
                        //        usuario = eCabecera[23].ToString(),
                        //        Ca02_ValorVenta = eCabecera[8].ToString().Trim(),
                        //        Ca02_TaxAmountIgv = eCabecera[8].ToString().Trim(),
                        //        Ca02_TaxAmountIsc = eCabecera[8].ToString().Trim(),
                        //        Ca02_TaxAmount_TaxSubTotalIgv = eCabecera[8].ToString().Trim(),
                        //        Ca02_TaxAmount_TaxSubTotalIsc = eCabecera[8].ToString().Trim(),
                        //        Ca02_TaxAmountOth = eCabecera[8].ToString().Trim(),
                        //        Ca02_ValorUnitario = eCabecera[8].ToString().Trim(),
                        //        tipoMoneda = eCabecera[8].ToString().Trim(),
                        //        Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        //        Ca14_TaxAmount_TaxSubTotalIgv = "1000",
                        //        Ca05_TaxAmount_TaxSubTotalICBPER =
                        //        line[2].Trim() == "40" ? "9995" : UTilidades.EsCodigoGratuito(line[2].Trim()) ? "9996"
                        //        : UTilidades.Exonerado(line[2].Trim()) ? "9997" : UTilidades.Inafecto(line[2].Trim()) ? "9998" :
                        //        "1000"
                        //    });
                        //}
                        #endregion
                        Detalle.Add(new beNotaCreditoDetalle()
                        {
                            accion = 1,
                            index = dex,
                            cantidad = line[7] == "" ? 0 : Convert.ToDecimal(line[7]),
                            codigoSunat = line[5].Trim(),
                            descripcion = line[6].Trim(),
                            unidadMedida = line[3].Trim(),
                            UnidadMedidaText = UnidadMedidaText.Item1,
                            UnidadAbreviatura = UnidadMedidaText.Item2,
                            codigo = line[4].Trim(),
                            codigoPrecioUnitario = UTilidades.EsCodigoGratuito(line[2]) ? "02" : "01",
                            valorUnitario = vu_detalle,
                            ValorVenta = bi_detalle,
                            PrecioVenta = bi_detalle,
                            precioUnitario =
                                                    ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                    && line[2] != "40"
                                                    && !UTilidades.EsCodigoGratuito(line[2]) ? (ICBPER + (importe_detalle)) / cantidad :
                                                    ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                    && line[2] == "40" ? 1 :
                                                    UTilidades.EsCodigoGratuito(line[2]) ? vu_detalle :
                                                     (igv_detalle > 0 && cantidad == 1 && ITEMOTROSCARGOS.Count == 0 && montodescuento_detalle == 0) ?
                                                     vu_detalle + igv_detalle + otrosTributos + oc_detalle + isc_detalle :
                                                     ITEMOTROSCARGOS.Any(x =>
                                                     {
                                                         string[] otline = x.Split('|');
                                                         return int.Parse(otline[2]) == int.Parse(line[0]);
                                                     }) && igv_detalle > 0 ? bi + igv_detalle :
                                                     ITEMOTROSCARGOS.Any(x =>
                                                     {
                                                         string[] otline = x.Split('|');
                                                         return int.Parse(otline[2]) == int.Parse(line[0]);
                                                     }) && igv_detalle == 0 ? importe_detalle / cantidad :
                                                     importe_detalle > 0 && cantidad > 0 ? importe_detalle / cantidad :
                                                     0,
                            idIGV = "1000",
                            codigoGravExoIna = line[2],
                            graExoIna = line[2] == "10" ? "1001" : line[2] == "30" ? "1002" : line[2] == "20" ? "1003" : line[2] == "40" ? "1000" : "1004",
                            pctIGV = configuracion.CSuc_PorcentajeIGV * 100,
                            igv = igv_detalle,
                            idISC = "2000",
                            codigoTipoISC = "01",
                            tipoISC = null,
                            pctISC = pctISC_detalle,
                            isc = isc_detalle,
                            idOTH = "9999",
                            codeOTH = "OTH",
                            nameOTH = "OTROS",
                            tipoOTH = "I",
                            pctOTH = pctOTH_detalle,
                            idICBPER = "7152",
                            otrosTributosDetalle = otrosTributos_detalle,
                            descuento = montodescuento_detalle,
                            baseImponible = bi_detalle,
                            importeTotalItem = importe_detalle,
                            codeIGV = "VAT",
                            nameIGV = "IGV",
                            codeISC = "EXC",
                            nameISC = "ISC",
                            codigoMotivoDescuento = null,
                            codigoMotivoOtrosCargosDetalle = "48",
                            cantidadICBPER = 0,
                            perUnitICBPER = 0,
                            montoICBPER = 0,
                            usuario = eCabecera[23].ToString(),
                            Ca02_ValorVenta = eCabecera[8].ToString().Trim(),
                            Ca02_TaxAmountIgv = eCabecera[8].ToString().Trim(),
                            Ca02_TaxAmountIsc = eCabecera[8].ToString().Trim(),
                            Ca02_TaxAmount_TaxSubTotalIgv = eCabecera[8].ToString().Trim(),
                            Ca02_TaxAmount_TaxSubTotalIsc = eCabecera[8].ToString().Trim(),
                            Ca02_TaxAmountOth = eCabecera[8].ToString().Trim(),
                            Ca02_ValorUnitario = eCabecera[8].ToString().Trim(),
                            tipoMoneda = eCabecera[8].ToString().Trim(),
                            Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            Ca14_TaxAmount_TaxSubTotalIgv = "1000",
                            Ca05_TaxAmount_TaxSubTotalICBPER =
                                line[2].Trim() == "40" ? "9995" : UTilidades.EsCodigoGratuito(line[2].Trim()) ? "9996"
                                : UTilidades.Exonerado(line[2].Trim()) ? "9997" : UTilidades.Inafecto(line[2].Trim()) ? "9998" :
                                "1000"
                        });
                        dex++;
                    }
                    oNotaCredito.lDetalle = Detalle;
                    #endregion
                }

                #region Total


                oNotaCredito.eTotal = new beNotaCreditoImpuesto
                {
                    accion = 1,
                    tipoIGV = "I",
                    idIGV = "1000",
                    codeIGV = "VAT",
                    nameIGV = "IGV",
                    codigotGravada = "1001",
                    idISC = "2000",
                    tipoISC = "I",
                    codeISC = "EXC",
                    nameISC = "ISC",
                    idINF = "9998",
                    codeINF = "FRE",
                    nameINF = "INA",
                    idEXO = "9997",
                    codeEXO = "VAT",
                    nameEXO = "EXO",
                    codigotExonerada = "1003",
                    nameExonerada = "EXONERADA",
                    tipoExonerada = "T",
                    idGRT = "9996",
                    idOTH = "9999",
                    nameOTH = "OTROS",
                    codeOTH = "OTH",
                    tipoOTH = "I",
                    codeGRT = "FRE",
                    nameGRT = "GRA",
                    tipoExportacion = "T",
                    codigotExportacion = "1000",
                    tipoGravada = "T",
                    nameGravada = "GRAVADO",
                    idEXP = "9995",
                    codeEXP = "FRE",
                    nameEXP = "EXP",
                    nameExportacion = "EXP",
                    idICBPER = "7152",
                    codeICBPER = "OTH",
                    nameICBPER = "ICBPER",
                    tipoICBPER = "I",
                    codigotDescuento = "2005",
                    nameDescuento = "DESCUENTO",
                    tipoDescuento = "T",
                    codigotGratuita = "1004",
                    tipoGratuita = "T",
                    nameGratuita = "GRATUITO",
                    codigotInafecta = "1002",
                    nameInafecta = "INAFECTO",
                    tipoInafecta = "T",
                    tIgv = IGV,
                    tIsc = ISCTOTAL,
                    tImporteTotal = IMPORTETOTAL,
                    tOtrosTributos = tOtrosTributos,
                    tInafecta = 0,
                    tExonerada = 0,
                    tGratuita = 0,
                    tExportacion = 0,
                    tValorVenta = BASEIMPONIBLE,
                    tBaseImponible = BASEIMPONIBLE,
                    usuario = eCabecera[23].ToString(),
                    Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Ca02_TaxAmount_TaxSubTotal = eCabecera[8].ToString().Trim(),
                    Ca02_TaxAmount = eCabecera[8].ToString().Trim()
                };
                #endregion


                //if (_DataDocReferencia.Gravado > 0)
                //{
                //    oNotaCredito.eTotal.tGravada = (decimal)_DataDocReferencia.Gravado;
                //}
                //else
                //{
                //    oNotaCredito.eTotal.tGravada = tGravada;
                //}

                oNotaCredito.eTotal.tGravada = tGravada;

                // Calcular tBaseImponible y tOTHBaseAmount
                if (tExonerada > 0)
                {
                    oNotaCredito.eTotal.tBaseImponible = oNotaCredito.eTotal.tGravada + tInafecta + tExonerada + tExportacion;
                    oNotaCredito.eTotal.tValorVenta = oNotaCredito.eTotal.tBaseImponible;
                }
                else if (tInafecta > 0)
                {

                    oNotaCredito.eTotal.tBaseImponible = oNotaCredito.eTotal.tGravada + tInafecta + tExonerada + tExportacion;
                    oNotaCredito.eTotal.tValorVenta = oNotaCredito.eTotal.tBaseImponible;

                }
                else if (tGravada > 0)
                {
                    oNotaCredito.eTotal.tBaseImponible = oNotaCredito.eTotal.tGravada + tInafecta + tExonerada + tExportacion;
                    oNotaCredito.eTotal.tValorVenta = oNotaCredito.eTotal.tBaseImponible;
                }
                else
                {
                    //oBoleta.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + sumIgvDetalle - oBoleta.eTotal.tDescuentoGlobal;
                    oNotaCredito.eTotal.tBaseImponible = oNotaCredito.eTotal.tGravada + tInafecta + tExonerada + tExportacion + sumIgvDetalle;
                }


                #region CampoAdicional
                int z = 1;

                var valoresCA = new brConsultar().ListarCampoAdicional(eCabecera[4].Trim(), configuracion.Rubr_IdRubro.ToString(), eCabecera[2].Trim(), false);
                if (eCabecera.Length >= 49)
                {
                    for (int x = 34; x < 49; x++)
                    {
                        var campoAdicional = eCabecera[x].Trim();
                        if (!string.IsNullOrEmpty(campoAdicional) && valoresCA != null)
                        {
                            var valor = campoAdicional.Split(':');
                            var valorCampo = string.Join(":", valor.Skip(1));
                            foreach (var item in valoresCA)
                            {
                                if (valor[0].Trim().ToLower() == item.IdCampoAdicional.ToLower())
                                {
                                    oCampoAdicional.Add(new beEmisorCampoAdicionalRegistro
                                    {
                                        Accion = 1,
                                        IdEmisor = eCabecera[4].Trim(),
                                        IdRubro = configuracion.Rubr_IdRubro,
                                        IdCampoAdicional = item.IdCampoAdicional,
                                        Ca01_Id = eCabecera[2].Trim(),
                                        Serie = eCabecera[21].Trim(),
                                        Numero = Convert.ToInt32(eCabecera[22].Trim()),
                                        Index = z,
                                        Titulo = item.Titulo,
                                        Valor = valorCampo,
                                        EsDetalle = item.EsDetalle,
                                        EnXML = item.EnXML,
                                        EnRepresentacionImpresa = item.EnRepresentacionImpresa
                                    });
                                    oNotaCredito.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                                }
                                z++;
                            }
                        }
                        else
                        {
                            oNotaCredito.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                        }
                    }
                }
                else
                {
                    oNotaCredito.lCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();
                }

                #endregion


                oNotaCredito.eTotal.tInafecta = tInafecta;
                oNotaCredito.eTotal.tExonerada = tExonerada;
                oNotaCredito.eTotal.tGratuita = tGratuita;
                oNotaCredito.eTotal.tExportacion = tExportacion;

                if (tOtrosCargos == 0)
                {
                    oNotaCredito.eTotal.tOtrosCargos = OCTOTAL;
                }
                else
                {
                    oNotaCredito.eTotal.tOtrosCargos = tOtrosCargos;
                }

                if (tOtrosTributos == 0)
                {
                    oNotaCredito.eTotal.tOtrosTributos = OTTOTAL;
                }
                else
                {
                    oNotaCredito.eTotal.tOtrosTributos = tOtrosTributos;
                }


                //tOTHBaseAmount
                if ((tOtrosTributos > 0) && BASEIMPONIBLE > sumbaseImponibleDetalle && sumbiconOTributos == 0)
                {
                    oNotaCredito.eTotal.tOTHBaseAmount = (BASEIMPONIBLE - sumbaseImponibleDetalle) - sumbisinOTributos;
                }
                else if ((tOtrosTributos > 0) && BASEIMPONIBLE == sumbaseImponibleDetalle && sumbisinOTributos > 0)
                {
                    oNotaCredito.eTotal.tOTHBaseAmount = (BASEIMPONIBLE) - sumbisinOTributos;
                }
                else if (sumbiconOTributos > 0)
                {
                    oNotaCredito.eTotal.tOTHBaseAmount = sumbiconOTributos;
                }
                else
                {
                    oNotaCredito.eTotal.tOTHBaseAmount = 0;
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("DescomponerNotaCredito", ex);
            }


            return oNotaCredito;
        }
    }
}
