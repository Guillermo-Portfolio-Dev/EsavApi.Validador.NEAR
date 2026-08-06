using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.ValidacionXTipoDoc
{
    public class Validar31 : brGenerico
    {
        public static async Task<List<beRechazo>> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();

            try
            {
                var configuracion = await new brConfiguracion().Consultar(lCabecera[3].ToString(), lCabecera[4].ToString());
                bool existeDestinatario = false;
                bool existeSubcontrato = false;
                bool existePagoServicio = false;
                string campo18 = lCabecera.Length > 17 ? lCabecera[17]?.Trim() : "";
                string campo23 = lCabecera.Length > 22 ? lCabecera[22]?.Trim() : "";

                for (int j = 0; j < lineas.Length; j++)
                {
                    var line = lineas[j].Split('|');
                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    #region CABECERA
                    if (line[0].Trim() == "210")
                    {
                        DateTime FEmision;
                        DateTime FInicioTraslado;
                        string[] formatos = {
                            "dd/MM/yyyy HH:mm:ss",
                            "dd/MM/yy HH:mm:ss",
                            "yyyy-MM-dd HH:mm:ss",
                            "MM/dd/yyyy HH:mm:ss",
                            "MM/dd/yy HH:mm:ss",
                            "dd/MM/yyyy"
                        };

                        DateTime hoy = DateTime.Today;
                        DateTime fechaLimite = hoy.AddDays(-1); // Ayer

                        // Validar que las fechas se parsearon correctamente
                        bool fechaEmisionValida = DateTime.TryParseExact(lCabecera[5], formatos, null, System.Globalization.DateTimeStyles.None, out FEmision);
                        bool fechaInicioTrasladoValida = DateTime.TryParseExact(lCabecera[7], formatos, null, System.Globalization.DateTimeStyles.None, out FInicioTraslado);

                        // 1. CONTROL DE FORMATO (Si alguna no parsea, se rechaza de inmediato)
                        if (!fechaEmisionValida || !fechaInicioTrasladoValida)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "2",
                                "FORMATO DE FECHA INVÁLIDO EN FECHA DE EMISIÓN Y/O FECHA DE INICIO DE TRASLADO.",
                                txt));
                        }
                        // 2. VALIDACIÓN DE FECHA DE EMISIÓN (Solo ayer y hoy)
                        else if (FEmision.Date < fechaLimite || FEmision.Date > hoy)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "2",
                                "FECHA DE EMISION FUERA DE RANGO. SOLO SE ACEPTA DESDE AYER HASTA LA FECHA ACTUAL.",
                                txt));
                        }
                        // 3. VALIDACIÓN DE FECHA DE TRASLADO (Solo ayer o cualquier día del futuro)
                        else if (FInicioTraslado.Date < fechaLimite)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "2",
                                "FECHA DE INICIO DE TRASLADO NO VALIDA. SE ACEPTA MÁXIMO 1 DÍA ATRÁS DE LA FECHA ACTUAL.",
                                txt));
                        }

                        var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[3], lCabecera[11], lCabecera[13], lCabecera[4], lCabecera[2]);
                        if (DataEmisor.serieUsuario == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "9", "USUARIO NO CONFIGURADO PARA ESTA SERIE.", txt));
                        }

                        decimal.TryParse(lCabecera[15], out decimal pesoBruto);
                        if (pesoBruto < 0.10M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "11",
                                "EL PESO BRUTO TOTAL DEBE SER MAYOR A CERO VALOR MINIMO 0.10.",
                                txt));
                        }
                    }
                    #endregion

                    #region DETALLE
                    if (Regex.IsMatch(line[0], @"^\d+") && line.Length <= 9)
                    {
                        decimal.TryParse(line[4], out decimal cant);

                        if (cant < 0.001M)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                                        lCabecera,
                                                        "10",
                                                        "CANTIDAD DEBE SER MAYOR A CERO VALOR MINIMO 0.001 (SIN REDONDEAR).",
                                                        txt));
                        }
                    }
                    #endregion

                    #region CONDUCTOR
                    if (line[0].Trim().ToUpper() == "CONDUCTOR")
                    {
                        var licencia = line[6]?.Trim();
                        var tipoDocIdentidad = line[2]?.Trim();
                        var tipoConductor = line[1]?.Trim();
                        var nroDocIdentidad = line[3]?.Trim();
                        var NombreConductor = line[4]?.Trim();
                        var ApellidoConductor = line[5]?.Trim();

                        var soloNumeros = Regex.Replace(licencia ?? "", @"\D", "");

                        if (string.IsNullOrWhiteSpace(licencia) ||
                            !Regex.IsMatch(licencia, @"^[A-Za-z0-9\-]+$") ||
                            soloNumeros.All(c => c == '0'))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "3",
                                "LICENCIA DE CONDUCIR SOLO SE PERMITEN LETRAS Y NUMEROS, NO SE ACEPTA SOLO CEROS(0000000000). :: ID – CONDUCTOR.",
                                txt));
                        }

                        var maxPermitidos = new[] { "PRINCIPAL", "SECUNDARIO" };

                        if (!maxPermitidos.Contains(tipoConductor))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "30",
                                "SOLO PUEDE HABER CONDUCTOR PRINCIPAL O SECUNDARIO.",
                                txt));
                        }

                        int countPrincipal = lineas.Count(x => x.ToUpper().Contains("PRINCIPAL"));
                        int countSecundario = lineas.Count(x => x.ToUpper().Contains("SECUNDARIO"));

                        if (countPrincipal > 1 || countSecundario > 1)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "30",
                                "SOLO PUEDE HABER UN CONDUCTOR PRINCIPAL Y UN CONDUCTOR SECUNDARIO.",
                                txt));
                        }

                        if (string.IsNullOrEmpty(licencia) || string.IsNullOrEmpty(tipoDocIdentidad) || string.IsNullOrEmpty(nroDocIdentidad)
                        || string.IsNullOrEmpty(NombreConductor) || string.IsNullOrEmpty(ApellidoConductor))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                                        lCabecera,
                                                        "7",
                                                        "LOS DATOS DEL CONDUCTOR ES OBLIGATORIO CUANDO EL TIPO DE TRANSPORTE ES PRIVADO.",
                                                        txt));
                        }

                        if (!UTilidades.TipoDocumentoIdentidad(tipoDocIdentidad))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                                        lCabecera,
                                                        "12",
                                                        "TIPO DE DOCUMENTO DEL CONDUCTOR NO EXISTE.",
                                                        txt));
                        }

                        bool esValida = licencia.Length >= 9 &&
                                    licencia.Length <= 10 &&
                                    Regex.IsMatch(licencia, @"^[a-zA-Z0-9]+$");

                        if (!esValida)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "13",
                                "NUMERO DE LICENCIA DE CONDUCIR DE LA LINEA CONDUCTOR DEBE SER ALFANUMERICO ENTRE 9 A 10 CARACTERES.",
                                txt));
                        }
                        if (tipoDocIdentidad == "1")
                        {
                            if (string.IsNullOrEmpty(nroDocIdentidad) || nroDocIdentidad.Length != 8)
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                                        lCabecera,
                                                        "21",
                                                        "SI TIPO DE DOCUMENTO DE CONDUCTOR ES DNI, EL NUMERO DEBE SER DE 8 DIGITOS.",
                                                        txt));
                            }
                        }
                    }
                    #endregion

                    #region VEHICULO
                    if (line[0].Trim().ToUpper() == "VEHICULO")
                    {
                        var numero_placa = line.Length > 1 ? line[1]?.Trim() : "";
                        var TUCE = line.Length > 2 ? line[2]?.Trim() : "";
                        var numero_autorizacion = line.Length > 3 ? line[3]?.Trim() : "";
                        var codigo_entidad_emisora_autorizacion = line.Length > 4 ? line[4]?.Trim() : "";

                        if (string.IsNullOrEmpty(numero_placa) ||
                            string.IsNullOrEmpty(TUCE) ||
                            string.IsNullOrEmpty(numero_autorizacion) ||
                            string.IsNullOrEmpty(codigo_entidad_emisora_autorizacion))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "8",
                                "LOS DATOS DEL VEHÍCULO SON OBLIGATORIOS CUANDO EL TIPO DE TRANSPORTE ES PRIVADO.",
                                txt));
                        }
                        if (string.IsNullOrEmpty(numero_placa) || numero_placa.Length < 6 || numero_placa.Length > 8 ||
                        !Regex.IsMatch(numero_placa, @"^[A-Za-z0-9]+$"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "20",
                                "LA PLACA SOLO PUEDE TENER DE 6 A 8 CARACTERES ALFANUMERICOS, SIN ESPACIOS NI GUIONES.", txt));
                        }
                        if (!string.IsNullOrEmpty(TUCE))
                        {
                            if (string.IsNullOrWhiteSpace(TUCE) || TUCE.Length < 10 || TUCE.Length > 15 || !TUCE.All(char.IsLetterOrDigit))
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "23",
                                    "El campo Tarjeta Única de Circulación Electrónica es de 10 a 15 caracteres sin espacios ni guiones.",
                                    txt));
                            }
                        }
                    }
                    #endregion

                    #region GPPYL - PUNTO PARTIDA_LLEGA
                    if (line[0].Trim().ToUpper() == "GPPYL")
                    {
                        var ubigeoLlegada = line.Length > 3 ? line[3]?.Trim() : "";
                        var ubigeoPartida = line.Length > 1 ? line[1]?.Trim() : "";
                        var direccionPartida = line.Length > 2 ? line[2]?.Trim() : "";
                        var direccionLlegada = line.Length > 4 ? line[4]?.Trim() : "";

                        var rucPuntoPartida = line.Length > 5 ? line[5]?.Trim() : "";
                        var codigoEstablecimientoPartida = line.Length > 6 ? line[6]?.Trim() : "";
                        var rucPuntoLlegada = line.Length > 8 ? line[8]?.Trim() : "";
                        var codigoEstablecimientoLlegada = line.Length > 9 ? line[9]?.Trim() : "";

                        var existeUbi = new brConsultar().ObtenerUbigeo(ubigeoLlegada);
                        if (existeUbi.Trim() == "")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "4", "UBIGEO DE LLEGADA NO EXISTE. :: ID - GPPYL.", txt));
                        }
                        if (line.Length < 5)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "16", "LINEA GPPYL INCOMPLETA REVISAR CAMPOS SEGUN EL MANUAL.", txt));
                        }
                        if (rucPuntoLlegada != "" || rucPuntoPartida != "")
                        {
                            if (rucPuntoLlegada.Length != 11 || rucPuntoPartida.Length != 11)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "19", "LA LÍNEA GPPYL DEBE SER UN RUC ASOCIADO AL PUNTO DE PARTIDA / LLEGADA.", txt));
                            }
                        }
                    }
                    #endregion

                    #region DESTINATARIO
                    if (line[0].Trim().ToUpper() == "DESTINATARIO")
                    {
                        existeDestinatario = true;
                        var tipoDocDestinatario = line[1]?.Trim();
                        var numeroDoc = line[2]?.Trim();
                        if (!UTilidades.TipoDocumentoIdentidad(tipoDocDestinatario))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                        "14", "TIPO DE DOCUMENTO DEL DESTINATARIO NO EXISTE.", txt));
                        }
                        if (tipoDocDestinatario == "6" &&
                        (numeroDoc.Length != 11 || !numeroDoc.All(char.IsDigit)))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "17",
                                "SI EL TIPO DE DOCUMENTO DE IDENTIDAD ES 6 = RUC, DEBE TENER 11 CARACTERES Y SOLO SE ACEPTAN NÚMEROS (LÍNEA DESTINATARIO).",
                                txt));
                        }
                        if (tipoDocDestinatario == "1" &&
                        (numeroDoc.Length != 8 || !numeroDoc.All(char.IsDigit)))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "792",
                                "SI EL TIPO DE DOCUMENTO DE IDENTIDAD ES 1 = DNI, DEBE TENER 8 CARACTERES Y SOLO SE ACEPTAN NÚMEROS (LÍNEA DESTINATARIO).",
                                txt));
                        }
                    }
                    #endregion

                    #region CAMPO_ADICIONAL
                    if (line[0].Trim().ToUpper() == "CAMPOADICIONAL")
                    {
                        var campo = line[1];
                        if (!string.IsNullOrEmpty(campo) && !Regex.IsMatch(campo, @"^[^:]+:\s?.+"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "15", $"CAMPO ADICIONAL {j + 1} MAL ESTRUCTURADO.", txt));
                        }
                    }
                    #endregion

                    #region TRANSPORTISTA
                    if (line[0].Trim().ToUpper() == "GDTP")
                    {
                        var tipoDoc = line[1];
                        var numeroDoc = line[2];

                        if (tipoDoc.Trim() != "6")
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "22", "EL TIPO DE DOCUMENTO DEL TRANSPORTISTA SOLO PUEDE SER RUC .", txt));
                        }
                        //if (lCabecera[3].Trim() == numeroDoc)
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                        //        "24", "El remitente no puede ser igual al ruc del transportista.", txt));
                        //}
                    }
                    #endregion

                    #region DOC_REL
                    if (line[0].Trim().ToUpper() == "GDOCREF")
                    {
                        var codigoDocRelacionado = line[1]?.Trim();
                        var numDocRelacionado = line[3]?.Trim();
                        var tipoDocRel = line.Length > 3 ? line[4].Trim() : "";
                        var codigoDoc = new string[] { "01", "03", "04", "12", "48", "09", "31" };

                        if (codigoDoc.Contains(codigoDocRelacionado) &&
                            !string.IsNullOrWhiteSpace(numDocRelacionado) && tipoDocRel != "6" && numDocRelacionado.Length == 11 &&
                            (numDocRelacionado.StartsWith("10") || numDocRelacionado.StartsWith("20")) &&
                            numDocRelacionado.All(char.IsDigit))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "27",
                                "GDOCREF - Si el 'Código del tipo de documento relacionado' es '01', '03', '04', '12', '48', '09', '31', el Tipo de documento del emisor del documento relacionado debe ser Número de RUC.",
                                txt));
                        }
                    }
                    #endregion

                    #region SUBCONTRATO
                    if (line[0].Trim().ToUpper() == "SUBCONTRATO")
                    {
                        existeSubcontrato = true;
                    }
                    #endregion

                    #region PAGOSERVICIO
                    if (line[0].Trim().ToUpper() == "PAGOSERVICIO")
                    {
                        existePagoServicio = true;
                    }
                    #endregion
                }


                if (!existeDestinatario)
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                        "6", "LINEA DE DESTINATARIO ES OBLIGATORIO.", txt));
                }

                if (campo18 == "1" && !existeSubcontrato)
                {
                    lRechazos.AddRange(await AgregarRechazo(
                        lCabecera,
                        "25",
                        "SI EL CAMPO 18 DE LA CABECERA ES VALOR 1 ENTONCES DEBE ENVIARSE LA LINEA SUBCONTRATO.",
                        txt));
                }
                if (campo23 == "1" && !existePagoServicio)
                {
                    lRechazos.AddRange(await AgregarRechazo(
                        lCabecera,
                        "26",
                        "SI EL CAMPO 23 DE LA CABECERA ES VALOR 1 ENTONCES DEBE ENVIARSE LA LINEA PAGOSERVICIO.",
                        txt));
                }
            }
            catch (Exception ex)
            {
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
