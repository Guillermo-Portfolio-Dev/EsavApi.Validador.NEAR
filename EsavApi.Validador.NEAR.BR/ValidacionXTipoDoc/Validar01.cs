using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.Factura;
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
    public class Validar01 : brGenerico
    {
        public static async Task<List<beRechazo>> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();
            try
            {
                var configuracion = await new brConfiguracion().Consultar(lCabecera[4].ToString(), lCabecera[5].ToString());
                var rucCache = new Dictionary<string, int>();
                bool tieneDetalle = false;

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
                var detalleFP = lineas.Where(x => x.ToUpper().StartsWith("FORMAPAGO")).ToList();
                var anticipo = lineas.Where(x => x.ToUpper().StartsWith("ANTICIPO")).ToList();
                var otrosTributos = lineas.Where(x => x.ToUpper().StartsWith("OTROSTRIBUTOS")).ToList();
                var otrosCargos = lineas.Where(x => x.ToUpper().StartsWith("OTROSCARGOS")).ToList();
                var descuentoGlobal = lineas.Where(x => x.ToUpper().StartsWith("DESCUENTO")).ToList();
                var formaPago = lineas.Where(x => x.ToUpper().StartsWith("FORMAPAGO")).FirstOrDefault();
                var cuotas = lineas.Where(x => x.ToUpper().StartsWith("CUOTAS")).ToList();
                var paqueteTuristico = lineas.Where(x => x.Trim().ToUpper().StartsWith("D|")).ToList();
                var detalleAdicional = lineas.Where(x => x.Trim().ToUpper().StartsWith("ITEMSP")).ToList();

                decimal tolerancia = 0.60M;
                decimal tipoCambio = 1;
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
                        "DOCASOCIADO",
                        "DETDETRACCION",
                        "D|"
                    };

                    if (prefijosConocidos.Any(pref => upper.StartsWith(pref)))
                        return false;

                    return true;

                }).ToList();

                var columnas = ITEMDESCUENTO.Any()
                                                        ? Enumerable.Range(0, ITEMDESCUENTO.Max(x => x.Split('|').Length))
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
                decimal.TryParse(lCabecera[10], out var DESCUENTO);
                decimal.TryParse(lCabecera[10], out var PDESCUENTO);
                var ICBPER = 0M;
                if (lCabecera.Length > 49)
                {
                    decimal.TryParse(lCabecera[49], out ICBPER);
                }
                var OTTOTAL = decimal.TryParse(lCabecera[15], out var temp) ? temp :
              (decimal.TryParse(lCabecera[50], out temp) ? temp : 0);


                if (lineasDesconocidas.Count > 0)
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "655", "LINEA DESCONOCIDA O INCORRECTA EN EL SU ARCHIVO TXT VERIFICAR.", txt));
                }
                if (lCabecera[0] != "210")
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "344", "VERSION DE TXT NO EXISTE, CONFIGURAR VERSION.", txt));
                }
                if (lCabecera[0] == "210")
                {
                    string numeroDocumento = lCabecera[22].TrimStart('0');
                    if (string.IsNullOrWhiteSpace(numeroDocumento) || numeroDocumento == "0" || numeroDocumento.Length > 8)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "306", "NUMERO DE DOCUMENTO NO SE PERMITE ESPACIOS EN BLANCO O CERO, MAXIMO 8 CARACTERES.", txt));
                    }
                    var indicadorDetraccion = lCabecera[28].Trim() == "" ? "0" : lCabecera[28].Trim();
                    if (lCabecera[3].Trim() == "1001" && indicadorDetraccion == "1" && detraccion.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "612", "SI EL TIPO DE OPERACION ES 1001 Y EL CAMPO 34 TIENE EL VALOR 1, SE DEBE ENVIAR LA LINEA DETRACCION.", txt));
                    }
                    if (lCabecera[3].Trim() == "1001" && indicadorDetraccion == "0"
                        && !new brArchivo().ObtenerEmpresaSinIndicador(lCabecera[4].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "633", "SI EL TIPO DE OPERACION ES 1001, EL CAMPO 29 DEBE TENER VALOR 1.", txt));
                    }

                    var valor = lCabecera[33]?.Trim();
                    decimal numero;
                    var indicadorAnticipo = string.IsNullOrEmpty(valor) ||
                                            (!decimal.TryParse(valor, out numero) ? false : numero == 0)
                                            ? "0" : valor;
                    //if (lCabecera[3].Trim() == "0200" && lCabecera[28].Trim() == "1" && detraccion.Count <= 0)
                    //{
                    //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "624", "NO SE ADMITE OTROS TRIBUTOS NI OTROS TRIBUTOS GLOBALES CUANDO EL TIPO DE OPERACIOM ES 0200.", txt));
                    //}



                    if ((lCabecera[3].Trim() != "1001" && lCabecera[3].Trim() != "1004") && detraccion.Count > 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "573", "CUANDO SE ENVIA LINEA DETRACCION EL TIPO DE OPERACION SOLO PUEDE SER 1001 O 1004.", txt));
                    }

                    if (lCabecera[3].Trim() == "1004")
                    {
                        var codigodetraccion = detraccion.FirstOrDefault()?.Split('|').ElementAtOrDefault(1)?.Trim();

                        if (string.IsNullOrEmpty(codigodetraccion) || codigodetraccion != "027" || Detdetraccion.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "576",
                                "Si EL TIPO DE OPERACIÓN ES 1004, SE DEBE ENVIAR LÍNEA ADICIONAL DETDETRACCION (SOLO CÓDIGO 027).",
                                txt
                            ));
                        }
                    }
                    if (lCabecera[3].Trim() == "1001")
                    {
                        var codigodetraccion = detraccion.FirstOrDefault()?.Split('|')[1].Trim();

                        if (string.IsNullOrEmpty(codigodetraccion) || codigodetraccion == "027")
                        {
                            if (Detdetraccion.Count == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "561",
                                    "Si EL TIPO DE OPERACIÓN ES 1001 Y EL CÓDIGO DE DETRACCIÓN ES 027, SE DEBE ENVIAR LÍNEA ADICIONAL DETDETRACCION.",
                                    txt
                                ));
                            }
                        }
                    }

                    //indicadorAnticipo = lCabecera[33].Trim() == "" ? "0" : lCabecera[33].Trim();
                    tipoCambio = lCabecera[8].Trim() == "USD" ?
                       new brConsultar().ObtenerTipoCambio(Convert.ToDateTime(lCabecera[6]).ToString("yyyy-MM-dd"), lCabecera[8].Trim()) : 1;

                    if (detraccion.Count > 0)
                    {
                        var codigoDetraccion = detraccion.FirstOrDefault().Split('|')[1];

                        if (lCabecera[3].Trim() == "1004" && codigoDetraccion == "027" && (IMPORTETOTAL * tipoCambio) < 400 && indicadorAnticipo == "0")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "577", "SI SE ENVIA TIPO OPERACION 1004, ENTONCES EL IMPORTE TOTAL DEBE SER MAYOR O IGUAL A 400.", txt));
                        }
                        if (lCabecera[3].Trim() == "1001" && codigoDetraccion != "027" && (IMPORTETOTAL * tipoCambio) < 700 && indicadorAnticipo == "0")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "578", "SI SE ENVIA TIPO OPERACION 1001, ENTONCES EL IMPORTE TOTAL DEBE SER MAYOR O IGUAL A 700.", txt));
                        }

                        if (codigoDetraccion != "027" && Detdetraccion.Count > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "596",
                                    "LA LINEA DETDETRACCION SOLO SE ENVIA SI EL CODIGO DE DETRACCION ES 027.",
                                    txt));
                        }

                        if (codigoDetraccion == "027" && Detdetraccion.Count > 0)
                        {
                            bool todosEstan = detalleLines
                                             .All(item => Detdetraccion.Any(d => d.Split('|')[1].Trim() == item[0].ToString()));
                            if (!todosEstan)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "597",
                                    "CUANDO SEA UNA DETRACCION 027, SE DEBE ENVIAR LINEA DETDETRACCION REFERENCIADO PARA CADA ITEM.",
                                    txt));
                            }
                        }
                    }

                    if (UTilidades.TipoOperacionExportacion(lCabecera[3].Trim()) && otrosCargos.Count > 0)
                    {
                        if (otrosCargos.FirstOrDefault().Split('|')[3] != "46" && otrosCargos.FirstOrDefault().Split('|')[3] != "50")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "636", "SI UNA BOLETA DE EXPORTACIÖN TIENE OTROS CARGOS GLOBALES ENTONCES EL CÓDIGO DE MOTIVO SOLO PUEDE SER 46 o 50.", txt));
                        }
                    }

                    if (ITEMDESCUENTO.Any() && columnas.Count > 3)
                    {
                        if (UTilidades.TipoOperacionExportacion(lCabecera[3].Trim()))
                        {
                            if (ITEMDESCUENTO.Count > 0 && columnas[3][0] != "01")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "626", "PARA EXPORTACIÓN CON DESCUENTO POR DETALLE SOLO SE ADMITE EL CÓDIGO 01 EN CÓDIGO DESCUENTO.", txt));
                            }
                        }

                        if (columnas[3][0] != "01")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "626", "LINEA DE DESCUENTO POR ITEM SOLO PERMITE EL CÓDIGO 01.", txt));
                        }
                    }

                    if (TipoDocNombreTxt != "01")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "324", "ERROR EN EL TIPO DE DOCUMENTO DEL NOMBRE DEL TXT.", txt));
                    }

                    if (lCabecera[8].Trim() != "PEN" && lCabecera[8].Trim() != "USD")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "329", "TIPO DE MONEDA NO PERMITIDO, SOLO SE ACEPTA PEN, USD", txt));
                    }
                    if (lCabecera[2] != TipoDocNombreTxt)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "5", "TIPO DE DOCUMENTO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL TIPO DE DOCUMENTO EN LA CABECERA.", txt));
                    }
                    if (!lCabecera[21].StartsWith("F") && lCabecera[2] == "01")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "13", "ERROR EN SERIE, NO CORRESPONDE AL TIPO DE DOCUMENTO.", txt));
                    }

                    //else
                    //{
                    //    if (Math.Abs((BASEIMPONIBLE + IGV + ISCTOTAL + OCTOTAL + OTTOTAL - DESCUENTO) - IMPORTETOTAL) > tolerancia)
                    //    {
                    //        lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                    //    }
                    //}
                    //if (OCTOTAL > 0)
                    //{
                    //    if (IMPORTETOTAL < ((BASEIMPONIBLE - ) + OCTOTAL))
                    //    {
                    //        lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                    //    }
                    //}
                    if (!int.TryParse(lCabecera[22], out int correlativo))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "21", "ERROR EN CORRELATIVO NO ES NUMERICO.", txt));
                    }
                    if (lCabecera[2] == "03" && (lCabecera[18].ToUpper() != "NA" || lCabecera[17].Trim() != "0"))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "32", "ERROR ESTE DOCUMENTO NO PUEDE TENER DOCUMENTO RELACIONADO.", txt));
                    }
                    if (lCabecera[2] == "03" && (lCabecera[18].ToUpper() != "NA" || lCabecera[17].Trim() != "0"))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "33", "ERROR TIPO DE DOCUMENTO RELACIONADO DEBE SER NA PARA ESTE TIPO DE DOCUMENTO.", txt));
                    }
                    if (indicadorAnticipo != "0" && indicadorAnticipo != "1")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "444", "IDENTIFICADOR DE ANTICIPO ERRONEO.", txt));
                    }
                    if (lCabecera[32].Trim() == "1" && otrosCargos.Count == 0 && ITEMOTROSCARGOS.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "465", "FALTA LINEA EXTRA DE OTROS CARGOS GLOBALES.", txt));
                    }
                    if (decimal.TryParse(lCabecera[31].Trim(), out decimal indicadordescuentoGlobal) && indicadordescuentoGlobal > 0 && descuentoGlobal.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "481", "FALTA LINEA DESCUENTO GLOBAL.", txt));
                    }
                    //if (decimal.TryParse(lCabecera[50].Trim(), out decimal otrosTributoGlobal) && otrosTributoGlobal > 0 && otrosTributos.Count == 0)
                    //{
                    //    lRechazos.AddRange(AgregarRechazo(lCabecera, "481", "FALTA LINEA DESCUENTO GLOBAL.", txt));
                    //}

                    if (lCabecera[3].Trim() != "0101" && anticipo.Count == 0)
                    {
                        if (!decimal.TryParse(lCabecera[16], out decimal importe) || importe == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "104", "IMPORTE TOTAL NO PUEDE SER 0000000000 O NO NUMERICO.", txt));
                        }
                    }

                    if (lCabecera.Length >= 49)
                    {
                        for (int i = 34; i < 49; i++)
                        {
                            string campo = lCabecera[i].Trim();

                            if (!string.IsNullOrEmpty(campo) && !Regex.IsMatch(campo, @"^[^:]+:\s?.*$"))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "412", $"CAMPO {i + 1} MAL ESTRUCTURADO.", txt));
                            }
                        }
                    }


                    DateTime FEmision;

                    string[] formatos = {
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yy HH:mm:ss"
                    };

                    string[] formatosV = {
                        "dd/MM/yyyy",
                    };

                    if (!DateTime.TryParseExact(lCabecera[7], formatosV, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime FVencimiento))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "623", "FECHA DE VENCIMIENTO INVÁLIDA O VACÍA.", txt));
                    }

                    // Validación de la fecha de emisión
                    if (string.IsNullOrWhiteSpace(lCabecera[6]) || !DateTime.TryParseExact(lCabecera[6], formatos, null, System.Globalization.DateTimeStyles.None, out FEmision))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "609", "FECHA DE EMISIÓN INVÁLIDA O VACÍA.", txt));
                    }

                    if (!string.IsNullOrWhiteSpace(lCabecera[6]) && DateTime.TryParseExact(lCabecera[6], formatos, null, System.Globalization.DateTimeStyles.None, out FEmision))
                    {

                        int comparacion = DateTime.Compare(FVencimiento.Date, FEmision.Date);
                        if (comparacion < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "491", "LA FECHA DE VENCIMIENTO NO PUEDE SER MENOR A LA FECHA DE EMISION.", txt));
                        }

                        if (FEmision.Date > DateTime.Now.Date)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "102", $"FECHA DE EMISION NO PUEDE SER MAYOR AL DIA DE HOY {FEmision}.", txt));
                        }

                        if ((DateTime.Now.Date - FEmision.Date).TotalDays > 3 && UTilidades.Documento3Dias(lCabecera[2]))
                        {
                            var ClienteDentroFecha = new brArchivo().ObtenerEmpresa(lCabecera[4].Trim(), FEmision.Date, lCabecera[2].Trim());
                            if (!ClienteDentroFecha)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "101", "FA, NC O ND DE FACTURA FECHA DE EMISION ES MAYOR A 3 DIAS, BOLETA FECHA DE EMISION ES MAYOR A 5 DIAS,NC O ND DE BOLETA FECHA DE EMISION ES MAYOR A 5 DIAS.", txt));
                            }
                        }
                    }

                    var indicadorDescuento = string.IsNullOrEmpty(lCabecera[31].Trim()) ? "0" : lCabecera[31].Trim();
                    if (descuentoGlobal.Count > 0 && lCabecera[31].Trim() != indicadorDescuento)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "443", "LINEA DE DESCUENTO GLOBAL NO SE PUEDE LEER, VERIFICAR EL CAMPO DESCUENTO GLOBAL DE LA CABECERA.", txt));
                    }

                    var ExisteRuc = new brConsultar().ConsultarRuc(lCabecera[4].Trim());
                    if (ExisteRuc == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "532", "EL RUC EMISOR NO ES VALIDO.", txt));
                    }
                    if (ExisteRuc == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "39", "ERROR EN EL RUC INTERNO DEL EMISOR.", txt));
                    }
                    string lineaCliente = Array.Find(lineas, x => x.ToUpper().Contains("CLIENTE"));
                    if (lineaCliente == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "535", "DEBE HABER LINEA CLIENTE EN LA FACTURA.", txt));
                    }
                    string lineaItem = Array.Find(lineas, x => x.ToUpper().Contains("BIEN") || x.ToUpper().Contains("SERVICIO"));
                    if (lineaItem == null)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "534", "DEBE HABER AL MENOS 1 DETALLE EN LA FACTURA.", txt));
                    }

                    var DataEmisor = new brConsultar().ConsultarDataEmisor(
                                    lCabecera[4].Trim(), lCabecera[21].Trim(), lCabecera[23].Trim(), lCabecera[5].Trim(), lCabecera[2].Trim());

                    if (!UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "340", "ERROR TIPO DE DOCUMENTO EN LA CABECERA, NO ACEPTADO.", txt));
                    }
                    if (DataEmisor.emisor == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "301", "EMISOR NO EXISTE.", txt));
                    }
                    if (DataEmisor.serie == 0 || DataEmisor.serieUsuario == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "335", "SERIE NO EXISTE.", txt));
                    }
                    if (DataEmisor.serieUsuario == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "85", "USUARIO NO CONFIGURADO PARA ESTA SERIE.", txt));
                    }
                    if (DataEmisor.usuario == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "303", "USUARIO NO ESTA CONFIGURADO PARA ESTE EMISOR.", txt));
                    }
                    if (DataEmisor.sucursal == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "337", "SUCURSAL NO SE ENCUENTRA REGISTRADA.", txt));
                    }
                    if (DataEmisor.sucursalUsuario == 0 && DataEmisor.sucursal == 1 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "338", "USUARIO NO CONFIGURADO PARA ESTA SUCURSAL.", txt));
                    }

                    if (!UTilidades.TipoOperacion(lCabecera[3]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "408", "TIPO DE OPERACION INVALIDO.", txt));
                    }
                    if (!UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "371", "TIPO DE DOCUMENTO INCLUIDO NO VALIDO, REVISAR TABLA TIPO DE DOCUMENTO INCLUIDO.", txt));
                    }
                    if (UTilidades.TipoOperacionExportacion(lCabecera[3].Trim()) && lCabecera[33].Trim() == "1")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "601", "NO HAY ANTICIPOS PARA EXPORTACION, ANTICIPOS ES PARA VENTA INTERNA GRAVADO, INAFECTO Y EXONERADO.", txt));
                    }
                    //if (UTilidades.TipoOperacionDetraccion(lCabecera[3].Trim()))
                    //{
                    //    lRechazos.AddRange(AgregarRechazo(lCabecera, "602", "LAS BOLETAS NO PUEDEN TENER DETRACCION.", txt));
                    //}
                }
                #endregion


                // Validaciones de detalle
                decimal SumDetalleIGV = 0;
                decimal SumDetalleIGVSinIna = 0;
                decimal SumDetalleTotal = 0;
                decimal SumDetalleISC = 0;
                decimal SumDetalleBI = 0;
                decimal sumDetalleSubTotal = 0;
                decimal SumDetalleBITotal = 0;
                decimal SumDetalleBISinDescuento = 0;
                decimal SumDetalleOT = 0;
                decimal SumDetalleDESC = 0;
                decimal SumDetaleICBPER = 0;
                decimal SumDetalleOC = 0;
                decimal SumDetalleBIconDescGlobal = 0;
                decimal SumDetalleGratuitos = 0;
                decimal SumDetalleIGVInafectos = 0;
                decimal SumDetalleIGVAfectos = 0;

                int cantidadICBPER = 0;
                int detallesProcesados = 0;


                decimal SummontoAnticipado = 0;

                foreach (var line in detalleLines)
                {
                    var parts = line.Split('|');
                    #region DETALLE             


                    if (parts[1].ToUpper().Trim() == "BIEN" || parts[1].ToUpper().Trim() == "SERVICIO")
                    {

                        if (parts[1].ToUpper().Trim() == "BIEN")
                        {
                            var tipoOperacionFactura = UTilidades.TipoOperacionFacturaServicio(lCabecera[3].Trim());
                            if (tipoOperacionFactura)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "559", "LAS LINEAS DE DETALLE SOLO PUEDEN SER SERVICIOS SI EL TIPO DE OPERACIÓN ES 0201, 0202, 0205 o 0208.", txt));
                            }
                        }

                        if (parts.Length < 20)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "421", $"CANTIDAD DE CAMPOS INCORRECTO EN EL DETALLE. LINEA - {parts[0]}", txt));
                            return lRechazos;
                        }

                        if (descuentoGlobal.Count > 0)
                        {
                            if (descuentoGlobal.FirstOrDefault().Split('|')[3] == "02" && !UTilidades.EsCodigoGravado(parts[2]))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "622", $"DESCUENTO GLOBAL QUE AFECTA A LA BASE IMPONIBLE SOLO PUEDE IR CON DETALLES GRAVADO. LINEA {parts[0]}", txt));
                            }
                        }

                        var tipoAfectacionImporte0 = UTilidades.TipoAfectacionImporte0(parts[2]);
                        if (!tipoAfectacionImporte0)
                        {
                            if (!decimal.TryParse(lCabecera[16], out decimal importe) || importe == 0 && anticipo.Count == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "104", $"IMPORTE TOTAL NO PUEDE SER 0000000000 O NO NUMERICO. LINEA {parts[0]} ", txt));
                            }
                        }
                        var exportaciones = UTilidades.TipoOperacionExportacion(lCabecera[3].Trim());
                        if (exportaciones && parts[2].Trim() != "40")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "604", $"SI ES EXPORTACIÓN LA AFECTACIÓN IGV SOLO PUEDE SER 40, LINEA {parts[0]}", txt));
                        }
                        if (decimal.TryParse(parts[19], out decimal importeVenta) && importeVenta <= 0 && !UTilidades.EsCodigoGratuito(parts[2]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "312", "IMPORTE DE VENTA DEBE SER MAYOR A CERO SI ES DIFERENTE DE GRATUITO.", txt));
                        }
                        if (lCabecera[3].Trim() == "0200" && (OTTOTAL > 0 || SumDetalleOT > 0))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "637", "NO SE ADMITE OTROS TRIBUTOS NI OTROS TRIBUTOS GLOBALES CUANDO EL TIPO DE OPERACION ES 0200.", txt));
                        }

                        if (decimal.TryParse(parts[7], out decimal cantidad) && cantidad <= 0.001m)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "313", "CANTIDAD DEBE SER MAYOR A CERO, VALOR MÍNIMO 0.001 (SIN REDONDEAR).", txt));
                        }

                        if (parts[1].ToUpper().Trim() == "SERVICIO" && parts[3].ToUpper().Trim() != "ZZ")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "574", "EN CASO EL DETALLE SEA SERVICIO SE TIENE QUE ENVIAR CON UNIDAD DE MEDIDA COMO ZZ INDICANDO QUE ES SERVICIO.", txt));
                        }

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


                        if (desc > 0)
                        {
                            if (pctdesc < 0 || pctdesc > 1)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "645", $"EL PORCENTAJE DESCUENTO DEL DETALLE SOLO PUEDE TENER VALOR DE 0 A 1. LINEA:{line[0]}", txt));
                            }

                            decimal montoDescCalculado = (vu * cant) * pctdesc;
                            if (Math.Abs(montoDescCalculado - desc) > 0.1m)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "648", $"EL MONTO DESCUENTO DEL DETALLE MAL CALCULADO. LINEA:{parts[0]}", txt));
                            }
                        }
                        if (isc > 0)
                        {
                            if (pctisc < 0 || pctisc > 1)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "644", $"EL PORCENTAJE DE ISC DEL DETALLE SOLO PUEDE TENER VALOR DE 0 A 1. LINEA:{line[0]}", txt));
                            }
                        }
                        if (ot > 0)
                        {
                            if (pctot < 0 || pctot > 10)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "646", $"EL PORCENTAJE DE OT DEL DETALLE SOLO PUEDE TENER VALOR DE 0 A 10. LINEA:{line[0]}", txt));
                            }
                        }
                        if (oc > 0)
                        {
                            if (pctoc < 0 || pctoc > 1)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "646", $"EL PORCENTAJE DE OC DEL DETALLE SOLO PUEDE TENER VALOR DE 0 A 1. LINEA:{line[0]}", txt));
                            }
                        }

                        if (Math.Abs((bi * pctot) - ot) > tolerancia)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "642", "PORCENTAJE DE OTROS TRIBUTOS POR DETALLE MAL CALCULADO.", txt));
                        }


                        if (decimal.TryParse(parts[8], out decimal valorUnitario) && valorUnitario <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "311", "VALOR UNITARIO DEBE SER MAYOR A CERO.", txt));
                        }
                        if (bi <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "393", "LA BASE IMPONIBLE DEL DETALLE, DEBE SER MAYOR A CERO, VALOR MINIMO 0.01 (SIN REDONDEAR).", txt));
                        }
                        //if (ot > 0)
                        //{
                        //    lRechazos.AddRange(AgregarRechazo(lCabecera, "610", $"NO SE DEBE ENVIAR OTROS TRIBUTOS A NIVEL DE LOS DETALLES LINEA. {parts[0]}", txt));
                        //}

                        if (igv <= 0 && UTilidades.EsCodigoGravado(parts[2]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "330", "IGV DEBE SER MAYOR A CERO CUANDO SEA UNA OPERACIÓN GRAVADA.", txt));
                        }

                        if (!UTilidades.TipoAfectacion(parts[2]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "331", "TIPO DE OPERACION NO ACEPTADA, SOLO ACEPTA GRAVADO, INAFECTO, EXONERADO O GRATUITO.", txt));
                        }

                        if (!UTilidades.EsGR_EXO_EXP_INA(parts[2]))
                        {
                            if (igv != 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "333", "IGV DEBE SER CERO CUANDO SEA UNA OPERACIÓN INAFECTO, EXONERADA, EXPORTACIÓN O GRATUITO.", txt));
                            }
                        }
                        if (igv <= 0 && UTilidades.EsGravadoGratuito(parts[2]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "377", "EL IGV DEBE SER MAYOR A CERO CUANDO SEA UNA OPERACION GRATUITO APLICADA A BIENES.", txt));
                        }

                        if (ot > 0)
                        {
                            if (Math.Abs((ot / bi) - pctot) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "477", "ERROR EN EL CÁLCULO DE OTROS TRIBUTOS EN EL DETALLE.", txt));
                            }
                        }

                        if (exportaciones && (igv > 0 || isc > 0))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "618", "EL TIPO DE OPERACIÓN DE EXPORTACIÓN NO SE VE AFECTADO POR EL PAGO DE TRIBUTOS INCLUYENDO IGV E ISC.", txt));
                        }

                        if (exportaciones && (desc > 0 || pctdesc > 0) && descuentoGlobal.Count > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "608", $"CUANDO ES UNA EXPORTACION NO DEBE PERMITIR DESCUENTO GLOBAL QUE AFECTA A LA BASE(02) Y DESCUENTO POR ITEM QUE AFECTA A LA BASE (03). LINEA {parts[0]}", txt));
                        }
                        if (Math.Abs((vu * cant) - desc - bi) > tolerancia && ITEMDESCUENTO.Count == 0 && ITEMOTROSCARGOS.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "386", "VALOR UNITARIO POR CANTIDAD, MENOS DESCUENTO NO ES IGUAL A LA BASE IMPONIBLE.", txt));
                        }

                        if (Math.Abs((vu * cant) - desc - bi) > 0.50M && ITEMDESCUENTO.Count == 0 && ITEMOTROSCARGOS.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "403", $"ERROR EN EL CÁLCULO DE LA BASE IMPONIBLE EN EL DETALLE. LINEA {parts[0]}", txt));
                        }
                        if (ITEMDESCUENTO.Count > 0)
                        {
                            foreach (var item in ITEMDESCUENTO)
                            {
                                if (item.Split('|')[1].ToUpper().Trim() == "DESCUENTO")
                                {
                                    if (item.Split('|')[3].ToUpper().Trim() == "01")
                                    {
                                        if (Math.Abs((vu * cant) - bi) > 0.50M)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "386", "VALOR UNITARIO POR CANTIDAD, MENOS DESCUENTO NO ES IGUAL A LA BASE IMPONIBLE.", txt));
                                        }
                                        if (Math.Abs((vu * cant) - bi) > 0.50M)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "403", $"ERROR EN EL CÁLCULO DE LA BASE IMPONIBLE EN EL DETALLE. LINEA {parts[0]}", txt));
                                        }
                                    }
                                }
                            }

                        }
                        if (importeTotal != 0 && (UTilidades.EsCodigoGratuito(parts[2]) && UTilidades.TipoOperacionExportacion(lCabecera[3])))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "378", "EL IMPORTE TOTAL DEBE SER CERO CUANDO SEA TRANSFERENCIA GRATUITA.", txt));
                        }

                        decimal.TryParse(parts[7], out decimal valor7);
                        decimal.TryParse(parts[8], out decimal valor8);
                        decimal.TryParse(parts[9], out decimal valor9);
                        decimal.TryParse(parts[10], out decimal valor10);
                        decimal.TryParse(parts[11], out decimal valor11);
                        decimal.TryParse(parts[12], out decimal valor12);
                        decimal.TryParse(parts[13], out decimal valor13);
                        decimal.TryParse(parts[14], out decimal valor14);
                        decimal.TryParse(parts[15], out decimal valor15);
                        decimal.TryParse(parts[16], out decimal valor16);
                        decimal.TryParse(parts[17], out decimal valor17);
                        decimal.TryParse(parts[18], out decimal valor18);
                        decimal.TryParse(parts[19], out decimal valor19);

                        if (valor7 < 0 || valor8 < 0 || valor9 < 0 || valor10 < 0
                            || valor11 < 0 || valor12 < 0 || valor13 < 0 || valor14 < 0
                            || valor15 < 0 || valor15 < 0 || valor16 < 0 || valor17 < 0
                            || valor18 < 0 || valor19 < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "383", "NO SE PERMITEN EL INGRESO DE VALORES NEGATIVOS.", txt));
                        }

                        var exportacion = UTilidades.TipoOperacionExportacion(lCabecera[3]);
                        if (!exportacion)
                        {
                            if (desc > 0 || pctdesc > 0)
                            {
                                if (Math.Abs(((vu * cant) + igv - importeTotal) - desc) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "426", "MONTO DE DESCUENTO EN EL DETALLE, MAL CALCULADO.", txt));
                                }
                            }
                        }


                        if (string.IsNullOrWhiteSpace(parts[6]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "407", $"ERROR, DETALLE SIN DESCRIPCION. LINEA {parts[0]}", txt));
                        }

                        var OG = UTilidades.EsCodigoGratuito(parts[2]);
                        var INAF = UTilidades.Inafecto(parts[2]);
                        var EXO = UTilidades.Exonerado(parts[2]);
                        decimal suma = bi + igv + isc + oc + ot - desc;
                        decimal bi_ = 0;
                        decimal igv_ = 0;
                        if (columnas.Count > 3)
                        {
                            if (columnas[3][0] == "01")
                            {
                                bi_ = cant * vu;
                                igv_ = INAF == false && EXO == false && parts[2].Trim() != "40" ? ((bi_ + (ITEMOTROSCARGOS.Count > 0 ? oc : 0)) * configuracion.CSuc_PorcentajeIGV) : 0;
                                var sumaImportetotal = bi_ + igv_;
                                if (!OG && Math.Abs((sumaImportetotal - ((bi_ + igv_ + oc) * pctdesc)) - importeTotal) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "404", $"ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN EL DETALLE. LINEA {parts[0]}", txt));
                                }
                            }
                        }
                        else
                        {
                            bi_ = (cant * vu) - desc;
                            igv_ = INAF == false && EXO == false && parts[2].Trim() != "40" ?
                                ((bi_ + (ITEMOTROSCARGOS.Count > 0 ? oc : 0)) * configuracion.CSuc_PorcentajeIGV) : 0;

                            if (isc > 0)
                            {
                                igv_ = (bi_ + isc) * configuracion.CSuc_PorcentajeIGV;
                                if (!OG && Math.Abs(((bi_ + isc) + igv_ + oc + ot) - importeTotal) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "404", $"ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN EL DETALLE. LINEA {parts[0]}", txt));
                                }
                            }
                            else
                            {
                                if (!OG && Math.Abs((bi_ + igv_ + oc + ot) - importeTotal) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "404", $"ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN EL DETALLE. LINEA {parts[0]}", txt));
                                }

                            }

                        }



                        var OGG = UTilidades.EsCodigoGratuitoGravado(parts[2]);
                        if (!OGG && (igv == 0 && isc == 0 && importeTotal == 0 && bi == 0) && vu > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "336", $"EL IMPORTE DE UNA OPERACION GRATUITA DEBE SER 0. Linea {parts[0]}", txt));
                        }

                        if (OG && parts[1].ToUpper().Trim() == "SERVICIO" && igv < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "381", "EL IGV DEBE SER CERO CUANDO SEA UNA OPERACION GRATUITO APLICADA A SERVICIOS", txt));
                        }

                        if (OG && valor19 > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "422", "EL IMPORTE TOTAL DEBE SER CERO CUANDO SEA GRATUITO", txt));
                        }
                        if (OG && vu < 0.01M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "450", $"VALOR UNITARIO DEBE SER MAYOR A 0.05, CUANDO SEA UNA OPERACION GRATUITA LINEA {parts[0]}", txt));
                        }
                        if (oc > 0)
                        {
                            decimal toleranciaoc = 0.000001M;
                            int decimalCount = BitConverter.GetBytes(decimal.GetBits(valor16)[3])[2];
                            decimal cal = Math.Round(oc / importeTotal, decimalCount);

                            if (Math.Abs(cal - valor16) > toleranciaoc)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "625", $"PORCENTAJE DE OTROS CARGOS POR DETALLE MAL CALCULADO. LINEA {parts[0]}", txt));
                            }
                        }

                        var tipoAfectacion = UTilidades.TipoAfectacion(parts[2]);
                        if (!tipoAfectacion)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "410", $"TIPO DE AFECTACION INVALIDO. LINEA {parts[0]}", txt));
                        }

                        if (exportacion)
                        {
                            string valor = parts[5].Trim();
                            if (string.IsNullOrEmpty(valor) || !int.TryParse(valor, out _))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "528", $"CUANDO ES UNA EXPORTACION DEBE INDICAR COD. PRODUCTO SUNAT DEBE SER NUMERICO. LINEA {parts[0]}", txt));
                            }
                        }

                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(parts[3].Trim());
                        if (string.IsNullOrEmpty(UnidadMedidaText.Item1))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "356", "CODIGO DE UNIDAD DE MEDIDA NO ACEPTADO, REVISAR TABLA UNIDAD DE MEDIDA.", txt));
                        }
                        if (parts[3].Trim() != parts[3].Trim().ToUpper())
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "647",
                                $"CODIGO DE UNIDAD DE MEDIDA DEBE SER EN MAYÚSCULAS campo 4 DEL DETALLE. linea:{parts[0]}", txt));
                        }
                        if (lCabecera[33].Trim() == "1")
                        {
                            bool todasIguales = detalleLines.All(linea => linea.Split('|')[2].Trim() == parts[2].Trim());
                            if (!todasIguales)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "630", "CUANDO ES UN DOCUMENTO POR ANTICIPO LAS LINEAS DE DETALLE DEBEN TENER EL MISMO TIPO DE AFECTACION.", txt));
                            }
                        }

                        if (anticipo.Count > 0)
                        {
                            foreach (var item in anticipo)
                            {
                                var anticipo_ = item.Split('|');
                                decimal.TryParse(anticipo_[8], out decimal ma);
                                var docCliente = clienteLine.Split('|');
                                var anticipos = new brConsultar().FacturaAnticipo_Obtener(lCabecera[4], docCliente[2], docCliente[1], anticipo_[4].Trim(), anticipo_[5]);
                                var inafecto = UTilidades.Inafecto(parts[2]);
                                var gravado = UTilidades.EsCodigoGravado(parts[2]);
                                var exonerado = UTilidades.Exonerado(parts[2]);
                                var exportacion_ = parts[2].Trim() == "40" ? true : false;
                                SummontoAnticipado += ma;

                                if (anticipos != null)
                                {
                                    if (!inafecto && anticipos.Inafecto > 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "631", "LA AFECTACION DEL IGV NO COINCIDE CON EL DEL ANTICIPO.", txt));
                                    }
                                    if (!exonerado && anticipos.Exonerado > 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "631", "LA AFECTACION DEL IGV NO COINCIDE CON EL DEL ANTICIPO.", txt));
                                    }
                                    if (!gravado && anticipos.Gravado > 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "631", "LA AFECTACION DEL IGV NO COINCIDE CON EL DEL ANTICIPO.", txt));
                                    }
                                    if (!exportacion_ && anticipos.Exportacion > 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "631", "LA AFECTACION DEL IGV NO COINCIDE CON EL DEL ANTICIPO.", txt));
                                    }
                                }

                                if (anticipo_[3] != "02")
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "639", "EL CODIGO DEL TIPO DE COMPROBANTE QUE REALIZA ANTICIPO TIENE QUE SER 02.", txt));
                                }

                                bool todasIguales = detalleLines.All(linea => linea.Split('|')[2].Trim() == parts[2].Trim());
                                if (todasIguales)
                                {
                                    bool exo = UTilidades.Exonerado(parts[2].Trim());
                                    bool grav = UTilidades.EsCodigoGravado(parts[2].Trim());
                                    bool ina = UTilidades.Inafecto(parts[2].Trim());
                                    if (exo || grav || ina)
                                    {
                                        var codigoAnticipo = anticipo_[10].Trim() == "" ? "04" : anticipo_[10].Trim();
                                        if (grav && codigoAnticipo != "04")
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "637", "SI LA OPERACION ES GRAVADA DEBE TENER EL CODIGO 04, SI ES EXONERADA CODIGO 05 U SI ES INAFECTO CODIGO 06.", txt));
                                        }
                                        if (exo && codigoAnticipo != "05")
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "637", "SI LA OPERACION ES GRAVADA DEBE TENER EL CODIGO 04, SI ES EXONERADA CODIGO 05 U SI ES INAFECTO CODIGO 06.", txt));
                                        }
                                        if (ina && codigoAnticipo != "06")
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "637", "SI LA OPERACION ES GRAVADA DEBE TENER EL CODIGO 04, SI ES EXONERADA CODIGO 05 U SI ES INAFECTO CODIGO 06.", txt));
                                        }

                                    }
                                }
                            }

                        }

                        if (!string.IsNullOrWhiteSpace(parts[5]))
                        {
                            if (!Regex.IsMatch(parts[5], @"^\d{8}$"))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "406", "CODIGO DE PRODUCTO DE INVENTARIO DE SUNAT, NO EXISTE Y DEBE SER NUMÉRICO Y DE 8 DÍGITOS.", txt));
                            }
                            else
                            {
                                var codigoProductoSunat = new brConsultar().ListarCodigoProductoSunat(parts[5]);

                                if (codigoProductoSunat.Count == 0)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "406", "CODIGO DE PRODUCTO DE INVENTARIO DE SUNAT, NO EXISTE Y DEBE SER NUMÉRICO Y DE 8 DÍGITOS.", txt));
                                }
                            }
                        }

                        if (igv < 0.01M && igv > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "582", $"EL IGV DEL DETALLE NO PUEDE SER MENOR A 0.01. - linea:{line[0]}", txt));
                        }

                        if (parts[6].ToUpper().Trim() == "bolsa plástica")
                        {

                            if (vu + igv != importeTotal)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "467", "MONTO TOTAL DEL ICBPER EN EL DETALLE, MAL CALCULADO.", txt));
                            }

                            if (parts[1].ToUpper().Trim() != "BIEN" && parts[3].ToUpper().Trim() != "NIU")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "470", "EN EL DETALLE BOLSA PLASTICA,  EL CAMPO TIPO DE DETALLE DEBE SER BIEN Y EN EL CAMPO UNIDAD DE MEDIDA DEBE SER NIU.", txt));
                            }
                        }

                        if (descuentoGlobal.Count == 0)
                        {
                            if (!UTilidades.EsCodigoGratuito(parts[2]))
                            {
                                decimal delta = 0.01M;
                                var igvRedondeado = Math.Round(igv, configuracion.CEmi_CantidadDecimalDetalle);
                                var calculo = !UTilidades.Exonerado(parts[2]) && !UTilidades.Inafecto(parts[2]) ?
                                    Math.Round(Math.Abs((bi * configuracion.CSuc_PorcentajeIGV) - igvRedondeado), configuracion.CEmi_CantidadDecimalDetalle)
                                    : 0;

                                if (isc > 0)
                                {
                                    calculo = Math.Round(Math.Abs(((bi + isc) * configuracion.CSuc_PorcentajeIGV) - igv), configuracion.CEmi_CantidadDecimalDetalle);
                                    if (calculo > delta && !UTilidades.TipoOperacionExportacion(lCabecera[3]) && lCabecera[3] != "0101")
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "400", $"ERROR EN EL CÁLCULO DEL IGV EN EL DETALLE. LINEA {parts[0]}", txt));
                                    }
                                }
                                else
                                {
                                    if (calculo > delta && !UTilidades.TipoOperacionExportacion(lCabecera[3]) && lCabecera[3] != "0101")
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "400", $"ERROR EN EL CÁLCULO DEL IGV EN EL DETALLE. LINEA {parts[0]}", txt));
                                    }

                                    //if (bi * configuracion.CSuc_PorcentajeIGV - igv > delta)
                                    //{
                                    //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "400", $"ERROR EN EL CÁLCULO DEL IGV EN EL DETALLE. LINEA {parts[0]}", txt));
                                    //}
                                }

                            }

                        }

                        tieneDetalle = true;
                        var BS = line.Split('|');
                        SumDetalleIGV += !UTilidades.EsCodigoGratuito(BS[2]) ? Convert.ToDecimal(!string.IsNullOrEmpty(BS[12]) ? BS[12] : "0") : 0;
                        SumDetalleIGVSinIna += !UTilidades.Inafecto(BS[2]) ? Convert.ToDecimal(!string.IsNullOrEmpty(BS[12]) ? BS[12] : "0") : 0;
                        SumDetalleTotal += Convert.ToDecimal(!string.IsNullOrEmpty(BS[19]) ? Convert.ToDecimal(BS[19]) : 0);
                        sumDetalleSubTotal += cant * vu;
                        SumDetalleISC += Convert.ToDecimal(!string.IsNullOrEmpty(BS[13]) ? Convert.ToDecimal(BS[13]) : 0);
                        SumDetalleGratuitos += UTilidades.EsCodigoGratuito(BS[2]) ? Convert.ToDecimal(BS[11]) : 0;
                        SumDetalleIGVInafectos += UTilidades.Inafecto(BS[2])
                                    ? Convert.ToDecimal(BS[12] == "" ? "0" : BS[12])
                                    : 0;
                        SumDetalleIGVAfectos += UTilidades.EsCodigoGratuitoGravado(BS[2]) ? Convert.ToDecimal(BS[12] == "" ? "0" : BS[12])
                                    : 0;
                        if (ITEMOTROSCARGOS.Count > 0)
                        {
                            SumDetalleBISinDescuento += (((valor7 * valor8) - desc) + oc);
                        }
                        else
                        {
                            SumDetalleBISinDescuento += (((valor7 * valor8) - desc));
                        }

                        SumDetalleBI += !UTilidades.EsCodigoGratuito(BS[2]) ? Convert.ToDecimal(!string.IsNullOrEmpty(BS[11]) ? BS[11] : "0") : 0;
                        SumDetalleBITotal += Convert.ToDecimal(!string.IsNullOrEmpty(BS[11]) ? BS[11] : "0");
                        SumDetalleOT += Convert.ToDecimal(!string.IsNullOrEmpty(BS[17]) ? Convert.ToDecimal(BS[17]) : 0);
                        SumDetalleOC += Convert.ToDecimal(!string.IsNullOrEmpty(BS[15]) ? Convert.ToDecimal(BS[15]) : 0);
                        SumDetalleDESC += Convert.ToDecimal(!string.IsNullOrEmpty(BS[9]) ? Convert.ToDecimal(BS[9]) : 0);
                        SumDetaleICBPER += parts[6].ToUpper().Trim() == "BOLSA PLASTICA" ? importeTotal : 0;
                        cantidadICBPER += parts[6].ToUpper().Trim() == "bolsa plastica" ? Convert.ToInt32(cant) : 0;
                        SumDetalleBIconDescGlobal += !UTilidades.EsCodigoGratuito(BS[2]) ? (!string.IsNullOrEmpty(BS[11]) ? Convert.ToDecimal(BS[11]) : 0) : 0;


                        detallesProcesados++;

                        if (detallesProcesados < detalleLines.Count)
                        {
                            continue;
                        }

                        decimal otrosTributosCabecera = 0.00M;

                        bool todasSonGratuitas = detalleLines.All(linea =>
                                    UTilidades.EsGravadoGratuito(linea.Split('|')[2].Trim())
                                );

                        if (todasSonGratuitas && (descuentoGlobal.Count > 0 || otrosCargos.Count > 0 || SumDetalleOC > 0 || SumDetalleOT > 0))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "575", "NO SE PUEDE APLICAR DESCUENTO, OTROS CARGOS NI NINGÚN OTRO MODIFICADOR DE LINEA SI EL DETALLE ES GRATUITO.", txt));
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
                        anticipo.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "585", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO COINCIDE CON LA CABECERA.", txt));
                        }

                        decimal subtotal = Math.Round(SUBTOTAL, 2, MidpointRounding.AwayFromZero)
                            - Math.Round(sumDetalleSubTotal, 2, MidpointRounding.AwayFromZero);
                        if ((Math.Abs(subtotal) > 0.1M) &&
                        descuentoGlobal.Count == 0 &&
                        otrosCargos.Count == 0 &&
                        ITEMOTROSCARGOS.Count == 0 &&
                        ITEMDESCUENTO.Count == 0 &&
                        anticipo.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "587", "LA SUMA DEL SUBTOTAL DEL DETALLE NO COINCIDE CON EL SUBTOTAL CABECERA.", txt));
                        }

                        decimal igv_d = Math.Round(SumDetalleIGVSinIna, 2, MidpointRounding.AwayFromZero)
                            - Math.Round(IGV, 2, MidpointRounding.AwayFromZero);
                        if ((Math.Abs(igv_d) > 0.1M) &&
                            descuentoGlobal.Count == 0 &&
                            otrosCargos.Count == 0 &&
                            ITEMOTROSCARGOS.Count == 0 &&
                            ITEMDESCUENTO.Count == 0 &&
                            anticipo.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "586", "LA SUMA DE LOS IGV DEL DETALLE NO CUADRA CON EL IGV DE LA CABECERA.", txt));
                        }

                        if (ITEMOTROSCARGOS.Count > 0)
                        {
                            if (((BASEIMPONIBLE - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + ISCTOTAL + OTTOTAL) - IMPORTETOTAL > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                            }

                            if (ITEMOTROSCARGOS.Any(cadena => cadena.Split('|').Contains("47")) && lCabecera[3].Trim() != "0101")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "640", "NO SE ACEPTA CODIGO OTROS CARGOS 47 A NIVEL DE DETALLE REVISAR LINEA ITEM OTROS CARGOS.", txt));
                            }
                        }

                        if (lCabecera[3].Trim() == "0200" && detalleLines.Any(x => x.Split('|')[1].Trim().ToUpper() == "SERVICIO"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "581", "CUANDO EL TIPO OPERACION ES 0200, LOS DETALLES SOLO DEBEN SER BIENES.", txt));
                        }

                        if (descuentoGlobal.Count > 0)
                        {
                            var desGlobal = descuentoGlobal.FirstOrDefault().Split('|');
                            decimal.TryParse(desGlobal[2], out var montoDesc);
                            if (desGlobal[3].Trim() == "03" || desGlobal[3].Trim() == "05" || desGlobal[3].Trim() == "06")
                            {
                                if (((BASEIMPONIBLE - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + ISCTOTAL + OCTOTAL + OTTOTAL - montoDesc) - IMPORTETOTAL > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                                }
                            }

                        }

                        if (todasSonGratuitas && ISCTOTAL > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "564", "LOS DETALLES DEL TXT NO PUEDEN TENER ISC CUANDO TODOS LOS DETALLES SON GRATUITOS.", txt));
                        }

                        if (OCTOTAL > 0)
                        {
                            if (SumDetalleOC > 0)
                            {
                                //if (IMPORTETOTAL < ((BASEIMPONIBLE - SumDetalleBI) + SumDetalleIGVAfectos + SumDetalleOC))
                                //if (IMPORTETOTAL < ((BASEIMPONIBLE - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + SumDetalleOC))
                                //{
                                //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                                //}
                                if (IMPORTETOTAL < (BASEIMPONIBLE + OCTOTAL))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                                }
                            }
                            else
                            {
                                if (otrosCargos.Count > 0)
                                {
                                    var otrosC = otrosCargos.FirstOrDefault().Split('|');
                                    var codigo = otrosC[3].Trim();

                                    if (codigo == "49")
                                    {
                                        if (IMPORTETOTAL < ((BASEIMPONIBLE - SumDetalleGratuitos) + OCTOTAL))
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                                        }
                                    }
                                    else
                                    {
                                        decimal baseImponible = Math.Round(BASEIMPONIBLE - SumDetalleGratuitos, 2);
                                        decimal _igv = Math.Round(IGV - SumDetalleIGVAfectos, 2);
                                        decimal _otrosCargos = Math.Round(OCTOTAL, 2);
                                        decimal totalCalculado = baseImponible + _igv + _otrosCargos;

                                        if (Math.Round(IMPORTETOTAL, 2) + 0.01M < Math.Round(totalCalculado, 2))
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                                        }
                                    }
                                }
                                else
                                {
                                    if (IMPORTETOTAL < ((BASEIMPONIBLE - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + OCTOTAL))
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "EL IMPORTE TOTAL NO PUEDE SER MENOR A LA SUMA DE TOTAL OTROS CARGOS + BASE IMPONIBLE.", txt));
                                    }
                                }
                            }

                        }

                        if ((descuentoGlobal.Count > 0 || SumDetalleDESC > 0) && SumDetalleTotal == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "638", "NO SE ACEPTA DESCUENTOS EN OPERACIONES GRATUITAS.", txt));
                        }
                        if (descuentoGlobal.Count == 0)
                        {
                            if (IGV == SumDetalleIGVAfectos)
                            {
                                if (Math.Abs(((SUBTOTAL - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + OCTOTAL + OTTOTAL - DESCUENTO) - IMPORTETOTAL) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                                }
                            }
                            else
                            {
                                if (Math.Abs(((SUBTOTAL - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + ISCTOTAL + OCTOTAL + OTTOTAL - DESCUENTO) - IMPORTETOTAL) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                                }
                            }
                        }

                        if (lCabecera.Length > 50)
                        {
                            if (string.IsNullOrEmpty(lCabecera[50]?.ToString()) || lCabecera[50]?.ToString() == "0")
                            {
                                otrosTributosCabecera = 0.00M;
                            }
                            else if (!decimal.TryParse(lCabecera[50]?.ToString(), out otrosTributosCabecera))
                            {
                                otrosTributosCabecera = 0.00M;
                            }
                        }

                        if (SumDetalleTotal > 0)
                        {
                            if (descuentoGlobal.Count > 0)
                            {
                                var desGlobal = descuentoGlobal.FirstOrDefault().Split('|');
                                string codigo = desGlobal[3].Trim();
                                //decimal porcentaje = Convert.ToDecimal(desGlobal[2].Trim());
                                if (codigo == "02")
                                {

                                    if (Math.Abs(((BASEIMPONIBLE - SumDetalleGratuitos) + (IGV - SumDetalleIGVAfectos) + ISCTOTAL + OCTOTAL + OTTOTAL) - IMPORTETOTAL) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "635", "ERROR EN EL CÁLCULO DEL IMPORTE TOTAL EN LA CABECERA.", txt));
                                    }

                                    //if ((SumDetalleBI * porcentaje != BASEIMPONIBLE) && SumDetalleGratuitos == 0)
                                    //{
                                    //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "584", "LA SUMATORIA DEL TOTAL DEL VALOR DE VENTA MAS LOS IMPUESTOS NO CONCUERDA CON LA BASE IMPONIBLE.", txt));
                                    //}
                                }
                            }
                        }

                        if (descuentoGlobal.Count > 0)
                        {
                            var desGlobal = descuentoGlobal.FirstOrDefault().Split('|');
                            decimal montDescGlobal = Convert.ToDecimal(desGlobal[2]);
                            if (Math.Abs((SumDetalleDESC + montDescGlobal) - DESCUENTO) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "538", "LA SUMATORIA DE DESCUENTOS DE ITEM Y GLOBALES NO COINCIDEN CON LA CABECERA.", txt));
                            }
                        }
                        else
                        {
                            if (Math.Abs(SumDetalleDESC - DESCUENTO) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "538", "LA SUMATORIA DE DESCUENTOS DE ITEM Y GLOBALES NO COINCIDEN CON LA CABECERA.", txt));
                            }
                        }

                        string[] lineaOT = otrosTributos.Count > 0 ? otrosTributos[0].Split('|') : Array.Empty<string>();

                        if (lineaOT.Length > 1 && decimal.TryParse(lineaOT[1], out var valorLineaOT))
                        {
                            if (SumDetalleOT + valorLineaOT != OTTOTAL)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "482", "LA SUMA DE OTROS TRIBUTOS DE TODOS LOS DETALLES + OTROS TRIBUTOS GLOBALES, NO CUADRA CON EL TOTAL DE OTROS TRIBUTOS EN LA CABECERA.", txt));
                            }
                        }

                        if (lineaOT.Length > 1)
                        {
                            if (SumDetalleOT != (OTTOTAL == 0 ? 0.00M : OTTOTAL) ||
                            SumDetalleOT != otrosTributosCabecera)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "427", "LA SUMA DE LOS OTROS TRIBUTOS DEL DETALLE, NO CUADRA CON LOS OTROS TRIBUTOS TOTAL DE LA CABECERA.", txt));
                            }
                        }

                        string[] lineaOC = otrosCargos.Count > 0 ? otrosCargos[0].Split('|') : Array.Empty<string>();

                        if (lineaOC.Length > 1 && decimal.TryParse(lineaOC[2], out var valorLineaOC))
                        {
                            if (SumDetalleOC + valorLineaOC != OCTOTAL && !UTilidades.CodigoCargos(lineaOC[3]))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "480", "LA SUMA DE OTROS CARGOS DE TODOS LOS DETALLES + OTROS CARGOS GLOBALES, NO CUADRA CON EL TOTAL DE OTROS CARGOS EN LA CABECERA.", txt));
                            }
                        }


                        if (lCabecera.Length > 50)
                        {
                            if (OTTOTAL < 0 && decimal.TryParse(lCabecera[50], out var cabeceraValor) && cabeceraValor < 0 && otrosTributos.Count > 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "483", "LINEA DE OTROS TRIBUTOS GLOBAL NO SE PUEDE LEER, VERIFICAR EL CAMPO OTROS TRIBUTOS GLOBAL DE LA CABECERA.", txt));
                            }
                        }

                        //if ((SumDetaleICBPER + (icbper.Count > 0 ? icbper.Select(x => Convert.ToDecimal(x.Split('|')[4])).Sum() : 0)) != ICBPER)
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "468", "LA SUMA DEL ICBPER DE TODOS LOS DETALLES, NO CUADRA CON EL TOTAL DEL ICBPER EN LA CABECERA.", txt));
                        //}

                        if (icbper.Count > 0)
                        {
                            var contieneLeyenda = detalleLines.Any(x =>
                                new string(x.Normalize(System.Text.NormalizationForm.FormD)
                                          .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                                          .ToArray())
                                .ToLower()
                                .Contains("bolsa plastica"));

                            if (!contieneLeyenda)
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "583",
                                    "PARA IMPUESTO ICBPER DE BOLSA PLÁSTICA, EN LA DESCRIPCIÓN DEL DETALLE DEBE DECIR LA LEYENDA 'BOLSA PLÁSTICA'.",
                                    txt));
                            }

                            //if ((Math.Round(SumDetalleTotal + ICBPER, 2) != Math.Round(IMPORTETOTAL, 2)) &&
                            //    descuentoGlobal.Count == 0 &&
                            //    otrosCargos.Count == 0 &&
                            //    ITEMOTROSCARGOS.Count == 0 &&
                            //    ITEMDESCUENTO.Count == 0 &&
                            //    anticipo.Count == 0)
                            //{
                            //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "590", "SI ENVIA ICBPER EL IMPORTE TOTAL DE LA CABECERA DEBE SER LA SUMA DEL DETALLE + ICBPER.", txt));
                            //}
                        }

                        var bolsasPlasticas = detalleLines.Where(x => x.IndexOf("BOLSA PLASTICA", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                        bool sonGratuitas = false;
                        decimal sumICBPER = 0;
                        if (bolsasPlasticas.Count > 0)
                        {
                            foreach (var item in bolsasPlasticas)
                            {
                                sonGratuitas = UTilidades.EsCodigoGratuitoGravado(item.Split('|')[2]);
                            }

                            foreach (var x in icbper)
                            {
                                var cantICBPER = x.Split('|')[2];
                                var importeICBPER = x.Split('|')[4];
                                var tasa = x.Split('|')[3];
                                sumICBPER += Convert.ToDecimal(cantICBPER) * Convert.ToDecimal(tasa);
                            }
                        }
                        if (sumICBPER != ICBPER && !sonGratuitas)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "468", "LA SUMA DEL ICBPER DE TODOS LOS DETALLES, NO CUADRA CON EL TOTAL DEL ICBPER EN LA CABECERA.", txt));
                        }

                        if (otrosCargos.Count > 0)
                        {
                            var valorOT = otrosCargos.FirstOrDefault().Split('|');
                            if (valorOT[3] == "46")
                            {
                                decimal.TryParse(valorOT[2], out decimal OTGlobal);
                                if (OCTOTAL == 0)
                                {
                                    if (Math.Abs((SumDetalleTotal + OTGlobal) - IMPORTETOTAL) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "320", "LA SUMA DEL TOTAL DEL DETALLE NO CUADRA CON EL TOTAL DE LA CABECERA", txt));
                                    }
                                }
                                else
                                {
                                    if (descuentoGlobal.Count > 0)
                                    {
                                        decimal pctDescuento = Convert.ToDecimal(descuentoGlobal.FirstOrDefault().Split('|')[1]);
                                        if (descuentoGlobal.FirstOrDefault().Split('|')[3] == "02") //AFECTA A LA BI
                                        {
                                            var SumDetalleTotal_ = SumDetalleBI - (SumDetalleBI * pctDescuento) + IGV + OCTOTAL;
                                            //decimal IGV_2 = SumDetalleTotal_ * configuracion.CSuc_PorcentajeIGV;

                                            if (Math.Abs(SumDetalleTotal_ - IMPORTETOTAL) > tolerancia)
                                            {
                                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "320", "LA SUMA DEL TOTAL DEL DETALLE NO CUADRA CON EL TOTAL DE LA CABECERA", txt));
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs((SumDetalleTotal + OCTOTAL) - IMPORTETOTAL) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "320", "LA SUMA DEL TOTAL DEL DETALLE NO CUADRA CON EL TOTAL DE LA CABECERA", txt));
                                        }
                                    }

                                }
                            }
                        }
                        if (descuentoGlobal.Count == 0 && otrosCargos.Count == 0)
                        {
                            if (Math.Abs(SumDetalleTotal - IMPORTETOTAL) > tolerancia && anticipo.Count == 0)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "320", "LA SUMA DEL TOTAL DEL DETALLE NO CUADRA CON EL TOTAL DE LA CABECERA", txt));
                            }
                            if (ITEMOTROSCARGOS.Count > 0)
                            {

                                var codigoIO = ITEMOTROSCARGOS.Any(x => x.Split('|')[3] == "47");
                                if (codigoIO)
                                {
                                    if (Math.Abs((SumDetalleBISinDescuento) - (BASEIMPONIBLE - DESCUENTO)) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA", txt));
                                    }
                                }
                                else
                                {
                                    if (Math.Abs((SumDetalleBISinDescuento) - (BASEIMPONIBLE + OCTOTAL - DESCUENTO)) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA", txt));
                                    }
                                }

                            }
                            else
                            {
                                if (columnas.Count > 3)
                                {
                                    if (Math.Abs(((SumDetalleBI + SumDetalleGratuitos) - SumDetalleDESC) - (BASEIMPONIBLE - DESCUENTO)) > tolerancia && anticipo.Count == 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA.", txt));
                                    }
                                }
                                else
                                {
                                    if (Math.Abs((SumDetalleBISinDescuento - SumDetalleDESC) - (BASEIMPONIBLE - DESCUENTO)) > tolerancia && anticipo.Count == 0)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA.", txt));
                                    }
                                }

                            }

                        }
                        if (descuentoGlobal.Count == 0)
                        {
                            if (anticipo.Count > 0)
                            {
                                decimal sumAnti = 0;
                                decimal sumIGV = 0;
                                foreach (var item in anticipo)
                                {
                                    var anti = item.Split('|');
                                    var docCliente = clienteLine.Split('|');
                                    var anticipos = new brConsultar().FacturaAnticipo_Obtener(lCabecera[4], docCliente[2], docCliente[1], anti[4], anti[5]);
                                    decimal montoPendienteAnticipo = new brConsultar().ObtenerMontoPendienteAnticipo(lCabecera[4], "01", anti[4], anti[5]);
                                    sumAnti += montoPendienteAnticipo;
                                    if (anticipos == null)
                                    {
                                        anticipos = new beFacturaAnticipo();
                                    }
                                    sumIGV += anticipos.IGV;
                                }

                                if (SummontoAnticipado != sumAnti)
                                {
                                    if (Math.Abs(Math.Round((IGV - SumDetalleIGVAfectos) + sumIGV, configuracion.CSuc_CantidadDecimal) - Math.Round((SumDetalleIGV + SumDetalleIGVAfectos), configuracion.CEmi_CantidadDecimalDetalle)) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                    }
                                }
                            }
                            else
                            {
                                if (otrosCargos.Count > 0)
                                {
                                    var codigo = otrosCargos.First().Split('|')[3];
                                    if (codigo != "49")
                                    {
                                        if (Math.Abs(Math.Round(IGV, configuracion.CSuc_CantidadDecimal) - Math.Round(SumDetalleIGV + SumDetalleIGVAfectos, configuracion.CEmi_CantidadDecimalDetalle)) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                        }
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(Math.Round(IGV, configuracion.CSuc_CantidadDecimal) - Math.Round(SumDetalleIGV + SumDetalleIGVAfectos, configuracion.CEmi_CantidadDecimalDetalle)) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                    }
                                }

                            }
                        }
                        if (descuentoGlobal.Count > 0)
                        {
                            var variables = descuentoGlobal.FirstOrDefault().Split('|');
                            decimal.TryParse(variables[1], out decimal descuento);
                            var montoDesc = SumDetalleBI * descuento;
                            var nuevaBI = SumDetalleBI - montoDesc + SumDetalleGratuitos;
                            var nuevaBI_Redondeada = Math.Round(nuevaBI, configuracion.CSuc_CantidadDecimal);
                            var IGV_Calculado = SumDetalleIGV > 0 ? nuevaBI_Redondeada * configuracion.CSuc_PorcentajeIGV : 0;
                            var IGV_Redondeado = Math.Round(IGV, configuracion.CSuc_CantidadDecimal);

                            if (variables[3].Trim() == "02" || variables[3].Trim() == "00")
                            {
                                if (Math.Abs(BASEIMPONIBLE - nuevaBI_Redondeada) > 0.3M)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                        "721",
                                        "SI HAY DESCUENTO GLOBAL QUE AFECTA A LA BASE IMPONIBLE ENTONCES LA BASE IMPONIBLE DE LA CABECERA DEBE SER IGUAL A LA SUMATORIA DE LA BASE IMPONIBLE DEL DETALLE MENOS EL MONTO DE DESCUENTO.", txt));
                                }
                            }

                            if (variables[3].Trim() == "03")
                            {
                                if (Math.Abs((SumDetalleIGV + SumDetalleIGVAfectos) - IGV) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                }
                            }

                            else
                            {
                                if (Math.Abs(IGV_Calculado - IGV_Redondeado) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                }
                            }

                            if (Math.Abs((nuevaBI * configuracion.CSuc_PorcentajeIGV) - IGV_Redondeado) > tolerancia && IGV > 0 && !UTilidades.TipoOperacionExportacion(lCabecera[3]) && lCabecera[3] != "0101")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "400", $"ERROR EN EL CÁLCULO DEL IGV EN EL DETALLE. LINEA {parts[0]}", txt));
                            }
                        }


                        var motivo = descuentoGlobal.Count > 0 && !string.IsNullOrEmpty(descuentoGlobal[0]) ? descuentoGlobal[0].Split('|') : new string[0];
                        if (motivo.Length > 3 && motivo[3].Trim() != "02")
                        {
                            if (decimal.TryParse(motivo[2].Trim(), out decimal motivoDecimal) && Math.Abs(SumDetalleTotal - IMPORTETOTAL - motivoDecimal) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "320", "LA SUMA DEL TOTAL DEL DETALLE NO CUADRA CON EL TOTAL DE LA CABECERA", txt));
                            }

                            if (motivo[3].Trim() == "03")
                            {
                                if (Math.Abs(Math.Round((IGV - SumDetalleIGVAfectos), configuracion.CSuc_CantidadDecimal) - Math.Round(SumDetalleIGV, configuracion.CEmi_CantidadDecimalDetalle)) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                }
                                decimal.TryParse(motivo[2], out decimal montoDescGlobal);
                                if (Math.Abs((SumDetalleDESC + montoDescGlobal) - DESCUENTO) > tolerancia && !exportacion)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "391", "LA SUMA DEL DESCUENTO DE TODOS LOS DETALLES, NO CUADRA CON EL TOTAL DESCUENTO DE LA CABECERA", txt));
                                }
                                if (SumDetalleDESC > 0)
                                {
                                    //if (Math.Abs((SumDetalleBI - (montoDescGlobal)) - (BASEIMPONIBLE)) > tolerancia)
                                    //{
                                    //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA", txt));
                                    //}
                                    if (Math.Abs((SumDetalleBI - montoDescGlobal) - BASEIMPONIBLE) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA", txt));
                                    }
                                }
                                else
                                {
                                    if (Math.Abs((SumDetalleBI + SumDetalleGratuitos) - BASEIMPONIBLE) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "376", "LA SUMA DE LA BASE IMPONIBLE DEL DETALLE NO CUADRA CON LA BASE IMPONIBLE TOTAL DE LA CABECERA", txt));
                                    }
                                }

                            }

                        }
                        if (motivo.Length > 3 && (motivo[3].Trim() == "02" || motivo[3].Trim() == "03"))
                        {
                            decimal.TryParse(motivo[2].Trim(), out decimal montodescuento);
                            if (Math.Abs((SumDetalleDESC + montodescuento) - DESCUENTO) > tolerancia && !exportacion)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "391", "LA SUMA DEL DESCUENTO DE TODOS LOS DETALLES, NO CUADRA CON EL TOTAL DESCUENTO DE LA CABECERA", txt));
                            }
                        }
                        else
                        {
                            if (Math.Abs(SumDetalleDESC - DESCUENTO) > tolerancia && !exportacion)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "391", "LA SUMA DEL DESCUENTO DE TODOS LOS DETALLES, NO CUADRA CON EL TOTAL DESCUENTO DE LA CABECERA", txt));
                            }
                        }

                        if (Math.Abs(SumDetalleISC - ISCTOTAL) > tolerancia)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "323", "LA SUMA DEL ISC DEL DETALLE NO CUADRA CON EL ISC TOTAL DE LA CABECERA", txt));
                        }

                        if (Math.Abs(SumDetalleOT - OTTOTAL) > tolerancia)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "425", "LA SUMA DE LOS OTROS TRIBUTOS DEL DETALLE, NO CUADRA CON LOS OTROS TRIBUTOS TOTAL DE LA CABECERA.", txt));
                        }
                        if (IGV > 0)
                        {
                            if (!UTilidades.EsCodigoGratuito(parts[2].Trim()))
                            {
                                decimal totalRedondeado = Math.Round(SumDetalleTotal, configuracion.CSuc_CantidadDecimal);
                                decimal sumaComponentes = 0;
                                if (columnas.Count > 3)
                                {
                                    if (columnas[3][0] == "01")
                                    {
                                        sumaComponentes =
                                           Math.Round(SumDetalleIGV, configuracion.CSuc_CantidadDecimal) +
                                           Math.Round(SumDetalleBI, configuracion.CSuc_CantidadDecimal) +
                                           Math.Round(SumDetalleOT, configuracion.CSuc_CantidadDecimal) +
                                           (ITEMOTROSCARGOS?.Count > 0 ? 0 : Math.Round(SumDetalleOC, configuracion.CSuc_CantidadDecimal)) - SumDetalleDESC;
                                        if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                        }
                                    }
                                }
                                else
                                {
                                    if (otrosCargos.Count > 0)
                                    {
                                        var otglobal = otrosCargos.FirstOrDefault().Split('|');
                                        if (otglobal[3] == "46" || otglobal[3] == "45" || otglobal[3] == "49" || otglobal[3] == "50" || otglobal[3] == "51" || otglobal[3] == "52" || otglobal[3] == "53")
                                        {
                                            sumaComponentes =
                                              Math.Round(IGV - SumDetalleIGVAfectos, configuracion.CSuc_CantidadDecimal) +
                                              Math.Round((BASEIMPONIBLE - SumDetalleGratuitos), configuracion.CSuc_CantidadDecimal) +
                                              Math.Round(SumDetalleOT, configuracion.CSuc_CantidadDecimal) +
                                              Math.Round(OCTOTAL, configuracion.CSuc_CantidadDecimal) +
                                              (ITEMOTROSCARGOS?.Count > 0 ? 0 : Math.Round(SumDetalleOC, configuracion.CSuc_CantidadDecimal));

                                            totalRedondeado = IGV > 0 && descuentoGlobal.Count == 0 ?
                                                          totalRedondeado + OCTOTAL : totalRedondeado + IGV + OCTOTAL;

                                            if (descuentoGlobal.Count > 0)
                                            {
                                                var codigoDescuento = descuentoGlobal.FirstOrDefault().Split('|')[3];
                                                decimal.TryParse(descuentoGlobal.FirstOrDefault().Split('|')[1], out decimal pctDescG);
                                                totalRedondeado = (SumDetalleBI - (SumDetalleBI * pctDescG)) + IGV + OCTOTAL;
                                                if (codigoDescuento == "02")
                                                {
                                                    if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                                    {
                                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                                    }
                                                }
                                                else
                                                {
                                                    if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                                    {
                                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (otglobal[3] == "49")
                                                {
                                                    var nuevaBI = SumDetalleBI + (SumDetalleBI * Convert.ToDecimal(otglobal[1]));
                                                    if ((nuevaBI + IGV) - IMPORTETOTAL > tolerancia)
                                                    {
                                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                                    }
                                                }
                                                else
                                                {
                                                    if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                                    {
                                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                                    }
                                                }

                                            }


                                        }
                                        else
                                        {
                                            sumaComponentes =
                                               Math.Round(SumDetalleIGV, configuracion.CSuc_CantidadDecimal) +
                                               Math.Round(SumDetalleBI, configuracion.CSuc_CantidadDecimal) +
                                               Math.Round(SumDetalleOT, configuracion.CSuc_CantidadDecimal) +
                                               (ITEMOTROSCARGOS?.Count > 0 ? 0 : Math.Round(SumDetalleOC, configuracion.CSuc_CantidadDecimal));
                                            if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                            {
                                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                            }
                                        }

                                    }
                                    else
                                    {
                                        sumaComponentes =
                                           Math.Round((SumDetalleIGV), configuracion.CSuc_CantidadDecimal) +
                                           Math.Round(SumDetalleBI, configuracion.CSuc_CantidadDecimal) +
                                           Math.Round(SumDetalleOT, configuracion.CSuc_CantidadDecimal) +
                                           Math.Round(SumDetalleISC, configuracion.CSuc_CantidadDecimal) +
                                           (ITEMOTROSCARGOS?.Count > 0 ? 0 : Math.Round(SumDetalleOC, configuracion.CSuc_CantidadDecimal));
                                        if (Math.Abs(totalRedondeado - sumaComponentes) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "545", "EL IMPORTE TOTAL DEBE SER LA SUMA DE LA BASE IMPONIBLE MAS IGV.", txt));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion                    
                }

                #region FORMAPAGO

                decimal sumMontoCuota = 0;
                decimal MontoDetraccion = 0;

                if (formaPago != null && formaPago != "")
                {
                    var FormaPago = formaPago.Split('|');
                    var descripcion = FormaPago[1].Trim();
                    decimal.TryParse(FormaPago[2], out decimal montopendiente);
                    var fechaPago = FormaPago[3].Trim();
                    var pagoCuotas = FormaPago[4].Trim();

                    //if (descripcion.ToUpper() == "CONTADO")
                    //{
                    //    if (FormaPago[2].Trim() == "")
                    //    {
                    //        lRechazos.AddRange(await AgregarRechazo(lCabecera, "589", "LINEA FORMA DE PAGO, CAMPO MONTO PENDIENTE NO PUEDE SER VACIO.", txt));
                    //    }
                    //}

                    if (montopendiente < 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                               "847",
                               $"EL MONTO PENDIENTE DE PAGO DE LA FORMA DE PAGO NO PUEDE SER NEGATIVO.",
                               txt));
                    }

                    foreach (var c in cuotas)
                    {
                        var cuota = c.Split('|');
                        decimal.TryParse(cuota[2], out decimal montocuota);
                        sumMontoCuota += montocuota;

                        if (montocuota < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "846",
                                $"EL MONTO DE LA CUOTA NO PUEDE SER NEGATIVO. {cuota[1]}",
                                txt));
                        }

                        if (!Regex.IsMatch(cuota[1], @"^CUOTA\d{3}$", RegexOptions.IgnoreCase))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "592",
                                "SI FORMA DE PAGO ES CREDITO, CUOTA DEBE SER CUOTA<NNN> ALFANUMERICO, EJEMPLO: CUOTA001.",
                                txt));
                        }
                    }

                    if (descripcion.ToUpper() == "CREDITO")
                    {
                        if (detraccion.Count > 0)
                        {
                            var montoDetraccion = detraccion.FirstOrDefault().Split('|')[4];
                            decimal.TryParse(montoDetraccion, out decimal montoD);
                            MontoDetraccion = montoD;
                        }

                        if (detraccion.Count > 0)
                        {
                            var detraccionI = lCabecera[8].Trim() == "USD"
                                ? MontoDetraccion / tipoCambio
                                : MontoDetraccion;

                            detraccionI = Math.Round(detraccionI, 2);
                            decimal montoCalculado = Math.Round(IMPORTETOTAL, 2);
                            if (montopendiente > montoCalculado + 1M)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                        "594",
                                        "CUANDO SE ENVIA UNA DETRACCION DE FORMA DE PAGO A CREDITO, EL MONTO PENDIENTE PUEDE SER MENOR O IGUAL AL MONTO TOTAL.",
                                        txt));
                            }
                        }

                    }

                    if (descripcion.ToUpper() == "CREDITO" && cuotas.Count > 0)
                    {
                        if (montopendiente != sumMontoCuota)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "593",
                                    "LA SUMA DE LAS CUOTAS DEBE SER IGUAL AL MONTO NETO PENDIENTE DE PAGO.",
                                    txt));
                        }
                    }

                    if (descripcion.ToUpper() == "CREDITO" && !cuotas.Any())
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "584", "SI LA FORMA DE PAGO ES CREDITO DEBEN COLOCAR LINEA CUOTA.", txt));
                    }

                    if (montopendiente > IMPORTETOTAL)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "557", "EL MONTO NETO PENDIENTE DE PAGO DEBE SER MENOR O IGUAL AL IMPORTE TOTAL DEL COMPROBANTE.", txt));
                    }

                    if (descripcion.ToUpper() == "CONTADO" && Convert.ToDateTime(fechaPago).Date != Convert.ToDateTime(lCabecera[6].Trim()).Date)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "509", "SI FORMA DE PAGO ES CONTADO, EN EL CAMPO FECHA DE PAGO DEBE IR LA FECHA DE EMISION.", txt));
                    }

                    if (descripcion.ToUpper() == "CREDITO" && cuotas.Count > 0)
                    {
                        var fechavencimiento = Convert.ToDateTime(lCabecera[7].Trim()).Date;
                        var fechaemision = Convert.ToDateTime(lCabecera[6].Trim()).Date;
                        var fechaUltimaCuota = cuotas.Last().Split('|');

                        var fechaPagoDt = Convert.ToDateTime(fechaPago).Date;
                        var fechaVencimientoDt = fechavencimiento.Date;

                        if (!DateTime.TryParse(fechaUltimaCuota[3], out DateTime fechaCuotaDt))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "995",
                                "FECHA DE ULTIMA CUOTA MAL ESTRUCTURADA O FORMATO INVALIDO.", txt));
                            return lRechazos;
                        }

                        if (fechaPagoDt != fechaVencimientoDt || fechaCuotaDt != fechaVencimientoDt)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "554",
                                "SI FORMA DE PAGO ES CREDITO, FECHA DE VENCIMIENTO, FECHA DE PAGO Y FECHA DE ULTIMA CUOTA DEBEN SER IGUALES.", txt));
                        }
                        if (fechaCuotaDt <= fechaemision)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "588",
                                "FECHA DE PAGO UNICO O DE LAS CUOTAS NO PUEDE SER ANTERIOR O IGUAL A LA FECHA DE EMISION DEL COMPROBANTE.", txt));
                        }

                        if (montopendiente == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "521", "LINEA FORMA DE PAGO, CAMPO MONTO PENDIENTE NO PUEDE SER CERO.", txt));
                        }
                    }

                }
                else
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "556", "FALTA LINEA FORMA DE PAGO.", txt));
                }
                #endregion

                #region RETENCION
                if (retencion.Count > 0)
                {
                    foreach (var item in retencion)
                    {
                        var parts = item.Split('|');
                        var codigo = parts[1].Trim();
                        decimal.TryParse(parts[2], out decimal pctretencion);
                        decimal.TryParse(parts[3], out decimal montoRetencion);
                        decimal.TryParse(parts[4], out decimal montoBase);

                        decimal retencionCalculada = Math.Truncate(pctretencion * IMPORTETOTAL * 100) / 100;
                        decimal montoRetencionTruncado = Math.Truncate(montoRetencion * 100) / 100;

                        if (Math.Abs(retencionCalculada - montoRetencionTruncado) > 0.01M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "543", "MONTO DE RETENCIÓN MAL CALCULADO.", txt));
                        }
                        if (codigo != "62")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "544", "CÓDIGO DE RETENCIÓN INCORRECTO.", txt));
                        }
                        if (pctretencion != 0.03M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "546", "PORCENTAJE DE RETENCIÓN INCORRECTO.", txt));
                        }
                    }
                }
                #endregion

                #region DESCUENTO GLOBAL
                if (descuentoGlobal.Count > 0)
                {
                    foreach (var item in descuentoGlobal)
                    {
                        var parts = item.Split('|');
                        if (decimal.TryParse(parts[2], out decimal descuento) && descuento < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "390", "EL MONTO DEL DESCUENTO GLOBAL DEBE SER MAYOR A CERO.", txt));
                        }
                        if (decimal.TryParse(parts[1], out decimal porcentaje) && porcentaje <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "395", "EL PORCENTAJE DEL DESCUENTO GLOBAL NO PUEDE SER MENOR O IGUAL A CERO.", txt));
                        }
                        if (porcentaje <= 0 || porcentaje >= 1)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "395", "EL PORCENTAJE DEL DESCUENTO GLOBAL NO PUEDE SER MENOR O IGUAL A CERO NI MAYOR O IGUAL A 1.", txt));
                        }
                        if (parts[3].Trim() == "02")
                        {
                            if (decimal.TryParse(parts[2], out decimal monto) && Math.Abs((porcentaje * SumDetalleBIconDescGlobal) - monto) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "396", "EL MONTO DEL DESCUENTO GLOBAL ESTÁ MAL CALCULADO.", txt));
                            }
                            var exportacion = UTilidades.TipoOperacionExportacion(lCabecera[3]);
                            if (otrosCargos.Count > 0)
                            {
                                var otroCargoGlobal = otrosCargos.FirstOrDefault().Split('|');
                                if (otroCargoGlobal[3] == "46")
                                {
                                    if (Math.Abs(((SumDetalleBIconDescGlobal - descuento) + IGV + OCTOTAL) - IMPORTETOTAL) > tolerancia && !exportacion)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "392", "LA SUMA DEL IMPORTE TOTAL DE LOS DETALLES MENOS EL MONTO DEL DESCUENTO GLOBAL, DEBE SER IGUAL AL IMPORTE TOTAL DE LA CABECERA", txt));
                                    }
                                }
                            }
                            else
                            {
                                if (descuentoGlobal.Count > 0)
                                {
                                    var codigoDescGlobal = descuentoGlobal.FirstOrDefault().Split('|')[3];

                                    if (codigoDescGlobal == "02")
                                    {
                                        if (Math.Abs((((BASEIMPONIBLE - SumDetalleGratuitos)) + (IGV - SumDetalleIGVAfectos)) - IMPORTETOTAL) > tolerancia && !exportacion)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "392", "LA SUMA DEL IMPORTE TOTAL DE LOS DETALLES MENOS EL MONTO DEL DESCUENTO GLOBAL, DEBE SER IGUAL AL IMPORTE TOTAL DE LA CABECERA", txt));
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(((BASEIMPONIBLE - descuento) + IGV) - IMPORTETOTAL) > tolerancia && !exportacion)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "392", "LA SUMA DEL IMPORTE TOTAL DE LOS DETALLES MENOS EL MONTO DEL DESCUENTO GLOBAL, DEBE SER IGUAL AL IMPORTE TOTAL DE LA CABECERA", txt));
                                        }
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(((BASEIMPONIBLE - descuento) + IGV) - IMPORTETOTAL) > tolerancia && !exportacion)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "392", "LA SUMA DEL IMPORTE TOTAL DE LOS DETALLES MENOS EL MONTO DEL DESCUENTO GLOBAL, DEBE SER IGUAL AL IMPORTE TOTAL DE LA CABECERA", txt));
                                    }
                                }

                            }
                        }
                        if (UTilidades.TipoOperacionExportacion(lCabecera[3].Trim()) && (parts[3].Trim() == "00" || parts[3].Trim() == "02"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "608", "CUANDO ES UNA EXPORTACION NO DEBE PERMITIR DESCUENTO GLOBAL QUE AFECTA A LA BASE(02) Y DESCUENTO POR ITEM QUE AFECTA A LA BASE (03).", txt));
                        }

                        //if ()
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                        //}

                        if (parts[3] == "02" || parts[3] == "03" || parts[3] == "04" || parts[3] == "05" || parts[3] == "06")
                        {
                            //decimal base_ = porcentaje * SumDetalleBI;
                            //decimal igv_ = porcentaje * SumDetalleIGV;

                            decimal nuevoIGV = BASEIMPONIBLE * configuracion.CSuc_PorcentajeIGV; //NUEVO IGV CON DESCUENTO GLOBAL Q AFECTA A LA BASE

                            if (IGV > 0)
                            {
                                if (Math.Abs(nuevoIGV - IGV) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                                }
                            }
                        }
                        else
                        {
                            if (Math.Abs(Math.Round(SumDetalleBI, configuracion.CSuc_CantidadDecimal) - Math.Round(BASEIMPONIBLE, configuracion.CSuc_CantidadDecimal)) > tolerancia
                             && Math.Abs(Math.Round(BASEIMPONIBLE, configuracion.CSuc_CantidadDecimal) + Math.Round(IGV, configuracion.CSuc_CantidadDecimal)
                                 - Math.Round(IMPORTETOTAL, configuracion.CSuc_CantidadDecimal)) > tolerancia)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "322", "LA SUMA DEL IGV DEL DETALLE NO CUADRA CON EL IGV TOTAL DE LA CABECERA.", txt));
                            }
                        }

                    }

                }
                #endregion

                #region ICBPER
                if (icbper.Count > 0)
                {
                    foreach (var item in icbper)
                    {
                        var parts = item.Split('|');

                        if (parts[1].ToUpper().Trim() == "ICBPER")
                        {
                            if (!UTilidades.TasaICBPER(parts[3].Trim()))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "466", "TASA DEL ICBPER, NO CORRESPONDE AL AÑO ACTUAL.", txt));
                            }
                            //if (lCabecera[])
                            //{
                            //    lRechazos.AddRange(AgregarRechazo(lCabecera, "471", "NO EXISTE DETALLE CON LA DESCRIPCION BOLSA PLASTICA O EL ID DEL DETALLE EN EL ICBPER ES EL INCORRECTO.", txt));
                            //}
                            if (Convert.ToInt32(parts[2].Trim()) != cantidadICBPER)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "469", "LA CANTIDAD DEL ICBPER DEBE SER IGUAL A LA CANTIDAD DEL DETALLE BOLSA PLASTICA.", txt));
                            }
                        }
                    }
                }
                #endregion

                #region OTROS TRIBUTOS
                if (otrosTributos.Count > 0)
                {
                    foreach (var item in otrosTributos)
                    {
                        var parts = item.Split('|');
                        //if (lCabecera[32].Trim() != "1")
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "462", "LINEA DE OTROS TRIBUTOS GLOBAL NO SE PUEDE LEER, VERIFICAR EL CAMPO OTROS CARGOS GLOBAL DE LA CABECERA.", txt));
                        //}
                        //if (parts[3].Trim() != "0.50")
                        //{
                        //    lRechazos.AddRange(AgregarRechazo(lCabecera, "466", "TASA DEL ICBPER, NO CORRESPONDE AL AÑO ACTUAL.", txt));
                        //}
                    }
                }
                #endregion

                #region OTROS CARGOS
                if (otrosCargos.Count > 0)
                {
                    foreach (var item in otrosCargos)
                    {
                        var parts = item.Split('|');
                        if (lCabecera[32].Trim() != "1")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "462", "LINEA DE OTROS CARGOS GLOBAL NO SE PUEDE LEER, VERIFICAR EL CAMPO OTROS CARGOS GLOBAL DE LA CABECERA.", txt));
                        }
                        //if (OTTOTAL <= 0)
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "634", "SI HAY LINEA DE OTROS CARGOS GLOBAL, EL VALOR DEL CAMPO TOTAL, OTROS CARGOS(15) DE LA CABECERA NO PUEDE SER 0 O MENOR.", txt));
                        //}
                        if (!UTilidades.MotivoOtrosCargos(parts[3]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "463", "EN LA LINEA DE OTROS CARGOS GLOBAL, MOTIVO DE OTROS CARGOS NO EXISTE, VERIFICAR CODIGO DE OTROS CARGOS.", txt));
                        }

                        if (Convert.ToDecimal(parts[2].Trim()) != OCTOTAL)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "537", "ERROR EN MONTO DE RECARGO.", txt));
                        }

                        if (decimal.TryParse(parts[1], out var porcentajeOT) &&
                            decimal.TryParse(parts[2], out var montoOT))
                        {
                            var montoCalculado = SumDetalleBI * porcentajeOT;
                            if (parts[3] == "46" || parts[3] == "45" || parts[3] == "49" || parts[3] == "50" || parts[3] == "51" || parts[3] == "52" || parts[3] == "53")
                            {
                                if (descuentoGlobal.Count > 0)
                                {
                                    var descGlobal = descuentoGlobal.FirstOrDefault().Split('|');
                                    if (descGlobal[3] == "02" || descGlobal[3] == "03" || descGlobal[3] == "04" || descGlobal[3] == "05" || descGlobal[3] == "06")
                                    {
                                        decimal nuevabi = SumDetalleBI * Convert.ToDecimal(descGlobal[1]);
                                        montoCalculado = (SumDetalleBI - nuevabi) * porcentajeOT;

                                        if (Math.Abs(montoCalculado - montoOT) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                        }
                                    }
                                }
                                else
                                {
                                    if (parts[3] == "50" || parts[3] == "48")
                                    {
                                        montoCalculado = SumDetalleTotal * porcentajeOT;
                                        if (Math.Abs(montoCalculado - montoOT) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(montoCalculado - montoOT) > tolerancia)
                                        {
                                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                        }
                                    }
                                }

                            }
                            else
                            {
                                if (Math.Abs(montoCalculado - montoOT) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "464", "MONTO DE OTROS CARGOS GLOBAL MAL CALCULADO.", txt));
                                }
                            }
                        }
                    }
                }
                #endregion

                #region ANTICIPOS
                if (anticipo.Count > 0)
                {
                    decimal montoAnticipo = anticipo.Count > 0
                                        ? anticipo.Sum(x =>
                                        {
                                            var partes = x.Split('|');
                                            return partes.Length > 8 && decimal.TryParse(partes[8], out decimal valor) ? valor : 0;
                                        })
                                        : 0;
                    if (montoAnticipo > SumDetalleTotal)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "641", "EL MONTO ANTICIPADO NO PUEDE SER MAYOR AL TOTAL DEL CONSUMO.", txt));
                    }

                    if (SumDetalleTotal - montoAnticipo != IMPORTETOTAL)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "652", "EL IMPORTE TOTAL DE LA CABECERA DEBE SER EL RESULTADO DEL IMPORTE TOTAL DEL ITEM MENOS EL IMPORTE DEL DOCUMENTO ANTICIPADO.", txt));
                    }
                    foreach (var item in anticipo)
                    {
                        var parts = item.Split('|');
                        var docCliente = clienteLine.Split('|');
                        var anticipos = new brConsultar().FacturaAnticipo_Obtener(lCabecera[4], docCliente[2], docCliente[1], parts[4], parts[5]);
                        var montoPendienteAnticipo = new brConsultar().ObtenerMontoPendienteAnticipo(lCabecera[4], "01", parts[4], parts[5]);

                        if (parts[7].Trim() != lCabecera[8].Trim())
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "428", "LA MONEDA DEL DOCUMENTO ANTICIPADO, NO PUEDE SER DISTINTO AL DOCUMENTO ANTICIPO.", txt));
                        }
                        if (anticipos == null)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "431", "EN EL ANTICIPO, DOCUMENTO NO EXISTE, O NO ES UN DOCUMENTO ANTICIPADO, O LA INFORMACION DEL CLIENTE ES DISTINTO.", txt));
                        }

                        //if (anticipos.IGV - SumDetalleIGV != 0)
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "653", "ERROR EN EL CÁLCULO DEL IGV DE LA CABECERA AL CONSUMIR ANTICIPO.", txt));
                        //}
                        if (SumDetalleIGV - anticipos?.IGV < 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "653", "ERROR EN EL CÁLCULO DEL IGV DE LA CABECERA AL CONSUMIR ANTICIPO.", txt));
                        }

                        decimal.TryParse(parts[6], out decimal montoAnticipado);
                        if (montoAnticipado > SumDetalleTotal)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "452", "EL MONTO DEL DOC. ANTICIPADO DEBE SER MENOR AL MONTO DEL DOC. ANTICIPO.", txt));
                        }

                        if (DateTime.TryParseExact(parts[2].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaDocumentoAnticipado)
                            && DateTime.TryParse(lCabecera[6].Trim(), out DateTime fechaDocumentoAnticipo))
                        {
                            if (fechaDocumentoAnticipado > fechaDocumentoAnticipo)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "474", "LA FECHA EN EL DOCUMENTO ANTICIPADO, NO PUEDE SER MAYOR A LA DEL DOCUMENTO ANTICIPO.", txt));
                            }
                        }

                        if (montoAnticipado > montoPendienteAnticipo)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "632", "EL MONTO DE ANTICIPO PENDIENTE A CONSUMIR INSUFICIENTE.", txt));
                        }
                        //if (parts[10].Trim() == "")
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "629", "NO ESTA COLOCANDO EL CODIGO DE ANTICIPO EN LA LINEA ANTICIPO.", txt));
                        //}


                        //if (anticipos != null)
                        //{
                        //    if (fechaDocumentoAnticipado.Date != Convert.ToDateTime(anticipos.FechaEmision).Date)
                        //    {

                        //        lRechazos.AddRange(await AgregarRechazo(lCabecera, "475", "LA FECHA EN EL DOCUMENTO ANTICIPADO, NO CONCUERDA CON LO ENVIADO ANTERIORMENTE.", txt));
                        //    }
                        //}
                        if (lCabecera[2].Trim() == "01" && (parts[3].Trim() != "02" && parts[3].Trim() != "01"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "476", "EL TIPO DE COMPROBANTE EN EL DOCUMENTO ANTICIPADO, NO CONCUERDA CON EL TIPO DE DOCUMENTO ANTICIPO.", txt));
                        }
                        if (lCabecera[2].Trim() == "03" && (parts[3].Trim() != "03"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "476", "EL TIPO DE COMPROBANTE EN EL DOCUMENTO ANTICIPADO, NO CONCUERDA CON EL TIPO DE DOCUMENTO ANTICIPO.", txt));
                        }

                        //var cliente = clienteLine.Split('|');
                        //if (cliente[2].Trim() != lCabecera[8].Trim())
                        //{
                        //    lRechazos.AddRange(AgregarRechazo(lCabecera, "428", "LA MONEDA DEL DOCUMENTO ANTICIPADO, NO PUEDE SER DISTINTO AL DOCUMENTO ANTICIPO.", txt));
                        //}

                        if (!string.IsNullOrEmpty(parts[4].Trim()))
                        {
                            string campo = parts[4].Trim();
                            if (!Regex.IsMatch(campo, @"^F[A-Z0-9]{3}$"))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "430", "EN EL ANTICIPO, CAMPO SERIE DE COMPROBANTE ANTICIPADO MAL ESTRUCTURADO.", txt));
                            }
                        }
                    }
                }

                #endregion

                #region DETRACCION
                if (detraccion.Count > 0)
                {
                    foreach (var item in detraccion)
                    {
                        var parts = item.Split('|');
                        if (lCabecera[3].Trim() == "1001" && lCabecera[28].Trim() != "1"
                            && !new brArchivo().ObtenerEmpresaSinIndicador(lCabecera[4].Trim()))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "611", "SI EL TIPO DE OPERACION ES 1001 EL CAMPO 29 DEBE TENER EL VALOR 1.", txt));
                        }
                        if (lCabecera[3].Trim() == "1001" && parts[1].Trim() == "027" && (IMPORTETOTAL * tipoCambio) < 400)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "579", "SI EL TIPO OPERACION ES 1001 Y EL CODIGO DE DETRACCION ES 027 ENTONCES EL SERVICIO DEBE SER MAYOR O IGUAL A 400.", txt));
                        }

                        var codigo_catalogo_54 = new brConsultar().Catalogo_54(parts[1].Trim());

                        if (string.IsNullOrEmpty(codigo_catalogo_54))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "613", "EN LA LINEA DETRACCION ES OBLIGATORIO COMPLETAR EL CODIGO DE DETRACCION EN EL CAMPO 2.", txt));
                        }
                        if (decimal.TryParse(parts[3].Trim(), out decimal valorDetraccion) &&
                            ((parts[1].Trim() == "027" && valorDetraccion != 0.04m) ||
                             (parts[1].Trim() == "037" && valorDetraccion != 0.12m) ||
                             (parts[1].Trim() == "022" && valorDetraccion != 0.12m) ||
                             (parts[1].Trim() == "026" && valorDetraccion != 0.10m)))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "615", "EL PORCENTAJE DE DETRACCION NO CORRESPONDE DE ACUERDO AL CODIGO DE DETRACCION.", txt));
                        }
                        if (string.IsNullOrEmpty(parts[2].Trim()) || !Regex.IsMatch(parts[2].Trim(), @"^[0-9\-]+$"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "614", "EN LA LINEA DETRACCION ES OBLIGATORIO COMPLETAR EL NUMERO DE CUENTA DE DETRACCION EN EL CAMPO 3 (numérico con guiones).", txt));
                        }
                        if (decimal.TryParse(parts[4].Trim(), out decimal monto) && monto <= 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "616", "EN LA LINEA DETRACCION ES OBLIGATORIO COMPLETAR EL MONTO DE DETRACCION.", txt));
                        }
                        if (UTilidades.codBbSsDetraccionText(parts[1].Trim()) == "")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "624", "ERROR EN LA LÍNEA DE DETRACCION, COD BBSS NO EXISTE.", txt));
                        }

                        DateTime Femision;
                        string[] formatos = {
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yy HH:mm:ss"
                            };
                        DateTime.TryParseExact(lCabecera[6], formatos, null, System.Globalization.DateTimeStyles.None, out Femision);
                        tipoCambio = new brConsultar().ObtenerTipoCambio(Femision.ToString("yyyy-MM-dd"), lCabecera[8].Trim());
                        decimal.TryParse(parts[3], out decimal porcentaje);
                        if (lCabecera[8].Trim() == "PEN")
                        {
                            if (Detdetraccion.Count > 0)
                            {
                                var maximo = Detdetraccion
                                    .Select(x => x.Split('|'))
                                    .Where(arr => arr.Length > 7)
                                    .Select(arr => decimal.Parse(arr[7], CultureInfo.InvariantCulture))
                                    .Max();

                                if (maximo > IMPORTETOTAL)
                                {
                                    if (Math.Abs((maximo * porcentaje) - monto) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                    }
                                }
                                else
                                {
                                    if (Math.Abs((IMPORTETOTAL * porcentaje) - monto) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                    }
                                }
                            }
                            else
                            {
                                if (Math.Abs((IMPORTETOTAL * porcentaje) - monto) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                }
                            }
                        }
                        if (lCabecera[8].Trim() == "USD")
                        {
                            if (Detdetraccion.Count > 0)
                            {
                                var maximo = Detdetraccion
                                    .Select(x => x.Split('|'))
                                    .Where(arr => arr.Length > 7)
                                    .Select(arr => decimal.Parse(arr[7], CultureInfo.InvariantCulture))
                                    .Max();

                                if (maximo > IMPORTETOTAL * tipoCambio)
                                {
                                    if ((maximo * porcentaje - monto) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                    }
                                }
                                else
                                {
                                    if ((IMPORTETOTAL * porcentaje * tipoCambio - monto) > tolerancia)
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                    }
                                }
                            }
                            else
                            {
                                if ((IMPORTETOTAL * porcentaje * tipoCambio - monto) > tolerancia)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "617", "MONTO DE DETRACCION MAL CALCULADO.", txt));
                                }
                            }
                        }
                    }
                }
                #endregion

                #region DETDETRACCION
                if (Detdetraccion.Count > 0)
                {
                    var valorReferencialStr = Detdetraccion.FirstOrDefault().Split('|')[7].Trim();
                    var valorReferencialCE = Detdetraccion.FirstOrDefault().Split('|')[8].Trim();
                    var valorReferencialCU = Detdetraccion.FirstOrDefault().Split('|')[9].Trim();
                    var ubigeoPuntoPartida = Detdetraccion.FirstOrDefault().Split('|')[2].Trim();
                    var DireccionPuntoViaje = Detdetraccion.FirstOrDefault().Split('|')[3].Trim();
                    var UbigeoPuntoDestino = Detdetraccion.FirstOrDefault().Split('|')[4].Trim();
                    var DireccionPuntoDestinoViaje = Detdetraccion.FirstOrDefault().Split('|')[5].Trim();

                    if (decimal.TryParse(valorReferencialStr, out decimal valorReferencial))
                    {
                        if (valorReferencial == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "591", "EL CAMPO VALOR REFERENCIAL DEL SERVICIO DE TRANSPORTE DE LA LINEA DETDETRACCION NO PUEDE SER 0.", txt));
                        }
                    }
                    if (decimal.TryParse(valorReferencialCE, out decimal _valorReferencialCE))
                    {
                        if (_valorReferencialCE == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "595", "EL CAMPO VALOR REFERENCIAL DE LA CARGA EFECTIVA DE LA LINEA DETDETRACCION NO PUEDE SER 0.", txt));
                        }
                    }
                    if (decimal.TryParse(valorReferencialCU, out decimal _valorReferencialCU))
                    {
                        if (_valorReferencialCU == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "596", "EL CAMPO VALOR REFERENCIAL DE LA CARGA UTIL DE LA LINEA DETDETRACCION NO PUEDE SER 0.", txt));
                        }
                    }
                    else
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "591", "EL CAMPO VALOR REFERENCIAL NO ES UN NUMERO VALIDO.", txt));
                    }

                    if (lCabecera[3].Trim() == "1004")
                    {
                        if (string.IsNullOrEmpty(ubigeoPuntoPartida) || string.IsNullOrEmpty(DireccionPuntoViaje) || string.IsNullOrEmpty(UbigeoPuntoDestino)
                            || string.IsNullOrEmpty(DireccionPuntoDestinoViaje))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "600",
                                "Si EL TIPO DE OPERACIÓN ES 1004, ES OBLIGATORIO ENVIAR LOS CAMPOS 3,4,5,6 DE LA LINEA DETDETRACCION.",
                                txt));
                        }
                    }
                }
                #endregion

                #region PAQUETE TURISTICO
                if (paqueteTuristico.Count > 0 && lCabecera[3].Trim() == "0205")
                {
                    bool coinciden = detalleLines.All(x => paqueteTuristico.Any(y => x.Split('|')[0].Trim() == y.Split('|')[1].Trim()));
                    if (!coinciden)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "566", "SI HAY LINEA DE PAQUETE TURISTICO SE DEBE INDICAR LA LINEA DE DETALLE QUE AFECTA.", txt));
                    }

                    var docidentidad = paqueteTuristico.Any(x => UTilidades.TipoDocumentoIdentidadPaqueteTuristico(x.Split('|')[4].Trim()));
                    if (!docidentidad)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "567", "SI HAY LINEA DE PAQUETE TURISTICO SE DEBE INDICAR UN TIPO DE DOCUMENTO DE IDENTIDAD VALIDO.", txt));
                    }

                    var nroDocumentoIdentidad = paqueteTuristico.Any(x => string.IsNullOrEmpty(x.Split('|')[5].Trim()));
                    if (nroDocumentoIdentidad)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "568", "SI HAY LINEA DE PAQUETE TURISTICO SE DEBE INDICAR EL NUMERO DEL DOCUMENTO DE IDENTIDAD DEL HUESPED.", txt));
                    }

                    var codigoPais = paqueteTuristico.Any(x => string.IsNullOrEmpty(x.Split('|')[6].Trim()));
                    if (codigoPais)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "569", "SI HAY LINEA DE PAQUETE TURISTICO SE DEBE INDICAR EL CODIGO DEL PAIS DE EMISION DEL PASAPORTE.", txt));
                    }

                    var razonsocial = paqueteTuristico.Any(x => string.IsNullOrEmpty(x.Split('|')[3].Trim()));
                    if (razonsocial)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "570", "SI HAY LINEA DE PAQUETE TURISTICO SE DEBE  INDICAR EL NOMBRE Y APELLIDO O DENOMINACION O RAZON SOCIAL DEL HUESPED.", txt));
                    }

                    var nroDocumentoHuesped = paqueteTuristico.Select(x => x.Split('|')[5]);

                    if (nroDocumentoHuesped.Any(doc => string.IsNullOrWhiteSpace(doc) || doc.Length < 3 || doc.Length > 20 || !Regex.IsMatch(doc, @"^[a-zA-Z0-9.-]+$")))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "571", "EL NUMERO DE DOCUMENTO DE IDENTIDAD DEL HUESPED TIENE QUE SER ENTRE 3 Y 20 CARACTERES ALFANUMERICOS.", txt));
                    }

                    var nombreapellidohuesped = paqueteTuristico.Select(x => x.Split('|')[3]);

                    if (nombreapellidohuesped.Any(doc => string.IsNullOrWhiteSpace(doc) || doc.Length < 3 || doc.Length > 200 || !Regex.IsMatch(doc, @"^[\p{L}\p{N} .,&()-]+$")))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "572", "EL NOMBRE Y APELLIDO O DENOMINACION O RAZON SOCIAL DEL HUESPED DEBE DE SER ENTRE 3 Y 200 CARACTERES ALFANUMERICOS.", txt));
                    }

                    if (lCabecera[3].Trim() == "0205" && paqueteTuristico.Count != detalleLines.Count)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "565", "PARA EL TIPO DE OPERACION 0205 ES OBLIGATORIO QUE HAYA UNA LINEA DE PAQUETE TURISTICO POR CADA DETALLE.", txt));
                    }




                }
                #endregion

                #region DETALLEADICIONAL - VENTA SECTOR PUBLICO
                if (detalleAdicional.Count > 0)
                {
                    foreach (var item in detalleAdicional)
                    {
                        var nroExpediente = item.Split('|')[2].Trim();
                        var codigoUnidadEjecutora = item.Split('|')[3].Trim();
                        var nroProcesoSeleccion = item.Split('|')[4].Trim();
                        var nroContrato = item.Split('|')[5].Trim();

                        if (!UTilidades.EsAlfanumerico(nroExpediente, 20))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "649",
                                "EL NRO DE EXPEDIENTE DEBE SER ALFANUMÉRICO HASTA 20 CARACTERES.", txt));
                        }
                        if (!UTilidades.EsAlfanumerico(codigoUnidadEjecutora, 10))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "649",
                                "EL CÓDIGO DE UNIDAD EJECUTORA DEBE SER ALFANUMÉRICO HASTA 10 CARACTERES.", txt));
                        }
                        if (!UTilidades.EsAlfanumerico(nroProcesoSeleccion, 30))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "649",
                                "EL NRO DE PROCESO DE SELECCION DEBE SER ALFANUMÉRICO HASTA 30 CARACTERES.", txt));
                        }
                        if (!UTilidades.EsAlfanumerico(nroContrato, 30))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "649",
                                "EL NRO DE CONTRATO DEBE SER ALFANUMÉRICO HASTA 30 CARACTERES.", txt));
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
                            if (parts[1].Trim() == "6")
                            {
                                if (existeRuc == 0)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "7", "ERROR RUC CLIENTE NO ENCONTRADO EN SUNAT.", txt));
                                }
                                if (existeRuc == 0)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "17", "ERROR RUC CLIENTE NO ACTIVO EN SUNAT.", txt));
                                }
                                if (existeRuc == 0)
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "501", "EL CLIENTE NO SE ENCUENTRA ACTIVO EN SUNAT.", txt));
                                }
                                if (retencion.Count > 0)
                                {
                                    var agenteRetenencion = new brConsultar().ObtenerRuc(parts[2].Trim(), lCabecera[4].Trim());
                                    if (agenteRetenencion.AgenteRetencion == "FALSE")
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "542", "CLIENTE NO ES AGENTE DE RETENCIÓN.", txt));
                                    }
                                }
                            }

                            if ((lCabecera[3].Trim() == "0101" || lCabecera[3].Trim() == "1001") && parts[1].Trim() != "6")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "560", "SI EL TIPO OPERACION ES VENTA INTERNA(0101) o DETRACCION(1001) EL TIPO DE DOCUMENTO SOLO DEBE SER RUC(6).", txt));
                            }

                            if ((UTilidades.TipoOperacionExportacion(lCabecera[3].Trim()) && parts[1].Trim() == "1"))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "558", "SI ES EXPORTACIÓN, EL TIPO DE DOCUMENTO DEL CLIENTE NO PUEDE SER DNI(1).", txt));
                            }

                            var tiposPermitidos = new HashSet<string> { "4", "7", "A", "B", "C", "D" };
                            string numeroDocumento = parts[2].Trim();

                            bool esValido = Regex.IsMatch(numeroDocumento, @"^[a-zA-Z0-9]+$");

                            if (tiposPermitidos.Contains(parts[1]) && !esValido)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "495",
                                    "PARA LOS TIPOS DE DOCUMENTOS (0,4,7,A,B,C,D) EL NUMERO DE DOCUMENTO DE IDENTIDAD SOLO SE PERMITE CARACTERES ALFANUMERICOS SIN ESPACIO NI GUION.",
                                    txt));
                            }

                            if (IMPORTETOTAL > 700 &&
                                (string.IsNullOrWhiteSpace(parts[2].Trim()) || string.IsNullOrWhiteSpace(parts[3].Trim())))
                            {
                                if (parts[2].Trim().StartsWith("2") && string.IsNullOrWhiteSpace(parts[7].Trim()))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "307", "FACTURA CON MONTO TOTAL MAYOR A 700 SIN INFORMACION DEL CLIENTE COMPLETA. CAMPOS OBLIGATORIOS: DOCUMENTO DEL CLIENTE, NOMBRES O RAZON SOCIAL Y DIRECCION.", txt));
                                }
                                if (parts[2].Trim().StartsWith("1"))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "307", "FACTURA CON MONTO TOTAL MAYOR A 700 SIN INFORMACION DEL CLIENTE COMPLETA. CAMPOS OBLIGATORIOS: DOCUMENTO DEL CLIENTE, NOMBRES O RAZON SOCIAL.", txt));
                                }
                            }
                            if (parts[2].Trim() == lCabecera[4].Trim())
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "497", "EL CLIENTE NO PUEDE SER EL MISMO QUE EL EMISOR.", txt));
                            }
                            if (string.IsNullOrEmpty(parts[3].Trim()) || parts[3].Trim().Length < 3)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "500", "LA RAZON SOCIAL O NOMBRES DEL CLIENTE ES OBLIGATORIO, MINIMO 3 CARACTERES Y MAXIMO 1500 CARACTERES.", txt));
                            }
                            if (parts[1] == "1" && (parts[2]?.Trim().Length != 8 || !long.TryParse(parts[2]?.Trim(), out _)))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "484", "NUMERO DE DOCUMENTO DE IDENTIDAD DEL CLIENTE DEBE TENER 8 CARACTERES OBLIGATORIO CUANDO EL TIPO DE DOCUMENTO DE IDENTIDAD SEA 1=DNI, Y SOLO SE ACEPTA NUMEROS.", txt));
                            }
                            //if (UTilidades.TipoDocumentoIdentidadExtra(parts[1].Trim()) && (parts[2].Trim().Length > 15 || parts[2].Trim().Length < 1) && long.TryParse(parts[2].Trim(), out _))
                            //{
                            //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "619", "NUMERO DE DOCUMENTO DE IDENTIDAD DEBE TENER DE 1 A 15 CARACTERES OBLIGATORIO CUANDO EL TIPO DE DOCUMENTO DE IDENTIDAD SEA 4=CARNET DE EXTRANJERIA, Y SOLO SE ACEPTA NUMEROS.", txt));
                            //}

                            if (!UTilidades.TipoDocumentoIdentidad(parts[1].Trim()))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "375", "TIPO DE DOCUMENTO DE IDENTIDAD DEL CLIENTE NO ACEPTADO, REVISAR TABLA TIPO DE DOCUMENTO DE IDENTIDAD DEL CLIENTE.", txt));
                            }

                            var exportacion = UTilidades.TipoOperacionExportacionFactura(lCabecera[3].Trim());
                            if (exportacion && parts[1].Trim() == "6")
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera, "650", "Tipo de operación es exportación, no puede ser RUC.", txt));
                            }

                            //if (lCabecera[3].Trim() == "0101" && parts[1].Trim() != "6")
                            //{
                            //    lRechazos.AddRange(await AgregarRechazo(lCabecera, "555", "SI ES VENTA INTERNA(0101) SOLO SE ACEPTA RUC(6).", txt));
                            //}

                            var tiposDocAllow = new HashSet<string> { "7", "B", "G" };
                            if (lCabecera[3].Trim() == "2106" && !tiposDocAllow.Contains(parts[1].Trim()))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "603",
                                    "Si EL TIPO DE OPERACIÓN ES 2106, TIPO DE DOCUMENTO DE IDENTIDAD DEL CLIENTE SOLO SE ACEPTA  7, B y G.", txt));
                            }

                        }
                        #endregion
                    }
                }
                else
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "651", "NO ESTA ENVIANDO LINEA CLIENTE O ESTA MAL EL IDENTIFICADOR.", txt));
                }

                if (!tieneDetalle)
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "355", "TIPO DE DETALLE NO ACEPTADO, SOLO SE ACEPTA BIEN O SERVICIO.", txt));
                    lRechazos.AddRange(await AgregarRechazo(lCabecera, "317", "DOCUMENTO NO CUENTA CON DETALLE.", txt));
                }
            }
            catch (Exception ex)
            {
                await LogAsync($"Validar01 - {txt[5]}", ex);
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
