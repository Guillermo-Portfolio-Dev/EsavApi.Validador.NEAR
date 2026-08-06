using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.ValidacionXTipoDoc
{
    public class validar08 : brGenerico
    {
        public static async Task<List<beRechazo>> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();
            try
            {
                var existe = new brConsultar().Consultar(lCabecera[4].Trim(), lCabecera[5].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim());
                var docReferencia = new brConsultar().ExisteDocReferencia(lCabecera[4], lCabecera[5].Trim(), lCabecera[19], lCabecera[20], lCabecera[18].Trim());
                var FechaDocReferencia = new brConsultar().ConsultarDocReferenciaValidar(lCabecera[4].Trim(), lCabecera[5].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim(), lCabecera[18].Trim());
                var configuracion = await new brConfiguracion().Consultar(lCabecera[4].ToString(), lCabecera[5].ToString());
                var importe = new brConsultar().ConsultaImporte(lCabecera[4].Trim(), lCabecera[5].Trim(), lCabecera[19].Trim(), lCabecera[18].Trim(), lCabecera[20].Trim());
                var _DataDocReferencia = lCabecera[17].Trim() == "1" ?
                    new brConsultar().ObtenerDocElectronicoForNC(lCabecera[4].Trim(), lCabecera[18].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim()) : null;
                var rucCache = new Dictionary<string, int>();

                var clienteLine = lineas.FirstOrDefault(x => x.ToUpper().StartsWith("CLIENTE"));
                //var detalleLines = lineas.Where(x => x.ToUpper().Contains("BIEN") || x.ToUpper().Contains("SERVICIO")).ToList();
                var detalleLines = lineas
                            .Where(x =>
                            {
                                var partes = x.Split('|');
                                return partes.Length > 5 &&
                                       int.TryParse(partes[0], out _) &&
                                       (partes[1].ToUpper() == "BIEN" || partes[1].ToUpper() == "SERVICIO");
                            })
                            .ToList();
                var anticipo = lineas.Where(x => x.ToUpper().StartsWith("ANTICIPO")).ToList();
                var otrosTributos = lineas.Where(x => x.ToUpper().StartsWith("OTROSTRIBUTOS")).ToList();
                var otrosCargos = lineas.Where(x => x.ToUpper().StartsWith("OTROSCARGOS")).ToList();
                var descuentoGlobal = lineas.Where(x => x.ToUpper().StartsWith("DESCUENTO")).ToList();
                var formaPago = lineas.Where(x => x.ToUpper().StartsWith("FORMAPAGO")).FirstOrDefault();
                var cuotas = lineas.Where(x => x.ToUpper().StartsWith("CUOTAS")).ToList();
                var fecha = lineas.Where(x => x.ToUpper().StartsWith("FECHA")).ToList();
                var paqueteTuristico = lineas.Where(x => x.Trim().ToUpper().StartsWith("D|")).ToList();
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

                var CUOTAS = lineas.Where(x =>
                {
                    var partes = x.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    return partes.Length > 1 && partes[0].ToUpper() == "ITEM" && partes[1].ToUpper() == "CUOTAS";
                }).ToList();

                var icbper = lineas.Where(x => x.ToUpper().StartsWith("ICBPER")).ToList();
                var retencion = lineas.Where(x => x.ToUpper().StartsWith("RETENCION")).ToList();
                var detraccion = lineas.Where(x => x.ToUpper().StartsWith("DETRACCION")).ToList();
                var Detdetraccion = lineas.Where(x => x.ToUpper().StartsWith("DETDETRACCION")).ToList();

                var columnas = ITEMDESCUENTO.Any() ? Enumerable.Range(0, ITEMDESCUENTO.Max(x => x.Split('|').Length))
                                                                    .Select(i => ITEMDESCUENTO.Select(x => x.Split('|')[i]).ToList())
                                                                    .ToList()
                                                        : new List<List<string>>();

                #region CABECERA 
                decimal.TryParse(lCabecera[13], out var IGV);
                decimal.TryParse(lCabecera[9], out var SUBTOTAL);
                decimal.TryParse(lCabecera[11], out var BASEIMPONIBLE);
                decimal.TryParse(lCabecera[16], out var IMPORTETOTAL);
                decimal.TryParse(lCabecera[12], out var ISCTOTAL);
                decimal.TryParse(lCabecera[14], out var OCTOTAL);
                decimal.TryParse(lCabecera[15], out var OTTOTAL);
                decimal.TryParse(lCabecera[10], out var DESCUENTO);
                decimal.TryParse(lCabecera[10], out var PDESCUENTO);


                if (lCabecera[0] != "210")
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "344", "VERSION DE TXT NO EXISTE, CONFIGURAR VERSION.", txt));
                }
                if (lCabecera[0] == "210")/*CABECERA*/
                {
                    DateTime fechaEmision = DateTime.Parse(lCabecera[6]);
                    if (fechaEmision.Date > DateTime.Now.Date)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "107", "LA FECHA DE EMISIÓN NO PUEDE SER POSTERIOR A LA FECHA ACTUAL.", txt));
                    }
                    if ((DateTime.Now - fechaEmision).TotalDays > 5)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "106", "DOCUMENTO FUERA DE FECHA. RECUERDA QUE LA EMISION DE BOLETA, NOTA DE CREDITO Y DEBITO RELACIONADA A BOLETA SON A 5 DIAS CALENDARIO.", txt));
                    }
                    if ((DateTime.Now - fechaEmision).Days > 3 && lCabecera[18] == "01")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "541",
                            "DOCUMENTO FUERA DE FECHA. RECUERDA QUE LA EMISION DE FACTURA, NOTA DE CREDITO Y DEBITO RELACIONADA A FACTURA SON A 3 DIAS CALENDARIO.",
                            txt));
                    }
                    if (fecha.Count == 0 && lCabecera[3].Trim() == "03" && lCabecera[17].Trim() == "1" && (string.IsNullOrWhiteSpace(_DataDocReferencia.Serie) && _DataDocReferencia.Numero == 0))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "525",
                            "CUANDO ES EL TIPO DE NOTA DE DEBITO ES PENALIDAD, DEBE ENVIAR LINEA FECHA DEL DOCUMENTO RELACIONADO.",
                            txt));
                    }
                    //if (existe > 0)
                    //{
                    //    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                    //    "365",
                    //    "YA SE HA GENERADO UNA NOTA DE CREDITO PARA EL DOCUMENTO DE REFERENCIA.",
                    //    txt));
                    //}
                    if (_DataDocReferencia != null)
                    {
                        if (lCabecera[8].Trim() != _DataDocReferencia.Moneda)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                    "568",
                                                    "LA MONEDA DE LA NOTA DE DEBITO NO COINCIDE CON LA MONEDA DEL DOCUMENTO DE REFERENCIA.",
                                                    txt));
                        }
                    }
                    string tipoDoc = lCabecera[18].Trim();
                    string serie = lCabecera[19].Trim();
                    string _numero = lCabecera[20].Trim();

                    if (tipoDoc != "05")
                    {
                        if (!string.IsNullOrWhiteSpace(tipoDoc) &&
                        !string.IsNullOrWhiteSpace(serie) &&
                        !string.IsNullOrWhiteSpace(_numero))
                        {
                            bool serieValida =
                                (tipoDoc == "01" && serie.StartsWith("F")) ||
                                (tipoDoc == "03" && serie.StartsWith("B"));

                            bool numeroEsNumerico = _numero.All(char.IsDigit);

                            if (!serieValida || !numeroEsNumerico)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                           "567",
                                                           "SI EL TIPO DOC. REF. ES 01 DEBE INICIAR CON F, SI ES 03 DEBE INICIAR CON B Y EL CAMPO 21 DEBE SER NUMÉRICO.",
                                                           txt));
                            }
                        }
                    }
                    if (lCabecera[31].Trim() == "1")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                        "504",
                        "NO SE APLICA DESCUENTO GLOBAL PARA ESTE TIPO DE DOCUMENTO.",
                        txt));
                    }
                    if (IMPORTETOTAL <= 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                        "572",
                        "EL IMPORTE TOTAL DE UNA NOTA DE DEBITO NO PUEDE SER 0.",
                        txt));
                    }
                    if (lCabecera[17].Trim() != "2" && lCabecera[3].Trim() != "02" && lCabecera[3].Trim() != "01" && lCabecera[3].Trim() != "03")
                    {
                        decimal importeTotalRedondeado = Math.Round(IMPORTETOTAL, 2, MidpointRounding.AwayFromZero);
                        decimal importeRefRedondeado = Math.Round(importe, 2, MidpointRounding.AwayFromZero);

                        if (importeTotalRedondeado > importeRefRedondeado && lCabecera[18] != "05")
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "366",
                                "EL IMPORTE TOTAL DE LA NOTA DE DEBITO NO PUEDE SER MAYOR AL IMPORTE TOTAL DEL DOCUMENTO DE REFERENCIA.",
                                txt));
                        }
                    }

                    if (lCabecera[17].Trim() == "2")
                    {
                        if (fecha.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "555",
                            "SI EL CAMPO 18 TIENE VALOR 2 DOC. EXTERNO, ENTONCES SU TXT DEBE TENER LINEA FECHA.",
                            txt));
                        }
                    }
                    if (fecha.Count > 0)
                    {
                        string fechaStr = fecha.FirstOrDefault().Split('|')[1].Trim();

                        if (!DateTime.TryParseExact(
                                fechaStr,
                                "dd/MM/yyyy",
                                CultureInfo.GetCultureInfo("es-PE"),
                                DateTimeStyles.None,
                                out DateTime fechaValida))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "571",
                                "EN LA LINEA FECHA, EL CAMPO 2 DEBE SER UNA FECHA VÁLIDA.",
                                txt));
                        }
                    }
                    if (lCabecera.Length > 52)
                    {
                        if (lCabecera[2] == "07" && (lCabecera[52].Trim() != "" || lCabecera[52].Trim() != null))
                        {
                            Regex regex = new Regex("^[a-zA-Z0-9]*$");
                            if (lCabecera[52].Length > 500 && regex.IsMatch(lCabecera[52]))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "501",
                                    "EL MOTIVO DE EMISION DEBE SER ALFANUMERICO DE 500 CARACTERES.",
                                    txt));
                            }

                            //if (string.IsNullOrEmpty(lCabecera[52].Trim()))
                            //{
                            //    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            //        "570",
                            //        "OBLIGATORIO ENVIAR MOTIVO (CAMPO 53 DE LA CABECERA DEL TXT ) DE EMISION SI ES NOTA DE CREDITO Y/O NOTA DE DEBITO.",
                            //        txt));
                            //}

                        }
                    }


                    if (lCabecera[18].Trim() == "03")
                    {

                        if (!lCabecera[21].Trim().StartsWith("B"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "563",
                                    "SI TIPO DE DOCUMENTO MODIFICADO POR LA NOTA DE DEBITO ES 03, ENTONCES SERIE DE ND DEBE COMENZAR CON B.",
                                    txt));
                        }
                    }
                    if (lCabecera[18].Trim() == "01")
                    {
                        if (!lCabecera[21].Trim().StartsWith("F"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "562",
                                    "SI TIPO DE DOCUMENTO MODIFICADO POR LA NOTA DE DEBITO ES 01, ENTONCES SERIE DE ND DEBE COMENZAR CON F.",
                                    txt));
                        }
                    }

                    string[] tiposDocRequierenNumerico = {
                                        "05", "06", "12", "13", "15", "16", "18", "21", "28", "30",
                                        "34", "37", "42", "43", "45", "55", "11", "17", "23", "24", "56"
                                    };

                    if (tiposDocRequierenNumerico.Contains(lCabecera[18].Trim()))
                    {
                        if (!long.TryParse(lCabecera[22]?.Trim(), out _))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "553",
                                "CUANDO EL TIPO DE DOC. REFERENCIA ES 05, EL CORRELATIVO DEBE SER NUMÉRICO.",
                                txt));
                        }
                    }

                    if (lCabecera[18].Trim() == "05")
                    {
                        var serieBA = lCabecera[21].Trim();

                        if (!serieBA.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "556",
                                "CUANDO EL DOCUMENTO REFERENCIA ES 05, LA SERIE DE LA NC Y ND DEBE EMPEZAR SOLO CON F (BOLETOS AÉREOS)",
                                txt));
                        }
                    }

                    if (lCabecera[4].Length != 11)
                    {
                        var ExisteRuc = new brConsultar().ConsultarRuc(lCabecera[4]);
                        if (ExisteRuc == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                   "532",
                                   "EL RUC EMISOR NO ES VALIDO.",
                                   txt));
                        }

                    }

                    if (lCabecera[17] == "2")
                    {
                        string lineaFecha = Array.Find(lineas, x => x.ToUpper().Contains("FECHA"));
                        if (lineaFecha == null)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                   "524",
                                   "SI LA ND ESTA RELACIONADA A UN DOCUMENTO EXTERNO ES OBLIGATORIO ENVIAR LA LINEA FECHA.",
                                   txt));
                        }
                    }
                    string lineaCliente = Array.Find(lineas, x => x.ToUpper().Contains("CLIENTE"));
                    if (lineaCliente == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "535",
                                  "DEBE HABER LINEA CLIENTE EN LA ND.",
                                  txt));
                    }
                    string lineaItem = Array.Find(lineas, x => x.ToUpper().Contains("BIEN") || x.ToUpper().Contains("SERVICIO"));
                    if (lineaItem == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "534",
                                  "DEBE HABER AL MENOS 1 DETALLE EN LA ND.",
                                  txt));
                    }
                    if (lCabecera[17] == "1" && lCabecera[18] != "05")
                    {

                        if (docReferencia == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "503",
                                  "EL DOCUMENTO REFERENCIA NO EXISTE.",
                                  txt));
                        }
                    }
                    if (DateTime.Parse(lCabecera[6]) < FechaDocReferencia)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "528",
                                  "LA FECHA DE LA ND NO PUEDE SER MENOR A LA FECHA DE EMISION DEL DOCUMENTO REFERENCIA.",
                                  txt));
                    }

                    string[] formatos = {
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yy HH:mm:ss"
                        };
                    DateTime FEmision;
                    DateTime FVencimiento = DateTime.ParseExact(lCabecera[7], "dd/MM/yyyy", null);

                    bool parseado = DateTime.TryParseExact(
                        lCabecera[6],
                        formatos,
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out FEmision);

                    if (!parseado)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "554",
                                  "FORMATO DE FECHA DE EMISIÓN NO VÁLIDO.",
                                  txt));
                    }
                    else
                    {
                        int comparacion = DateTime.Compare(FVencimiento.Date, FEmision.Date);
                        if (comparacion < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "533",
                                      "LA FECHA DE VENCIMIENTO NO PUEDE SER MENOR A LA FECHA DE EMISIÓN.",
                                      txt));
                        }
                    }

                    var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                    if (DataEmisor.serie == 0 || DataEmisor.serieUsuario == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "529",
                                  "NO EXISTE SERIE CONFIGURADA.",
                                  txt));
                    }
                    if (DataEmisor.usuario == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "530",
                                  "NO EXISTE USUARIO CONFIGURADO.",
                                  txt));
                    }
                    if (DataEmisor.sucursal == 0 || DataEmisor.sucursalUsuario == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "531",
                                  "NO EXISTE SUCURSAL CONFIGURADA.",
                                  txt));
                    }

                    decimal SumIGVDetalle = 0;
                    String[] parts_ = null;
                    if (otrosCargos.Count > 0)
                    {
                        parts_ = otrosCargos[0].Split('|');
                    }
                    foreach (var item in detalleLines)
                    {
                        var BS = item.Split('|');
                        SumIGVDetalle += Convert.ToDecimal(!string.IsNullOrEmpty(BS[12])
                                ? Math.Round(Convert.ToDecimal(BS[12]), configuracion.CEmi_CantidadDecimalDetalle, MidpointRounding.AwayFromZero) : 0);
                    }
                    var BI_SINOTC = parts_ != null && parts_[3] == "49" ? BASEIMPONIBLE - Convert.ToDecimal(parts_[2]) : 0;
                    var IGV_ORIGINAL = IGV - (BI_SINOTC * configuracion.CSuc_PorcentajeIGV);
                    var IGVRedondeado = Math.Round(IGV - (BI_SINOTC == 0 ? 0 : IGV_ORIGINAL), configuracion.CSuc_CantidadDecimal, MidpointRounding.AwayFromZero);

                    if (Math.Abs(IGVRedondeado - SumIGVDetalle) > 0.50m)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "542",
                                  "EL IGV DE LA CABECERA DEBE SER LA SUMA DE TODOS LOS IGV DEL DETALLE.",
                                  txt));
                    }


                    if (!UTilidades.TipoOperacionNotaDebito(lCabecera[3].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "548",
                                  "EL CODIGO DE TIPO DE OPERACION NO EXISTE.",
                                  txt));
                    }

                    if (descuentoGlobal.Count > 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "557",
                                  "LA NOTA DE DEBITO NO PUEDE TENER LINEA DESCUENTO.",
                                  txt));
                    }

                    var valor = lCabecera[33]?.Trim();
                    decimal numero;
                    var indicadorAnticipo = string.IsNullOrEmpty(valor) ||
                                            (!decimal.TryParse(valor, out numero) ? false : numero == 0)
                                            ? "0" : valor;

                    if (indicadorAnticipo == "1")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "558",
                                  "EN NOTA DE DEBITO NO HAY ANTICIPO.",
                                  txt));
                    }
                }
                #endregion

                #region DETALLE
                // Validaciones de detalle
                decimal SumDetalleIGVSinIna = 0;
                decimal SumDetalleTotal = 0;
                decimal SumDetalleISC = 0;
                decimal SumDetalleBI = 0;
                decimal sumDetalleSubTotal = 0;
                decimal SumDetalleBITotal = 0;
                decimal SumDetalleOT = 0;
                decimal SumDetalleDESC = 0;
                decimal SumDetaleICBPER = 0;
                decimal SumDetalleOC = 0;
                decimal SumDetalleBIconDescGlobal = 0;
                decimal SumDetalleGratuitos = 0;
                decimal SumDetalleBIInafectos = 0;

                int cantidadICBPER = 0;
                int detallesProcesados = 0;

                foreach (var line in detalleLines)
                {
                    var parts = line.Split('|');

                    decimal.TryParse(parts[12], out decimal igv);
                    decimal.TryParse(parts[8], out decimal vu);
                    decimal.TryParse(parts[7], out decimal cant);
                    decimal.TryParse(parts[9], out decimal desc);
                    decimal.TryParse(parts[10], out decimal pctdesc);
                    decimal.TryParse(parts[11], out decimal bi);
                    decimal.TryParse(parts[13], out decimal isc);
                    decimal.TryParse(parts[14], out decimal pctisc);
                    decimal.TryParse(parts[15], out decimal oc);
                    decimal.TryParse(parts[16], out decimal pctoc);
                    decimal.TryParse(parts[17], out decimal ot);
                    decimal.TryParse(parts[18], out decimal pctot);
                    decimal.TryParse(parts[19], out decimal importeTotal);


                    if (lCabecera[3].Trim() != "03")
                    {
                        var esExpo = parts[2].Trim() == "40" ? true : false;
                        if (esExpo && importeTotal == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "559",
                                      "PARA LA AFECTACION DE EXPORTACION EL IMPORTE NO DEBE SER 0.00.",
                                      txt));
                        }
                    }

                    if (desc != 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "505",
                                  "EN LA NOTA DE DEBITO NO PUEDE HABER DETALLES CON DESCUENTO.",
                                  txt));
                    }

                    var BasexImpuesto = !UTilidades.Inafecto(parts[2].Trim()) ?
                            Math.Round(configuracion.CSuc_PorcentajeIGV * (bi + isc), 2, MidpointRounding.AwayFromZero) : 0;
                    var igvRedondeado = Math.Round(igv, 2, MidpointRounding.AwayFromZero);
                    var esIna = UTilidades.Inafecto(parts[2]) || UTilidades.sinIGV(parts[2]);
                    var esExo = UTilidades.Exonerado(parts[2]);
                    var esExpo_ = parts[2].Trim() == "40" ? true : false;
                    if (!esExo && !esIna && !esExpo_)
                    {
                        if (Math.Abs(igvRedondeado - BasexImpuesto) > 0.50M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "543",
                                  "EL IGV DEBE SER IGUAL AL IMPUESTO MULTIPLICADO POR LA BASE IMPONIBLE.",
                                  txt));
                        }
                    }


                    if (lCabecera[3].Trim() != "13" && lCabecera[3].Trim() != "03")
                    {
                        if (UTilidades.EsCodigoGravado(parts[2].Trim()) && igv <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "544",
                                  "CUANDO EL TIPO DE AFECTACIÓN ES GRAVADO DEBE EXISTIR IGV.",
                                  txt));
                        }
                    }

                    if (pctoc <= 0 && pctoc >= 1)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "538",
                                  "EL PORCENTAJE DE OTROS CARGOS SOLO DEBE TENER EL VALOR ENTRE 0 A 1.",
                                  txt));
                    }


                    if (Math.Abs(bi - (cant * vu)) > 0.01M)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "546",
                            "LA BASE IMPONIBLE ES IGUAL A LA CANTIDAD * VALOR UNITARIO.",
                            txt));
                    }

                    if (string.IsNullOrEmpty(parts[6].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "547",
                                  "EL CAMPO DESCRIPCION EN EL DETALLE NO PUEDE ESTAR VACIO.",
                                  txt));
                    }

                    var UnidadMedidaText = new brConsultar().UnidaMedidaText(parts[3].Trim());
                    if (UnidadMedidaText.Item1 == null || UnidadMedidaText.Item1 == "")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "550",
                                  "LA UNIDAD DE MEDIDA NO EXISTE.",
                                  txt));
                    }

                    if (!UTilidades.TipoAfectacion(parts[2].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "552",
                                  "TIPO DE AFECTACION AL IGV NO EXISTE.",
                                  txt));
                    }

                    int index = int.Parse(parts[0].Trim());

                    if (_DataDocReferencia != null)
                    {
                        if (_DataDocReferencia.EstadoComprobante)
                        {
                            if (parts[2].Trim() != _DataDocReferencia.Detalle.OrderBy(x => x.ID).ElementAt(index - 1).SubAfectacionIGV)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                          "565",
                                          $"EL TIPO DE AFECTACION DE IGV TIENE QUE SER EL MISMO QUE EL PRESENTE EN EL DOCUMENTO DE REFERENCIA, REVISAR LINEA {parts[0]}.",
                                          txt));
                            }
                        }
                    }

                    SumDetalleBI += !UTilidades.EsCodigoGratuito(parts[2]) ? bi : 0;
                    SumDetalleBITotal += bi;
                    SumDetalleOT += ot;
                    SumDetalleOC += oc;
                    SumDetalleDESC += desc;
                    SumDetalleIGVSinIna += !UTilidades.Inafecto(parts[2]) ? igv : 0;
                    SumDetaleICBPER += parts[6].ToUpper().Trim() == "BOLSA PLASTICA" ? importeTotal : 0;
                    SumDetalleISC += isc;
                    cantidadICBPER += parts[6].ToUpper().Trim() == "bolsa plastica" ? Convert.ToInt32(cant) : 0;
                    SumDetalleBIconDescGlobal += !UTilidades.EsCodigoGratuito(parts[2]) ? bi : 0;
                    SumDetalleGratuitos += UTilidades.EsCodigoGratuito(parts[2]) ? bi + igv : 0;
                    SumDetalleTotal += importeTotal;
                    sumDetalleSubTotal += cant * vu;
                    SumDetalleBIInafectos += UTilidades.Inafecto(parts[2]) ? bi : 0;


                    detallesProcesados++;

                    if (detallesProcesados < detalleLines.Count)
                    {
                        continue;
                    }

                    if (IGV > 0)
                    {
                        String[] _parts = null;
                        if (otrosCargos.Count > 0)
                        {
                            _parts = otrosCargos[0].Split('|');
                        }
                        //decimal totalEsperado = Math.Round(IGV + BASEIMPONIBLE + SumDetalleOT - SumDetalleBIInafectos, configuracion.CSuc_CantidadDecimal);
                        decimal suma = IGV + BASEIMPONIBLE + SumDetalleOT + SumDetalleOC + SumDetalleISC - SumDetalleGratuitos;
                        suma += SumDetalleOC > 0 ? 0 : (_parts != null && _parts[3] != "49" ? OCTOTAL : 0);
                        suma += SumDetalleOT > 0 ? 0 : OTTOTAL;
                        decimal totalEsperado = Math.Round(
                            decimal.Parse(suma.ToString("F" + configuracion.CSuc_CantidadDecimal)),
                            configuracion.CSuc_CantidadDecimal,
                            MidpointRounding.AwayFromZero
                        );
                        if (Math.Abs(IMPORTETOTAL - totalEsperado) > 0.50M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                      "545",
                                                      "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MÁS IGV.",
                                                      txt));
                        }
                    }

                    if ((Math.Abs(Math.Round(SumDetalleTotal, 2, MidpointRounding.AwayFromZero) -
                          Math.Round(IMPORTETOTAL, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                        descuentoGlobal.Count == 0 &&
                        otrosCargos.Count == 0 &&
                        ITEMOTROSCARGOS.Count == 0 &&
                        ITEMDESCUENTO.Count == 0 &&
                        anticipo.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "580", "EL IMPORTE TOTAL DE LA CABECERA NO COINCIDE CON LA SUMA TOTAL DEL DETALLE.", txt));
                    }

                    if ((Math.Abs(Math.Round(SumDetalleBITotal, 2, MidpointRounding.AwayFromZero)
                        - Math.Round(BASEIMPONIBLE, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                    descuentoGlobal.Count == 0 &&
                    otrosCargos.Count == 0 &&
                    ITEMOTROSCARGOS.Count == 0 &&
                    ITEMDESCUENTO.Count == 0 &&
                    anticipo.Count == 0 && lCabecera[3].Trim() != "02" && lCabecera[3].Trim() != "01" && lCabecera[3].Trim() != "03")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "585", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO COINCIDE CON LA CABECERA.", txt));
                    }

                    if ((Math.Abs(Math.Round(SUBTOTAL, 2, MidpointRounding.AwayFromZero)
                        - Math.Round(sumDetalleSubTotal, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                    descuentoGlobal.Count == 0 &&
                    otrosCargos.Count == 0 &&
                    ITEMOTROSCARGOS.Count == 0 &&
                    ITEMDESCUENTO.Count == 0 &&
                    anticipo.Count == 0 && lCabecera[3].Trim() != "02" && lCabecera[3].Trim() != "01" && lCabecera[3].Trim() != "03")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "587", "LA SUMA DEL SUBTOTAL DEL DETALLE NO COINCIDE CON EL SUBTOTAL CABECERA.", txt));
                    }

                    if ((Math.Abs(Math.Round(SumDetalleIGVSinIna, 2, MidpointRounding.AwayFromZero)
                        - Math.Round(IGV, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                        descuentoGlobal.Count == 0 &&
                        otrosCargos.Count == 0 &&
                        ITEMOTROSCARGOS.Count == 0 &&
                        ITEMDESCUENTO.Count == 0 &&
                        anticipo.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "586", "LA SUMA DE LOS IGV DEL DETALLE NO CUADRA CON EL IGV DE LA CABECERA.", txt));
                    }

                    if (otrosCargos.Count > 0)
                    {
                        foreach (var item in otrosCargos)
                        {
                            var parts_ = item.Split('|');

                            if (parts_[3].Trim() == "49")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "569", "NO SE PUEDE ENVIAR LINEA OTROS CARGOS CON CODIGO 49.", txt));
                            }

                            if (decimal.TryParse(parts_[1], out var porcentajeOT) &&
                                decimal.TryParse(parts_[2], out var montoOT))
                            {
                                var montoCalculado = SumDetalleBI * porcentajeOT;
                                if (parts_[3] == "46" || parts_[3] == "45" || parts_[3] == "50"
                                    || parts_[3] == "51" || parts_[3] == "52" || parts_[3] == "53")
                                {
                                    if (descuentoGlobal.Count > 0)
                                    {
                                        var descGlobal = descuentoGlobal.FirstOrDefault().Split('|');
                                        if (descGlobal[3] == "02" || descGlobal[3] == "03" || descGlobal[3] == "04" || descGlobal[3] == "05" || descGlobal[3] == "06")
                                        {
                                            decimal nuevabi = SumDetalleBI * Convert.ToDecimal(descGlobal[1]);
                                            montoCalculado = (SumDetalleBI - nuevabi) * porcentajeOT;

                                            if (Math.Abs(montoCalculado - montoOT) > 0.50M)
                                            {
                                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (parts_[3] == "50" || parts_[3] == "48")
                                        {
                                            montoCalculado = SumDetalleTotal * porcentajeOT;
                                            if (Math.Abs(montoCalculado - montoOT) > 0.50M)
                                            {
                                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                            }
                                        }
                                        else
                                        {
                                            if (Math.Abs(montoCalculado - montoOT) > 0.50M)
                                            {
                                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    if (Math.Abs(montoCalculado - montoOT) > 0.50M)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region RETENCION
                if (retencion.Count > 0)
                {
                    foreach (var item in retencion)
                    {
                        if ((lCabecera[2] != "07" && lCabecera[18] != "01") && lCabecera[2] != "07")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "352",
                                  "RETENCION SOLO APLICA A FACTURA, NOTA DE CREDITO DE FACTURA Y NOTA DE DEBITO DE FACTURA.",
                                  txt));
                        }
                    }
                }
                #endregion


                // Validaciones de cliente
                if (clienteLine != null)
                {
                    var parts = clienteLine.Split('|');
                    if (!rucCache.TryGetValue(parts[2], out int existeRuc))
                    {
                        existeRuc = new brConsultar().ConsultarRuc(parts[1].Trim() == "6" && parts[2].Length > 11 ? "" : parts[2]);
                        rucCache[parts[2]] = existeRuc;

                        #region CLIENTE
                        if (parts[0].ToUpper().Trim() == "CLIENTE")
                        {
                            if (parts.Length < 4)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "566",
                                      "LINEA CLIENTE INCOMPLETA. DEBE ENVIARSE AL MENOS HASTA EL CAMPO 4.",
                                      txt));
                            }
                            if (parts[1].Trim() == "1")
                            {
                                string patron = @"^\d{8}$";
                                if (!Regex.IsMatch(parts[2], patron))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "536",
                                      "SI LA LINEA DEL CLIENTE ENVIA CODIGO 1 ENTONCES DEBE SER UN DNI DE 8 DIGITOS.",
                                      txt));
                                }
                            }
                            if (parts[1].Trim() == "6")
                            {
                                string patron = @"^\d{11}$";
                                if (!Regex.IsMatch(parts[2], patron))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "537",
                                      "SI LA LINEA DEL CLIENTE ENVIA CODIGO 6 ENTONCES DEBE SER UN RUC DE 11 DIGITOS.",
                                      txt));
                                }
                                var ExisteRuc = new brConsultar().ConsultarRuc(parts[2]);
                                if (ExisteRuc == 0)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "540",
                                      "EL RUC NO EXISTE.",
                                      txt));
                                }

                                if (string.IsNullOrEmpty(parts[3].Trim()))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "549",
                                      "EL CAMPO RAZON SOCIAL EN LA LINEA CLIENTE NO PUEDE IR VACIO.",
                                      txt));
                                }
                            }

                        }
                        #endregion
                    }
                }

                if (detalleLines.Count == 0)
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "355", "TIPO DE DETALLE NO ACEPTADO, SOLO SE ACEPTA BIEN O SERVICIO.", txt));
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "317", "DOCUMENTO NO CUENTA CON DETALLE.", txt));
                }
            }
            catch (Exception ex)
            {
                await LogAsync("Validar08", ex);
                lRechazos = new List<beRechazo>();
                lRechazos.AddRange(await AgregarRechazo(lCabecera, "300", "CONTENIDO INCORRECTO O INVALIDO.", txt));
            }

            return lRechazos;
        }
        private static Task<List<beRechazo>> AgregarRechazo(string[] lCabecera, string codigo, string descripcion, string[] txt)
        {
            string tipoDoc = txt[5].Substring(33, 2);
            List<beRechazo> lRechazos = new List<beRechazo>();

            string[] formatos = {
                "dd/MM/yyyy HH:mm:ss",
            };
            DateTime FE;
            string fecha = DateTime.TryParseExact(lCabecera[6], formatos, null, System.Globalization.DateTimeStyles.None, out FE) ? FE.ToString("dd/MM/yyyy HH:mm:ss") : "";
            DateTime.TryParse(lCabecera[6], out DateTime fechaEmision);
            lRechazos.Add(new beRechazo
            {
                RUC = lCabecera[4],
                Sede = lCabecera[5],
                Serie = lCabecera[21],
                Numero = lCabecera[22],
                CodigoRechazo = codigo,
                Descripcion = descripcion,
                TipoDoc = tipoDoc,
                FechaEmision = fecha != "" ? fechaEmision : DateTime.Now,
                FechaTransferencia = DateTime.Now,
                Txt = txt[5],
                TipoMoneda = lCabecera[8]
            });

            return Task.FromResult(lRechazos);
        }
    }
}
