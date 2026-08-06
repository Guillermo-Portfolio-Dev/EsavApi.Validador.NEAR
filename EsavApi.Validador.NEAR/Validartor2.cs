using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.ComunicacionBaja;
using EsavApi.Validador.NEAR.BE.ResumenDiario;
using EsavApi.Validador.NEAR.BR;
using EsavApi.Validador.NEAR.BR.Boleta;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.BR.ComunicacionBaja;
using EsavApi.Validador.NEAR.BR.DocumentoCobranza;
using EsavApi.Validador.NEAR.BR.Factura;
using EsavApi.Validador.NEAR.BR.GuiaRemision;
using EsavApi.Validador.NEAR.BR.GuiaRemisionTransportista;
using EsavApi.Validador.NEAR.BR.NotaCredito;
using EsavApi.Validador.NEAR.BR.NotaDebito;
using EsavApi.Validador.NEAR.BR.ResumenDiario;
using EsavApi.Validador.NEAR.BR.ValeCredito;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace EsavApi.Validador.NEAR
{
    public class Validartor2 : brGenerico
    {
        private static readonly int TiempoDeEspera = int.Parse(ConfigurationManager.AppSettings["TiempoDeEspera"]);
        private static readonly string cola = ConfigurationManager.AppSettings["NombreCola"];
        private bool EnProceso = false;


        bool GetOcupado(string typeDoc)
        {
            switch (typeDoc)
            {
                case "00": return EnProceso;
                default: return false;
            }
        }
        void SetOcupado(string typeDoc, bool state)
        {
            switch (typeDoc)
            {
                case "00": EnProceso = state; break;
                default: break;
            }
        }

        public void Inicio()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            Task.Run(async () => await Validar());
            Console.Read();
        }
        public async Task Validar()
        {

            while (true)
            {
                LogMensaje($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} => Iniciando Proceso...", ConsoleColor.Cyan);

                var msgcola = await new brCola().RecepcionColaAsync2($"{cola}");

                if (msgcola != null && msgcola.Any())
                {
                    var tareas = msgcola.Select(async item =>
                    {
                        try
                        {
                            var queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"{cola}".ToLower());
                            var (lineas, contenidoTexto) = ObtenerLineasDesdeMensajeAsync(item);
                            if (lineas == null)
                            {
                                await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                                return;
                            }

                            var nametxt = contenidoTexto.Split('\\');
                            string correlativoTxt = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? nametxt[5].Substring(18, 10) : nametxt[5].Substring(15, 10);

                            string tipoDoc;
                            int idx = nametxt[5].IndexOf("ANU");
                            if (idx >= 0)
                            {
                                tipoDoc = nametxt[5].Substring(idx, 3);
                            }
                            else
                            {
                                idx = nametxt[5].IndexOf("anu");
                                if (idx >= 0)
                                    tipoDoc = nametxt[5].Substring(idx, 3);
                                else
                                {
                                    string resto = nametxt[5].Substring(33);
                                    string tipo2 = resto.Substring(0, 2);
                                    string tipo3 = resto.Substring(0, 3);
                                    var tiposValidos = new HashSet<string> { "01", "03", "07", "08", "09", "31", "99", "100", "101", "105" };
                                    tipoDoc = tiposValidos.Contains(tipo3) ? tipo3 : tipo2;
                                }
                            }

                            var lCabecera = lineas[0].Split('|');

                            var RowKey = tipoDoc.ToUpper() != "ANU" && tipoDoc != "09" && tipoDoc != "31" ?
                                $"{lCabecera[4].Trim()}-{lCabecera[5].Trim()}-{lCabecera[21].Trim()}-{lCabecera[22].Trim()}" : "";

                            if (tipoDoc.ToUpper() == "ANU" && item.DequeueCount == 1)
                            {
                                LogMensaje($"Documento Baja ({lCabecera[4]}-{lCabecera[6]}-{lCabecera[7]}) se procesará en 1H... | {DateTime.Now:dd-MM-yyyy HH:mm:ss}", ConsoleColor.Yellow);
                                await queue.UpdateMessageAsync(
                                    item.MessageId,
                                    item.PopReceipt,
                                    item.MessageText,
                                    visibilityTimeout: TimeSpan.FromHours(1)).ConfigureAwait(false);
                                return;
                            }

                            var rechazos = await new brValidador().ValidarTXT(lineas, tipoDoc, correlativoTxt, nametxt);
                            bool seProceso = false;

                            if (rechazos.Any())
                            {
                                await ProcesarRechazosAsync(rechazos, nametxt[1]);
                                if (lCabecera.Length > 11)
                                {
                                    if (tipoDoc.Trim() == "09")
                                    {
                                        var RowKeyG = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{lCabecera[14].Trim()}-{lCabecera[15].Trim()}";
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKeyG, rechazos);
                                    }
                                    else if (tipoDoc.Trim() == "31")
                                    {
                                        //ruc-sede-serie-numero
                                        var RowKeyG = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{lCabecera[11].Trim()}-{lCabecera[12].Trim()}";
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKeyG, rechazos);
                                    }
                                    else
                                    {
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKey, rechazos);
                                    }
                                }
                                else
                                {
                                    DateTime.TryParse(lCabecera[5], out DateTime FE);
                                    var rowKeyBaja = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{FE.ToString("yyyyMMdd")}-1";
                                    await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            rowKeyBaja, rechazos);
                                }
                            }
                            else
                            {
                                if (tipoDoc.Trim() == "09")
                                {
                                    await GuardarRechazoOtrAsync(nametxt, lCabecera);
                                }
                                else if (tipoDoc.Trim() == "31")
                                {
                                    await GuardarRechazoGTAsync(nametxt, lCabecera);
                                }
                                else
                                {
                                    await GuardarRechazoAsync(nametxt, lCabecera);
                                }

                                if (lCabecera.Length > 11)
                                {
                                    if (tipoDoc.Trim() == "09")
                                    {
                                        var RowKeyG = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{lCabecera[14].Trim()}-{lCabecera[15].Trim()}";
                                        LogMensaje($"Documento Válido ({lCabecera[3]}-{lCabecera[14]}-{lCabecera[15]}) Iniciando Registro... | {DateTime.Now:dd-MM-yyyy HH:mm:ss}", ConsoleColor.Yellow);
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKeyG, rechazos);
                                    }
                                    else if (tipoDoc.Trim() == "31")
                                    {
                                        var RowKeyG = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{lCabecera[11].Trim()}-{lCabecera[12].Trim()}";
                                        LogMensaje($"Documento Válido ({lCabecera[3]}-{lCabecera[11]}-{lCabecera[12]}) Iniciando Registro... | {DateTime.Now:dd-MM-yyyy HH:mm:ss}", ConsoleColor.Yellow);
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKeyG, rechazos);
                                    }
                                    else
                                    {
                                        LogMensaje($"Documento Válido ({lCabecera[4]}-{lCabecera[21]}-{lCabecera[22]}) Iniciando Registro... | {DateTime.Now:dd-MM-yyyy HH:mm:ss}", ConsoleColor.Yellow);
                                        await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            RowKey, rechazos);
                                    }

                                }
                                else
                                {
                                    DateTime.TryParse(lCabecera[5], out DateTime FE);
                                    var rowKeyBaja = $"{lCabecera[3].Trim()}-{lCabecera[4].Trim()}-{FE.ToString("yyyyMMdd")}-1";
                                    LogMensaje($"Documento Baja Válido ({lCabecera[4]}-{lCabecera[6]}-{lCabecera[7]}) Iniciando Registro... | {DateTime.Now:dd-MM-yyyy HH:mm:ss}", ConsoleColor.Yellow);
                                    await new brStorage().EliminarRegistrosPorRowKeyPrefix(
                                            $"Rechazos{nametxt[1]}",
                                            rowKeyBaja, rechazos);
                                }

                                seProceso = await ProcesarDocumentoValidoAsync(lineas, tipoDoc, nametxt[5]);
                            }

                            try
                            {
                                if (tipoDoc.ToUpper() == "ANU")
                                {
                                    DateTime fechaHoraActual = DateTime.Now;
                                    var error = rechazos.Any(x => x.CodigoRechazo == "1");
                                    if (error && item.DequeueCount > 1)
                                    {
                                        DateTime fechaHoraMensaje = item.InsertedOn.Value.DateTime;
                                        TimeZoneInfo peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                                        DateTime fechaHoraMensajePeru = TimeZoneInfo.ConvertTimeFromUtc(fechaHoraMensaje, peruTimeZone);

                                        TimeSpan tiempoEnCola = fechaHoraActual - fechaHoraMensajePeru;

                                        if (tiempoEnCola.TotalHours <= 24)
                                        {
                                            await queue.UpdateMessageAsync(
                                            item.MessageId,
                                            item.PopReceipt,
                                            item.MessageText,
                                            visibilityTimeout: TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                                            return;
                                        }
                                        else
                                        {
                                            await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                                        }
                                    }
                                    else
                                    {
                                        await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                                    }
                                }
                                else
                                {
                                    if (seProceso || rechazos.Count > 0)
                                    {
                                        await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                                    }
                                    //else
                                    //{
                                    //    await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {

                                _ = LogAsync($"EliminarMensajeCola - {contenidoTexto}", ex);
                            }
                        }
                        catch (Exception)
                        {
                            await new brCola().EliminarMensajeColaAsync($"{cola}", item.MessageId, item.PopReceipt);
                            //await LogAsync("ValidarMsg", ex);
                        }
                    });

                    await Task.WhenAll(tareas);
                }
                else
                {
                    LogMensaje($"Cola {cola} vacía", ConsoleColor.Gray);
                    await Task.Delay(TiempoDeEspera * 1000);
                }
            }
        }

        private (string[] lineas, string contenido) ObtenerLineasDesdeMensajeAsync(dynamic mensaje)
        {
            if (mensaje?.Body == null)
            {
                LogMensaje("El mensaje no contiene un cuerpo válido.", ConsoleColor.Red);
                return (null, null);
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(mensaje.Body.ToString());

                using (var stream = new MemoryStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Position = 0;

                    string contenidoTexto = Encoding.UTF8.GetString(bytes);

                    bool existe = DescargaArchivo(stream, "", contenidoTexto);

                    if (!existe)
                    {
                        return (null, null);
                    }

                    stream.Position = 0;

                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string textoLeido = reader.ReadToEnd();

                        // Si el texto leido contiene caracteres incorrectos (como "�" en lugar de la "Ñ"), intenta con otra codificación
                        if (textoLeido.Contains("�"))
                        {
                            // Si hay caracteres corruptos, intenta con Windows-1252
                            stream.Position = 0; // Restablecer la posición del stream
                            using (var reader1252 = new StreamReader(stream, Encoding.GetEncoding("windows-1252")))
                            {
                                textoLeido = reader1252.ReadToEnd();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(textoLeido) || textoLeido.All(c => c == '\0'))
                        {
                            LogMensaje("El contenido descargado está vacío, no es válido o no existe.", ConsoleColor.Red);
                            return (null, null);
                        }

                        // Dividir las líneas del texto
                        string[] lineas = textoLeido.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

                        return (lineas, contenidoTexto);
                    }
                }
            }
            catch (FormatException)
            {
                LogMensaje("El contenido del mensaje no está en formato Base64 válido.", ConsoleColor.Red);
                return (null, null);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al procesar mensaje: {ex.Message}", ConsoleColor.Red);
                return (null, null);
            }
        }

        private async Task<bool> ProcesarRechazosAsync(List<beRechazo> rechazos, string RucEmisor)
        {
            int guardados = 0;
            int errores = 0;

            var tareas = rechazos.Select(async rechazo =>
            {
                try
                {
                    return await new brStorage().GuardarRechazos(rechazo, RucEmisor);
                }
                catch
                {
                    return false;
                }
            });

            var resultados = await Task.WhenAll(tareas);

            guardados = resultados.Count(r => r);
            errores = resultados.Length - guardados;

            var primerRechazo = rechazos.FirstOrDefault();
            string mensaje = errores == 0
                ? $"Documento {(primerRechazo?.Txt?.ToUpper().Contains("ANU") ?? false ? "Baja" : "")} Rechazado: {primerRechazo?.RUC}-{primerRechazo?.Serie}-{primerRechazo?.Numero}, {rechazos?.Count ?? 0} Rechazos."
                : $"Se procesaron {rechazos.Count} rechazos. Éxitos: {guardados}, Errores: {errores}.";

            LogMensaje(mensaje, errores == 0 ? ConsoleColor.Red : ConsoleColor.Yellow);

            return errores == 0;
        }
        private async Task<bool> GuardarRechazoAsync(string[] nametxt, string[] lCabecera)
        {
            DateTime.TryParse(lCabecera[6], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
            DateTime.TryParse(lCabecera[5], out DateTime FE);
            var rechazo = new beRechazo
            {
                RUC = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? lCabecera[3] : lCabecera[4],
                Sede = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? lCabecera[4] : lCabecera[5],
                Serie = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? FE.ToString("yyyyMMdd") : lCabecera[21],
                Numero = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? "1" : lCabecera[22],
                Txt = nametxt[5],
                TipoMoneda = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? "" : lCabecera[8],
                TipoDoc = lCabecera[2],
                FechaEmision = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? FE : fechaEmision,
                FechaTransferencia = DateTime.Now,
                CodigoRechazo = "000",
                Descripcion = nametxt[5].Contains("ANU") || nametxt[5].Contains("anu") ? $"El documento {lCabecera[6]}-{lCabecera[7]} ha sido dado de baja" : "El documento ha sido aceptado."
            };

            bool guardado = await new brStorage().GuardarRechazos(rechazo, nametxt[1]);

            return guardado;
        }
        private async Task<bool> GuardarRechazoOtrAsync(string[] nametxt, string[] lCabecera)
        {
            DateTime.TryParse(lCabecera[5], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
            var rechazo = new beRechazo
            {
                RUC = lCabecera[3],
                Sede = lCabecera[4],
                Serie = lCabecera[14],
                Numero = lCabecera[15],
                Txt = nametxt[5],
                TipoMoneda = lCabecera[6],
                TipoDoc = lCabecera[2],
                FechaEmision = fechaEmision,
                FechaTransferencia = DateTime.Now,
                CodigoRechazo = "000",
                Descripcion = "El documento ha sido aceptado."
            };

            bool guardado = await new brStorage().GuardarRechazos(rechazo, nametxt[1]);

            return guardado;
        }
        private async Task<bool> GuardarRechazoGTAsync(string[] nametxt, string[] lCabecera)
        {
            DateTime.TryParse(lCabecera[5], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaEmision);
            var rechazo = new beRechazo
            {
                RUC = lCabecera[3],
                Sede = lCabecera[4],
                Serie = lCabecera[11],
                Numero = lCabecera[12],
                Txt = nametxt[5],
                TipoMoneda = lCabecera[6],
                TipoDoc = lCabecera[2],
                FechaEmision = fechaEmision,
                FechaTransferencia = DateTime.Now,
                CodigoRechazo = "000",
                Descripcion = "El documento ha sido aceptado."
            };

            bool guardado = await new brStorage().GuardarRechazos(rechazo, nametxt[1]);

            return guardado;
        }
        private async Task<bool> ProcesarDocumentoValidoAsync(string[] lineas, string tipoDoc, string nombreTxt)
        {
            string comprobante = string.Empty;

            var procesadores = new Dictionary<string, Func<Task<string>>>
                    {
                        { "07", () => GuardarNotaCreditoAsync(lineas) },
                        { "08", () => GuardarNotaDebitoAsync(lineas) },
                        { "09", () => GuardarGuiaRemisionAsync(lineas) },
                        { "31", () => GuardarGuiaTransportistaAsync(lineas) },
                        { "03", () => GuardarBoletaAsync(lineas) },
                        { "01", () => GuardarFacturaAsync(lineas) },
                        { "99", () => GuardarDocumentoCobranzaAsync(lineas) },
                        { "105", () => GuardarValeCreditoAsync(lineas) },
                        { "ANU", () => GuardarAnulacionAsync(lineas) }
                    };

            if (procesadores.TryGetValue(tipoDoc.ToUpper(), out var procesador))
            {
                comprobante = await procesador();
            }

            bool guardado = !string.IsNullOrEmpty(comprobante);
            string resultado = guardado ? "Comprobante Aceptado" : "Error al Generar Json";
            LogMensaje($"{resultado}: {comprobante}", guardado ? ConsoleColor.Green : ConsoleColor.Red);

            if (!guardado)
                _ = GuardarLogTxt($"NO SE GENERO JSON => {nombreTxt}", tipoDoc);

            return guardado;
        }
        private async Task<string> GuardarNotaCreditoAsync(string[] lineas)
        {
            try
            {
                var notaCredito = await new brDescomponerNotaCredito().DescomponerNotaCredito(lineas);
                if (notaCredito != null)
                {
                    string fecha1 = notaCredito.eCabecera.fechaEmision.Substring(0, 10);
                    var dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-PE"));
                    notaCredito.eCabecera.fechaEmision = dt1.ToString("dd/MM/yyyy");
                    /*F_vencimiento*/
                    string fecha2 = notaCredito.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-PE"));
                    notaCredito.eCabecera.fechaVencimiento = dt2.ToString("dd/MM/yyyy");

                    if (notaCredito.eCabecera.FechaPago != null)
                    {
                        string fecha3 = notaCredito.eCabecera.FechaPago.Substring(0, 10);
                        DateTime dt3 = DateTime.Parse(fecha3, CultureInfo.GetCultureInfo("es-PE"));
                        notaCredito.eCabecera.FechaPago = dt2.ToString("dd/MM/yyyy");
                    }

                    var nombre = $"{notaCredito.eCabecera.rucEmisor}-{notaCredito.eCabecera.tipoDocEmision}-{notaCredito.eCabecera.serie}-{notaCredito.eCabecera.numero}";
                    var ruta = $"PE/{notaCredito.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{notaCredito.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string ncJson = new JavaScriptSerializer().Serialize(notaCredito);
                    byte[] ncBytes = Ansi1252.GetBytes(ncJson);

                    using (var memoryStream = new MemoryStream(ncBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "07");
                }

                string comprobante = $"{notaCredito.eCabecera.rucEmisor}-{notaCredito.eCabecera.serie}-{notaCredito.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la nota de crédito: {ex.Message}", ConsoleColor.Red);
                return ("");
            }
        }
        private async Task<string> GuardarNotaDebitoAsync(string[] lineas)
        {
            try
            {
                var notaDebito = await new brDescomponerNotaDebito().DescomponerNotaDebito(lineas);
                if (notaDebito != null)
                {
                    string fecha1 = notaDebito.eCabecera.fechaEmision.Substring(0, 10);
                    var dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-PE"));
                    notaDebito.eCabecera.fechaEmision = dt1.ToString("dd/MM/yyyy");
                    /*F_vencimiento*/
                    string fecha2 = notaDebito.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-PE"));
                    notaDebito.eCabecera.fechaVencimiento = dt2.ToString("dd/MM/yyyy");

                    var nombre = $"{notaDebito.eCabecera.rucEmisor}-{notaDebito.eCabecera.tipoDocEmision}-{notaDebito.eCabecera.serie}-{notaDebito.eCabecera.numero}";
                    var ruta = $"PE/{notaDebito.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{notaDebito.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string ncJson = new JavaScriptSerializer().Serialize(notaDebito);
                    byte[] ncBytes = Ansi1252.GetBytes(ncJson);

                    using (var memoryStream = new MemoryStream(ncBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "08");
                }

                string comprobante = $"{notaDebito.eCabecera.rucEmisor}-{notaDebito.eCabecera.serie}-{notaDebito.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la nota de Debito: {ex.Message}", ConsoleColor.Red);
                return ("");
            }
        }
        private async Task<string> GuardarGuiaRemisionAsync(string[] lineas)
        {
            try
            {
                var guiaRemision = await new brDescomponerGuiaRemision().DescomponerGuiaRemitenteV2(lineas);

                if (guiaRemision != null)
                {
                    /*F_emision*/
                    string fecha1 = guiaRemision.eRemitente.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    guiaRemision.eRemitente.fechaEmision = dt1.ToString("dd/MM/yyyy");
                    /*F_TRASLADO*/
                    string traslado = guiaRemision.eRemitente.fechaInicioTrasladoPrivado.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(traslado, CultureInfo.GetCultureInfo("es-ES"));
                    guiaRemision.eRemitente.fechaInicioTrasladoPrivado = dt2.ToString("dd/MM/yyyy");
                    /*F_ENTREGA*/
                    string entrega = guiaRemision.eRemitente.fechaEntregaBienesEmpresaTransporte.Substring(0, 10);
                    DateTime dt3 = DateTime.Parse(entrega, CultureInfo.GetCultureInfo("es-ES"));
                    guiaRemision.eRemitente.fechaEntregaBienesEmpresaTransporte = dt3.ToString("dd/MM/yyyy");

                    var nombre = $"{guiaRemision.eRemitente.rucEmisor}-{guiaRemision.eRemitente.tipoDocEmision}-{guiaRemision.eRemitente.serie}-{guiaRemision.eRemitente.numero}";
                    var ruta = $"PE/{guiaRemision.eRemitente.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{guiaRemision.eRemitente.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string guiaJson = new JavaScriptSerializer().Serialize(guiaRemision);
                    byte[] guiaBytes = Ansi1252.GetBytes(guiaJson);

                    using (var memoryStream = new MemoryStream(guiaBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "09");
                }

                string comprobante = $"{guiaRemision.eRemitente.rucEmisor}-{guiaRemision.eRemitente.serie}-{guiaRemision.eRemitente.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la guía de remisión: {ex.Message}", ConsoleColor.Red);
                return (ex.Message);
            }
        }
        private async Task<string> GuardarGuiaTransportistaAsync(string[] lineas)
        {
            try
            {
                var guiaTransportista = await new brDescomponerGuiaTransportista().DescomponerGuiaTransportista(lineas);

                if (guiaTransportista != null)
                {
                    /*F_emision*/
                    string fecha1 = guiaTransportista.eTransportista.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    guiaTransportista.eTransportista.fechaEmision = dt1.ToString("dd/MM/yyyy");
                    /*F_TRASLADO*/
                    if (guiaTransportista.eTransportista.fechaInicioTrasladoPrivado != null)
                    {
                        string traslado = guiaTransportista.eTransportista.fechaInicioTrasladoPrivado.Substring(0, 10);
                        DateTime dt2 = DateTime.Parse(traslado, CultureInfo.GetCultureInfo("es-ES"));
                        guiaTransportista.eTransportista.fechaInicioTrasladoPrivado = dt2.ToString("dd/MM/yyyy");
                    }

                    /*F_ENTREGA*/
                    if (guiaTransportista.eTransportista.fechaEntregaBienesEmpresaTransporte != null)
                    {
                        string entrega = guiaTransportista.eTransportista.fechaEntregaBienesEmpresaTransporte.Substring(0, 10);
                        DateTime dt3 = DateTime.Parse(entrega, CultureInfo.GetCultureInfo("es-ES"));
                        guiaTransportista.eTransportista.fechaEntregaBienesEmpresaTransporte = dt3.ToString("dd/MM/yyyy");
                    }

                    var nombre = $"{guiaTransportista.eTransportista.rucEmisor}-{guiaTransportista.eTransportista.tipoDocEmision}-{guiaTransportista.eTransportista.serie}-{guiaTransportista.eTransportista.numero}";
                    var ruta = $"PE/{guiaTransportista.eTransportista.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{guiaTransportista.eTransportista.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string guiaJson = new JavaScriptSerializer().Serialize(guiaTransportista);
                    byte[] guiaBytes = Ansi1252.GetBytes(guiaJson);

                    using (var memoryStream = new MemoryStream(guiaBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "31");
                }

                string comprobante = $"{guiaTransportista.eTransportista.rucEmisor}-{guiaTransportista.eTransportista.serie}-{guiaTransportista.eTransportista.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la guía de transportista: {ex.Message}", ConsoleColor.Red);
                return ("");
            }
        }
        private async Task<string> GuardarBoletaAsync(string[] lineas)
        {
            try
            {
                var boleta = await new brDescomponerBoleta().DescomponerBoleta(lineas);

                if (boleta != null)
                {
                    string fecha1 = boleta.eCabecera.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    boleta.eCabecera.fechaEmision = dt1.ToString("dd/MM/yyyy");

                    string fecha2 = boleta.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-ES"));
                    boleta.eCabecera.fechaVencimiento = dt2.ToString("dd/MM/yyyy");

                    var nombre = $"{boleta.eCabecera.rucEmisor}-{boleta.eCabecera.tipoDocEmision}-{boleta.eCabecera.serie}-{boleta.eCabecera.numero}";
                    var ruta = $"PE/{boleta.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{boleta.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string boletaJson = new JavaScriptSerializer().Serialize(boleta);
                    byte[] boletaBytes = Ansi1252.GetBytes(boletaJson);

                    using (var memoryStream = new MemoryStream(boletaBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "03");
                }

                string comprobante = boleta == null ? "" : $"{boleta.eCabecera.rucEmisor}-{boleta.eCabecera.serie}-{boleta.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la boleta: {ex.Message}", ConsoleColor.Red);
                return ("GuardarBoletaAsync");
            }
        }
        private async Task<string> GuardarDocumentoCobranzaAsync(string[] lineas)
        {
            try
            {
                var documentoCobranza = await new brDescomponerDocumentoCobranza().DescomponerDocumentoCobranza(lineas);

                if (documentoCobranza != null)
                {
                    string fecha1 = documentoCobranza.eCabecera.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    documentoCobranza.eCabecera.fechaEmision = dt1.ToString("dd-MM-yyyy");

                    string fecha2 = documentoCobranza.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-ES"));
                    documentoCobranza.eCabecera.fechaVencimiento = dt2.ToString("dd-MM-yyyy");

                    var nombre = $"{documentoCobranza.eCabecera.rucEmisor}-{documentoCobranza.eCabecera.tipoDocEmision}-{documentoCobranza.eCabecera.serie}-{documentoCobranza.eCabecera.numero}";
                    var ruta = $"PE/{documentoCobranza.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{documentoCobranza.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string boletaJson = new JavaScriptSerializer().Serialize(documentoCobranza);
                    byte[] boletaBytes = Ansi1252.GetBytes(boletaJson);

                    using (var memoryStream = new MemoryStream(boletaBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "99");
                }

                string comprobante = documentoCobranza == null ? "" : $"{documentoCobranza.eCabecera.rucEmisor}-{documentoCobranza.eCabecera.serie}-{documentoCobranza.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la DC: {ex.Message}", ConsoleColor.Red);
                return ("GuardarDocumentoCobranzaAsync");
            }
        }
        private async Task<string> GuardarValeCreditoAsync(string[] lineas)
        {
            try
            {
                var valeCredito = await new brDescomponerValeCredito().DescomponerValeCredito(lineas);

                if (valeCredito != null)
                {
                    string fecha1 = valeCredito.eCabecera.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    valeCredito.eCabecera.fechaEmision = dt1.ToString("dd-MM-yyyy");

                    string fecha2 = valeCredito.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-ES"));
                    valeCredito.eCabecera.fechaVencimiento = dt2.ToString("dd-MM-yyyy");

                    var nombre = $"{valeCredito.eCabecera.rucEmisor}-{valeCredito.eCabecera.tipoDocEmision}-{valeCredito.eCabecera.serie}-{valeCredito.eCabecera.numero}";
                    var ruta = $"PE/{valeCredito.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{valeCredito.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);

                    string boletaJson = new JavaScriptSerializer().Serialize(valeCredito);
                    byte[] boletaBytes = Ansi1252.GetBytes(boletaJson);

                    using (var memoryStream = new MemoryStream(boletaBytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", memoryStream);
                    }

                    // Enviar el registro a la base de datos
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "105");
                }

                string comprobante = valeCredito == null ? "" : $"{valeCredito.eCabecera.rucEmisor}-{valeCredito.eCabecera.serie}-{valeCredito.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la VC: {ex.Message}", ConsoleColor.Red);
                return ("GuardarValeCreditoAsync");
            }
        }
        private async Task<string> GuardarFacturaAsync(string[] lineas)
        {
            try
            {
                var factura = await new brDescomponerFactura().DescomponerFactura(lineas);
                if (factura != null)
                {
                    // F_emision
                    string fecha1 = factura.eCabecera.fechaEmision.Substring(0, 10);
                    DateTime dt1 = DateTime.Parse(fecha1, CultureInfo.GetCultureInfo("es-ES"));
                    factura.eCabecera.fechaEmision = dt1.ToString("dd/MM/yyyy");

                    // F_vencimiento
                    string fecha2 = factura.eCabecera.fechaVencimiento.Substring(0, 10);
                    DateTime dt2 = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-ES"));
                    factura.eCabecera.fechaVencimiento = dt2.ToString("dd/MM/yyyy");

                    // F_PAGO
                    string fecha3 = factura.eCabecera.FechaPago.Substring(0, 10);
                    DateTime dt3 = DateTime.Parse(fecha3, CultureInfo.GetCultureInfo("es-ES"));
                    factura.eCabecera.FechaPago = dt3.ToString("dd/MM/yyyy"); // ← corregido

                    var nombre = $"{factura.eCabecera.rucEmisor}-{factura.eCabecera.tipoDocEmision}-{factura.eCabecera.serie}-{factura.eCabecera.numero}";
                    var ruta = $"PE/{factura.eCabecera.rucEmisor}/{dt1.Year}/{dt1.Month}/{dt1.Day}/{factura.eCabecera.tipoDocEmision}/{nombre}";

                    System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);
                    var serializer = new JavaScriptSerializer
                    {
                        MaxJsonLength = int.MaxValue
                    };
                    string json = serializer.Serialize(factura);
                    byte[] bytes = Ansi1252.GetBytes(json);

                    using (var stream = new MemoryStream(bytes))
                    {
                        await SubirJson(ruta, $"{nombre}.json", stream);
                    }

                    // Enviar a la cola
                    await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "01");
                }

                string comprobante = factura == null ? "" : $"{factura.eCabecera.rucEmisor}-{factura.eCabecera.serie}-{factura.eCabecera.numero}";

                return (comprobante);
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la factura: {ex.Message}", ConsoleColor.Red);
                return ("GuardarFacturaAsync");
            }
        }
        private async Task<string> GuardarAnulacionAsync(string[] lineas)
        {
            try
            {
                var cabecera = lineas[0].Split('|');
                var RA = new beComunicacionBajaObj();
                string comprobanteRA = "";

                var RC = new beResumenDiarioObj();
                string comprobanteRC = "";

                switch (cabecera[2].Trim())
                {

                    case "07":
                        if (cabecera[6].StartsWith("F"))
                        {
                            RA = await new brDescomponerComunicacionBaja().DescomponerComunicacionBaja(lineas);
                            if (RA != null)
                            {
                                await EnviarRA(RA);
                                comprobanteRA = $"{RA.eCabBaja.rucEmisor}-{RA.eCabBaja.serie}-{RA.eCabBaja.numero}";
                                return (comprobanteRA);
                            }
                            return "";

                        }
                        else
                        {
                            RC = await new brDescomponerResumenDiario().DescomponerResumenDiario(lineas);
                            if (RC != null)
                            {
                                await EnviarRC(RC);
                                comprobanteRC = $"{RC.eCabBaja.rucEmisor}-{RC.eCabBaja.serie}-{RC.eCabBaja.numero}";
                                return (comprobanteRC);
                            }

                            return "";
                        }
                    case "08":
                        if (cabecera[6].StartsWith("F"))
                        {
                            RA = await new brDescomponerComunicacionBaja().DescomponerComunicacionBaja(lineas);
                            if (RA != null)
                            {
                                await EnviarRA(RA);
                                comprobanteRA = $"{RA.eCabBaja.rucEmisor}-{RA.eCabBaja.serie}-{RA.eCabBaja.numero}";
                                return (comprobanteRA);
                            }

                            return "";
                        }
                        else
                        {
                            RC = await new brDescomponerResumenDiario().DescomponerResumenDiario(lineas);
                            if (RC != null)
                            {
                                await EnviarRC(RC);
                                comprobanteRC = $"{RC.eCabBaja.rucEmisor}-{RC.eCabBaja.serie}-{RC.eCabBaja.numero}";
                                return (comprobanteRC);
                            }

                            return "";
                        }
                    case "01":
                        RA = await new brDescomponerComunicacionBaja().DescomponerComunicacionBaja(lineas);
                        if (RA != null)
                        {
                            await EnviarRA(RA);
                            comprobanteRA = $"{RA.eCabBaja.rucEmisor}-{RA.eCabBaja.serie}-{RA.eCabBaja.numero}";
                            return (comprobanteRA);
                        }

                        return "";

                    case "03":
                        RC = await new brDescomponerResumenDiario().DescomponerResumenDiario(lineas);
                        if (RC != null)
                        {
                            await EnviarRC(RC);
                            comprobanteRC = $"{RC.eCabBaja.rucEmisor}-{RC.eCabBaja.serie}-{RC.eCabBaja.numero}";
                            return (comprobanteRC);
                        }

                        return "";

                    default:
                        return "";
                }
            }
            catch (Exception ex)
            {
                LogMensaje($"Error al guardar la anulación: {ex.Message}", ConsoleColor.Red);
                return ("");
            }
        }

        private async Task EnviarRA(beComunicacionBajaObj RA)
        {
            /*F_emision*/
            RA.eCabBaja.fechaEmision = Convert.ToDateTime(RA.eCabBaja.fechaEmision).ToString("dd/MM/yyyy");
            /*F_emision_Documento*/
            RA.eCabBaja.FechaEmisionDocumentos = Convert.ToDateTime(RA.eCabBaja.FechaEmisionDocumentos).ToString("dd/MM/yyyy");

            var nombre = $"{RA.eCabBaja.rucEmisor}-{RA.eCabBaja.tipoDocEmision}-{RA.eCabBaja.serie}-{RA.eCabBaja.numero}";
            var ruta = $"PE/{RA.eCabBaja.rucEmisor}/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/{RA.eCabBaja.tipoDocEmision}/RA-VALIDADOR/{nombre}";

            System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);
            await SubirJson(ruta, $"{nombre}.json", new MemoryStream(Ansi1252.GetBytes(new JavaScriptSerializer().Serialize(RA))));
            await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "ra");
        }
        private async Task EnviarRC(beResumenDiarioObj RC)
        {
            RC.eCabBaja.fechaEmision = Convert.ToDateTime(RC.eCabBaja.fechaEmision).ToString("dd/MM/yyyy");
            RC.eCabBaja.FechaDocumento = Convert.ToDateTime(RC.eCabBaja.FechaDocumento).ToString("dd/MM/yyyy");

            var nombre = $"{RC.eCabBaja.rucEmisor}-{RC.eCabBaja.tipoDocEmision}-{RC.eCabBaja.serie}-{RC.eCabBaja.numero}";
            var ruta = $"PE/{RC.eCabBaja.rucEmisor}/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/{RC.eCabBaja.tipoDocEmision}/RC-VALIDADOR/{nombre}";

            System.Text.Encoding Ansi1252 = System.Text.Encoding.GetEncoding(1252);
            await SubirJson(ruta, $"{nombre}.json", new MemoryStream(Ansi1252.GetBytes(new JavaScriptSerializer().Serialize(RC))));
            await new brCola().EnviarRegistroBD($"{ruta}/{nombre}.json", "rc");
        }
    }
}
