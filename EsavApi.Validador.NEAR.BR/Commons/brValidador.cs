using EsavApi.Validador.BR.RechazosXTipoDoc;
using EsavApi.Validador.BR.ValidacionXTipoDoc;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.BR.ValidacionXTipoDoc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR
{
    public class brValidador : brGenerico
    {
        public async Task<List<beRechazo>> ValidarTXT(string[] lineas, string tipoDoc, string correlativoTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();
            var lCabecera = lineas[0].Split('|');

            var detalleLines = lineas
                            .Where(x =>
                            {
                                var partes = x.Split('|');
                                return partes.Length > 5 &&
                                       int.TryParse(partes[0], out _) &&
                                       (partes[1].ToUpper() == "BIEN" || partes[1].ToUpper() == "SERVICIO");
                            })
                            .ToList();

            if (lCabecera.Length == 0)
            {
                throw new FormatException("La cabecera del archivo está vacía o no tiene el formato esperado.");
            }

            if (detalleLines.Count > 700)
            {
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                lRechazos.Add(new beRechazo
                {
                    RUC = lCabecera[4],
                    Sede = lCabecera[5],
                    Serie = lCabecera[21],
                    Numero = lCabecera[22],
                    CodigoRechazo = "910",
                    Descripcion = "EL LIMITE MAXIMO DE ITEMS(LINEA DETALLE) ES DE 700 POR DOCUMENTO.",
                    TipoDoc = lCabecera[2],
                    FechaEmision = fechaEmision,
                    FechaTransferencia = DateTime.Now,
                    TipoMoneda = lCabecera[8].Trim(),
                    Txt = txt[5]
                });
                return lRechazos;
            }

            #region boleta
            if (tipoDoc.Trim() == "03")
            {

                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (!int.TryParse(lCabecera[22].Trim().ToString(), out _))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "9",
                        Descripcion = "EL NUMERO DEL DOCUMENTO ERRÓNEO, DEBE SER NUMÉRICO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                string nombreSinExtension = Path.GetFileNameWithoutExtension(txt[5]);
                if (!Regex.IsMatch(nombreSinExtension, @"^\d{38}$"))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "595",
                        Descripcion = "NOMBRE DEL TXT MAL ESTRUCTIRADO, FAVOR DE VERIFICAR EL MANULA DE GENERACION DE TXT.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar03.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region factura
            else if (tipoDoc.Trim() == "01")
            {

                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                string nombreSinExtension = Path.GetFileNameWithoutExtension(txt[5]);
                if (!Regex.IsMatch(nombreSinExtension, @"^\d{38}$"))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "595",
                        Descripcion = "NOMBRE DEL TXT MAL ESTRUCTIRADO, FAVOR DE VERIFICAR EL MANULA DE GENERACION DE TXT.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                foreach (var linea in lineas)
                {
                    if (!EsLineaValida(linea))
                    {
                        lRechazos.Add(new beRechazo
                        {
                            RUC = lCabecera[4],
                            Sede = lCabecera[5],
                            Serie = lCabecera[21],
                            Numero = lCabecera[22],
                            CodigoRechazo = "598",
                            Descripcion = "TXT CONTIENE LÍNEA QUE NO CORRESPONDE SEGÚN EL MANUAL, VERIFICAR.",
                            TipoDoc = lCabecera[2],
                            FechaEmision = fechaEmision,
                            FechaTransferencia = DateTime.Now,
                            TipoMoneda = lCabecera[8].Trim(),
                            Txt = txt[5]
                        });

                        return lRechazos;
                    }
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar01.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region notacredito
            else if (tipoDoc.Trim() == "07")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                var docReferencia = new brConsultar().ExisteDocReferencia(lCabecera[4], lCabecera[5].Trim(), lCabecera[19], lCabecera[20], lCabecera[18].Trim());
                if (lCabecera[17] == "1" && lCabecera[18] != "05")
                {
                    if (docReferencia == 0)
                    {
                        lRechazos.Add(new beRechazo
                        {
                            RUC = lCabecera[4],
                            Sede = lCabecera[5],
                            Serie = lCabecera[21],
                            Numero = lCabecera[22],
                            CodigoRechazo = "503",
                            Descripcion = "EL DOCUMENTO REFERENCIA NO EXISTE.",
                            TipoDoc = lCabecera[2],
                            FechaEmision = fechaEmision,
                            FechaTransferencia = DateTime.Now,
                            TipoMoneda = lCabecera[8].Trim(),
                            Txt = txt[5]
                        });
                        return lRechazos;
                    }
                }
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (!int.TryParse(lCabecera[22].Trim(), out int valNumero) || valNumero <= 0)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "9",
                        Descripcion = "EL NUMERO DEL DOCUMENTO ERRÓNEO, DEBE SER NUMÉRICO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar07.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region notadebito
            else if (tipoDoc.Trim() == "08")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                var docReferencia = new brConsultar().ExisteDocReferencia(lCabecera[4], lCabecera[5].Trim(), lCabecera[19], lCabecera[20], lCabecera[18].Trim());
                if (lCabecera[17] == "1" && lCabecera[18] != "05")
                {
                    if (docReferencia == 0)
                    {
                        lRechazos.Add(new beRechazo
                        {
                            RUC = lCabecera[4],
                            Sede = lCabecera[5],
                            Serie = lCabecera[21],
                            Numero = lCabecera[22],
                            CodigoRechazo = "503",
                            Descripcion = "EL DOCUMENTO REFERENCIA NO EXISTE.",
                            TipoDoc = lCabecera[2],
                            FechaEmision = fechaEmision,
                            FechaTransferencia = DateTime.Now,
                            TipoMoneda = lCabecera[8].Trim(),
                            Txt = txt[5]
                        });
                        return lRechazos;
                    }
                }
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (!int.TryParse(lCabecera[22].Trim(), out int valNumero) || valNumero <= 0)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "9",
                        Descripcion = "EL NUMERO DEL DOCUMENTO ERRÓNEO, DEBE SER NUMÉRICO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(validar08.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region guiaremision
            else if (tipoDoc.Trim() == "09")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[3], lCabecera[14], lCabecera[16], lCabecera[4], lCabecera[2]);
                string[] formatos = {
                        "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yy HH:mm:ss"
                    };
                DateTime.TryParseExact(lCabecera[5], formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[14],
                        Numero = lCabecera[15],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[3], lCabecera[14], lCabecera[15], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[14],
                        Numero = lCabecera[15],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[14],
                        Numero = lCabecera[15],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[3].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[14],
                        Numero = lCabecera[15],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[14],
                        Numero = lCabecera[15],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar09.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region GUIA T.
            else if (tipoDoc.Trim() == "31")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[3], lCabecera[11], lCabecera[13], lCabecera[4], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[11],
                        Numero = lCabecera[12],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[11],
                        Numero = lCabecera[12],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[11],
                        Numero = lCabecera[12],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[3].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[11],
                        Numero = lCabecera[12],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[6].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar31.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region ANULACION
            else if (tipoDoc.Trim().ToUpper() == "ANU")
            {

                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);

                if (Path.GetFileNameWithoutExtension(txt[5].Trim()).Length != 41)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[6],
                        Numero = lCabecera[7],
                        CodigoRechazo = "649",
                        Descripcion = $"TXT mal estructurado, favor de revisar el nombre del archivo txt.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = "",
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                var existeDocumento = new brConsultar().ExisteDocReferencia(lCabecera[3], lCabecera[4], lCabecera[6], lCabecera[7].Trim(), lCabecera[2]);

                if (existeDocumento == 0)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[3],
                        Sede = lCabecera[4],
                        Serie = lCabecera[6],
                        Numero = lCabecera[7],
                        CodigoRechazo = "1",
                        Descripcion = $"EL DOCUMENTO A ANULAR NO EXISTE.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = "",
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                else
                {
                    lRechazos = ValidarANU.Validar(lineas, lCabecera, tipoDoc, txt);
                }
            }
            #endregion

            #region DocumentoCobranza
            else if (tipoDoc.Trim() == "99")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                string nombreSinExtension = Path.GetFileNameWithoutExtension(txt[5]);
                if (!Regex.IsMatch(nombreSinExtension, @"^\d{38}$"))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "595",
                        Descripcion = "NOMBRE DEL TXT MAL ESTRUCTIRADO, FAVOR DE VERIFICAR EL MANULA DE GENERACION DE TXT.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                foreach (var linea in lineas)
                {
                    if (!EsLineaValida(linea))
                    {
                        lRechazos.Add(new beRechazo
                        {
                            RUC = lCabecera[4],
                            Sede = lCabecera[5],
                            Serie = lCabecera[21],
                            Numero = lCabecera[22],
                            CodigoRechazo = "598",
                            Descripcion = "TXT CONTIENE LÍNEA QUE NO CORRESPONDE SEGÚN EL MANUAL, VERIFICAR.",
                            TipoDoc = lCabecera[2],
                            FechaEmision = fechaEmision,
                            FechaTransferencia = DateTime.Now,
                            TipoMoneda = lCabecera[8].Trim(),
                            Txt = txt[5]
                        });

                        return lRechazos;
                    }
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar99.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region ValeCredito
            else if (tipoDoc.Trim() == "105")
            {
                var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[4], lCabecera[21], lCabecera[23], lCabecera[5], lCabecera[2]);
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                if (DataEmisor.estado == 2)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "451",
                        Descripcion = "ESTIMADO CLIENTE, USTED PRESENTA UNA FACTURA EMITIDA PENDIENTE DE PAGO. DE HABERLA CANCELADO, ENVIAR EL COMPROBANTE DE PAGO AL WHATSAPP 944 003 729, DONDE EL ÁREA DE FINANZAS CONFIRMARÁ EL ABONO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (txt[5].Substring(0, 11).Trim() != lCabecera[4].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "620",
                        Descripcion = "RUC EMISOR EN EL NOMBRE DEL TXT NO CONCUERDA CON EL RUC EMISOR EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                var Duplicado = new brConsultar().ExisteDocDuplicado(lCabecera[4], lCabecera[21], lCabecera[22], lCabecera[2]);
                if (Duplicado == 1)
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "305",
                        Descripcion = "DOCUMENTO DUPLICADO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                if (correlativoTxt != lCabecera[1].Trim())
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "4",
                        Descripcion = "CORRELATIVO EN EL NOMBRE DEL TXT NO CONCUERDA CON EL CORRELATIVO EN LA CABECERA.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                string nombreSinExtension = Path.GetFileNameWithoutExtension(txt[5]);
                if (!Regex.IsMatch(nombreSinExtension, @"^\d{38}$"))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "595",
                        Descripcion = "NOMBRE DEL TXT MAL ESTRUCTIRADO, FAVOR DE VERIFICAR EL MANULA DE GENERACION DE TXT.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }

                if (ValidarCaracteresExtraños(lineas))
                {
                    lRechazos.Add(new beRechazo
                    {
                        RUC = lCabecera[4],
                        Sede = lCabecera[5],
                        Serie = lCabecera[21],
                        Numero = lCabecera[22],
                        CodigoRechazo = "623",
                        Descripcion = "EL TXT CONTIENE CARACTERES EXTRAÑOS NO VALIDOS, VERIFICAR SU ARCHIVO.",
                        TipoDoc = lCabecera[2],
                        FechaEmision = fechaEmision,
                        FechaTransferencia = DateTime.Now,
                        TipoMoneda = lCabecera[8].Trim(),
                        Txt = txt[5]
                    });
                    return lRechazos;
                }
                foreach (var linea in lineas)
                {
                    if (!EsLineaValida(linea))
                    {
                        lRechazos.Add(new beRechazo
                        {
                            RUC = lCabecera[4],
                            Sede = lCabecera[5],
                            Serie = lCabecera[21],
                            Numero = lCabecera[22],
                            CodigoRechazo = "598",
                            Descripcion = "TXT CONTIENE LÍNEA QUE NO CORRESPONDE SEGÚN EL MANUAL, VERIFICAR.",
                            TipoDoc = lCabecera[2],
                            FechaEmision = fechaEmision,
                            FechaTransferencia = DateTime.Now,
                            TipoMoneda = lCabecera[8].Trim(),
                            Txt = txt[5]
                        });

                        return lRechazos;
                    }
                }

                var tareas = new List<Task<List<beRechazo>>>();
                tareas.Add(Validar105.Validar(lineas, lCabecera, tipoDoc, txt));
                var resultados = await Task.WhenAll(tareas);
                foreach (var resultado in resultados)
                {
                    lRechazos.AddRange(resultado);
                }
            }
            #endregion

            #region OTR
            else
            {
                DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
                lRechazos.Add(new beRechazo
                {
                    RUC = lCabecera[4],
                    Sede = lCabecera[5],
                    Serie = lCabecera[21],
                    Numero = lCabecera[22],
                    CodigoRechazo = "649",
                    Descripcion = $"El tipo de documento '{tipoDoc}' no es soportado.",
                    TipoDoc = lCabecera[2],
                    FechaEmision = fechaEmision,
                    FechaTransferencia = DateTime.Now,
                    TipoMoneda = lCabecera[8].Trim(),
                    Txt = txt[5]
                });
                return lRechazos;
            }
            #endregion

            return lRechazos;
        }

        public bool ValidarCaracteresExtraños(string[] lineas)
        {
            bool tiene = false;
            //var regexCaracteresInvalidos = new Regex(@"[^\u0009\u000A\u000D\u0020-\u007E\u00A0-\u017F]", RegexOptions.Compiled);
            var regexCaracteresInvalidos = new Regex(@"[^\u0009\u000A\u000D\u0020-\u007E\u00A0-\u017F\u2022\u2013\u201C\u201D\u2019]", RegexOptions.Compiled);

            foreach (var linea in lineas)
            {
                if (regexCaracteresInvalidos.IsMatch(linea))
                {
                    return tiene = true;
                }
            }

            return tiene;
        }
        private bool EsLineaValida(string linea)
        {
            return linea.Contains("|");
        }
    }
}
