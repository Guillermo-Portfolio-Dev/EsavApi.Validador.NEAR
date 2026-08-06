using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.BR.RechazosXTipoDoc
{
    public class Validar07 : brGenerico
    {
        public static async Task<List<beRechazo>> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();
            try
            {
                var existe = lCabecera[18].Trim() != "05" ? new brConsultar().Consultar(lCabecera[4].Trim(), lCabecera[5].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim()) : 0;
                var docReferencia = lCabecera[18].Trim() != "05" ? new brConsultar().ExisteDocReferencia(lCabecera[4], lCabecera[5].Trim(), lCabecera[19], lCabecera[20], lCabecera[18].Trim()) : 0;
                var estadoSunat = new brConsultar().EstadoSunat(lCabecera[4].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim(), lCabecera[18].Trim());
                var FechaDocReferencia = new brConsultar().ConsultarDocReferenciaValidar(
                    lCabecera[4].Trim(), lCabecera[5].Trim(), lCabecera[19].Trim(), lCabecera[20].Trim(), lCabecera[18].Trim());
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

                var lineasSinCabecera = lineas.Skip(1).ToList();
                var lineasDesconocidas = lineasSinCabecera.Where(x =>
                {
                    var upper = x.Trim().ToUpper();
                    var partes = upper.Split('|');
                    if (partes.Length > 5 &&
                        int.TryParse(partes[0], out _) &&
                        (partes[1] == "BIEN" || partes[1] == "SERVICIO"))
                        return false;

                    var prefijosConocidos = new[]
                    {
                        "CLIENTE",
                        "FORMAPAGO",
                        "ANTICIPO",
                        "OTROSTRIBUTOS",
                        "OTROSCARGOS",
                        "DESCUENTO",
                        "CUOTAS",
                        "ITEMSP",
                        "ITEM|OTROSCARGOS",
                        "ITEM|DESCUENTO",
                        "ITEM|PLACA",
                        "ITEM|CUOTAS",
                        "ICBPER",
                        "RETENCION",
                        "DETRACCION",
                        "DETDETRACCION",
                        "DOCASOCIADO",
                        "D|",
                        "FECHA"
                    };

                    if (prefijosConocidos.Any(pref => upper.StartsWith(pref)))
                        return false;

                    return true;

                }).ToList();

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

                    if (_DataDocReferencia != null)
                    {
                        if (string.IsNullOrWhiteSpace(_DataDocReferencia.Serie) || string.IsNullOrWhiteSpace(_DataDocReferencia.Moneda) || _DataDocReferencia.Numero == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                       "588",
                       "EL DOCUMENTO EN REFERENCIA SE ENCUENTRA ANULADO O NO EXISTE.",
                       txt));
                            return lRechazos;
                        }

                    }

                    if (lineasDesconocidas.Count > 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "655", "LINEA DESCONOCIDA O INCORRECTA EN EL SU ARCHIVO TXT VERIFICAR.", txt));
                    }

                    DateTime fechaEmision = DateTime.Parse(lCabecera[6]);
                    if (fechaEmision.Date > DateTime.Now.Date)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "107", "LA FECHA DE EMISIÓN NO PUEDE SER POSTERIOR A LA FECHA ACTUAL.", txt));
                    }
                    if ((DateTime.Now.Date - fechaEmision.Date).TotalDays > 5)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "106", "DOCUMENTO FUERA DE FECHA. RECUERDA QUE LA EMISION DE BOLETA, NOTA DE CREDITO Y DEBITO RELACIONADA A BOLETA SON A 5 DIAS CALENDARIO.", txt));
                    }

                    if ((DateTime.Now.Date - fechaEmision.Date).Days > 3 && lCabecera[18] == "01")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "541",
                            "DOCUMENTO FUERA DE FECHA. RECUERDA QUE LA EMISION DE FACTURA, NOTA DE CREDITO Y DEBITO RELACIONADA A FACTURA SON A 3 DIAS CALENDARIO.",
                            txt));
                    }
                    if (existe > 0 && lCabecera[3].Trim() == "01" && lCabecera[17].Trim() != "2")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                        "365",
                        "YA SE HA GENERADO UNA NOTA DE CREDITO PARA EL DOCUMENTO DE REFERENCIA.",
                        txt));
                    }

                    if (lCabecera[17].Trim() == "1" && lCabecera[3].Trim() == "01")
                    {
                        if (IMPORTETOTAL != (decimal)_DataDocReferencia.ImporteTotal)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                    "581",
                                                    "SI TIPO DE OPERACION ES ANULACION DE LA OPERACION(01), LOS MONTOS DEBEN SER LOS MISMOS QUE EL DEL DOCUMENTO DE REFERENCIA.",
                                                    txt));
                        }
                    }

                    if (_DataDocReferencia != null)
                    {
                        if (lCabecera[8].Trim() != _DataDocReferencia.Moneda)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                    "568",
                                                    "LA MONEDA DE LA NOTA DE CRÉDITO NO COINCIDE CON LA MONEDA DEL DOCUMENTO DE REFERENCIA.",
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
                                (tipoDoc == "03" && serie.StartsWith("B")) ||
                                (tipoDoc == "01" && serie.StartsWith("E001")) ||
                                (tipoDoc == "01" && serie.StartsWith("0")) ||
                                (tipoDoc == "03" && serie.StartsWith("0")) ||
                                (tipoDoc == "03" && serie.StartsWith("EB01"));


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
                    if (lCabecera[17].Trim() != "2")
                    {
                        decimal importeTotalRedondeado = Math.Round(IMPORTETOTAL, 2, MidpointRounding.AwayFromZero);
                        decimal importeRefRedondeado = Math.Round(importe, 2, MidpointRounding.AwayFromZero);

                        if (importeTotalRedondeado > importeRefRedondeado && lCabecera[18] != "05")
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "366",
                                "EL IMPORTE TOTAL DE LA NOTA DE CREDITO NO PUEDE SER MAYOR AL IMPORTE TOTAL DEL DOCUMENTO DE REFERENCIA.",
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

                    if (lCabecera[3].Trim() == "13" && lCabecera[18].Trim() != "01")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "560",
                                    "LA NOTA DE CRÉDITO DE TIPO 13 DEBE ESTAR VINCULADA AL DOCUMENTO DE REFERENCIA FACTURA.",
                                    txt));
                    }


                    if (lCabecera[18].Trim() == "03")
                    {

                        if (!lCabecera[21].Trim().StartsWith("B") && !lCabecera[21].Trim().StartsWith("0"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "563",
                                    "SI TIPO DE DOCUMENTO MODIFICADO POR LA NOTA DE CREDITO ES 03, ENTONCES SERIE DE NC DEBE COMENZAR CON B O 0.",
                                    txt));
                        }
                    }
                    if (lCabecera[18].Trim() == "01")
                    {
                        if (!lCabecera[21].Trim().StartsWith("F") && !lCabecera[21].Trim().StartsWith("0"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "562",
                                    "SI TIPO DE DOCUMENTO MODIFICADO POR LA NOTA DE CREDITO ES 01, ENTONCES SERIE DE NC DEBE COMENZAR CON F 0 0.",
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
                                   "SI LA NC ESTA RELACIONADA A UN DOCUMENTO EXTERNO ES OBLIGATORIO ENVIAR LA LINEA FECHA.",
                                   txt));
                        }
                    }
                    string lineaCliente = Array.Find(lineas, x => x.ToUpper().Contains("CLIENTE"));
                    if (lineaCliente == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "535",
                                  "DEBE HABER LINEA CLIENTE EN LA NC.",
                                  txt));
                    }
                    string lineaItem = Array.Find(lineas, x => x.ToUpper().Contains("BIEN") || x.ToUpper().Contains("SERVICIO"));
                    if (lineaItem == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "534",
                                  "DEBE HABER AL MENOS 1 DETALLE EN LA NC.",
                                  txt));
                    }
                    //if (lCabecera[17] == "1" && lCabecera[18] != "05")
                    //{

                    //    if (docReferencia == 0)
                    //    {
                    //        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                    //              "503",
                    //              "EL DOCUMENTO REFERENCIA NO EXISTE.",
                    //              txt));
                    //    }
                    //}
                    if (lCabecera[17] == "0" && lCabecera[18] != "05")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "526",
                                  "PARA NC SE TIENE QUE ENVIAR 1 O 2 EN EL CAMPO 18 CABECERA.",
                                  txt));
                    }

                    if (DateTime.Parse(lCabecera[6]) < FechaDocReferencia)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "528",
                                  "LA FECHA DE LA NC NO PUEDE SER MENOR A LA FECHA DE EMISION DEL DOCUMENTO REFERENCIA.",
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

                    if (lCabecera[17].Trim() == "1")
                    {
                        if (estadoSunat != 0)
                        {
                            var rucCliente = clienteLine.Split('|');
                            var estadoSunatApi = new brConsultar().SunatConsultaApi(rucCliente[2].Trim(), lCabecera[18].Trim(), lCabecera[19].Trim(), int.Parse(lCabecera[20].Trim()), FechaDocReferencia.ToString("dd/MM/yyyy"), (decimal)_DataDocReferencia.ImporteTotal);

                            if (estadoSunatApi != "2" && estadoSunatApi != "1")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "622",
                                      "EL DOCUMENTO EN REFERENCIA AUN NO SE ENCUENTRA ACEPTADO POR SUNAT.",
                                      txt));
                            }
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


                    if (!UTilidades.TipoOperacionNotaCredito(lCabecera[3].Trim()))
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
                                  "LA NOTA DE CREDITO NO PUEDE TENER LINEA DESCUENTO.",
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
                                  "EN NOTA DE CREDITO NO HAY ANTICIPO.",
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
                int lineaActual = 0;
                foreach (var line in detalleLines)
                {
                    lineaActual++;
                    var parts = line.Split('|');
                    decimal resultado;

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

                    if (Decimal.TryParse(parts[19], out resultado))
                    {
                        if (resultado > 0 && IMPORTETOTAL > 0 && lCabecera[2] == "07" && (lCabecera[3] == "13" || lCabecera[3] == "03"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "522",
                                  "LOS MONTOS DEBE IR CERO EN LA CABECERA Y EN EL DETALLE CUANDO LA NOTA DE CREDITO SEA DE TIPO OPERACION 13 y 03, SOLO LA CANTIDAD DEBE SER MAYOR A CERO.",
                                  txt));
                        }
                    }

                    if (lCabecera[3].Trim() == "03")
                    {
                        bool hayImportes = vu != 0 || desc != 0 || pctdesc != 0 || bi != 0 || igv != 0 || isc != 0
                            || pctisc != 0 || oc != 0 || pctoc != 0 || ot != 0 || pctot != 0 || importeTotal != 0;

                        if (cant <= 0 || hayImportes)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "561",
                                  "PARA EL TIPO DE NOTA DE CRÉDITO 03, SOLO LA CANTIDAD PUEDE SER MAYOR A 0. LOS DEMÁS IMPORTES DEBEN SER 0.00.",
                                  txt));
                        }
                    }

                    if (lCabecera[3].Trim() != "13" && lCabecera[3].Trim() != "03")
                    {
                        if (importeTotal <= 0 && vu <= 0 && IMPORTETOTAL <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "525",
                                  "SI EL MOTIVO DE NC ES DIFERENTE A 03 Y 13 EL VALOR UNITARIO Y MONTO TOTAL NO PUEDEN SER 0.",
                                  txt));
                        }
                    }
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
                                  "EN LA NOTA DE CREDITO NO PUEDE HABER DETALLES CON DESCUENTO.",
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

                    decimal biRedondeado = Math.Round(bi, configuracion.CEmi_CantidadDecimalDetalle);
                    decimal calculoRedondeado = Math.Round(cant * vu, configuracion.CEmi_CantidadDecimalDetalle);
                    if (Math.Abs(biRedondeado - calculoRedondeado) > 0.1M)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "546",
                            $"LA BASE IMPONIBLE ES IGUAL A LA CANTIDAD * VALOR UNITARIO. LINEA {line[0]}",
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
                        //string[] no_aplica_tipo_nc =  { "07", "04" };
                        if (_DataDocReferencia.EstadoComprobante && lCabecera[3].Trim() == "01")
                        {
                            if (parts[2].Trim() != _DataDocReferencia.Detalle.OrderBy(x => x.ID).ElementAt(lineaActual - 1).SubAfectacionIGV)
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
                        anticipo.Count == 0 && lCabecera[3].Trim() != "07")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "580", "EL IMPORTE TOTAL DE LA CABECERA NO COINCIDE CON LA SUMA TOTAL DEL DETALLE.", txt));
                    }

                    if ((Math.Abs(Math.Round(SumDetalleBITotal, 2, MidpointRounding.AwayFromZero)
                        - Math.Round(BASEIMPONIBLE, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                    descuentoGlobal.Count == 0 &&
                    otrosCargos.Count == 0 &&
                    ITEMOTROSCARGOS.Count == 0 &&
                    ITEMDESCUENTO.Count == 0 &&
                    anticipo.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "585", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO COINCIDE CON LA CABECERA.", txt));
                    }

                    if ((Math.Abs(Math.Round(SUBTOTAL, 2, MidpointRounding.AwayFromZero)
                        - Math.Round(sumDetalleSubTotal, 2, MidpointRounding.AwayFromZero)) > 0.1M) &&
                    descuentoGlobal.Count == 0 &&
                    otrosCargos.Count == 0 &&
                    ITEMOTROSCARGOS.Count == 0 &&
                    ITEMDESCUENTO.Count == 0 &&
                    anticipo.Count == 0)
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

                #region FORMAPAGO

                if (lCabecera[18].Trim() == "01" && lCabecera[3].Trim() == "13")
                {
                    if (formaPago != null && formaPago != "")
                    {
                        var FormaPago = formaPago.Split('|');
                        var descripcion = FormaPago[1].Trim();
                        decimal.TryParse(FormaPago[2], out decimal montopendiente);
                        var fechaPago = FormaPago[3].Trim();
                        var pagoCuotas = FormaPago[4].Trim();

                        if (lCabecera[3] == "13" && FormaPago[1].Trim().ToUpper() != "CREDITO")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                          "519",
                                          "CUANDO FORMA DE PAGO ES PARA UNA NOTA DE CREDITO DE TIPO DE OPERACION 13, SOLO SE ACEPTA FORMA DE PAGO CREDITO.",
                                          txt));
                        }
                        if (FormaPago[1].Trim().ToUpper() == "CREDITO")
                        {
                            if (cuotas.Count == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                                      "527",
                                                                      "PARA NC DE TIPO OPERACION 13 DEBE INCLUIR LINEA CUOTAS.",
                                                                      txt));
                            }

                        }

                        if (lCabecera[3] == "13" && descripcion != "CREDITO")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "570",
                                  "CUANDO FORMA DE PAGO ES PARA UNA NOTA DE CREDITO DE TIPO DE OPERACION 13, SOLO SE ACEPTA FORMA DE PAGO CREDITO.",
                                  txt));
                        }
                        if (descripcion.Trim().ToUpper() == "CREDITO")
                        {
                            if (cuotas.Count == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                  "527",
                                  "PARA NC DE TIPO OPERACION 13 DEBE INCLUIR LINEA CUOTAS.",
                                  txt));
                            }

                            if (montopendiente == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                       "588",
                                       "SI LA FORMA DE PAGO ES CREDITO Y DE TIPO 13, DEBE ENVIAR EL MONTO PENDIENTE DE PAGO EN LA NC LINEA FORMAPAGO",
                                       txt));
                            }

                            if (cuotas.Any())
                            {
                                foreach (var c in cuotas)
                                {
                                    var montocuota = c.Split('|');
                                    decimal.TryParse(montocuota[2], out decimal monto);
                                    if (monto == 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                         "587",
                                         "SI LA FORMA DE PAGO ES CREDITO Y DE TIPO 13, DEBE ENVIAR EL MONTO DE PAGO CUOTA EN LA NC LINEA CUOTAS",
                                         txt));
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "556", "FALTA LINEA FORMA DE PAGO.", txt));
                    }
                }
                if (!string.IsNullOrEmpty(formaPago))
                {
                    if (lCabecera[3].Trim() != "13")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "565",
                                      "SOLO SE ENVIA FORMA DE PAGO A NC DE TIPO OPERACION 13.",
                                      txt));
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
                            if (parts[1] != "0")
                            {
                                if (parts.Length < 4 || parts.Take(4).Any(x => string.IsNullOrWhiteSpace(x)))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(
                                        lCabecera,
                                        "566",
                                        "LINEA CLIENTE INCOMPLETA. DEBE ENVIARSE AL MENOS HASTA EL CAMPO 4.",
                                        txt
                                    ));
                                }
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
                            if (!UTilidades.TipoDocumentoIdentidadExtra(parts[1].Trim()))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                      "539",
                                      "EL TIPO DE DOCUMENTO(CAMPO 2) DE LA LINEA CLIENTE NO SE ENCUENTRA EN EL CATÁLOGO 06 DE SUNAT.",
                                      txt));
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
                await LogAsync("Validar07", ex);
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
