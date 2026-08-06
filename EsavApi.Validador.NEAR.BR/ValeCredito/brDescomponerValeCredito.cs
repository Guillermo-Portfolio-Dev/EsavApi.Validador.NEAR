using EsavApi.Validador.BR;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.ValeCredito;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.ValeCredito
{
    public class brDescomponerValeCredito : brGenerico
    {
        public async Task<beValeCreditoObj> DescomponerValeCredito(string[] lineas)
        {
            beValeCreditoObj oValeCredito = new beValeCreditoObj();
            List<beValeCreditoDetalle> Detalle = new List<beValeCreditoDetalle>();
            List<beEmisorCampoAdicionalRegistro> oCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();

            try
            {
                string[] eCabecera = lineas[0].Split('|');
                var configuracion = await new brConfiguracion().Consultar(eCabecera[4].ToString(), eCabecera[5].ToString());
                var descuentoGlobal = lineas.FirstOrDefault(x => x.ToUpper().StartsWith("DESCUENTO"));
                var clienteLine = lineas.FirstOrDefault(x => x.ToUpper().StartsWith("CLIENTE"));
                var otrosCargos = lineas.FirstOrDefault(x => x.ToUpper().StartsWith("OTROSCARGOS"));
                var desc = string.IsNullOrEmpty(descuentoGlobal) ? new string[4] : descuentoGlobal.Split('|');
                var FechaTipoCambio = eCabecera[6].Substring(0, 10).Split('/');
                var icbper = lineas.Where(x => x.ToUpper().StartsWith("ICBPER")).ToList();

                var ITEMOTROSCARGOS = lineas.Where(x =>
                {
                    var partes = x.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return partes.Length > 1 && partes[0].ToUpper() == "ITEM" && partes[1].ToUpper() == "OTROSCARGOS";
                }).ToList();
                var ITEMDESCUENTO = lineas.Where(x =>
                {
                    var partes = x.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return partes.Length > 1 && partes[0].ToUpper() == "ITEM" && partes[1].ToUpper() == "DESCUENTO";
                }).ToList();
                var ITEMPLACA = lineas.Where(x =>
                {
                    var partes = x.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return partes.Length > 1 && partes[0].ToUpper() == "ITEM" && partes[1].ToUpper() == "PLACA";
                }).ToList();
                var detalleExtra = lineas.FirstOrDefault(x => x.Trim().StartsWith("D") && (x.Trim().Length == 1 || !char.IsLetter(x.Trim()[1])));
                var detalleExtraHPT = detalleExtra == null ? new string[1] { "" } : new string[] { detalleExtra };
                int dex = 1;

                decimal tGravada = 0;
                decimal tInafecta = 0;
                decimal tExonerada = 0;
                decimal tGratuita = 0;
                decimal tExportacion = 0;
                decimal tOtrosTributos = 0;
                decimal tOtrosCargos = 0;
                decimal timporteTotal = 0;
                decimal sumbaseImponibleDetalle = 0;
                decimal sumbisinOTributos = 0;
                decimal sumbiconOTributos = 0;
                decimal sumIgvDetalle = 0;
                decimal sumpctOtroscargos = 0;
                decimal sumdescuentoDetalle = 0;

                var IGV = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[13]) ? Convert.ToDecimal(eCabecera[13]) : 0);
                var BASEIMPONIBLE = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[11]) ? Convert.ToDecimal(eCabecera[11]) : 0);
                var SUBTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[9]) ? Convert.ToDecimal(eCabecera[9]) : 0);
                var IMPORTETOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[16]) ? Convert.ToDecimal(eCabecera[16]) : 0);
                var ISCTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[12]) ? Convert.ToDecimal(eCabecera[12]) : 0);
                var OTTOTAL = decimal.TryParse(eCabecera[15], out var temp_) ? temp_ : (decimal.TryParse(eCabecera[50], out temp_) ? temp_ : 0);
                var OCTOTAL = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[14]) ? Convert.ToDecimal(eCabecera[14]) : 0);
                var DESCUENTO = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[10]) ? Convert.ToDecimal(eCabecera[10]) : 0);
                var PDESCUENTO = Convert.ToDecimal(!string.IsNullOrEmpty(eCabecera[10]) ? Convert.ToDecimal(eCabecera[10]) : 0);
                var ICBPER = (eCabecera.Length > 49 && !string.IsNullOrEmpty(eCabecera[49]))
                                ? Convert.ToDecimal(eCabecera[49])
                                : 0m;


                for (int i = 0; i < lineas.Length; i++)
                {
                    var line = lineas[i].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    #region CABECERA

                    if (line[0].Trim() == "210" && line.Length > 20 &&
                        (line[1].ToUpper() != "BIEN" && line[1].ToUpper() != "SERVICIO"))
                    {
                        oValeCredito.eCabecera = new beValeCredito
                        {
                            accion = 1,
                            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                            numero = int.Parse(eCabecera[22]) == 0 ? 0 : int.Parse(eCabecera[22]),
                            email = line[i] == "Cliente" ? line[9] : "",
                            fechaEmision = eCabecera[6].ToString().Trim(),
                            horaEmision = DateTime.ParseExact(eCabecera[6], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("HH:mm:ss"),
                            fechaVencimiento = eCabecera[7].ToString().Trim(),
                            tipoMoneda = eCabecera[8].ToString().Trim(),
                            tipoMonedaText = eCabecera[8].ToString() == "USD" ? "DÓLARES AMERICANOS" : "SOLES",
                            tipoMonedaSimbolo = eCabecera[8].ToString() == "USD" ? "$" : "S/",
                            IdSucursal = configuracion.Sucu_IdSucursal,
                            observacion = eCabecera[24],
                            docIdentidad = line[0] == "Cliente" ? line[2].Trim() : "",
                            razonSocial = line[0] == "Cliente" ? UTilidades.LimpiarTexto(line[3].Trim()) : "",
                            tipoDocumento = line[0] == "Cliente" ? line[1].Trim() : "",
                            tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[1]),

                            tipoCambio = eCabecera[8].ToString() == "USD" ? new brObtenerTipoCambio().Obtener($"{FechaTipoCambio[2]}-{FechaTipoCambio[1]}-{FechaTipoCambio[0]}", eCabecera[8].ToString()) : 1,
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
                                VistaPdf = configuracion.CSuc_VistaPdf + "ValeCredito",
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
                            direccionEmisor = configuracion.Emis_Direccion,
                            emailEmisor = configuracion.Emis_Correo,
                            telefonoEmisor = configuracion.Emis_Telefono,
                            faxEmisor = null,
                            distritoIdEmisor = configuracion.Sucu_Ubigeo,
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
                                correoSucursal = null,
                                telefonoSucursal = null,
                                ubigeo = configuracion.Sucu_Ubigeo,
                                departamento = configuracion.Depa_Descripcion,
                                provincia = configuracion.Prov_Descripcion,
                                distrito = configuracion.Dist_Descripcion,
                                pais = null,
                                idRubro = configuracion.Rubr_IdRubro,
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
                            Fecha = DateTime.Now.ToString("dd-MM-yyyy"),
                        };
                    }

                    #endregion

                    #region CLIENTE
                    if (line[0].ToUpper() == "CLIENTE")
                    {
                        oValeCredito.eCabecera.docIdentidad = line[2].Trim();
                        //oBoleta.eCabecera.razonSocial = line[1].Trim() == "6" ? new brConsultar().ObtenerRuc(line[2].Trim(), null).RazonSocial : line[3].Trim();
                        oValeCredito.eCabecera.razonSocial =
                            line[1].Trim() == "6"
                                ? new brConsultar().ObtenerRuc(line[2].Trim(), null).RazonSocial
                                : line[1].Trim() == "1"
                                    ? UTilidades.LimpiarTexto(line[3].Trim())
                                    : line[3].Trim();
                        oValeCredito.eCabecera.tipoDocumento = line[1].Trim();
                        oValeCredito.eCabecera.direccion = line[7].Trim();
                        oValeCredito.eCabecera.email = (line.Length > 9 &&
                            !string.IsNullOrWhiteSpace(line[9]) &&
                            !new[] { "-", ",", ".", ";", "null", "N/A" }.Contains(line[9].Trim()))
                            ? string.Join(";", line[9].Replace(",", ";").Split(';')
                                .Select(e => e.Trim())
                                .Where(e => { try { return new MailAddress(e).Address == e; } catch { return false; } }))
                            : "";
                        oValeCredito.eCabecera.tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[1]);
                    }
                    #endregion

                    #region DETALLE
                    if (line[1].ToUpper() == "BIEN" || line[1].ToUpper() == "SERVICIO")
                    {
                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(line[3]);
                        tGravada += line[2].Trim() == "10" ? Convert.ToDecimal(line[11]) : 0;
                        tInafecta += line[2].Trim() == "30" ? Convert.ToDecimal(line[11]) : 0;
                        tGratuita += UTilidades.EsCodigoGratuito(line[2]) == true ? Convert.ToDecimal(line[11]) : 0;
                        tExportacion += line[2] == "40" ? Convert.ToDecimal(line[11]) : 0;
                        tOtrosTributos += line[17] == "" ? 0 : Convert.ToDecimal(line[17]);
                        sumbisinOTributos += line[17] == "" || line[17] == "0" ? Convert.ToDecimal(line[11]) : 0;
                        sumbiconOTributos += (line[17] != "" && line[17] != "0" && line[17] != "0.00") ? Convert.ToDecimal(line[11]) : 0;
                        tOtrosCargos += line[15] == "" ? 0 : Convert.ToDecimal(line[15]);
                        timporteTotal += (line[19] == "0" || line[19] == "" ? 0 : decimal.TryParse(line[19], out var temp) ? temp : 0);
                        sumbaseImponibleDetalle += line[2].Trim() != "21" ? Convert.ToDecimal(line[11]) : 0;
                        sumpctOtroscargos += line[16] == "0" || line[16] == "" ? 0 : Convert.ToDecimal(line[16]);
                        sumIgvDetalle += (line[12] == "0" || line[12] == "") ? 0 : line[2].Trim() == "10" ? Convert.ToDecimal(line[12]) : 0;
                        sumdescuentoDetalle += (line[9] == "0" || line[9] == "") ? 0 : Convert.ToDecimal(line[9]);


                        decimal bi_detalle = decimal.TryParse(line[11], out var bi) ? bi : 0;
                        decimal vu_detalle = decimal.TryParse(line[8], out var vu) ? vu : 0;
                        decimal cantidad_detalle = decimal.TryParse(line[7], out var cantidad) ? cantidad : 0;
                        decimal pctdescuento_detalle = decimal.TryParse(line[10], out var pctdescuento_) ? pctdescuento_ : 0;
                        decimal montodescuento_detalle = decimal.TryParse(line[9], out var montodescuento) ? montodescuento : 0;
                        decimal igv_detalle = decimal.TryParse(line[12], out var igv_) ? igv_ : 0;
                        decimal importe_detalle = decimal.TryParse(line[19], out var imported) ? imported : 0;
                        decimal oc_detalle = decimal.TryParse(line[15], out var oc) ? oc : 0;
                        decimal pctOtrosCargos_detalle = decimal.TryParse(line[16], out var pctOtrosCargos) ? pctOtrosCargos : 0;
                        decimal otrosTributos_detalle = decimal.TryParse(line[17], out var otrosTributos) ? otrosTributos : 0;
                        decimal pctOTH_detalle = decimal.TryParse(line[18], out var pctOTH) ? pctOTH * 100 : 0;
                        decimal pctISC_detalle = decimal.TryParse(line[14], out var pctISC)
                            ? (UTilidades.TipoOperacionDetraccionParaIscBoleta(eCabecera[3].ToString().Trim()) ? pctISC * 100 : pctISC)
                            : 0;
                        decimal isc_detalle = decimal.TryParse(line[13], out var isc) ? isc : 0;

                        decimal valor9 = string.IsNullOrWhiteSpace(line[9]) ? 0m : Convert.ToDecimal(line[9]);
                        decimal valor11 = string.IsNullOrWhiteSpace(line[11]) ? 0m : Convert.ToDecimal(line[11]);
                        tExonerada +=
                            UTilidades.Exonerado(line[2]) && valor9 == 0m
                                ? valor11
                                : UTilidades.Exonerado(line[2]) && valor9 != 0m
                                    ? bi
                                    : 0m;

                        Detalle.Add(new beValeCreditoDetalle()
                        {
                            accion = 1,
                            index = dex.ToString(),
                            cantidad = cantidad_detalle,
                            descripcion = line[6].Trim().Replace("\\r\\n", "\n").Replace("<br>", "\n"),
                            codigo = line[4].Trim(),
                            unidadMedida = line[3].Trim(),
                            unidadMedidaText = UnidadMedidaText.Item1.Trim(),
                            valorUnitario = vu_detalle,
                            precioUnitario = ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                && line[2] != "40" && !UTilidades.EsCodigoGratuito(line[2]) ? (ICBPER + (importe_detalle)) / cantidad :
                                                ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                && line[2] == "40" ? 1 :
                                                 UTilidades.EsCodigoGratuito(line[2]) ? vu_detalle :
                                                 (igv_detalle > 0 && cantidad == 1 && ITEMOTROSCARGOS.Count == 0 && montodescuento_detalle == 0) ? vu_detalle + igv_detalle + otrosTributos + oc_detalle + isc_detalle :
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
                            codigoGravExoIna = line[2],
                            graExoIna = line[2] == "10" ? "1001" : line[2] == "30" ? "1002" : line[2] == "20" ? "1003" : line[2] == "40" ? "1000" : "1004",
                            igv = igv_detalle,
                            codigoTipoISC = "01",
                            isc = isc_detalle,
                            otrosTributosDetalle = otrosTributos_detalle,
                            otrosCargosDetalle = oc_detalle,
                            //descuento = montodescuento_detalle,
                            baseImponible = decimal.TryParse(line[11], out var baseImponible) && (!UTilidades.Exonerado(line[2]) || sumdescuentoDetalle == 0) ?
                                            baseImponible : sumdescuentoDetalle > 0 ? (vu * cantidad_detalle) - montodescuento_detalle : 0,
                            importeTotalItem = importe_detalle
                        });

                        oValeCredito.lDetalle = Detalle;
                        dex++;
                    }

                    #endregion

                    #region OrdenCompra
                    //if (line[0].Trim() == "ORDENCOMPRA")
                    //{
                    //    oOrdenCompra.Add(new beDocumentoCobranzaOrdenCompra
                    //    {
                    //        accion = 1,
                    //        IdEmisor = eCabecera[4].ToString().Trim(),
                    //        serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                    //        numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                    //        ordenCompra = eCabecera[26].ToString() == "" ? "" : eCabecera[26].ToString().Trim(),
                    //    });
                    //    oDocumentoCobranza.lOrdenCompra = oOrdenCompra.Count > 0 ? oOrdenCompra : new List<beDocumentoCobranzaOrdenCompra>();
                    //}
                    //else
                    //{
                    //    oDocumentoCobranza.lOrdenCompra = new List<beDocumentoCobranzaOrdenCompra>();
                    //}
                    #endregion                   

                }

                #region DocumentoDespacho
                //if (!string.IsNullOrEmpty(eCabecera[27].ToString().Trim()))
                //{
                //    oDespacho.Add(new beDocumentoCobranzaDocumentoDespacho
                //    {
                //        accion = 1,
                //        IdEmisor = eCabecera[4].ToString().Trim(),
                //        serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                //        numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                //        idDocRel = "09",
                //        docRel = eCabecera[27].ToString() == "" ? "" : eCabecera[27].ToString().Trim(),
                //    });
                //    oDocumentoCobranza.lDocDespacho = oDespacho.Count > 0 ? oDespacho : new List<beDocumentoCobranzaDocumentoDespacho>();
                //}
                //if (eCabecera.Length > 51)
                //{
                //    if (!string.IsNullOrEmpty(eCabecera[51].ToString().Trim()))
                //    {
                //        oDespacho.Add(new beDocumentoCobranzaDocumentoDespacho
                //        {
                //            accion = 1,
                //            IdEmisor = eCabecera[4].ToString().Trim(),
                //            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                //            numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                //            idDocRel = "31",
                //            docRel = eCabecera[51].ToString() == "" ? "" : eCabecera[51].ToString().Trim(),
                //        });
                //        oDocumentoCobranza.lDocDespacho = oDespacho.Count > 0 ? oDespacho : new List<beDocumentoCobranzaDocumentoDespacho>();
                //    }
                //}

                //if (oDocumentoCobranza.lDocDespacho == null)
                //{
                //    oDocumentoCobranza.lDocDespacho = new List<beDocumentoCobranzaDocumentoDespacho>();
                //    oDocumentoCobranza.lDocDespacho = oDocumentoCobranza.lDocDespacho.Count > 0 ? oDocumentoCobranza.lDocDespacho : new List<beDocumentoCobranzaDocumentoDespacho>();
                //}
                #endregion

                #region Total

                oValeCredito.eTotal = new ValeCreditoTotal
                {
                    tipoIGV = "I",
                    idIGV = "1000",
                    codeIGV = "VAT",
                    nameIGV = "IGV",
                    codigotGravada = "1001",
                    idISC = "2000",
                    codeISC = "EXC",
                    nameISC = "ISC",
                    codigotExonerada = "1003",
                    nameExonerada = "EXONERADA",
                    tipoExonerada = "T",
                    idOTH = "9999",
                    nameOTH = "OTROS",
                    codeOTH = "OTH",
                    tipoOTH = "I",
                    tipoISC = "I",
                    tipoExportacion = "T",
                    codigotExportacion = "1000",
                    tipoGravada = "T",
                    nameGravada = "GRAVADO",
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
                    tDescuento =
                    (desc != null && desc.Length > 2 &&
                    !string.IsNullOrEmpty(desc[2]) &&
                    desc[3].Trim() != "02" &&
                    decimal.TryParse(desc[2], out var tempDescuento)) ?
                    tempDescuento : (ITEMDESCUENTO.Count > 0 ? sumdescuentoDetalle : 0),
                    tIgv =
                        (eCabecera != null && eCabecera.Length > 13 && !string.IsNullOrEmpty(eCabecera[13]) && decimal.TryParse(eCabecera[13], out var tempIgv))
                        ? (eCabecera[3] != "0101" ? tempIgv : (tempIgv > 0 ? tempIgv : 0))
                        : 0m,
                    tDescuentoGlobal = (desc != null && desc.Length > 2 && !string.IsNullOrEmpty(desc[2]) && decimal.TryParse(desc[2], out var tempDescuentoGlobal)) ? tempDescuentoGlobal : 0,
                    tIcbper = (eCabecera != null && eCabecera.Length > 49 && !string.IsNullOrEmpty(eCabecera[49]) && eCabecera[49] != "0" && decimal.TryParse(eCabecera[49], out var tempIcbper)) ? tempIcbper : 0,
                    tSubtotal = 0,
                    nameExportacion = "EXPORTACIÓN",
                    tIsc = ISCTOTAL
                };
                #endregion

                #region CampoAdicional
                int j = 1;
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
                                        Index = j,
                                        Titulo = item.Titulo,
                                        Valor = valorCampo,
                                        EsDetalle = item.EsDetalle,
                                        EnXML = item.EnXML,
                                        EnRepresentacionImpresa = item.EnRepresentacionImpresa
                                    });
                                    oValeCredito.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                                }
                                j++;
                            }
                        }
                        else
                        {
                            oValeCredito.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                        }
                    }
                }
                else
                {
                    oValeCredito.lCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();
                }
                #endregion


                //Totroscargos
                decimal otrscargosLinea = (!string.IsNullOrEmpty(otrosCargos) && otrosCargos.Split('|').Length > 2 && decimal.TryParse(otrosCargos.Split('|')[2], out var tempOtrosCargosGlobal))
                        ? tempOtrosCargosGlobal
                          : 0m;

                if (otrosCargos != null && tOtrosCargos != 0)
                {
                    oValeCredito.eTotal.tOtrosCargos = otrscargosLinea;
                }
                else if (ITEMOTROSCARGOS.Count > 0)
                {
                    var afectaBI = ITEMOTROSCARGOS.Any(x => x.Split('|')[3].Trim() == "47");
                    if (afectaBI)
                    {
                        oValeCredito.eTotal.tOtrosCargos = 0;
                    }
                    else
                    {
                        oValeCredito.eTotal.tOtrosCargos = OCTOTAL;
                    }
                }
                else if (OCTOTAL > 0 && otrosCargos == null)
                {
                    oValeCredito.eTotal.tOtrosCargos = OCTOTAL;
                }
                else if (otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oValeCredito.eTotal.tOtrosCargos = 0;
                    }
                    else
                    {
                        oValeCredito.eTotal.tOtrosCargos = otrscargosLinea;
                    }
                }
                else if (otrosCargos == null && ITEMOTROSCARGOS.Count == 0)
                {
                    oValeCredito.eTotal.tOtrosCargos = OCTOTAL;
                }
                else if (otrscargosLinea > 0 && OCTOTAL == 0)
                {
                    oValeCredito.eTotal.tOtrosCargos = otrscargosLinea;
                }
                else
                {
                    oValeCredito.eTotal.tOtrosCargos = 0;
                }

                // Calcular tGravada
                if (desc != null && desc.Length > 3 && desc[3] != null)
                {
                    if (desc[3] == "03")
                    {
                        oValeCredito.eTotal.tGravada = tGravada;
                    }
                    if (tGravada > 0)
                    {
                        oValeCredito.eTotal.tGravada = tGravada - Convert.ToDecimal(desc[2]);
                    }
                    else
                    {
                        oValeCredito.eTotal.tGravada = tGravada;
                    }
                }
                else if (desc != null && desc.Length > 3 && desc[3]?.Trim() == "02")
                {
                    oValeCredito.eTotal.tGravada = Convert.ToDecimal(eCabecera?[11] ?? "0");
                }
                else if (otrosCargos != null && !UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()))
                {
                    if (otrosCargos.Split('|')[3] == "46")
                    {
                        oValeCredito.eTotal.tGravada = tGravada;
                    }
                    else if (tGravada > 0)
                    {
                        oValeCredito.eTotal.tGravada = tGravada + OCTOTAL;
                    }
                    else
                    {
                        oValeCredito.eTotal.tGravada = tGravada;
                    }

                }
                else
                {
                    oValeCredito.eTotal.tGravada = tGravada;
                }

                // Calcular tDescuentoGlobal
                oValeCredito.eTotal.tDescuentoGlobal = (desc[3] == null) ? 0 :
                    (desc[3].Trim() == "02" || desc[3].Trim() == "03" || desc[3].Trim() == "01") ? Convert.ToDecimal(desc[2]) : 0;

                if (otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oValeCredito.eTotal.tInafecta = tInafecta > 0 ? tInafecta : tInafecta;
                    }
                    else
                    {
                        oValeCredito.eTotal.tInafecta = tInafecta > 0 ? tInafecta : tInafecta;
                    }
                }
                else
                {
                    oValeCredito.eTotal.tInafecta = tInafecta > 0 ? tInafecta : tInafecta;
                }

                oValeCredito.eTotal.tExonerada = tExonerada;
                oValeCredito.eTotal.tGratuita = tGratuita;
                oValeCredito.eTotal.tExportacion = tExportacion;
                oValeCredito.eTotal.tOtrosTributos = tOtrosTributos;
                oValeCredito.eTotal.tImporteCobrar =
                    ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()) ?
                    IMPORTETOTAL + ICBPER : eCabecera != null && eCabecera.Length > 16 ? IMPORTETOTAL + ICBPER : 0;

                // Calcular tImporteTotal
                if (tExportacion > 0)
                {
                    if (oValeCredito.eTotal.tIcbper > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()))
                    {
                        oValeCredito.eTotal.tImporteTotal = BASEIMPONIBLE + ICBPER;
                    }
                    else
                    {
                        oValeCredito.eTotal.tImporteTotal = tExportacion;
                    }

                }
                else if (otrosCargos != null && decimal.TryParse(eCabecera[14].Trim(), out var cargo))
                {
                    var otclinea = otrosCargos.Split('|');
                    if (descuentoGlobal != null)
                    {
                        if (otclinea[3].Trim() == "49" || otclinea[3].Trim() == "46")
                        {
                            oValeCredito.eTotal.tImporteTotal = BASEIMPONIBLE + IGV;
                        }
                    }
                    else
                    {
                        if (otclinea[3].Trim() == "49" || otclinea[3].Trim() == "50")
                        {
                            oValeCredito.eTotal.tImporteTotal = IMPORTETOTAL;
                        }
                        else
                        {
                            oValeCredito.eTotal.tImporteTotal = timporteTotal;
                        }
                    }

                }
                else if (tOtrosCargos > 0 && tInafecta == 0 && tGratuita == 0 && tExportacion == 0 && tOtrosTributos == 0)
                {
                    oValeCredito.eTotal.tImporteTotal = (sumbaseImponibleDetalle + sumIgvDetalle + tOtrosCargos);
                }
                else if (tOtrosCargos > 0 && tInafecta == 0 && tGratuita > 0 && tExportacion == 0 && tOtrosTributos == 0)
                {
                    //oDocumentoCobranza.eTotal.tImporteTotal = (sumbaseImponibleDetalle + sumIgvDetalle);
                    oValeCredito.eTotal.tImporteTotal = timporteTotal;
                }
                else if (tInafecta > 0 && DESCUENTO == 0)
                {
                    if (tOtrosTributos > 0)
                    {
                        oValeCredito.eTotal.tImporteTotal = (tInafecta + tOtrosTributos);
                    }
                    else if (tExonerada > 0)
                    {
                        oValeCredito.eTotal.tImporteTotal = (tInafecta + tExonerada);
                    }
                    else if (tInafecta > 0 && tGravada == 0 && tExonerada == 0)
                    {
                        oValeCredito.eTotal.tImporteTotal = (tInafecta);
                    }
                    else
                    {
                        //oFactura.eTotal.tImporteTotal = (tInafecta);
                        oValeCredito.eTotal.tImporteTotal = (timporteTotal);
                    }

                }
                else if (descuentoGlobal != null)
                {
                    decimal montoDesc = Convert.ToDecimal(descuentoGlobal.Split('|')[2]);
                    if (tInafecta > 0 && DESCUENTO > 0)
                    {
                        oValeCredito.eTotal.tImporteTotal = tInafecta - montoDesc;
                    }
                    else
                    {
                        if (descuentoGlobal.Split('|')[3] == "02" || descuentoGlobal.Split('|')[3] == "03")
                        {
                            if (oValeCredito.eTotal.tExonerada > 0)
                            {
                                if (DESCUENTO > 0)
                                {
                                    oValeCredito.eTotal.tImporteTotal = tExonerada - montoDesc;
                                }
                                else
                                {
                                    oValeCredito.eTotal.tImporteTotal = tExonerada;
                                }
                            }
                            else
                            {
                                //oFactura.eTotal.tImporteTotal = (BASEIMPONIBLE - sumbaseImponibleDetalle) + (IGV - SumDetalleIGVAfectos);
                                if (descuentoGlobal.Split('|')[3] == "03")
                                {
                                    oValeCredito.eTotal.tImporteTotal = timporteTotal;
                                }
                                else
                                {
                                    oValeCredito.eTotal.tImporteTotal = IMPORTETOTAL + ICBPER;
                                }

                            }
                        }
                        else
                        {
                            oValeCredito.eTotal.tImporteTotal = BASEIMPONIBLE + IGV + montoDesc;
                        }
                    }
                }
                else
                {
                    //oFactura.eTotal.tImporteTotal =
                    //    tGravada + sumIgvDetalle + tInafecta + tOtrosCargos + tExonerada + tOtrosTributos + sumIscDetalle + ICBPER;
                    oValeCredito.eTotal.tImporteTotal = ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()) ?
                    IMPORTETOTAL + ICBPER : eCabecera != null && eCabecera.Length > 16 ? IMPORTETOTAL + ICBPER : 0;
                    //oFactura.eTotal.tImporteTotal =
                    //    IMPORTETOTAL + ICBPER;
                }

                if (tExonerada > 0)
                {
                    oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                }
                else if (tInafecta > 0)
                {
                    if (OCTOTAL > 0 && otrosCargos != null)
                    {
                        oValeCredito.eTotal.tBaseImponible = tInafecta + OCTOTAL;
                    }
                    else if (OCTOTAL > 0 && tOtrosCargos > 0)
                    {
                        oValeCredito.eTotal.tBaseImponible = tInafecta;
                    }
                    else
                    {
                        oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                    }

                }
                else if (tGravada > 0)
                {
                    if (descuentoGlobal != null)
                    {
                        if (desc[3] == "02")
                        {
                            oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion - Convert.ToDecimal(desc[2]);
                        }
                        if (desc[3] == "03")
                        {
                            oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                        }
                    }
                    else if (otrosCargos != null)
                    {
                        if (otrosCargos.Split('|')[3] == "46")
                        {
                            oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                        }
                        else
                        {
                            oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + OCTOTAL;
                        }

                    }
                    else
                    {
                        oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                    }
                }
                else
                {
                    oValeCredito.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + sumIgvDetalle + tGratuita;
                }

            }
            catch (Exception ex)
            {
                oValeCredito = null;
                _ = LogAsync("DescomponerValeCredito", ex);
            }

            return oValeCredito;
        }
    }
}
