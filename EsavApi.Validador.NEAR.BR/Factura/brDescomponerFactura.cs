using EsavApi.Validador.BR;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.Factura;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.Factura
{
    public class brDescomponerFactura : brGenerico
    {
        public async Task<beFacturaObj> DescomponerFactura(string[] lineas)
        {
            beFacturaObj oFactura = new beFacturaObj();
            List<beFacturaDetalle> Detalle = new List<beFacturaDetalle>();
            List<beFacturaDetalleTotal> DetalleTotal = new List<beFacturaDetalleTotal>();
            beFacturaGlobal oGlobal = new beFacturaGlobal();
            List<beFacturaDocumentoDespacho> oDespacho = new List<beFacturaDocumentoDespacho>();
            List<beFacturaDocumentoAdicional> oDocAdicional = new List<beFacturaDocumentoAdicional>();
            List<beFacturaOrdenCompra> oOrdenCompra = new List<beFacturaOrdenCompra>();
            List<beEmisorCampoAdicionalRegistro> oCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();
            List<CampoAdicionalRegistroDetalle> oCampoAdicionalDetalle = new List<CampoAdicionalRegistroDetalle>();
            var consultarCampoAdicional = new List<beCampoAdicional>();

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
                var detraccion = lineas.Where(x => x.ToUpper().StartsWith("DETRACCION")).ToList();
                var Detdetraccion = lineas.Where(x => x.ToUpper().StartsWith("DETDETRACCION")).ToList();
                var anticipo = lineas.Where(x => x.ToUpper().StartsWith("ANTICIPO")).ToList();
                var retencion = lineas.Where(x => x.ToUpper().StartsWith("RETENCION")).ToList();
                var formaPago = lineas.Where(x => x.ToUpper().StartsWith("FORMAPAGO")).FirstOrDefault();
                var cuotas = lineas.Where(x => x.ToUpper().StartsWith("CUOTAS")).ToList();
                var hospedaje = lineas.Where(x => x.Trim().ToUpper().StartsWith("D|")).ToList();
                var detalleAdicional = lineas.Where(x => x.Trim().ToUpper().StartsWith("ITEMSP")).ToList();

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
                //var tipoCambio = new brConsultar().ObtenerTipoCambio(Convert.ToDateTime(eCabecera[6]).ToString("yyyy-MM-dd"), eCabecera[8].Trim());
                int dex = 1;

                decimal tGravada = 0;
                decimal tInafecta = 0;
                decimal tExonerada = 0;
                decimal tGratuita = 0;
                decimal tExportacion = 0;
                decimal tOtrosTributos = 0;
                decimal tOtrosCargos = 0;
                decimal sumatImporteAnticipado = 0;
                decimal sumatIGVAnticipado = 0;
                decimal sumatGravadoAnticipado = 0;
                decimal timporteTotal = 0;
                decimal sumbaseImponibleDetalle = 0;
                decimal sumIgvDetalle = 0;
                decimal sumIscDetalle = 0;
                decimal sumpctOtroscargos = 0;
                decimal sumdescuentoDetalle = 0;
                decimal sumbisinOTributos = 0;
                decimal sumbiconOTributos = 0;
                decimal SumDetalleIGVAfectos = 0;

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
                        decimal montoAnticipo = anticipo.Count > 0
                                        ? anticipo.Sum(x =>
                                        {
                                            var partes = x.Split('|');
                                            return partes.Length > 8 && decimal.TryParse(partes[8], out decimal valor) ? valor : 0;
                                        })
                                        : 0;
                        oFactura.eCabecera = new beFactura
                        {
                            accion = 1,
                            idTipoFactura = eCabecera[3].ToString() == "" ? "" : eCabecera[3].ToString().Trim(),
                            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                            numero = int.Parse(eCabecera[22]) == 0 ? 0 : int.Parse(eCabecera[22]),
                            idTipoFacturaText = UTilidades.ObtenerDescripcionOperacion(eCabecera[3].ToString().Trim()),
                            email = line[i] == "Cliente" ? line[9] : "",
                            fechaEmision = eCabecera[6].ToString().Trim(),
                            horaEmision = DateTime.ParseExact(eCabecera[6], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("HH:mm:ss"),
                            fechaVencimiento = eCabecera[7].ToString().Trim(),
                            FechaPago = Convert.ToDateTime(eCabecera[7].ToString().Trim()).ToString("yyyy-MM-dd"),
                            tipoMoneda = eCabecera[8].ToString().Trim(),
                            tipoMonedaText = eCabecera[8].ToString() == "USD" ? "DÓLARES AMERICANOS" : "SOLES",
                            tipoMonedaSimbolo = eCabecera[8].ToString() == "USD" ? "$" : "S/",
                            IdSucursal = configuracion.Sucu_IdSucursal,
                            observacion = eCabecera[24],
                            docIdentidad = line[0] == "Cliente" ? line[2].Trim() : "",
                            //razonSocial = line[0] == "Cliente" ? UTilidades.LimpiarTexto(line[3].Trim()) : "",
                            tipoDocumento = line[0] == "Cliente" ? line[1].Trim() : "",
                            AccountingSupplierParty_Party_PostalAddress_Country_IdentificationCode = "PE",
                            AccountingCustomerParty_Party_PostalAddress_Country_IdentificationCode = "PE",
                            LegalMonetaryTotal_LineExtensionAmount = eCabecera[11] == "" ? 0 : Convert.ToDecimal(eCabecera[11]),
                            LegalMonetaryTotal_LineExtensionAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            LegalMonetaryTotal_TaxInclusiveAmount = anticipo.Count > 0 && IMPORTETOTAL != 0 && IMPORTETOTAL > montoAnticipo ? IMPORTETOTAL - montoAnticipo : IMPORTETOTAL,
                            LegalMonetaryTotal_TaxInclusiveAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            LegalMonetaryTotal_AllowanceTotalAmount =
                                    (eCabecera[10] == "" || ITEMDESCUENTO.Count == 0) ? 0 : Convert.ToDecimal(eCabecera[10]),
                            //LegalMonetaryTotal_AllowanceTotalAmount = 0,
                            LegalMonetaryTotal_AllowanceTotalAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            LegalMonetaryTotal_ChargeTotalAmount = eCabecera[14] == "" ? 0 : Convert.ToDecimal(eCabecera[14]),
                            LegalMonetaryTotal_ChargeTotalAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            LegalMonetaryTotal_PrepaidAmount = montoAnticipo,
                            LegalMonetaryTotal_PrepaidAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            LegalMonetaryTotal_PayableAmount = anticipo.Count > 0 && IMPORTETOTAL != 0 ? IMPORTETOTAL - montoAnticipo : IMPORTETOTAL,
                            LegalMonetaryTotal_PayableAmount_CurrencyID = eCabecera[8].ToString() == "" ? "" : eCabecera[8].ToString().Trim(),
                            //PaymentTerms_ID = codBbSsDetraccion 
                            //PaymentTerms_PaymentPercent = porcentajeDetraccion
                            //PaymentTerms_Amount = montoDetraccion
                            tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[1]),
                            codBbSsDetraccion = detraccion.Count > 0 ? detraccion.FirstOrDefault()?.Split('|')[1] : null,
                            codBbSsDetraccionText = detraccion.Count > 0 ? detraccion.FirstOrDefault()?.Split('|')[1] + "-" + UTilidades.codBbSsDetraccionText(detraccion.FirstOrDefault()?.Split('|')[1]) : null,
                            esDetraccion = detraccion.Count > 0 ? true : false,
                            porcentajeDetraccion = detraccion.Count > 0 ? decimal.Parse(detraccion.FirstOrDefault()?.Split('|')[3] ?? "0") * 100 : 0,
                            montoDetraccion = detraccion.Count > 0 ? decimal.Parse(detraccion.FirstOrDefault()?.Split('|')[4] ?? "0") : 0,
                            cuentaDetraccion = detraccion.Count > 0 ? detraccion.FirstOrDefault()?.Split('|')[2] : null,
                            CuentaDetraccion = detraccion.Count > 0 ? detraccion.FirstOrDefault()?.Split('|')[2] : null,
                            PagoDetraccionCode = detraccion.Count > 0 ? configuracion.CEmi_PagoDetraccion : "001",
                            PagoDetraccionText = detraccion.Count > 0 ? UTilidades.MedioPagoDetraccion(configuracion.CEmi_PagoDetraccion) : "Depósito en cuenta",
                            anticipo = eCabecera[33].Trim() == "1" ? true : false,



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
                                VistaPdf = configuracion.CSuc_VistaPdf + "Factura",
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
                                correoSucursal = configuracion.Sucu_Correo,
                                telefonoSucursal = configuracion.Sucu_Telefono,
                                webSucursal = configuracion.Sucu_Web,
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
                            tipoDocumentoAsociado = "1",
                            fileLogoPDFEmisor = configuracion.Form_Icono,
                            vUbl = "2.1",
                            vCustomID = "2.0",
                            tipoDocEmision = eCabecera[2].ToString().Trim(),
                            usuario = eCabecera[23].ToString().Trim(),
                            Fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            FormaPago = cuotas.Count > 0 ? "Credito" : "Contado",
                            MontoPendientePago = string.IsNullOrWhiteSpace(formaPago.Split('|')[2]) ? 0 : Convert.ToDecimal(formaPago.Split('|')[2])
                        };

                        #region ANTICIPO
                        List<beFacturaAnticipada> FacturaAnticipada = null;
                        if (anticipo.Count > 0)
                        {
                            FacturaAnticipada = new List<beFacturaAnticipada>();
                            foreach (var item in anticipo)
                            {
                                var anticipado = new brConsultar().FacturaAnticipo_Obtener(oFactura.eCabecera.rucEmisor, clienteLine.Split('|')[2], clienteLine.Split('|')[1], item.Split('|')[4], item.Split('|')[5]);
                                FacturaAnticipada.Add(new beFacturaAnticipada
                                {
                                    accion = 1,
                                    rucEmisor = eCabecera[4].ToString().Trim(),
                                    tipoDocCliente = anticipado.TipoDocCliente,
                                    docCliente = anticipado.DocCliente,
                                    nombreCliente = new brConsultar().ObtenerRuc(anticipado.DocCliente, null).RazonSocial,
                                    identificadorPago = item.Split('|')[4].Trim() + "-" + item.Split('|')[5].Trim(),
                                    gravadoAnticipo = anticipado.Gravado,
                                    exoneradoAnticipo = anticipado.Exonerado,
                                    inafectoAnticipo = anticipado.Inafecto,
                                    iscAnticipo = anticipado.ISC,
                                    igvAnticipo = anticipado.IGV,
                                    montoAnticipado = Convert.ToDecimal(item.Split('|')[6].Trim()),
                                    moneda = anticipado.Moneda,
                                    fechaPago = item.Split('|')[2].Trim(),
                                    serie = eCabecera[21].Trim(),
                                    numero = eCabecera[22].Trim(),
                                });
                                sumatImporteAnticipado += Convert.ToDecimal(item.Split('|')[6].Trim());
                                sumatIGVAnticipado += anticipado.IGV;
                                sumatGravadoAnticipado += anticipado.Gravado;
                                oDocAdicional.Add(new beFacturaDocumentoAdicional
                                {
                                    accion = 1,
                                    idDocAdi = item.Split('|')[3].Trim(),
                                    docAdi = item.Split('|')[4].Trim() + "-" + item.Split('|')[5].Trim()
                                });
                            }
                            oFactura.lDocAdicional = oDocAdicional;
                            oFactura.eCabecera.FacturaAnticipada = FacturaAnticipada;
                        }
                        else
                        {
                            oFactura.lDocAdicional = new List<beFacturaDocumentoAdicional>();
                        }
                        #endregion
                    }

                    #endregion

                    #region DOCASOCIADO
                    if (line[0].ToUpper() == "DOCASOCIADO")
                    {
                        oDocAdicional.Add(new beFacturaDocumentoAdicional
                        {
                            accion = 1,
                            idDocAdi = line[1].Trim(),
                            docAdi = line[2].Trim()
                        });

                        oFactura.lDocAdicional.AddRange(oDocAdicional);
                    }

                    #endregion

                    #region CLIENTE
                    if (line[0].ToUpper() == "CLIENTE")
                    {
                        var ubigeo = line[6].Trim() != "" ? new brConsultar().ObtenerUbigeo(line[6].Trim()) : "";
                        oFactura.eCabecera.docIdentidad = line[2].Trim();
                        //oFactura.eCabecera.razonSocial = line[1].Trim() == "6" ? new brConsultar().ObtenerRuc(line[2].Trim(), null).RazonSocial : line[3].Trim();
                        oFactura.eCabecera.razonSocial =
                            line[1].Trim() == "6"
                                ? new brConsultar().ObtenerRuc(line[2].Trim(), null).RazonSocial
                                : line[1].Trim() == "1"
                                    ? UTilidades.LimpiarTexto(line[2].Trim())
                                    : line[3].Trim();
                        oFactura.eCabecera.tipoDocumento = line[1].Trim();

                        if (line[1].Trim() == "6" && line[2].Trim().StartsWith("20") && string.IsNullOrEmpty(line[7].Trim()))
                        {
                            var direccion =
                                new brConsultar().ObtenerRuc(line[2].Trim(), null).Direcciones.FirstOrDefault(x => x.TipoDireccion.ToUpper().Trim() == "FISCAL");
                            oFactura.eCabecera.direccion = direccion.Direccion + " - " + direccion.Distrito + " - " + direccion.Provincia + " - " + direccion.Departamento;
                        }
                        else
                        {
                            oFactura.eCabecera.direccion = line[7].Trim();
                        }
                        oFactura.eCabecera.email = (line.Length > 9 &&
                            !string.IsNullOrWhiteSpace(line[9]) &&
                            !new[] { "-", ",", ".", ";", "null", "N/A" }.Contains(line[9].Trim()))
                            ? string.Join(";", line[9].Replace(",", ";").Split(';')
                                .Select(e => e.Trim())
                                .Where(e => { try { return new MailAddress(e).Address == e; } catch { return false; } }))
                            : "";
                        oFactura.eCabecera.AccountingCustomerParty_Party_PartyTaxScheme_RegistrationName = line[3].Trim();
                        oFactura.eCabecera.tipoDocumentoText = UTilidades.ObtenerTipoDocumentoText(line[1]);
                        oFactura.eCabecera.ubigeo = line[6].Trim();
                        if (!string.IsNullOrEmpty(ubigeo))
                        {
                            oFactura.eCabecera.departamento = ubigeo.Split('-').Length > 0 ? ubigeo.Split('-')[0].Trim() : "";
                            oFactura.eCabecera.provincia = ubigeo.Split('-').Length > 1 ? ubigeo.Split('-')[1].Trim() : "";
                            oFactura.eCabecera.distrito = ubigeo.Split('-').Length > 2 ? ubigeo.Split('-')[2].Trim() : "";
                        }
                        else
                        {
                            if (line[1].Trim() == "6" && line[2].Trim().StartsWith("20") && line.Length > 7 &&
                                string.IsNullOrWhiteSpace(line[7]))
                            {
                                var direccion =
                                new brConsultar().ObtenerRuc(line[2].Trim(), null).Direcciones.FirstOrDefault(x => x.TipoDireccion.ToUpper().Trim() == "FISCAL");

                                oFactura.eCabecera.departamento = direccion.Departamento;
                                oFactura.eCabecera.provincia = direccion.Provincia;
                                oFactura.eCabecera.distrito = direccion.Distrito;
                                oFactura.eCabecera.ubigeo = direccion.Ubigeo;
                                oFactura.eCabecera.direccion = direccion.Direccion;
                            }
                        }
                    }
                    #endregion

                    #region DETALLE
                    if (line[1].ToUpper() == "BIEN" || line[1].ToUpper() == "SERVICIO")
                    {
                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(line[3]);
                        tGravada += line[2].Trim() == "10" ? Convert.ToDecimal(line[11]) : 0;
                        tInafecta += line[2].Trim() == "30" ? Convert.ToDecimal(line[11]) : 0;
                        //tExonerada += UTilidades.Exonerado(line[2]) == true ? Convert.ToDecimal(line[11]) : 0;
                        tGratuita += UTilidades.EsCodigoGratuito(line[2]) == true ? Convert.ToDecimal(line[11]) : 0;
                        tExportacion += line[2] == "40" ? Convert.ToDecimal(line[11]) : 0;
                        tOtrosTributos += line[17] == "" ? 0 : Convert.ToDecimal(line[17]);
                        sumbisinOTributos += line[17] == "" || line[17] == "0" ? Convert.ToDecimal(line[11]) : 0;
                        sumbiconOTributos += (line[17] != "" && line[17] != "0" && line[17] != "0.00") ? Convert.ToDecimal(line[11]) : 0;
                        tOtrosCargos += line[15] == "" ? 0 : Convert.ToDecimal(line[15]);
                        timporteTotal += (line[19] == "0" || line[19] == "" ? 0 : decimal.TryParse(line[19], out var temp) ? temp : 0);
                        sumbaseImponibleDetalle += !UTilidades.EsCodigoGratuito(line[2]) && line[2].Trim() != "21" ? Convert.ToDecimal(line[11]) : 0;
                        sumpctOtroscargos += line[16] == "0" || line[16] == "" ? 0 : Convert.ToDecimal(line[16]);
                        sumIgvDetalle += (line[12] == "0" || line[12] == "") ? 0 : line[2].Trim() == "10" ? Convert.ToDecimal(line[12]) : 0;
                        sumIscDetalle += (line[13] == "0" || line[13] == "") ? 0 : UTilidades.EsCodigoGratuito(line[2]) ? 0 : Convert.ToDecimal(line[13]);
                        sumdescuentoDetalle += (line[9] == "0" || line[9] == "") ? 0 : Convert.ToDecimal(line[9]);
                        SumDetalleIGVAfectos += UTilidades.EsCodigoGratuitoGravado(line[2]) ? Convert.ToDecimal(line[12]) : 0;


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
                        //decimal pctISC_detalle = decimal.TryParse(line[14], out var pctISC) ? pctISC * 100 : 0;
                        decimal pctISC_detalle = decimal.TryParse(line[14], out var pctISC)
                            ? (UTilidades.TipoOperacionDetraccionParaIscFactura(eCabecera[3].ToString().Trim()) ? pctISC * 100 : pctISC)
                            : 0;
                        //decimal pctISC_detalle = decimal.TryParse(line[14], out var pctISC)
                        //    ? ((UTilidades.TipoOperacionDetraccionParaIscFactura(eCabecera[3].ToString().Trim()) ||
                        //    eCabecera[3].ToString().Trim() == "0101") && pctOTH_detalle == 0 && pctOtrosCargos_detalle == 0
                        //    ? pctISC * 100 : pctISC)
                        //    : 0;

                        decimal isc_detalle = decimal.TryParse(line[13], out var isc) ? isc : 0;

                        decimal valor9 = string.IsNullOrWhiteSpace(line[9]) ? 0m : Convert.ToDecimal(line[9]);
                        decimal valor11 = string.IsNullOrWhiteSpace(line[11]) ? 0m : Convert.ToDecimal(line[11]);

                        //tExonerada +=
                        //    UTilidades.Exonerado(line[2]) && valor9 == 0m
                        //        ? valor11
                        //        : UTilidades.Exonerado(line[2]) && valor9 != 0m
                        //            ? (cantidad_detalle * vu_detalle) - montodescuento_detalle
                        //            : 0m;

                        tExonerada +=
                            UTilidades.Exonerado(line[2]) && valor9 == 0m
                                ? valor11
                                : UTilidades.Exonerado(line[2]) && valor9 != 0m
                                    ? bi
                                    : 0m;

                        consultarCampoAdicional = ITEMPLACA.Count > 0 || detalleAdicional.Count > 0
                            && (consultarCampoAdicional.Count == 0) ?
                            new brConsultar().ListarCampoAdicional(
                                eCabecera[4].Trim(), configuracion.Rubr_IdRubro.ToString(), eCabecera[2].Trim(), true) :
                                consultarCampoAdicional;
                        Detalle.Add(new beFacturaDetalle()
                        {
                            accion = 1,
                            index = dex.ToString(),
                            cantidad = cantidad_detalle,
                            codigoSunat = line[5].Trim(),
                            descripcion = line[6].Trim().Replace("\\r\\n", "\n").Replace("<br>", "\n"),
                            codigo = line[4].Trim(),
                            unidadMedida = line[3].Trim(),
                            UnidadMedidaText = UnidadMedidaText.Item1.Trim(),
                            UnidadAbreviatura = UnidadMedidaText.Item2.Trim(),
                            BienServicioText = line[1].Trim(),
                            codigoPrecioUnitario = UTilidades.EsCodigoGratuito(line[2]) ? "02" : "01",
                            valorUnitario = vu_detalle,
                            precioUnitario =
                                                ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                && line[2] != "40" && !UTilidades.EsCodigoGravado(line[2])
                                                && !UTilidades.EsCodigoGratuito(line[2]) ? (ICBPER + (importe_detalle)) / cantidad :
                                                ((line[6].ToLower().Trim() == "bolsa plastica" || line[6].ToLower().Trim() == "bolsa plástica"))
                                                && line[2] == "40" ? 1 : UTilidades.EsCodigoGratuito(line[2]) ? vu_detalle :
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
                                                 }) && igv_detalle == 0 ? importe_detalle / cantidad:
                                                 ITEMDESCUENTO.Any(x =>
                                                 {
                                                     var otline = x.Split('|');
                                                     if (otline.Length < 3) return false;
                                                     return otline[2].Trim() == line[0].ToString();
                                                 }) ? bi - montodescuento :
                                                 importe_detalle > 0 && cantidad > 0 ? importe_detalle / cantidad :
                                                 0,
                            idIGV = "1000",
                            codigoGravExoIna = line[2],
                            graExoIna = line[2] == "10" ? "1001" : line[2] == "30" ? "1002" : line[2] == "20" ? "1003" : line[2] == "40" ? "1000" : "1004",
                            pctIGV = UTilidades.Inafecto(line[2].Trim()) || UTilidades.sinIGV(line[2].Trim()) ? 0 : configuracion.CSuc_PorcentajeIGV * 100,
                            igv = igv_detalle,
                            idISC = "2000",
                            codigoTipoISC = "01",
                            tipoISC = "I",
                            pctISC = pctISC_detalle,
                            isc = isc_detalle,
                            idOTH = "9999",
                            pctOTH = pctOTH_detalle,
                            otrosTributosDetalle = otrosTributos_detalle,
                            pctOtrosCargosDetalle = pctOtrosCargos_detalle,
                            otrosCargosDetalle = oc_detalle,
                            baseOtrosCargosDetalle = oc_detalle > 0 ? importe_detalle : 0,
                            porcentajedescuento = pctdescuento_detalle,
                            pctDescuentoDetalle = pctdescuento_detalle,
                            descuento = montodescuento_detalle,
                            baseDescuentoDetalle =
                            montodescuento_detalle > 0 && ITEMDESCUENTO.Count == 0 ?
                            vu_detalle * cantidad : ITEMDESCUENTO.Count > 0 ? (vu_detalle * cantidad) + igv_detalle : 0,
                            idICBPER = "7152",
                            baseImponible =
                                decimal.TryParse(line[11], out var baseImponible) && (!UTilidades.Exonerado(line[2]) || sumdescuentoDetalle == 0 || ITEMDESCUENTO.Any()) ?
                                            baseImponible : sumdescuentoDetalle > 0 ? (vu * cantidad_detalle) - montodescuento_detalle : 0,
                            codeIGV = "VAT",
                            nameIGV = "IGV",
                            codeISC = "EXC",
                            nameISC = "ISC",
                            codeOTH = "OTH",
                            nameOTH = "OTROS",
                            tipoOTH = "I",
                            codeICBPER = "OTH",
                            nameICBPER = "ICBPER",
                            tipoICBPER = "I",
                            codigoMotivoDescuento = ITEMDESCUENTO.Count > 0 ? "01" : "00",
                            TransporteCarga = Detdetraccion.Count > 0 ? new beFacturaDetalleTransporteCarga()
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                indexDTC = Detdetraccion[dex - 1].Split('|')[1].Trim(),
                                detalleViajeDTC = Detdetraccion[dex - 1].Split('|')[6].Trim(),
                                direccionDestinoDTC = Detdetraccion[dex - 1].Split('|')[5].Trim(),
                                direccionOrigenDTC = Detdetraccion[dex - 1].Split('|')[3].Trim(),
                                monedaValorReferencialCEDTC = "PEN",
                                monedaValorReferencialCUNDTC = "PEN",
                                monedaValorReferencialSTDTC = "PEN",
                                tipoValorReferencialCEDTC = "02",
                                tipoValorReferencialCUNDTC = "03",
                                tipoValorReferencialSTDTC = "01",
                                ubigeoDestinoDTC = Detdetraccion[dex - 1].Split('|')[4].Trim(),
                                ubigeoDestinoTextDTC = new brConsultar().ObtenerUbigeo(Detdetraccion[dex - 1].Split('|')[4].Trim()),
                                ubigeoOrigenDTC = Detdetraccion[dex - 1].Split('|')[2].Trim(),
                                ubigeoOrigenTextDTC = new brConsultar().ObtenerUbigeo(Detdetraccion[dex - 1].Split('|')[2].Trim()),
                                valorReferencialCEDTC = Convert.ToDecimal(Detdetraccion[dex - 1].Split('|')[8].Trim()),
                                valorReferencialCUNDTC = Convert.ToDecimal(Detdetraccion[dex - 1].Split('|')[9].Trim()),
                                valorReferencialSTDTC = Convert.ToDecimal(Detdetraccion[dex - 1].Split('|')[7].Trim())

                            } : null,
                            codigoMotivoOtrosCargosDetalle = otrosCargos != null ? "49" :
                                ITEMOTROSCARGOS.Count > 0 &&
                                ITEMOTROSCARGOS.Any(x => x.Split('|').Length > 3 && int.Parse(x.Split('|')[2]) == int.Parse(line[0]))
                                    ? ITEMOTROSCARGOS.FirstOrDefault(x => x.Split('|').Length > 3 && int.Parse(x.Split('|')[2]) == int.Parse(line[0]))?.Split('|')[3] ?? "48"
                                    : "48",
                            listaPropiedad = hospedaje.Count > 0
                                             ? new List<beFacturaDeliveryDetalle> {
                                                new beFacturaDeliveryDetalle
                                                {
                                                    IdDetalle = null,
                                                    idPropiedad = "4000",
                                                    descripcionPropiedad = "Hospedajes: Código de país de emisión del pasaporte",
                                                    valorPropiedad = hospedaje[dex - 1].Split('|')[6].Trim(),
                                                    enRepresentacionImpresa = false,
                                                    enXML = false,
                                                    index = dex,
                                                    Item = "1"
                                                },
                                                new beFacturaDeliveryDetalle
                                                {
                                                    IdDetalle = null,
                                                    idPropiedad = "4007",
                                                    descripcionPropiedad = "Hospedajes: Nombres y apellidos del huesped",
                                                    valorPropiedad = hospedaje[dex - 1].Split('|')[3].Trim(),
                                                    enRepresentacionImpresa = false,
                                                    enXML = false,
                                                    index = dex,
                                                    Item = "1"
                                                },
                                                new beFacturaDeliveryDetalle
                                                {
                                                    IdDetalle = null,
                                                    idPropiedad = "4008",
                                                    descripcionPropiedad = "Hospedajes: Tipo de documento de identidad del huesped",
                                                    valorPropiedad = hospedaje[dex - 1].Split('|')[4].Trim(),
                                                    enRepresentacionImpresa = false,
                                                    enXML = false,
                                                    index = dex,
                                                    Item = "1"
                                                },
                                                new beFacturaDeliveryDetalle
                                                {
                                                    IdDetalle = null,
                                                    idPropiedad = "4009",
                                                    descripcionPropiedad = "Hospedajes: Número de documento de identidad del huesped",
                                                    valorPropiedad = hospedaje[dex - 1].Split('|')[5].Trim(),
                                                    enRepresentacionImpresa = false,
                                                    enXML = false,
                                                    index = dex,
                                                    Item = "1"
                                                }
                                            } :
                                                    detalleAdicional.Count > 0 &&
                                                    detalleAdicional.Count >= dex &&
                                                    detalleAdicional[dex - 1].Split('|').Length > 2 &&
                                                    int.TryParse(detalleAdicional[dex - 1].Split('|')[1], out int valorDetalle) &&
                                                    valorDetalle == dex
                                                    ? new[] { "5000", "5001", "5002", "5003" }
                                                        .Where(id => consultarCampoAdicional.Any(c => c.IdCampoAdicional == id))
                                                        .Select(id =>
                                                        {
                                                            var partes = detalleAdicional[dex - 1].Split('|');
                                                            var campo = consultarCampoAdicional.First(c => c.IdCampoAdicional == id);
                                                            return new beFacturaDeliveryDetalle
                                                            {
                                                                Accion = 1,
                                                                IdDetalle = partes[1],
                                                                idPropiedad = campo.IdCampoAdicional,
                                                                descripcionPropiedad = campo.Descripcion,
                                                                valorPropiedad = partes[2],
                                                                enXML = campo.EnXML,
                                                                enRepresentacionImpresa = campo.EnRepresentacionImpresa,
                                                                index = dex,
                                                                Item = partes[1]
                                                            };
                                                        }).ToList()
                                            :
                                                    consultarCampoAdicional.Count == 0
                                                    || ITEMPLACA.Count <= dex - 1
                                                    || ITEMPLACA[dex - 1].Split('|').Length <= 3
                                                    || !int.TryParse(ITEMPLACA[dex - 1].Split('|')[2], out int placaDex)
                                                    || placaDex != dex
                                                    || consultarCampoAdicional.FirstOrDefault(c => c.IdCampoAdicional == "5010") == null
                                                    ? new List<beFacturaDeliveryDetalle>()
                                                    : new List<beFacturaDeliveryDetalle>
                                                    {
                                                        new beFacturaDeliveryDetalle
                                                        {
                                                            Accion = 1,
                                                            IdDetalle = ITEMPLACA[dex - 1].Split('|')[2],
                                                            idPropiedad = consultarCampoAdicional.First(c => c.IdCampoAdicional == "5010").IdCampoAdicional,
                                                            descripcionPropiedad = consultarCampoAdicional.First(c => c.IdCampoAdicional == "5010").Descripcion,
                                                            valorPropiedad = ITEMPLACA[dex - 1].Split('|')[3],
                                                            enXML = consultarCampoAdicional.First(c => c.IdCampoAdicional == "5010").EnXML,
                                                            index = dex,
                                                            enRepresentacionImpresa = consultarCampoAdicional.First(c => c.IdCampoAdicional == "5010").EnRepresentacionImpresa,
                                                            Item = ITEMPLACA[dex - 1].Split('|')[2]
                                                        }
                                                    },
                            cantidadICBPER = (line[6].ToLower().Trim().Contains("bolsa plastica") ||
                                    line[6].ToLower().Trim().Contains("bolsa plástica")) && icbper.Count > 0
                                    ? (int)(double.TryParse(line[7], out var cantICBPER) ? cantICBPER : 0) : 0,
                            perUnitICBPER = (line[6].ToLower().Trim().Contains("bolsa plastica") ||
                                    line[6].ToLower().Trim().Contains("bolsa plástica")) && icbper.Count > 0 ? 0.50M : 0,
                            montoICBPER = (line[6].ToLower().Trim().Contains("bolsa plastica") ||
                                    line[6].ToLower().Trim().Contains("bolsa plástica")) && icbper.Count > 0 ?
                                    (decimal.TryParse(line[7], out var cantidadbolsa) ? cantidad * 0.50M : 0) : 0,
                        });

                        oFactura.lDetalle = Detalle;

                        #region DETALLETOTAL
                        //DETALLE TOTAL
                        if (line[2].Trim() == "10")
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "1000",
                                Name = "IGV",
                                TaxTypeCode = "VAT",
                                Porcentaje = configuracion.CSuc_PorcentajeIGV * 100,
                                Amount = line[12] == "" ? 0 : Convert.ToDecimal(line[12]),
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = "10",
                                SubID = "1001"
                            });
                        }
                        if (otrosTributos_detalle > 0)
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "9999",
                                Name = "OTROS",
                                TaxTypeCode = "OTH",
                                Porcentaje = pctOTH_detalle,
                                Amount = otrosTributos_detalle,
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = "",
                                SubID = ""
                            });
                        }
                        if (icbper.Count > 0)
                        {
                            foreach (var item in icbper)
                            {
                                var parts = item.Split('|');
                                if (parts.Length >= 5 && Convert.ToInt32(parts[1]) == dex)
                                {
                                    var porcentaje = parts[3];
                                    var monto = parts[4];
                                    DetalleTotal.Add(new beFacturaDetalleTotal
                                    {
                                        Accion = 1,
                                        IdEmisor = eCabecera[4].Trim(),
                                        Serie = eCabecera[21].Trim(),
                                        Numero = eCabecera[22].Trim(),
                                        Tipo = "I",
                                        Index = dex.ToString(),
                                        ID = "7152",
                                        Name = "ICBPER",
                                        TaxTypeCode = "OTH",
                                        Porcentaje = Convert.ToDecimal(porcentaje),
                                        Amount = Convert.ToDecimal(monto),
                                        Amount_CurrencyID = eCabecera[8].Trim(),
                                        TypeCode = "",
                                        SubID = ""
                                    });
                                }
                            }
                        }
                        if (isc_detalle > 0)
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "2000",
                                Name = "ISC",
                                TaxTypeCode = "EXC",
                                Porcentaje = pctISC_detalle,
                                Amount = isc_detalle,
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = "01",
                                SubID = ""
                            });
                        }
                        if (line[2].Trim() == "30")
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "1000",
                                Name = "IGV",
                                TaxTypeCode = "VAT",
                                Porcentaje = 0,
                                Amount = line[12] == "" ? 0 : Convert.ToDecimal(line[12]),
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = "30",
                                SubID = "1002"
                            });
                        }
                        if (line[2].Trim() == "20")
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "1000",
                                Name = "IGV",
                                TaxTypeCode = "VAT",
                                Porcentaje = 0,
                                Amount = line[12] == "" ? 0 : Convert.ToDecimal(line[12]),
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = "20",
                                SubID = "1003"
                            });
                        }
                        //if (line[2].Trim() == "40")
                        if (UTilidades.EsCodigoGratuito(line[2].Trim()) || line[2].Trim() == "40")
                        {
                            DetalleTotal.Add(new beFacturaDetalleTotal
                            {
                                Accion = 1,
                                IdEmisor = eCabecera[4].Trim(),
                                Serie = eCabecera[21].Trim(),
                                Numero = eCabecera[22].Trim(),
                                Tipo = "I",
                                Index = dex.ToString(),
                                ID = "1000",
                                Name = "IGV",
                                TaxTypeCode = "VAT",
                                Porcentaje = line[2].Trim() == "40" ? 0 : configuracion.CSuc_PorcentajeIGV * 100,
                                Amount = line[12] == "" ? 0 : Convert.ToDecimal(line[12]),
                                Amount_CurrencyID = eCabecera[8].Trim(),
                                TypeCode = line[2].Trim(),
                                SubID = line[2].Trim() == "40" ? "1000" : "1004",
                            });
                        }
                        #endregion

                        oFactura.lDetalleTotal = DetalleTotal;

                        dex++;
                    }

                    #endregion

                    #region DocAdicional
                    //if (anticipo.Count > 0)
                    //{
                    //    oDocAdicional.Add(new beBoletaDocumentoAdicional
                    //    {
                    //        accion = 1,
                    //        idDocAdi = "",
                    //        docAdi = ""
                    //    });
                    //    oBoleta.lDocAdicional = oDocAdicional;
                    //}
                    #endregion

                    #region OrdenCompra
                    if (line[0].Trim() == "ORDENCOMPRA")
                    {
                        oOrdenCompra.Add(new beFacturaOrdenCompra
                        {
                            accion = 1,
                            IdEmisor = eCabecera[4].ToString().Trim(),
                            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                            numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                            ordenCompra = eCabecera[26].ToString() == "" ? "" : eCabecera[26].ToString().Trim(),
                        });
                        oFactura.lOrdenCompra = oOrdenCompra.Count > 0 ? oOrdenCompra : new List<beFacturaOrdenCompra>();
                    }
                    else
                    {
                        oFactura.lOrdenCompra = new List<beFacturaOrdenCompra>();
                    }
                    #endregion                   

                    #region OTROS TRIBUTOS
                    if (line[0].Trim() == "OTROSTRIBUTOS")
                    {
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                    }
                    #endregion
                }

                #region Total

                oFactura.eTotal = new FacturaTotalModel
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
                    tIsc = ISCTOTAL,
                    pctDescuentoGlobal = (desc != null && desc.Length > 1 && !string.IsNullOrEmpty(desc[1] ?? "") && decimal.TryParse(desc[1] ?? "0", out var tempDesc)) ? tempDesc : 0,
                    tDescuento =
                    (desc != null && desc.Length > 2 &&
                    !string.IsNullOrEmpty(desc[2]) &&
                    desc[3].Trim() != "02" &&
                    decimal.TryParse(desc[2], out var tempDescuento)) ?
                    tempDescuento : (ITEMDESCUENTO.Count > 0 ? sumdescuentoDetalle : 0),

                    tDescuentoGlobal = (desc != null && desc.Length > 2 && !string.IsNullOrEmpty(desc[2]) && decimal.TryParse(desc[2], out var tempDescuentoGlobal)) ? tempDescuentoGlobal : 0,
                    tIcbper = (eCabecera != null && eCabecera.Length > 49 && !string.IsNullOrEmpty(eCabecera[49]) && eCabecera[49] != "0" && decimal.TryParse(eCabecera[49], out var tempIcbper)) ? tempIcbper : 0,
                    codigoMotivotDescuentoGlobal = (desc != null && desc.Length > 3) ? desc[3] : null,
                    codigoMotivotOtrosCargosGlobal = otrosCargos == null ? "" : otrosCargos.Split('|')[3],
                    tSubtotal = 0,
                    nameExportacion = "EXPORTACIÓN",
                    pctOtrosCargos = tOtrosCargos > 0 ? tOtrosCargos : otrosCargos == null ? 0 : sumpctOtroscargos + ((otrosCargos != null && otrosCargos.Split('|').Length > 2 && decimal.TryParse(otrosCargos.Split('|')[1], out var otGlobal)) ? otGlobal : 0),
                    tIgv =
                        (eCabecera != null && eCabecera.Length > 13 && !string.IsNullOrEmpty(eCabecera[13]) && decimal.TryParse(eCabecera[13], out var tempIgv))
                        ? (eCabecera[3] != "0101" ? tempIgv : (tempIgv > 0 ? tempIgv : 0))
                        : 0m,
                    Amount_CurrencyID = (eCabecera != null && eCabecera.Length > 8) ? eCabecera[8].Trim() : "",
                    tBaseImponible = (eCabecera != null && eCabecera.Length > 11 && !string.IsNullOrEmpty(eCabecera[11])) && anticipo.Count == 0 && sumbaseImponibleDetalle > 0 ? BASEIMPONIBLE : anticipo.Count > 0 ? tGravada : 0,
                    tImporteAnticipado = sumatImporteAnticipado,
                    AnticipoAllowanceChargeReasonCode = anticipo.Count > 0 ? anticipo.FirstOrDefault().Split('|')[10] == "" ? "04" : anticipo.FirstOrDefault().Split('|')[10] : desc[3] != null ? "" : "04",
                    //tAnticipoDescuentoAmount = sumatImporteAnticipado / (configuracion.CSuc_PorcentajeIGV + 1),
                    tAnticipoDescuentoAmount = sumatImporteAnticipado - sumatIGVAnticipado,
                    tAnticipoDescuentoMultiplierFactorNumeric = 1,
                    //tAnticipoDescuentoBaseAmount = sumatImporteAnticipado / (configuracion.CSuc_PorcentajeIGV + 1),
                    tAnticipoDescuentoBaseAmount = sumatImporteAnticipado - sumatIGVAnticipado,
                    //pctOtrosCargoRecargo = otrosCargos == null ? 0 : (otrosCargos.Split('|').Length > 2 && decimal.TryParse(otrosCargos.Split('|')[1], out var tempOtrosCargoRecargo) ? tempOtrosCargoRecargo : 0),                    
                    tOtrosCargosGlobalBaseAmount = sumpctOtroscargos > 0 ? sumbaseImponibleDetalle : 0,
                    tOtrosCargosGlobal = otrosCargos == null ? 0 : (decimal.TryParse(otrosCargos.Split('|')[2], out var tempOtrosCargosGlobalFinal) ? tempOtrosCargosGlobalFinal : 0),
                    codigoMotivoRetencion = retencion.Count > 0 ? retencion.FirstOrDefault().Split('|')[1] : null,
                    pctRetencion = retencion.Count > 0 ? Convert.ToDecimal(retencion.FirstOrDefault().Split('|')[2]) : 0,
                    tRetencionBaseAmount = retencion.Count > 0 ? Convert.ToDecimal(retencion.FirstOrDefault().Split('|')[4]) : 0,
                    tRetencion = retencion.Count > 0 ? Convert.ToDecimal(retencion.FirstOrDefault().Split('|')[3]) : 0,
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
                                    oFactura.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                                }
                                j++;
                            }
                        }
                        else
                        {
                            oFactura.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<beEmisorCampoAdicionalRegistro>();
                        }
                    }
                }
                else
                {
                    oFactura.lCampoAdicional = new List<beEmisorCampoAdicionalRegistro>();
                }

                #endregion

                #region OrdenCompra
                if (!string.IsNullOrEmpty(eCabecera[26].ToString().Trim()))
                {
                    oOrdenCompra.Add(new beFacturaOrdenCompra
                    {
                        accion = 1,
                        IdEmisor = eCabecera[4].ToString().Trim(),
                        serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                        numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                        ordenCompra = eCabecera[26].ToString() == "" ? "" : eCabecera[26].ToString().Trim(),
                    });
                    oFactura.lOrdenCompra = oOrdenCompra;
                }
                #endregion

                #region DocumentoDespacho
                if (!string.IsNullOrEmpty(eCabecera[27].ToString().Trim()))
                {
                    oDespacho.Add(new beFacturaDocumentoDespacho
                    {
                        accion = 1,
                        IdEmisor = eCabecera[4].ToString().Trim(),
                        serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                        numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                        idDocRel = "09",
                        docRel = eCabecera[27].ToString() == "" ? "" : eCabecera[27].ToString().Trim(),
                    });
                    oFactura.lDocDespacho = oDespacho.Count > 0 ? oDespacho : new List<beFacturaDocumentoDespacho>();
                }
                if (eCabecera.Length > 51)
                {
                    if (!string.IsNullOrEmpty(eCabecera[51].ToString().Trim()))
                    {
                        oDespacho.Add(new beFacturaDocumentoDespacho
                        {
                            accion = 1,
                            IdEmisor = eCabecera[4].ToString().Trim(),
                            serie = eCabecera[21].ToString() == "" ? "" : eCabecera[21].ToString().Trim(),
                            numero = eCabecera[22].ToString() == "" ? "" : eCabecera[22].ToString().Trim(),
                            idDocRel = "31",
                            docRel = eCabecera[51].ToString() == "" ? "" : eCabecera[51].ToString().Trim(),
                        });
                        oFactura.lDocDespacho = oDespacho.Count > 0 ? oDespacho : new List<beFacturaDocumentoDespacho>();
                    }
                }

                if (oFactura.lDocDespacho == null)
                {
                    oFactura.lDocDespacho = new List<beFacturaDocumentoDespacho>();
                    oFactura.lDocDespacho = oFactura.lDocDespacho.Count > 0 ? oFactura.lDocDespacho : new List<beFacturaDocumentoDespacho>();
                }
                #endregion

                #region FORMAPAGO_CREDITO
                if (formaPago.Split('|')[1].ToUpper() == "CREDITO" && cuotas.Count() > 0)
                {
                    oFactura.eCabecera.Cuotas = new List<beFacturaFormaPagoCuota>();
                    foreach (var item in cuotas)
                    {
                        var idcuota = item.Split('|')[1];
                        var montocuota = item.Split('|')[2];
                        var fechapagocuota = item.Split('|')[3];
                        oFactura.eCabecera.Cuotas.Add(new beFacturaFormaPagoCuota
                        {
                            accion = 1,
                            IdCuota = Regex.Replace(idcuota, @"(\D+)(\d+)", m =>
                            char.ToUpper(m.Groups[1].Value[0]) + m.Groups[1].Value.Substring(1).ToLower() + m.Groups[2].Value),
                            MontoPagoCuota = Convert.ToDecimal(montocuota),
                            FechaPagoCuota = Convert.ToDateTime(fechapagocuota).ToString("yyyy-MM-dd"),
                        });
                    }
                }
                #endregion

                //Totroscargos
                decimal otrscargosLinea =
                    (!string.IsNullOrEmpty(otrosCargos) && otrosCargos.Split('|').Length > 2 && decimal.TryParse(otrosCargos.Split('|')[2], out var tempOtrosCargosGlobal))
                        ? tempOtrosCargosGlobal
                          : 0m;
                if (descuentoGlobal != null)
                {
                    decimal montoDesc = Convert.ToDecimal(descuentoGlobal.Split('|')[2]);
                    oFactura.eCabecera.LegalMonetaryTotal_AllowanceTotalAmount = montoDesc;
                }

                //oFactura.eTotal.tOtrosCargos =
                //    otrosCargos != null && tOtrosCargos != 0 ? otrscargosLinea
                //    : tOtrosCargos == 0 ? 0
                //    : otrosCargos == null && ITEMOTROSCARGOS.Count == 0 ? OCTOTAL
                //    : ITEMOTROSCARGOS.Count > 0 ? oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount
                //    : otrscargosLinea > 0 && OCTOTAL == 0 ? otrscargosLinea : 0;

                if (otrosCargos != null && tOtrosCargos != 0)
                {
                    oFactura.eTotal.tOtrosCargos = otrscargosLinea;
                }
                else if (ITEMOTROSCARGOS.Count > 0)
                {
                    var afectaBI = ITEMOTROSCARGOS.Any(x => x.Split('|')[3].Trim() == "47");
                    if (afectaBI)
                    {
                        oFactura.eTotal.tOtrosCargos = 0;
                    }
                    else
                    {
                        oFactura.eTotal.tOtrosCargos = OCTOTAL;
                    }
                }
                else if (OCTOTAL > 0 && otrosCargos == null)
                {
                    oFactura.eTotal.tOtrosCargos = OCTOTAL;
                }
                else if (otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oFactura.eTotal.tOtrosCargos = 0;
                    }
                    else
                    {
                        oFactura.eTotal.tOtrosCargos = otrscargosLinea;
                    }
                }
                else if (ITEMOTROSCARGOS.Count > 0)
                {
                    oFactura.eTotal.tOtrosCargos = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount;
                }
                else if (otrosCargos == null && ITEMOTROSCARGOS.Count == 0)
                {
                    oFactura.eTotal.tOtrosCargos = OCTOTAL;
                }
                else if (otrscargosLinea > 0 && OCTOTAL == 0)
                {
                    oFactura.eTotal.tOtrosCargos = otrscargosLinea;
                }
                else
                {
                    oFactura.eTotal.tOtrosCargos = 0;
                }


                oFactura.eTotal.pctOtrosCargosGlobales = oFactura.eTotal.pctOtrosCargos;
                // Calcular tGravada
                if (desc != null && desc.Length > 3 && desc[3] != null && (anticipo?.Count ?? 0) == 0)
                {
                    if (desc[3] == "03")
                    {
                        oFactura.eTotal.tGravada = tGravada;
                    }
                    else if (tGravada > 0)
                    {
                        oFactura.eTotal.tGravada = tGravada - Convert.ToDecimal(desc[2]);
                    }
                    else
                    {
                        oFactura.eTotal.tGravada = tGravada;
                    }
                }
                else if (desc != null && desc.Length > 3 && desc[3]?.Trim() == "02" && (anticipo?.Count ?? 0) == 0)
                {
                    oFactura.eTotal.tGravada = Convert.ToDecimal(eCabecera?[11] ?? "0");
                }
                else if ((anticipo?.Count ?? 0) > 0 && tGravada > 0)
                {
                    //oFactura.eTotal.tGravada = Convert.ToDecimal(eCabecera?[11] ?? "0") - (tGravada + tGratuita);
                    oFactura.eTotal.tGravada = sumbaseImponibleDetalle - sumatGravadoAnticipado;
                }
                else if (otrosCargos != null && !UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()))
                {
                    if (otrosCargos.Split('|')[3] == "46" || otrosCargos.Split('|')[3] == "50")
                    {
                        oFactura.eTotal.tGravada = tGravada;
                    }
                    else if (tGravada > 0)
                    {
                        oFactura.eTotal.tGravada = tGravada + OCTOTAL;
                    }
                    else
                    {
                        oFactura.eTotal.tGravada = tGravada;
                    }

                }
                else
                {
                    oFactura.eTotal.tGravada = tGravada;
                }

                // Calcular tDescuentoGlobal
                oFactura.eTotal.tDescuentoGlobal = (desc[3] == null) ? 0 :
                    (desc[3].Trim() == "02" || desc[3].Trim() == "03" || desc[3].Trim() == "01") ? Convert.ToDecimal(desc[2]) : 0;

                // Calcular tDescuentoGlobalBaseAmount
                if (desc != null && desc.Length > 3 && desc[3] == null && (anticipo?.Count ?? 0) == 0 && tOtrosTributos == 0 && tOtrosCargos == 0)
                {
                    oFactura.eTotal.tDescuentoGlobalBaseAmount = 0;
                }
                else if (desc != null && desc.Length > 3 && (desc[3]?.Trim() == "02" || desc[3]?.Trim() == "03" || desc[3]?.Trim() == "01") && (anticipo?.Count ?? 0) == 0)
                {
                    if (otrosCargos != null)
                    {
                        oFactura.eTotal.tDescuentoGlobalBaseAmount = sumbaseImponibleDetalle;
                    }
                    else
                    {
                        decimal descuentoGlobalABI = Convert.ToDecimal(desc[2]?.Trim());
                        if (desc[3]?.Trim() == "03")
                        {
                            if (sumdescuentoDetalle == 0)
                            {
                                //oBoleta.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle - descuentoGlobalABI) + IGV;
                                oFactura.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle) + IGV;
                            }
                            else
                            {
                                //oBoleta.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle - descuentoGlobalABI) + IGV;
                                oFactura.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle);
                            }
                        }
                        else if (desc[3]?.Trim() == "02")
                        {
                            //oFactura.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle);
                            //oFactura.eTotal.tDescuentoGlobalBaseAmount = (sumbaseImponibleDetalle - sumdescuentoDetalle);
                            if (sumdescuentoDetalle > 0)
                            {
                                oFactura.eTotal.tDescuentoGlobalBaseAmount = ((SUBTOTAL - SumDetalleIGVAfectos) - sumdescuentoDetalle);
                            }
                            else
                            {
                                //oFactura.eTotal.tDescuentoGlobalBaseAmount = ((SUBTOTAL - sumbaseImponibleDetalle) - sumdescuentoDetalle);
                                oFactura.eTotal.tDescuentoGlobalBaseAmount = sumbaseImponibleDetalle;
                            }

                        }
                        else
                        {
                            //oBoleta.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle - descuentoGlobalABI) + IGV;
                            oFactura.eTotal.tDescuentoGlobalBaseAmount = (SUBTOTAL - sumdescuentoDetalle) + descuentoGlobalABI;
                        }

                    }

                }
                else if ((anticipo?.Count ?? 0) > 0 || (tOtrosTributos > 0))
                {
                    //var montoAnticipo = anticipo.FirstOrDefault().Split('|')[8];
                    //decimal.TryParse(montoAnticipo, out decimal montoAnticipo_);
                    oFactura.eTotal.tDescuentoGlobalBaseAmount = timporteTotal;
                }
                else if (tOtrosCargos > 0)
                {
                    oFactura.eTotal.tDescuentoGlobalBaseAmount = (sumbaseImponibleDetalle + sumIgvDetalle);
                }
                else
                {
                    oFactura.eTotal.tDescuentoGlobalBaseAmount = 0;
                }

                // Calcular tOtrosCargosGlobalBaseAmount
                if (eCabecera[11] == "" || eCabecera[11] == "0")
                {
                    oFactura.eTotal.tOtrosCargosGlobalBaseAmount = 0;
                }
                else if (anticipo.Count > 0 || tOtrosTributos > 0)
                {
                    oFactura.eTotal.tOtrosCargosGlobalBaseAmount = oFactura.eTotal.tDescuentoGlobalBaseAmount;
                }
                else if (tOtrosCargos > 0)
                {
                    oFactura.eTotal.tOtrosCargosGlobalBaseAmount = (sumbaseImponibleDetalle + sumIgvDetalle);
                }
                else if (otrosCargos != null)
                {
                    var codigo = otrosCargos.Split('|');
                    if (codigo[3].Trim() != "47" && codigo[3].Trim() != "48" && codigo[3].Trim() != "50")
                    {

                        if (codigo[3].Trim() == "46")
                        {
                            oFactura.eTotal.tOtrosCargosGlobalBaseAmount = BASEIMPONIBLE - tGratuita;
                        }
                        else
                        {
                            //oFactura.eTotal.tOtrosCargosGlobalBaseAmount = BASEIMPONIBLE;
                            oFactura.eTotal.tOtrosCargosGlobalBaseAmount = sumbaseImponibleDetalle;
                        }
                    }
                    else if (codigo[3].Trim() == "50")
                    {
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = (sumbaseImponibleDetalle + sumIgvDetalle);
                    }
                    else
                    {
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = sumbaseImponibleDetalle > 0 ? SUBTOTAL : 0;
                    }
                }
                else if (desc != null && desc.Length > 3 && (desc[3]?.Trim() == "02" || desc[3]?.Trim() == "03" || desc[3]?.Trim() == "01"))
                {
                    decimal descuentoGlobalABI = Convert.ToDecimal(desc[2]?.Trim());
                    if (sumdescuentoDetalle == 0)
                    {
                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = sumbaseImponibleDetalle + IGV + ICBPER;
                    }
                    else
                    {

                        oFactura.eTotal.tOtrosCargosGlobalBaseAmount = sumbaseImponibleDetalle - descuentoGlobalABI + IGV + ICBPER;
                    }
                }
                else
                {
                    oFactura.eTotal.tOtrosCargosGlobalBaseAmount = sumbaseImponibleDetalle > 0 ? SUBTOTAL : 0;
                }

                // Asignaciones directas
                decimal ma = anticipo.Count > 0
                                       ? anticipo.Sum(x =>
                                       {
                                           var partes = x.Split('|');
                                           return partes.Length > 8 && decimal.TryParse(partes[8], out decimal valor) ? valor : 0;
                                       })
                                       : 0;
                if (otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oFactura.eTotal.tInafecta = tInafecta > 0 ? tInafecta - ma : tInafecta;
                    }
                    else
                    {
                        oFactura.eTotal.tInafecta = tInafecta > 0 ? tInafecta - ma : tInafecta;
                    }
                }
                else
                {
                    oFactura.eTotal.tInafecta = tInafecta > 0 ? tInafecta - ma : tInafecta;
                }

                oFactura.eTotal.tExonerada = anticipo.Count > 0 ? 0 : tExonerada;
                oFactura.eTotal.tGratuita = tGratuita;
                oFactura.eTotal.tExportacion = tExportacion;
                oFactura.eTotal.tOtrosTributos = tOtrosTributos;
                //oBoleta.eTotal.tOtrosCargos = tOtrosCargos;
                //oBoleta.eTotal.tOtrosCargos = tOtrosCargos + oBoleta.eTotal.tOtrosCargos;
                //oFactura.eTotal.tImporteCobrar =
                //    ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()) ?
                //    IMPORTETOTAL + ICBPER : eCabecera != null && eCabecera.Length > 16 ? IMPORTETOTAL
                //    : IMPORTETOTAL == ma ? IMPORTETOTAL - ma : 0;

                if (ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()))
                {
                    oFactura.eTotal.tImporteCobrar = IMPORTETOTAL + ICBPER;
                }
                else if (eCabecera != null && eCabecera.Length > 16 && ma == 0)
                {
                    oFactura.eTotal.tImporteCobrar = IMPORTETOTAL + ICBPER;
                }
                else if (IMPORTETOTAL == ma)
                {
                    oFactura.eTotal.tImporteCobrar = timporteTotal - ma;
                }

                else
                {
                    if (ma > 0)
                    {
                        if (IMPORTETOTAL != ma)
                        {
                            oFactura.eTotal.tImporteCobrar = IMPORTETOTAL;
                        }
                    }
                    else
                    {
                        oFactura.eTotal.tImporteCobrar = 0;
                    }
                }

                // Calcular tImporteTotal
                if (tExportacion > 0)
                {
                    if (oFactura.eTotal.tIcbper > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()))
                    {
                        oFactura.eTotal.tImporteTotal = BASEIMPONIBLE + ICBPER;
                    }
                    else
                    {
                        oFactura.eTotal.tImporteTotal = tExportacion;
                    }

                }
                else if (anticipo.Count > 0 && otrosCargos == null)
                {
                    //var montoAnticipo = anticipo.FirstOrDefault().Split('|')[8];
                    //decimal.TryParse(montoAnticipo, out decimal montoAnticipo_);
                    oFactura.eTotal.tImporteTotal = timporteTotal;
                }
                else if (otrosCargos != null && decimal.TryParse(eCabecera[14].Trim(), out var cargo))
                {
                    var otclinea = otrosCargos.Split('|');
                    if (descuentoGlobal != null)
                    {
                        if (otclinea[3].Trim() == "49" || otclinea[3].Trim() == "46")
                        {
                            oFactura.eTotal.tImporteTotal = BASEIMPONIBLE + IGV;
                        }
                    }
                    else
                    {
                        if (otclinea[3].Trim() == "49")
                        {
                            oFactura.eTotal.tImporteTotal = IMPORTETOTAL;
                        }
                        else
                        {
                            oFactura.eTotal.tImporteTotal = timporteTotal;
                        }
                    }

                }
                else if (tOtrosCargos > 0 && tInafecta == 0 && tGratuita == 0 && tExportacion == 0 && tOtrosTributos == 0)
                {
                    oFactura.eTotal.tImporteTotal = (sumbaseImponibleDetalle + sumIgvDetalle);
                }
                else if (tOtrosCargos > 0 && tInafecta == 0 && tGratuita > 0 && tExportacion == 0 && tOtrosTributos == 0)
                {
                    oFactura.eTotal.tImporteTotal = (sumbaseImponibleDetalle + sumIgvDetalle);
                }
                else if (tInafecta > 0)
                {
                    if (tOtrosTributos > 0)
                    {
                        oFactura.eTotal.tImporteTotal = (tInafecta + tOtrosTributos);
                    }
                    else if (tExonerada > 0)
                    {
                        oFactura.eTotal.tImporteTotal = (tInafecta + tExonerada);
                    }
                    else if (tInafecta > 0 && tGravada == 0 && tExonerada == 0)
                    {
                        oFactura.eTotal.tImporteTotal = (tInafecta);
                    }
                    else
                    {
                        //oFactura.eTotal.tImporteTotal = (tInafecta);
                        oFactura.eTotal.tImporteTotal = (timporteTotal);
                    }

                }
                else if (descuentoGlobal != null)
                {
                    decimal montoDesc = Convert.ToDecimal(descuentoGlobal.Split('|')[2]);
                    if (tInafecta > 0)
                    {
                        oFactura.eTotal.tImporteTotal = tInafecta;
                        oFactura.eTotal.BasetDescuento = DESCUENTO - montoDesc;
                    }
                    else
                    {
                        if (descuentoGlobal.Split('|')[3] == "02" || descuentoGlobal.Split('|')[3] == "03")
                        {
                            if (oFactura.eTotal.tExonerada > 0)
                            {
                                oFactura.eTotal.tImporteTotal = tExonerada;
                                oFactura.eTotal.BasetDescuento = 0;
                            }
                            else
                            {
                                //oFactura.eTotal.tImporteTotal = (BASEIMPONIBLE - sumbaseImponibleDetalle) + (IGV - SumDetalleIGVAfectos);
                                if (descuentoGlobal.Split('|')[3] == "03")
                                {
                                    oFactura.eTotal.tImporteTotal = timporteTotal;
                                    oFactura.eTotal.BasetDescuento = 0;
                                }
                                else
                                {
                                    oFactura.eTotal.tImporteTotal = IMPORTETOTAL + ICBPER;
                                    oFactura.eTotal.BasetDescuento = 0;
                                }

                            }
                        }
                        else
                        {
                            oFactura.eTotal.tImporteTotal = BASEIMPONIBLE + IGV + montoDesc;
                            oFactura.eTotal.BasetDescuento = SUBTOTAL - sumdescuentoDetalle;
                        }
                    }

                }
                else
                {
                    //oFactura.eTotal.tImporteTotal =
                    //    tGravada + sumIgvDetalle + tInafecta + tOtrosCargos + tExonerada + tOtrosTributos + sumIscDetalle + ICBPER;
                    if (ITEMDESCUENTO.Any())
                    {
                        oFactura.eTotal.tImporteTotal = ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()) ?
                    IMPORTETOTAL + ICBPER : eCabecera != null && eCabecera.Length > 16 ? IMPORTETOTAL + ICBPER + DESCUENTO : 0;
                    }
                    else
                    {
                        oFactura.eTotal.tImporteTotal = ICBPER > 0 && UTilidades.TipoOperacionExportacion(eCabecera[3].Trim()) ?
                    IMPORTETOTAL + ICBPER : eCabecera != null && eCabecera.Length > 16 ? IMPORTETOTAL + ICBPER : 0;
                    }
                    //oFactura.eTotal.tImporteTotal =
                    //    IMPORTETOTAL + ICBPER;
                }

                // Calcular valores monetarios
                if (otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount = 0;
                    }
                    //else
                    //{
                    //    oBoleta.eCabecera.LegalMonetaryTotal_ChargeTotalAmount = otrosCargos != null ? Convert.ToDecimal(otrosCargos.Split('|')[2]) : 0;
                    //}
                }
                else if (tOtrosCargos > 0)
                {
                    oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount = tOtrosCargos;
                }
                else
                {
                    oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount = otrosCargos != null ? Convert.ToDecimal(otrosCargos.Split('|')[2]) : 0;
                }
                //oFactura.eCabecera.LegalMonetaryTotal_PayableAmount =
                //    ITEMDESCUENTO.Count > 0 && IMPORTETOTAL != 0
                //        ? oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + oFactura.eTotal.tImporteTotal - ma - DESCUENTO :
                //        sumdescuentoDetalle == 0
                //        ? oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + (oFactura.eTotal.tImporteTotal) - ma :
                //        sumdescuentoDetalle > 0 && DESCUENTO > 0
                //        ? oFactura.eTotal.tBaseImponible + IGV + oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount :
                //     oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + (oFactura.eTotal.tImporteTotal - (DESCUENTO - sumdescuentoDetalle)) - ma;

                if (ITEMDESCUENTO.Count > 0 && IMPORTETOTAL != 0 && otrosCargos == null)
                {
                    oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + oFactura.eTotal.tImporteTotal - ma - DESCUENTO;
                }
                else if (sumdescuentoDetalle == 0 && otrosCargos != null)
                {
                    if (otrosCargos.Split('|')[3] == "49")
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eTotal.tImporteTotal - ma;
                    }
                    else
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = (oFactura.eTotal.tImporteTotal + OCTOTAL) - ma;
                    }

                }
                //else if (descuentoGlobal == null && sumdescuentoDetalle > 0)
                //{
                //    oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = (oFactura.eTotal.tExonerada - sumdescuentoDetalle) + OCTOTAL - ma;
                //}
                else if (sumdescuentoDetalle > 0 && DESCUENTO > 0 && descuentoGlobal != null)
                {
                    //oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eTotal.tBaseImponible + IGV + oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount;
                    var codigoDesc = descuentoGlobal.Split('|')[3];
                    decimal montoDesc = Convert.ToDecimal(descuentoGlobal.Split('|')[2]);

                    if (codigoDesc.Trim() == "03")
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eTotal.tImporteTotal - montoDesc;
                    }
                    else
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eTotal.tImporteTotal;
                    }

                }
                else if (ITEMOTROSCARGOS.Count > 0)
                {
                    var codigo = ITEMOTROSCARGOS.Any(x => x.Split('|')[3] == "47");
                    if (codigo)
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = (oFactura.eTotal.tImporteTotal - (DESCUENTO - sumdescuentoDetalle)) - ma;
                    }
                }
                else if (descuentoGlobal == null && sumdescuentoDetalle == 0)
                {
                    oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + oFactura.eTotal.tImporteTotal - ma;
                }
                else if (descuentoGlobal != null)
                {
                    var codigoDesc = descuentoGlobal.Split('|')[3];
                    decimal montoDesc = Convert.ToDecimal(descuentoGlobal.Split('|')[2]);

                    if (codigoDesc == "02")
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + (oFactura.eTotal.tImporteTotal) - ma;
                    }
                    else
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + (oFactura.eTotal.tImporteTotal - (DESCUENTO - sumdescuentoDetalle)) - ma;
                    }
                }
                else
                {
                    oFactura.eCabecera.LegalMonetaryTotal_PayableAmount = oFactura.eCabecera.LegalMonetaryTotal_ChargeTotalAmount + (oFactura.eTotal.tImporteTotal - (DESCUENTO - sumdescuentoDetalle)) - ma;
                }

                //oFactura.eCabecera.LegalMonetaryTotal_TaxInclusiveAmount = tOtrosCargos > 0 ? oFactura.eTotal.tImporteTotal + tOtrosCargos :
                //    OCTOTAL > 0 ? oFactura.eTotal.tImporteTotal :
                //    anticipo.Count > 0 && IMPORTETOTAL != 0 && IMPORTETOTAL > ma ? IMPORTETOTAL - ma :
                //    oFactura.eTotal.tImporteTotal >= ma ? oFactura.eTotal.tImporteTotal - ma :
                //    oFactura.eTotal.tImporteTotal;

                oFactura.eCabecera.LegalMonetaryTotal_TaxInclusiveAmount =
                    tOtrosCargos > 0 ? oFactura.eTotal.tImporteTotal + oFactura.eTotal.tOtrosCargos :
                    OCTOTAL > 0 ? oFactura.eTotal.tImporteTotal :
                    anticipo.Count > 0 && IMPORTETOTAL != 0 && IMPORTETOTAL > ma ? IMPORTETOTAL :
                    oFactura.eTotal.tImporteTotal >= ma ? oFactura.eTotal.tImporteTotal :
                    oFactura.eTotal.tImporteTotal;

                oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount =
                        IMPORTETOTAL != 0 ? oFactura.eTotal.tGravada + tInafecta + tExonerada + tExportacion : IMPORTETOTAL;
                oFactura.eCabecera.BaseImponible = oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount;
                // Calcular tBaseImponible y tOTHBaseAmount
                if (tExonerada > 0)
                {
                    oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                }
                else if (tInafecta > 0)
                {
                    if (anticipo.Count > 0 && IMPORTETOTAL == 0)
                    {
                        oFactura.eTotal.tBaseImponible = tInafecta;
                    }
                    else if (OCTOTAL > 0 && otrosCargos != null)
                    {
                        if (otrosCargos.Split('|')[3] == "46")
                        {
                            oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                        }
                        else
                        {
                            oFactura.eTotal.tBaseImponible = tInafecta + OCTOTAL;
                        }
                    }
                    else if (OCTOTAL > 0 && tOtrosCargos > 0)
                    {
                        oFactura.eTotal.tBaseImponible = tInafecta;
                    }
                    else
                    {
                        oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                    }

                }
                else if (tGravada > 0)
                {
                    if (descuentoGlobal != null)
                    {
                        if (desc[3] == "02")
                        {
                            oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion - Convert.ToDecimal(desc[2]);
                        }
                        if (desc[3] == "03")
                        {
                            oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                        }
                    }
                    else if (otrosCargos != null)
                    {
                        if (otrosCargos.Split('|')[3] == "46" || otrosCargos.Split('|')[3] == "50")
                        {
                            oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                        }
                        else
                        {
                            oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + OCTOTAL;
                        }

                    }
                    else
                    {
                        oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion;
                    }
                }
                else
                {
                    //oBoleta.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + sumIgvDetalle - oBoleta.eTotal.tDescuentoGlobal;
                    oFactura.eTotal.tBaseImponible = tGravada + tInafecta + tExonerada + tExportacion + sumIgvDetalle;
                }

                if ((tOtrosTributos > 0) && BASEIMPONIBLE > sumbaseImponibleDetalle && sumbiconOTributos == 0)
                {
                    oFactura.eTotal.tOTHBaseAmount = (BASEIMPONIBLE - sumbaseImponibleDetalle) - sumbisinOTributos;
                }
                else if ((tOtrosTributos > 0) && BASEIMPONIBLE == sumbaseImponibleDetalle && sumbisinOTributos > 0)
                {
                    oFactura.eTotal.tOTHBaseAmount = (BASEIMPONIBLE) - sumbisinOTributos;
                }
                else if (sumbiconOTributos > 0)
                {
                    oFactura.eTotal.tOTHBaseAmount = sumbiconOTributos;
                }
                else
                {
                    oFactura.eTotal.tOTHBaseAmount = 0;
                }

                //oBoleta.eTotal.tOTHBaseAmount = tGravada;
                //oBoleta.eTotal.tBaseImponible = tGratuita > 0 ? 0 : oBoleta.eTotal.tBaseImponible;

                oFactura.eCabecera.BaseImponible = oFactura.eTotal.tBaseImponible;

                #region GLOBAL
                oFactura.lGlobales = new List<beFacturaGlobal>();
                if (otrosCargos != null)
                {
                    oGlobal = new beFacturaGlobal
                    {
                        IdEmisor = eCabecera[4].Trim(),
                        Serie = eCabecera[21].Trim(),
                        Numero = eCabecera[22].Trim(),
                        ChargeIndicator = true,
                        AllowanceChargeReason = otrosCargos.Split('|')[3].Trim(),
                        MultiplierFactor = Convert.ToDecimal(otrosCargos.Split('|')[1].Trim()),
                        Amount = Convert.ToDecimal(otrosCargos.Split('|')[2].Trim()),
                        AmountCurrency = eCabecera[8],
                        BaseAmount = oFactura.eTotal.tOtrosCargosGlobalBaseAmount,
                        BaseAmountCurrency = eCabecera[8],
                    };

                    oFactura.lGlobales.Add(oGlobal);
                }
                if (descuentoGlobal != null)
                {
                    oGlobal = new beFacturaGlobal
                    {
                        IdEmisor = eCabecera[4].Trim(),
                        Serie = eCabecera[21].Trim(),
                        Numero = eCabecera[22].Trim(),
                        ChargeIndicator = true,
                        AllowanceChargeReason = descuentoGlobal.Split('|')[3].Trim(),
                        MultiplierFactor = Convert.ToDecimal(descuentoGlobal.Split('|')[1].Trim()),
                        Amount = Convert.ToDecimal(descuentoGlobal.Split('|')[2].Trim()),
                        AmountCurrency = eCabecera[8],
                        BaseAmount = oFactura.eTotal.tDescuentoGlobalBaseAmount,
                        BaseAmountCurrency = eCabecera[8],

                    };
                    oFactura.lGlobales.Add(oGlobal);
                }
                if (anticipo.Count > 0)
                {

                    oGlobal = new beFacturaGlobal
                    {
                        IdEmisor = eCabecera[4].Trim(),
                        Serie = eCabecera[21].Trim(),
                        Numero = eCabecera[22].Trim(),
                        ChargeIndicator = true,
                        AllowanceChargeReason = anticipo.FirstOrDefault().Split('|')[10],
                        MultiplierFactor = 1,
                        Amount = oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount == 0.0M && anticipo.Count > 0 ? sumbaseImponibleDetalle : ma,
                        AmountCurrency = eCabecera[8].Trim(),
                        BaseAmount = oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount == 0.0M && anticipo.Count > 0 ? sumbaseImponibleDetalle : ma,
                        BaseAmountCurrency = eCabecera[8].Trim(),

                    };
                    oFactura.lGlobales.Add(oGlobal);
                }

                if (descuentoGlobal != null)
                {
                    var codigoDesc = descuentoGlobal.Split('|')[3];
                    bool tieneDescuento = !string.IsNullOrWhiteSpace(eCabecera[10]) && ITEMDESCUENTO.Count > 0;
                    //(eCabecera[10] == "" || ITEMDESCUENTO.Count == 0) ? 0 : Convert.ToDecimal(eCabecera[10]),
                    if ((codigoDesc == "02" || codigoDesc == "00") || tieneDescuento)
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_AllowanceTotalAmount = 0;
                    }
                    else
                    {
                        oFactura.eCabecera.LegalMonetaryTotal_AllowanceTotalAmount = Convert.ToDecimal(eCabecera[10]);
                    }
                }

                if (oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount == 0.0M && anticipo.Count > 0)
                {
                    oFactura.eCabecera.LegalMonetaryTotal_LineExtensionAmount = sumbaseImponibleDetalle;
                }
                oFactura.lCampoAdicionalDetalle = oCampoAdicionalDetalle.Count > 0 ? oCampoAdicionalDetalle : new List<CampoAdicionalRegistroDetalle>();
                #endregion
            }
            catch (Exception ex)
            {
                oFactura = null;
                _ = LogAsync("DescomponerFactura", ex);
            }

            return oFactura;
        }
    }
}
