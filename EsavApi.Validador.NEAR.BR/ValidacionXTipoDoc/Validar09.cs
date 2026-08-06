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
    public class Validar09 : brGenerico
    {
        public static async Task<List<beRechazo>> Validar(string[] lineas, string[] lCabecera, string TipoDocNombreTxt, string[] txt)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-Pe");
            List<beRechazo> lRechazos = new List<beRechazo>();
            try
            {
                var configuracion = await new brConfiguracion().Consultar(lCabecera[3].ToString(), lCabecera[4].ToString());

                var DocumentoRelacionado = lineas.Where(x => x.ToUpper().StartsWith("GDOCREF")).ToList();
                var PuntoPartidaLlegada = lineas.Where(x => x.ToUpper().StartsWith("GPPYL")).ToList();
                var LineaTransportista = lineas.Where(x => x.ToUpper().StartsWith("GDTP")).ToList();
                var CampoAdicional = lineas.Where(x => x.ToUpper().StartsWith("CAMPOADICIONAL")).ToList();
                var GMIMP = lineas.Where(x => x.ToUpper().StartsWith("GMIMP")).ToList();
                var Comprador = lineas.Where(x => x.ToUpper().StartsWith("COMPRADOR")).ToList();
                var detalleLines = lineas
                            .Skip(1)
                            .Where(x => Regex.IsMatch(x, @"^\d+\|"))
                            .ToList();
                var lineaConductor = lineas
                                .Where(x =>
                                {
                                    var linea = x.Trim().ToUpper();
                                    return linea.StartsWith("CONDUCTOR");
                                })
                                .ToList();
                var lineaVehiculo = lineas
                                .Where(x =>
                                {
                                    var linea = x.Trim().ToUpper();
                                    return linea.StartsWith("VEHICULO");
                                })
                                .ToList();

                var lineaDestinatario = lineas
                                .Where(x =>
                                {
                                    var linea = x.Trim().ToUpper();
                                    return linea.StartsWith("DESTINATARIO");
                                })
                                .ToList();

                var lineaComprador = lineas
                                .Where(x =>
                                {
                                    var linea = x.Trim().ToUpper();
                                    return linea.StartsWith("COMPRADOR");
                                })
                                .ToList();

                var lineaProveedor = lineas
                                .Where(x =>
                                {
                                    var linea = x.Trim().ToUpper();
                                    return linea.StartsWith("PROVEEDOR");
                                })
                                .ToList();

                #region CABECERA

                if (lCabecera[0] == "210")
                {
                    DateTime FEmision;
                    DateTime FInicioTraslado;
                    string[] formatos = {
                         "dd/MM/yyyy HH:mm:ss",
                        "dd/MM/yy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",
                        "MM/dd/yyyy HH:mm:ss",
                        "MM/dd/yy HH:mm:ss",
                        "dd/MM/yyyy",
                        "d/MM/yyyy HH:mm:ss",
                        "d/MM/yyyy"
                    };
                    DateTime.TryParseExact(lCabecera[5].Trim(), formatos, null, System.Globalization.DateTimeStyles.None, out FEmision);
                    DateTime.TryParseExact(lCabecera[9].Trim(), formatos, null, System.Globalization.DateTimeStyles.None, out FInicioTraslado);
                    if (FInicioTraslado.Date < FEmision.Date)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "773", "FECHA DE INICIO DE TRASLADO NO PUEDE SER MENOR A LA FECHA DE EMISION.", txt));
                    }

                    DateTime hoy = DateTime.Today;
                    DateTime ayer = hoy.AddDays(-1);

                    // 1. VALIDACIÓN DE FECHA DE EMISIÓN (Estricta: Solo ayer y hoy)
                    if (FEmision.Date < ayer || FEmision.Date > hoy)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "727",
                            "FECHA DE EMISION FUERA DE RANGO. SOLO SE ACEPTA DESDE AYER HASTA LA FECHA ACTUAL.",
                            txt));
                    }

                    // 2. VALIDACIÓN DE FECHA DE TRASLADO (Solo ayer y cualquier fecha futura)
                    if (FInicioTraslado.Date < ayer)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "727",
                            "FECHA DE INICIO DE TRASLADO NO VALIDA. SE ACEPTA MÁXIMO 1 DÍA ATRÁS DE LA FECHA ACTUAL.",
                            txt));
                    }

                    var indicador_vehiculo_conductor = lCabecera.Length >= 26 ? lCabecera[25].Trim() : "";

                    if (lCabecera[8] == "01" && indicador_vehiculo_conductor == "1")
                    {
                        if (!LineaTransportista.Any() || !lineaConductor.Any() || !lineaVehiculo.Any())
                        {

                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "844",
                                "SI EL CAMPO 9 ENVIA 01 Y INDICADOR VEHICULO Y CONDUCTOR, SE DEBE ENVIAR LINEA GDTP, CONDUCTOR(TODOS LO CAMPOS) Y VEHICULO(CAMPO 1 Y 2).",
                                txt));
                        }

                        if (lineaVehiculo.Any())
                        {
                            foreach (var item in lineaVehiculo)
                            {
                                // Separamos la cadena utilizando el carácter pipe '|'
                                var camposVehiculo = item.Split('|');

                                // Si el formato correcto es estrictamente "VEHICULO|ABC1234", 
                                // el tamaño del array obtenido debe ser exactamente igual a 2.
                                // Si tiene 3 o más campos (por ejemplo si incluyeron la Tarjeta de Circulación), se rechaza.
                                if (camposVehiculo.Length < 2 || camposVehiculo.Skip(2).Any(campo => !string.IsNullOrWhiteSpace(campo)))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(
                                        lCabecera,
                                        "844",
                                        $"EL VEHICULO SOLO DEBE CONTENER EL CAMPO 1 (ID DIFERENCIAL) Y CAMPO 2 (NÚMERO DE PLACA). SE DETECTÓ INFORMACIÓN EN CAMPOS ADICIONALES.",
                                        txt));
                                }
                            }
                        }
                    }

                    if (lCabecera[8] == "01" && indicador_vehiculo_conductor != "1" && lineaConductor.Any() && lineaVehiculo.Any())
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                                       lCabecera,
                                       "845",
                                       $"si es un transporte público con vehiculo y conductor debe enviar el indicador del campo 26 de la cabecera.".ToUpper(),
                                       txt));
                    }

                    var DataEmisor = new brConsultar().ConsultarDataEmisor(lCabecera[3], lCabecera[14], lCabecera[16], lCabecera[3], lCabecera[2]);
                    if (DataEmisor.serieUsuario == 0 && UTilidades.TipoDocumentos(lCabecera[2]))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "85", "USUARIO NO CONFIGURADO PARA ESTA SERIE.", txt));
                    }

                    if (lCabecera[8].Trim() == "01" && LineaTransportista.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "752",
                            "SI EL TIPO DE TRANSPORTE ES 01, SE DEBE ENVIAR LINEA DE TRANSPORTISTA.",
                            txt));
                    }



                    if (lCabecera[7].Trim() == "02" && lineaProveedor.Count == 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "807",
                            "PARA EL MOTIVO DE TRASLADO 02 SE DEBE ENVIAR LINEA PROVEEDOR.",
                            txt));
                    }

                    var tiposPermitidos = new[] { "1", "4", "6", "7" };

                    if (lCabecera[7].Trim() == "02")
                    {
                        if (lineaProveedor.Count > 0)
                        {
                            string tipoDocProv = lineaProveedor.FirstOrDefault().Split('|')[1].Trim();

                            if (!tiposPermitidos.Contains(tipoDocProv))
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "840",
                                    "SI EL MOTIVO DE TRASLADO ES 02, EN LA LINEA PROVEEDOR CAMPO TIPO DE DOCUMENTO SOLO DEBE SER (1,4,6 Y 7).",
                                    txt));
                            }
                        }
                    }

                    if (lCabecera[7].Trim() == "08" || lCabecera[7].Trim() == "09" || lCabecera[7].Trim() == "19")
                    {
                        if (GMIMP.Any() && lCabecera[21].Trim() != "0" && !string.IsNullOrWhiteSpace(lCabecera[21].Trim()))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                   lCabecera,
                                   "843",
                                   "SOLO PUEDE ENVIAR NUMERO DE BULTOS O NUMERO DE CONTENEDOR, NO AMBOS.",
                                   txt));
                        }
                        //if (lCabecera.Length < 21)
                        //{
                        //    lRechazos.AddRange(await AgregarRechazo(
                        //            lCabecera,
                        //            "839",
                        //            "SI MOTIVO DE TRASLADO ES 08, 09 o 19 ENTONCES DEBE EXISTIR UN NUMERO DE CONTENEDOR O UN NUMERO DE DE BULTOS.",
                        //            txt));
                        //}
                        //else
                        //{
                        //    if (string.IsNullOrEmpty(lCabecera[21].Trim()))
                        //    {
                        //        lRechazos.AddRange(await AgregarRechazo(
                        //            lCabecera,
                        //            "839",
                        //            "SI MOTIVO DE TRASLADO ES 08, 09 o 19 ENTONCES DEBE EXISTIR UN NUMERO DE CONTENEDOR O UN NUMERO DE DE BULTOS.",
                        //            txt));
                        //    }
                        //}
                    }
                    if (lCabecera[7].Trim() != "13" && Comprador.Count > 0)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "822",
                            "EL COMPRADOR DEBE INDICARSE SOLO CON MOTIVO DE TRASLADO 13.",
                            txt));
                    }

                    if (lCabecera.Length > 22)
                    {
                        if (lCabecera[7].Trim() == "13" && string.IsNullOrEmpty(lCabecera[22].Trim()))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "809",
                                "SI EL MOTIVO DE TRASLADO ES OTROS(13), EN EL CAMPO 23 DE LA CABECERA SE TIENE QUE INDICAR LA DESCRIPCIÓN DEL MOTIVO DE TRASLADO.",
                                txt));
                        }

                        if (lCabecera[7].Trim() != "13" && !string.IsNullOrEmpty(lCabecera[22].Trim()))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "812",
                                "EL CAMPO 23 DE LA CABECERA  SOLO SE PUEDE ENVIAR SI EL MOTIVO DE TRASLADO ES OTROS(13).",
                                txt));
                        }
                    }

                    if (lCabecera[7].Trim() == "13")
                    {
                        string campo23 = lCabecera[22].Trim();
                        if (campo23.Length < 3 || !UTilidades.EsAlfanumericoConSlashYEspacio(campo23, 100))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "811",
                                "SI EL MOTIVO DE TRASLADO ES OTROS(13), EN EL CAMPO 23 DE LA CABECERA SOLO SE PERMITEN ENTRE 3 Y 100 CARACTERES ALFANUMERICOS.",
                                txt));
                        }
                    }

                    if ((lCabecera[7].Trim() == "08" || lCabecera[7].Trim() == "09") && GMIMP.Count > 0)
                    {
                        var contenedor = GMIMP.FirstOrDefault().Split('|')[1].Trim();
                        var precinto = GMIMP.FirstOrDefault().Split('|')[4].Trim();

                        if (!string.IsNullOrEmpty(contenedor) && string.IsNullOrEmpty(precinto))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "810",
                            "si motivo de traslado es 08 ó 09 y existe un contenedor, entonces debe ir el numero de precinto.",
                            txt));
                        }

                    }

                    if (lCabecera.Length > 24)
                    {
                        if (lCabecera[24] != "1")
                        {
                            if (lCabecera[8].Trim() == "02" && (lineaConductor.Count == 0 || lineaVehiculo.Count == 0))
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "741",
                                    "CUANDO EL TRANSPORTE ES PRIVADO DEBE HABER AL MENOS UNA LINEA DE VEHICULO Y CONDUCTOR.",
                                    txt));
                            }
                        }

                        if (lCabecera[24] == "1" && lCabecera[8].Trim() != "02")
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "799",
                                "SI EL INDICADOR M1 O L (CAMPO 25 DE LA CABECERA) ES 1, SOLO DEBE SER CON TIPO TRANSPORTE PRIVADO(02).",
                                txt));
                        }

                        if (lCabecera[24] == "1" && lineaConductor.Count > 0 && lineaVehiculo.Count > 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "802",
                                "SI EL INDICADOR M1 O L (CAMPO 25 DE LA CABECERA) ES 1, NO DEBE IR LA LINEA CONDUCTOR NI VEHICULO.",
                                txt));
                        }
                    }

                    if (lCabecera[8].Trim() == "01" && lineaConductor.Count > 0 && lineaVehiculo.Count > 0 && indicador_vehiculo_conductor != "1")
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "798",
                            "SI ES TRANSPORTE PUBLICO NO SE DEBE ENVIAR LINEA CONDUCTOR NI VEHÍCULO.",
                            txt));
                    }


                    if (!UTilidades.CodigoMotivoTraslado(lCabecera[7].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "790",
                            "MOTIVO DE TRASLADO ES OBLIGATORIO, REVISAR EL CATÁLOGO.",
                            txt));
                    }


                    decimal.TryParse(lCabecera[18], out decimal pesoBruto);
                    if (pesoBruto < 0.10M)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "314",
                            "EL PESO BRUTO TOTAL DEBE SER MAYOR A CERO VALOR MINIMO 0.10.",
                            txt));
                    }

                    if (DocumentoRelacionado != null && DocumentoRelacionado.Count > 0)
                    {
                        var primerItem = DocumentoRelacionado.FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(primerItem))
                        {
                            var partes = primerItem.Split('|');
                            if (partes.Length >= 3)
                            {
                                var tipoDoc = partes[1]?.Trim();
                                var docRela = partes[2]?.Trim();

                                if (tipoDoc == "01")
                                {
                                    var regex = new Regex(@"^F[A-Z0-9]{3}-[0-9]{1,8}$|^\(E001\)-[0-9]{1,8}$|^[0-9]{1,4}-[0-9]{1,8}$");
                                    if (!regex.IsMatch(docRela))
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(
                                            lCabecera,
                                            "314",
                                            "SI EL CODIGO DEL DOC. REL. ES 01 DEBE TENER LA SIGUIENTE ESTRUCTURA: " +
                                            "[F][A-Z0-9]{3}-[0-9]{1,8} " +
                                            "O (E001)-[0-9]{1,8} " +
                                            "O [0-9]{1,4}-[0-9]{1,8}.",
                                            txt));
                                    }
                                }

                                if (tipoDoc == "03")
                                {
                                    var regex = new Regex(@"^B[A-Z0-9]{3}-[0-9]{1,8}$|^\(EB01\)-[0-9]{1,8}$|^[0-9]{1,4}-[0-9]{1,8}$");
                                    if (!regex.IsMatch(docRela))
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(
                                            lCabecera,
                                            "314",
                                            "SI EL CODIGO DEL DOC. REL. ES 03 DEBE TENER LA SIGUIENTE ESTRUCTURA: " +
                                            "[B][A-Z0-9]{3}-[0-9]{1,8} " +
                                            "O (EB01)-[0-9]{1,8} " +
                                            "O [0-9]{1,4}-[0-9]{1,8}.",
                                            txt));
                                    }
                                }

                                if (tipoDoc == "09")
                                {
                                    var regex = new Regex(@"^T[A-Z0-9]{3}-[0-9]{1,8}$|^\(EG07\)-[0-9]{1,8}$|^[0-9]{1,4}-[0-9]{1,8}$");
                                    if (!regex.IsMatch(docRela))
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(
                                            lCabecera,
                                            "314",
                                            "SI EL CODIGO DEL DOC. REL. ES 03 DEBE TENER LA SIGUIENTE ESTRUCTURA: " +
                                            "[T][A-Z0-9]{3}-[0-9]{1,8} " +
                                            "O (EG07)-[0-9]{1,8} " +
                                            "O (EG02)-[0-9]{1,8}.",
                                            txt));
                                    }
                                }

                                if (tipoDoc == "12")
                                {
                                    var regex = new Regex(@"^T[A-Z0-9]{3}-[0-9]{1,8}$|^\(EG07\)-[0-9]{1,8}$|^[0-9]{1,4}-[0-9]{1,8}$");
                                    if (!regex.IsMatch(docRela))
                                    {
                                        lRechazos.AddRange(await AgregarRechazo(
                                            lCabecera,
                                            "314",
                                            "SI EL CODIGO DEL DOC. REL. ES 12 DEBE TENER LA SIGUIENTE ESTRUCTURA: " +
                                            "[a-zA-Z0-9-]{1,20}-[a-zA-Z0-9-]{1,20}.",
                                            txt));
                                    }
                                }
                            }
                        }
                    }

                    if (lCabecera[7].Trim() == "08")
                    {
                        if (DocumentoRelacionado.Count > 0)
                        {
                            var NroDam = DocumentoRelacionado.FirstOrDefault()?.Split('|')[2];

                            var regex = new Regex(@"^\d{3}-\d{4}-10-\d{1,6}$");

                            if (!regex.IsMatch(NroDam))
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "315",
                                    "SI ES MOTIVO IMPORTACIÓN EL DOCUMENTO DAM DEBE TENER LA SIGUIENTE ESTRUCTURA  XXX-XXXX-10-XXXXXX.",
                                    txt));
                            }
                        }
                    }

                    if (lCabecera[7].Trim() == "09")
                    {
                        if (DocumentoRelacionado.Count > 0)
                        {
                            var NroDam = DocumentoRelacionado.FirstOrDefault()?.Split('|')[2];

                            var regex = new Regex(@"^\d{3}-\d{4}-40-\d{6}$");

                            if (!regex.IsMatch(NroDam))
                            {
                                lRechazos.AddRange(await AgregarRechazo(
                                    lCabecera,
                                    "316",
                                    "SI ES MOTIVO EXPORTACIÓN EL DOCUMENTO DAM DEBE TENER LA SIGUIENTE ESTRUCTURA  XXX-XXXX-40-XXXXXX.",
                                    txt));
                            }
                        }
                    }

                    string[] tipoTransporte = { "01", "02" };
                    if (!tipoTransporte.Contains(lCabecera[8].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "701",
                            "TIPO DE TRANSPORTE NO EXISTE.",
                            txt));
                    }

                    if (lCabecera[7].Trim() == "08" || lCabecera[7].Trim() == "09")
                    {
                        if (DocumentoRelacionado.Count == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "728", "CUANDO EL MOTIVO ES 08=IMPORTACION o 09=EXPORTACION, DEBE DE TENER DOCUMENTO RELACIONADO(NUMERACION DAM).", txt));
                        }
                        else
                        {
                            var codigo = DocumentoRelacionado.FirstOrDefault().Split('|')[1];
                            var NroDam = DocumentoRelacionado.FirstOrDefault().Split('|')[2];

                            if (codigo.Trim() == "50" && string.IsNullOrEmpty(NroDam))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "728", "CUANDO EL MOTIVO ES 08=IMPORTACION o 09=EXPORTACION, DEBE DE TENER DOCUMENTO RELACIONADO(NUMERACION DAM).", txt));
                            }
                        }
                    }

                    if (DocumentoRelacionado.Count > 0 && (lCabecera[7].Trim() == "01" || lCabecera[7].Trim() == "03"))
                    {
                        var rucDocRel = DocumentoRelacionado.FirstOrDefault().Split('|')[3];

                        if (lCabecera[3].Trim() != rucDocRel)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "800", "EL RUC DEL EMISOR DEL DOCUMENTO RELACIONADO(01,03,12) no corresponde.", txt));
                        }
                    }


                }
                #endregion

                #region DETALLE
                foreach (var line in detalleLines)
                {
                    var parts = line.Split('|');

                    decimal.TryParse(parts[4], out decimal cant);

                    if (cant < 0.001M)
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                                                    lCabecera,
                                                    "313",
                                                    "CANTIDAD DEBE SER MAYOR A CERO VALOR MINIMO 0.001 (SIN REDONDEAR).",
                                                    txt));
                    }

                    if (lCabecera[7].Trim() != "08" && lCabecera[7].Trim() != "09")
                    {
                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(parts[3].Trim());
                        if (string.IsNullOrEmpty(UnidadMedidaText.Item1))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "836", "CODIGO DE UNIDAD DE MEDIDA NO ACEPTADO, REVISAR TABLA UNIDAD DE MEDIDA.", txt));
                        }
                    }


                    if ((lCabecera[7].Trim() == "08" || lCabecera[7].Trim() == "09" || lCabecera[7].Trim() == "19") && DocumentoRelacionado.Count > 0)
                    {
                        var codigoTipoDocRel = DocumentoRelacionado.FirstOrDefault().Split('|')[1];
                        if (codigoTipoDocRel == "50" || codigoTipoDocRel == "52")
                        {
                            var catalogo65 = new brConsultar().Catalogo_65(parts[3].Trim());
                            if (string.IsNullOrEmpty(catalogo65))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "838",
                                    "SI MOTIVO DE TRASLADO ES 08, 09 o 19 Y CODIGO DE TIPO DE DOCUMENTO RELACIONADO ES 50 O 52 ENTONCES LA UNIDAD DE MEDIDA DEBE DE ESTAR EN EL CATALOGO 65.", txt));
                            }
                        }
                    }
                }
                #endregion

                #region CONDUCTOR
                if (lineaConductor.Count > 0)
                {
                    var licencia = lineaConductor.FirstOrDefault()?.Split('|')[6]?.Trim();
                    var tipoDocIdentidad = lineaConductor.FirstOrDefault()?.Split('|')[2]?.Trim();
                    var tipoConductor = lineaConductor.FirstOrDefault()?.Split('|')[1]?.Trim();
                    var nroDocIdentidad = lineaConductor.FirstOrDefault()?.Split('|')[3]?.Trim();
                    var NombreConductor = lineaConductor.FirstOrDefault()?.Split('|')[4]?.Trim();
                    var ApellidoConductor = lineaConductor.FirstOrDefault()?.Split('|')[5]?.Trim();

                    var soloNumeros = Regex.Replace(licencia ?? "", @"\D", "");

                    if (string.IsNullOrWhiteSpace(licencia) ||
                        !Regex.IsMatch(licencia, @"^[A-Za-z0-9\-]+$") ||
                        soloNumeros.All(c => c == '0'))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "773",
                            "LICENCIA DE CONDUCIR SOLO SE PERMITEN LETRAS Y NUMEROS, NO SE ACEPTA SOLO CEROS(0000000000). :: ID – CONDUCTOR.",
                            txt));
                    }

                    if (lCabecera[8].Trim() == "02")
                    {
                        if (tipoConductor.ToUpper() != "PRINCIPAL")
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "804",
                            "CUANDO EL TRANSPORTE ES PRIVADO DEBE EXISTIR UN CONDUCTOR PRINCIPAL.",
                            txt));
                        }
                    }

                    if (string.IsNullOrEmpty(licencia) || string.IsNullOrEmpty(tipoDocIdentidad) || string.IsNullOrEmpty(nroDocIdentidad)
                        || string.IsNullOrEmpty(NombreConductor) || string.IsNullOrEmpty(ApellidoConductor))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                                                    lCabecera,
                                                    "739",
                                                    "LOS DATOS DEL CONDUCTOR ES OBLIGATORIO CUANDO EL TIPO DE TRANSPORTE ES PRIVADO.",
                                                    txt));
                    }

                    if (tipoDocIdentidad == "1")
                    {
                        if (string.IsNullOrEmpty(nroDocIdentidad) || nroDocIdentidad.Length != 8)
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                                    lCabecera,
                                                    "805",
                                                    "SI TIPO DE DOCUMENTO DE CONDUCTOR ES DNI, EL NUMERO DEBE SER DE 8 DIGITOS.",
                                                    txt));
                        }
                    }

                    if (!UTilidades.TipoDocumentoIdentidad(tipoDocIdentidad))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                                                    lCabecera,
                                                    "318",
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
                            "319",
                            "NUMERO DE LICENCIA DE CONDUCIR DE LA LINEA CONDUCTOR DEBE SER ALFANUMERICO ENTRE 9 A 10 CARACTERES.",
                            txt));
                    }
                }
                #endregion

                #region VEHICULO
                if (lineaVehiculo.Count > 0)
                {
                    var placa = lineaVehiculo.FirstOrDefault().Split('|')[1];
                    var TUCE = lineaVehiculo.FirstOrDefault().Length > 2 ? "" : lineaVehiculo.FirstOrDefault().Split('|')[2];

                    if (string.IsNullOrEmpty(placa) || placa.Length < 6 || placa.Length > 8 ||
                        !Regex.IsMatch(placa, @"^[A-Za-z0-9]+$"))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "803",
                            "LA PLACA SOLO PUEDE TENER DE 6 A 8 CARACTERES ALFANUMERICOS, SIN ESPACIOS NI GUIONES.", txt));
                    }

                    if (!string.IsNullOrEmpty(TUCE))
                    {
                        if (string.IsNullOrWhiteSpace(TUCE) || TUCE.Length < 10 || TUCE.Length > 15 || !TUCE.All(char.IsLetterOrDigit))
                        {
                            lRechazos.AddRange(await AgregarRechazo(
                                lCabecera,
                                "813",
                                "El campo Tarjeta Única de Circulación Electrónica es de 10 a 15 caracteres sin espacios ni guiones.",
                                txt));
                        }
                    }


                }
                #endregion

                #region TRANSPORTISTA
                if (LineaTransportista.Count > 0)
                {
                    var tipoDoc = LineaTransportista.FirstOrDefault().Split('|')[1];
                    var nroDoc = LineaTransportista.FirstOrDefault().Split('|')[2];

                    if (tipoDoc.Trim() != "6")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "808", "EL TIPO DE DOCUMENTO DEL TRANSPORTISTA SOLO PUEDE SER RUC .", txt));
                    }
                    var dataRuc = new brConsultar().ObtenerRuc(nroDoc.Trim(), null);
                    if (dataRuc == null ||
    !string.Equals(dataRuc.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase) ||
    !string.Equals(dataRuc.CondicionDomicilio, "HABIDO", StringComparison.OrdinalIgnoreCase))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera, "842", "EL NUMERO DE DOCUMENTO DEL TRANSPORTISTA NO ES VALIDO O NO ESTA ACTIVO HABIDO.", txt));
                    }
                }
                #endregion

                #region PROVEEDOR
                if (lineaProveedor.Count > 0)
                {
                    if (lCabecera[7].Trim() != "02" && lCabecera[7].Trim() != "13")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "814", "EL PROVEEDOR DEBE INDICARSE SOLO CON MOTIVO DE TRASLADO 02 o 13.", txt));
                    }

                    var campos = lineaProveedor.FirstOrDefault().Split('|');
                    if (campos.Length < 3)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "815", "LA LINEA PROVEEDOR NO CUMPLE CON EL FORMATO ESTABLECIDO.", txt));
                        return lRechazos;
                    }

                    if (!UTilidades.TipoDocumentoIdentidad(campos[1].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "816", "EL TIPO DE DOCUMENTO DEL PROVEEDOR NO ESTA EN EL LISTADO.", txt));
                    }

                    if (campos[1].Trim() == "6")
                    {
                        if (new brConsultar().ConsultarRuc(campos[2].Trim()) == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "817", "EL RUC DEL PROVEEDOR NO EXISTE.", txt));
                        }
                    }

                    if (campos[1].Trim() == "1" && campos[2].Trim().Length != 8)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "818", "SI EL TIPO DE DOCUMENTO DEL PROVEEDOR ES 1 ENTONCES EL NUMERO DE DOCUMENTO DEBE TENER 8 DIGITOS.", txt));
                    }
                    if (campos[1].Trim() == "6" && campos[2].Trim().Length != 11)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "819", "SI EL TIPO DE DOCUMENTO DEL PROVEEDOR ES 6 ENTONCES EL NUMERO DE DOCUMENTO DEBE TENER 11 DIGITOS.", txt));
                    }

                    if (UTilidades.TipoDocumentoIdentidad(campos[1].Trim()))
                    {
                        string tipoDoc = campos[1].Trim();
                        if (tipoDoc != "1" && tipoDoc != "6")
                        {
                            string numeroDoc = campos[2].Trim();
                            if (!string.IsNullOrEmpty(numeroDoc))
                            {

                                if (numeroDoc.Length > 15 || !numeroDoc.All(char.IsLetterOrDigit))
                                {
                                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                        "820",
                                        "SI EL TIPO DE DOCUMENTO DEL PROVEEDOR ESTA EN EL LISTADO Y ES DIFERENTE DE 1 Y 6, EL NUMERO DE DOCUMENTO PUEDE TENER HASTA 15 CARACTERES ALFANUMERICOS.",
                                        txt));
                                }

                            }
                            if (string.IsNullOrEmpty(numeroDoc))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "832",
                                    "EL NUMERO DE DOCUMENTO DEL PROVEEDOR NO PUEDE ESTAR VACIO.",
                                    txt));
                            }
                        }
                    }

                    if (campos[3].Trim().Length > 100)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "821", "LA RAZON SOCIAL DEL PROVEEDOR SOLO PUEDE TENER HASTA 100 CARACTERES.", txt));
                    }
                    if (string.IsNullOrEmpty(campos[3].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "830", "EN CASO DE MANDAR LINEA DE PROVEEDOR, LA RAZON SOCIAL DEL PROVEEDOR NO PUEDE ESTAR VACIA.", txt));
                    }
                }
                #endregion

                #region COMPRADOR
                if (Comprador.Count > 0)
                {
                    var linea_comprador = Comprador.FirstOrDefault().Split('|');
                    if (linea_comprador.Length <= 3 || linea_comprador.Length > 4)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "823", "LA LINEA COMPRADOR NO CUMPLE CON EL FORMATO ESTABLECIDO.", txt));
                        return lRechazos;
                    }

                    if (!UTilidades.TipoDocumentoIdentidad(linea_comprador[1].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "824", "EL TIPO DE DOCUMENTO DEL COMPRADOR NO ESTA EN EL LISTADO.", txt));
                    }

                    if (linea_comprador[1].Trim() == "6")
                    {
                        if (new brConsultar().ConsultarRuc(linea_comprador[2].Trim()) == 0)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "825", "EL RUC DEL COMPRADOR NO EXISTE.", txt));
                        }
                    }

                    if (linea_comprador[1].Trim() == "1" && linea_comprador[2].Trim().Length != 8)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "826", "SI EL TIPO DE DOCUMENTO DEL COMPRADOR ES 1 ENTONCES EL NUMERO DE DOCUMENTO DEBE TENER 8 DIGITOS.", txt));
                    }

                    if (linea_comprador[1].Trim() == "6" && linea_comprador[2].Trim().Length != 11)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "827", "SI EL TIPO DE DOCUMENTO DEL COMPRADOR ES 6 ENTONCES EL NUMERO DE DOCUMENTO DEBE TENER 11 DIGITOS.", txt));
                    }

                    if (linea_comprador[3].Trim().Length > 100)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "829", "LA RAZON SOCIAL DEL COMPRADOR SOLO PUEDE TENER HASTA 100 CARACTERES.", txt));
                    }

                    if (string.IsNullOrEmpty(linea_comprador[3].Trim()))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "831", "EN CASO DE MANDAR LINEA DE COMPRADOR, LA RAZON SOCIAL DEL COMPRADOR NO PUEDE ESTAR VACIA.", txt));
                    }

                    string tipoDoc = linea_comprador[1].Trim();
                    string numeroDoc = linea_comprador[2].Trim();

                    if (UTilidades.TipoDocumentoIdentidad(tipoDoc)
                        && tipoDoc != "6"
                        && tipoDoc != "1")
                    {
                        if (!string.IsNullOrEmpty(numeroDoc))
                        {
                            if (numeroDoc.Length > 15
                            || !UTilidades.EsAlfanumerico(numeroDoc, 15))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "828", "SI EL TIPO DE DOCUMENTO DEL COMPRADOR ESTA EN EL LISTADO Y ES DIFERENTE DE 1 Y 6, EL NUMERO DE DOCUMENTO PUEDE TENER HASTA 15 CARACTERES ALFANUMERICOS.", txt));
                            }
                        }

                        if (string.IsNullOrEmpty(numeroDoc))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "833",
                                "EL NUMERO DE DOCUMENTO DEL COMPRADOR NO PUEDE ESTAR VACIO.",
                                txt));
                        }
                    }
                }
                #endregion

                #region PUNTO_PARTIDA_LLEGADA
                if (PuntoPartidaLlegada.Count > 0)
                {
                    var longPPLL = PuntoPartidaLlegada.FirstOrDefault().Split('|');

                    var ubigeoLlegada = longPPLL.Length > 3 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[3]?.Trim() : "";
                    var ubigeoPartida = longPPLL.Length > 1 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[1]?.Trim() : "";
                    var direccionPartida = longPPLL.Length > 2 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[2]?.Trim() : "";
                    var direccionLlegada = longPPLL.Length > 4 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[4]?.Trim() : "";

                    var rucPuntoPartida = longPPLL.Length > 5 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[5]?.Trim() : "";
                    var codigoEstablecimientoPartida = longPPLL.Length > 6 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[6]?.Trim() : "";
                    var rucPuntoLlegada = longPPLL.Length > 8 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[8]?.Trim() : "";
                    var codigoEstablecimientoLlegada = longPPLL.Length > 9 ? PuntoPartidaLlegada.FirstOrDefault().Split('|')[9]?.Trim() : "";

                    var existeUbi = new brConsultar().ObtenerUbigeo(ubigeoLlegada);
                    if (existeUbi.Trim() == "")
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "773", "UBIGEO DE LLEGADA NO EXISTE. :: ID - GPPYL.", txt));
                    }

                    if (longPPLL.Length < 5)
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "322", "LINEA GPPYL INCOMPLETA REVISAR CAMPOS SEGUN EL MANUAL.", txt));
                    }
                    else
                    {
                        if (lCabecera[7].Trim() != "18")
                        {
                            if (string.IsNullOrEmpty(ubigeoLlegada) || string.IsNullOrEmpty(ubigeoPartida)
                                || string.IsNullOrEmpty(direccionPartida) || string.IsNullOrEmpty(direccionLlegada))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "322", "LINEA GPPYL INCOMPLETA REVISAR CAMPOS SEGUN EL MANUAL.", txt));
                            }
                        }
                    }

                    if (lCabecera[7].Trim() == "01" || lCabecera[7].Trim() == "02")
                    {
                        bool tieneAlMenosUno = !string.IsNullOrWhiteSpace(rucPuntoPartida) ||
                                               !string.IsNullOrWhiteSpace(codigoEstablecimientoPartida) ||
                                               !string.IsNullOrWhiteSpace(rucPuntoLlegada) ||
                                               !string.IsNullOrWhiteSpace(codigoEstablecimientoLlegada);

                        bool todosTienenValor = !string.IsNullOrWhiteSpace(rucPuntoPartida) &&
                                                !string.IsNullOrWhiteSpace(codigoEstablecimientoPartida) &&
                                                !string.IsNullOrWhiteSpace(rucPuntoLlegada) &&
                                                !string.IsNullOrWhiteSpace(codigoEstablecimientoLlegada);

                        if (tieneAlMenosUno && !todosTienenValor)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "795", "LINEA GPPYL (CAMPO 6,7,9,10) SI ALGUNO TIENE INFORMACION, DEBE ENVIAR LOS 3 RESTANTES.", txt));
                        }
                    }

                    if (lCabecera[7].Trim() != "18")
                    {
                        if (rucPuntoLlegada != "" || rucPuntoPartida != "")
                        {
                            if (rucPuntoLlegada.Length != 11 || rucPuntoPartida.Length != 11)
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "793", "LA LÍNEA GPPYL DEBE SER UN RUC ASOCIADO AL PUNTO DE PARTIDA / LLEGADA.", txt));
                            }
                        }

                        if (lCabecera[7].Trim() != "01" && lCabecera[7].Trim() != "02" && lCabecera[7].Trim() != "05"
                            && lCabecera[7].Trim() != "06" && lCabecera[7].Trim() != "13" && lCabecera[7].Trim() != "14")
                        {
                            if (string.IsNullOrWhiteSpace(codigoEstablecimientoPartida) ||
                            string.IsNullOrWhiteSpace(codigoEstablecimientoLlegada) ||
                            codigoEstablecimientoPartida.Length != 4 ||
                            codigoEstablecimientoLlegada.Length != 4 ||
                            !codigoEstablecimientoPartida.All(char.IsDigit) ||
                            !codigoEstablecimientoLlegada.All(char.IsDigit))
                            {
                                lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                    "794",
                                    "CÓDIGO DE ESTABLEC. DE PUNTO DE PARTIDA Y CÓDIGO DE ESTABLEC. DE PUNTO DE LLEGADA DEBE TENER 4 CARACTERES, Y SOLO SE ACEPTA NUMEROS.",
                                    txt));
                            }
                        }
                    }

                    if (lCabecera[7].Trim() == "02" || lCabecera[7].Trim() == "07" || lCabecera[7].Trim() == "08")
                    {
                        if (rucPuntoPartida == lCabecera[3].Trim())
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "745", "EN LA LINEA GPPYL, SI EL MOTIVO DE TRASLADO ES 02 O 07 O 08 EL RUC DE PUNTO DE PARTIDA NO PUEDE SER IGUAL AL EMISOR.", txt));
                        }
                    }

                    if (lCabecera[7].Trim() == "04")
                    {
                        if (string.IsNullOrEmpty(rucPuntoPartida) || string.IsNullOrEmpty(codigoEstablecimientoPartida) || string.IsNullOrEmpty(rucPuntoLlegada) || string.IsNullOrEmpty(codigoEstablecimientoLlegada))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "317", "SI ES MOTIVO TRASLADO ENTRE ESTABLECIMIENTOS ES OBLIGATORIO ENVIAR EL CAMPO 6,7,9,10 DE LA LINEA GPPYL.", txt));
                        }
                    }

                    if (lCabecera[7].Trim() == "04")
                    {
                        if (lCabecera[3].Trim() != rucPuntoPartida || lCabecera[3].Trim() != rucPuntoLlegada)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "797", "SI EL MOTIVO DE TRASLADO ES 04 ENTONCES EL RUC DE PUNTO DE PARTIDA Y EL RUC DE PUNTO DE LLEGADA DEBE SER IGUAL AL REMITENTE.", txt));
                        }
                    }

                    if (lCabecera[7].Trim() == "14")
                    {
                        if (lCabecera[3].Trim() == rucPuntoLlegada)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "835", "SI EL MOTIVO DE TRASLADO ES 14 ENTONCES EL NUMERO DE RUC ASOCIADO AL PUNTO DE LLEGADA NO DEBE SER IGUAL AL NUMERO DE RUC DEL REMITENTE.", txt));
                        }
                    }
                }
                else
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "841", "DEBE ENVIAR LINEA GPPYL ES OBLIGATORIO.", txt));
                }
                #endregion

                #region DESTINATARIO
                if (lineaDestinatario.Count == 0)
                {
                    lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "751", "LINEA DE DESTINATARIO ES OBLIGATORIO.", txt));
                }
                if (lineaDestinatario.Count > 0)
                {
                    var tipoDocDestinatario = lineaDestinatario.FirstOrDefault()?.Split('|')[1]?.Trim();
                    var numeroDoc = lineaDestinatario.FirstOrDefault()?.Split('|')[2]?.Trim();
                    if (!UTilidades.TipoDocumentoIdentidad(tipoDocDestinatario))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                                    "321", "TIPO DE DOCUMENTO DEL DESTINATARIO NO EXISTE.", txt));
                    }

                    if (tipoDocDestinatario == "6" &&
                        (numeroDoc.Length != 11 || !numeroDoc.All(char.IsDigit)))
                    {
                        lRechazos.AddRange(await AgregarRechazo(lCabecera,
                            "791",
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
                    if (lCabecera[7].Trim() == "04")
                    {
                        if (lCabecera[3].Trim() != numeroDoc)
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "796", "SI EL MOTIVO DE TRASLADO ES 04 ENTONCES EL REMITENTE DEBE SER IGUAL AL DESTINATARIO.", txt));
                        }
                    }

                    string[] traslados = { "02", "04", "07", "18" };
                    if (traslados.Contains(lCabecera[7].Trim()))
                    {
                        if (numeroDoc != lCabecera[3].Trim())
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "806", "PARA EL MOTIVO DE TRASLADO(02,04,07,18) INGRESADO EL DESTINATARIO DEBE SER IGUAL AL REMITENTE.", txt));
                        }
                    }

                    if (lCabecera[7].Trim() == "14")
                    {
                        if (numeroDoc == lCabecera[3].Trim())
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera,
                                "834", "SI EL MOTIVO DE TRASLADO ES 14 ENTONCES EL DESTINATARIO NO PUEDE SER IGUAL AL REMITENTE.", txt));
                        }
                    }
                }
                #endregion

                #region COMPRADOR
                if (lineaComprador.Count > 0)
                {
                    var tipoDocComprador = lineaComprador.FirstOrDefault()?.Split('|')[1]?.Trim();
                    if (!UTilidades.TipoDocumentoIdentidad(tipoDocComprador))
                    {
                        lRechazos.AddRange(await AgregarRechazo(
                            lCabecera,
                            "320",
                            "TIPO DE DOCUMENTO DEL COMPRADOR EN LA LINEA COMPRADOR NO EXISTE.",
                            txt));
                    }
                }
                #endregion

                #region CAMPO_ADICIONAL
                if (CampoAdicional.Count > 0)
                {
                    int i = 1;
                    foreach (var item in CampoAdicional)
                    {
                        var campo = item.Split('|')[1];
                        if (!string.IsNullOrEmpty(campo) && !Regex.IsMatch(campo, @"^[^:]+:\s?.+"))
                        {
                            lRechazos.AddRange(await AgregarRechazo(lCabecera, "412", $"CAMPO ADICIONAL {i + 1} MAL ESTRUCTURADO.", txt));
                        }
                        i++;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                await LogAsync("Validar09", ex);
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
            string fecha = DateTime.TryParseExact(lCabecera[5], formatos, null, System.Globalization.DateTimeStyles.None, out FE) ? FE.ToString("dd/MM/yyyy HH:mm:ss") : "";
            DateTime.TryParse(lCabecera[5], out DateTime fechaEmision);
            lRechazos.Add(new beRechazo
            {
                RUC = lCabecera[3],
                Sede = lCabecera[4],
                Serie = lCabecera[14],
                Numero = lCabecera[15],
                CodigoRechazo = codigo,
                Descripcion = descripcion,
                TipoDoc = tipoDoc,
                FechaEmision = fecha != "" ? fechaEmision : DateTime.Now,
                FechaTransferencia = DateTime.Now,
                Txt = txt[5],
                TipoMoneda = lCabecera[6]
            });

            return Task.FromResult(lRechazos);
        }
    }
}
