using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BR.Commons;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EsavApi.Validador.BR.ValidacionXTipoDoc
{
    public class ValidarANU : brGenerico
    {
        public static List<beRechazo> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();

            try
            {
                if (!int.TryParse(lCabecera[7].Trim().ToString(), out _))
                {
                    lRechazos.AddRange(AgregarRechazo(lCabecera, "9", "EL NUMERO DEL DOCUMENTO EN REFERENCIA ERRÓNEO, DEBE SER NUMÉRICO.", txt));
                    return lRechazos;
                }

                var configuracion = new brConfiguracion().Consultar(lCabecera[3].ToString(), lCabecera[4].ToString());
                var estadoSunat = new brConsultar().EstadoSunat(lCabecera[3], lCabecera[6], lCabecera[7].Trim(), lCabecera[2]);
                var comunicacionBaja = new brConsultar().ComunicacionBajaObtener(lCabecera[3], lCabecera[2], lCabecera[6], lCabecera[7].Trim());

                if (lineas.Length >= 1)
                {
                    foreach (var line in lineas)
                    {
                        var parts = line.Split('|');
                        if (string.IsNullOrWhiteSpace(parts[0])) continue;

                        #region CABECERA 
                        string[] formatos = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss" };
                        DateTime FEmision = DateTime.MinValue;
                        if (string.IsNullOrEmpty(lCabecera[5].Trim()) || !DateTime.TryParseExact(lCabecera[5].Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out FEmision))
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "7", "FECHA DE EMISION ERRÓNEA VERIFICAR CAMPO 6 DE LA CABECERA.", txt));
                        }
                        if (lCabecera[0] != "210")
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "5", "VERSION DE TXT NO EXISTE, CONFIGURAR VERSION.", txt));
                        }
                        if (estadoSunat != 0)
                        {
                            var estadoSunatApi = new brConsultar().SunatConsultaApi(lCabecera[3].Trim(), lCabecera[2].Trim(), lCabecera[6].Trim(), int.Parse(lCabecera[7]), FEmision.ToString("dd/MM/yyyy"), Convert.ToDecimal(comunicacionBaja.ImporteTotal));

                            if (estadoSunatApi != "2" && estadoSunatApi != "1")
                            {
                                lRechazos.AddRange(AgregarRechazo(lCabecera, "621", "EL DOCUMENTO A ANULAR AUN NO SE ENCUENTRA ACEPTADO POR SUNAT.", txt));
                            }
                        }
                        if (!comunicacionBaja.Estado)
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "339", "DOCUMENTO HA ANULAR YA SE HA DADO DE BAJA ANTERIORMENTE.", txt));
                        }

                        FEmision = DateTime.ParseExact(lCabecera[5].Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        if (FEmision > DateTime.Now)
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "2", "LA FECHA DE EMISION NO PUEDE SER MAYOR A LA FECHA ACTUAL", txt));
                        }
                        if (string.IsNullOrEmpty(lCabecera[9].Trim()))
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "3", "DEBE INDICAR EL MOTIVO DE BAJA", txt));
                        }

                        var valoresPermitidos = new List<string> { "01", "03", "07", "08" };

                        if (!valoresPermitidos.Contains(lCabecera[2]))
                        {
                            lRechazos.AddRange(AgregarRechazo(lCabecera, "4", "SOLO SE PUEDE ANULAR (FACTURA, BOLETA, NC, ND, RETENCION, PERCEPCION)", txt));
                        }

                        string fechaTexto = comunicacionBaja.Fecha.Trim();

                        fechaTexto = fechaTexto
                            .Replace("p. m.", "PM")
                            .Replace("a. m.", "AM")
                            .Replace("p.m.", "PM")
                            .Replace("a.m.", "AM");
                        string[] _formatos = new[]
                        {
                            "dd/MM/yyyy HH:mm:ss",
                            "d/M/yyyy HH:mm:ss",
                            "dd/MM/yyyy hh:mm:ss tt",
                            "d/M/yyyy hh:mm:ss tt",
                            "dd/MM/yyyy",
                            "d/M/yyyy"
                        };

                        DateTime fechaEmision;
                        bool fechaValida = DateTime.TryParseExact(
                            fechaTexto,
                            _formatos,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out fechaEmision);

                        if (!fechaValida)
                        {
                            lRechazos.AddRange(AgregarRechazo(
                                lCabecera,
                                "6",
                                "DOCUMENTO FUERA DE RANGO DE FECHA, SOLO SE PUEDE ANULAR HASTA LOS 7 DÍAS DE EMITIRLO",
                                txt
                            ));
                        }
                        else
                        {
                            // Validar 7 días
                            DateTime fechaLimite = fechaEmision.AddDays(7);

                            if (DateTime.Now > fechaLimite)
                            {
                                lRechazos.AddRange(AgregarRechazo(
                                    lCabecera,
                                    "6",
                                    "DOCUMENTO FUERA DE RANGO DE FECHA, SOLO SE PUEDE ANULAR HASTA LOS 7 DÍAS DE EMITIRLO",
                                    txt
                                ));
                            }
                        }

                        #endregion
                    }

                }


            }
            catch (Exception ex)
            {
                LogAsync("ValidarANU", ex).GetAwaiter().GetResult();
                DateTime.TryParse(lCabecera[6], out DateTime fechaEmision);
                lRechazos.Add(new beRechazo
                {
                    RUC = txt[5].Contains("ANU") ? lCabecera[3] : lCabecera[4],
                    Sede = txt[5].Contains("ANU") ? lCabecera[4] : lCabecera[5],
                    Serie = txt[5].Contains("ANU") ? Convert.ToDateTime(lCabecera[5]).ToString("yyyyMMdd") : lCabecera[21],
                    Numero = txt[5].Contains("ANU") ? "1" : lCabecera[22],
                    CodigoRechazo = "523",
                    Descripcion = ex.Message,
                    TipoDoc = TipoDocNombreTxt,
                    FechaEmision = fechaEmision,
                    FechaTransferencia = DateTime.Now,
                    Txt = txt[5],
                    TipoMoneda = txt[5].Contains("ANU") ? "1" : lCabecera[8]
                });
            }

            return lRechazos;
        }

        private static List<beRechazo> AgregarRechazo(string[] lCabecera, string codigo, string descripcion, string[] txt)
        {
            string tipoDoc = txt[5].Substring(33, 2);
            List<beRechazo> lRechazos = new List<beRechazo>();
            DateTime.TryParse(lCabecera[5], out DateTime fechaEmision);

            lRechazos.Add(new beRechazo
            {
                RUC = lCabecera[3],
                Sede = lCabecera[4],
                Serie = lCabecera[6],
                Numero = lCabecera[7],
                CodigoRechazo = codigo,
                Descripcion = descripcion,
                TipoDoc = lCabecera[2],
                FechaEmision = fechaEmision,
                FechaTransferencia = DateTime.Now,
                Txt = txt[5],
                TipoMoneda = ""
            });

            return lRechazos;
        }
    }
}
