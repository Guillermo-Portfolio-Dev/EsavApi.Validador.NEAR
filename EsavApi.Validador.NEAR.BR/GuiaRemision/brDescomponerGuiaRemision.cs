using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.GuiaRemision;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.GuiaRemision
{
    public class brDescomponerGuiaRemision : brGenerico
    {
        public async Task<beGuiaRemisionObjv2> DescomponerGuiaRemitenteV2(string[] lineas)
        {
            beGuiaRemisionObjv2 oGuiaRemitente = new beGuiaRemisionObjv2();
            List<GuiaRemisionConductor> Conductor = new List<GuiaRemisionConductor>();
            List<GuiaRemisionVehiculo> Vehiculo = new List<GuiaRemisionVehiculo>();
            List<GuiaRemisionDocumentoRelacionado> DocRel = new List<GuiaRemisionDocumentoRelacionado>();
            List<beGuiaRemisionOrdenCompra> OrdenCompra = new List<beGuiaRemisionOrdenCompra>();
            List<GuiaRemisionDetalle> Detalle = new List<GuiaRemisionDetalle>();
            List<EmisorCampoAdicionalRegistro> oCampoAdicional = new List<EmisorCampoAdicionalRegistro>();

            int index = 1;
            string[] eCabecera = lineas[0].Split('|');
            var CampoAdicional = lineas.Where(x => x.ToUpper().StartsWith("CAMPOADICIONAL")).ToList();
            var configuracion = await new brConfiguracion().Consultar(eCabecera[3].ToString(), eCabecera[4].ToString());
            var detalleLines = lineas
                            .Skip(1)
                            .Where(x => Regex.IsMatch(x, @"^\d+\|"))
                            .ToList();

            var GMIMP = lineas.Where(x => x.ToUpper().StartsWith("GMIMP")).ToList();
            var DocumentoRelacionado = lineas.Where(x => x.ToUpper().StartsWith("GDOCREF")).ToList();
            var Comprador = lineas.Where(x => x.ToUpper().StartsWith("COMPRADOR")).ToList();
            var Proveedor = lineas.Where(x => x.ToUpper().StartsWith("PROVEEDOR")).ToList();

            try
            {
                for (int j = 0; j < lineas.Length; j++)
                {
                    var line = lineas[j].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    #region CABECERA

                    if (line[0] == "210")
                    {
                        var indicador_vehiculo_conductor = eCabecera.Length >= 26 ? eCabecera[25].Trim() : "";
                        oGuiaRemitente.eRemitente = new beGuiaRemisionv2
                        {
                            accion = 1,
                            serie = eCabecera[14].ToString() == "" ? "" : eCabecera[14].ToString(),
                            numero = int.Parse(eCabecera[15]) == 0 ? 0 : int.Parse(eCabecera[15]),
                            fechaEmision = eCabecera[5].ToString(),
                            horaEmision = DateTime.Parse(eCabecera[5].ToString()).ToString("HH:mm:ss"),
                            tipoDocEmision = eCabecera[2].ToString(),
                            IdSucursal = configuracion.Sucu_IdSucursal,
                            tieneDocBaja = false,
                            serieBaja = eCabecera[12],
                            numeroBaja = eCabecera[13],
                            codigoTipoDocElectBaja = null,
                            tipoDocElectBaja = null,
                            observaciones = eCabecera[19],
                            RemitentenumeroAutorizacion = null,
                            RemitentecodigoEntidadAutorizadora = null,
                            EntidadAutorizadora = null,
                            tipoDocumentoProveedor = Proveedor.Count > 0 ? Proveedor.FirstOrDefault().Split('|')[1] : "",
                            tipoDocumentoProveedorText = Proveedor.Count > 0 ? UTilidades.ObtenerTipoDocumentoText(Proveedor.FirstOrDefault().Split('|')[1]) : "",
                            nroDocumentoProveedor = Proveedor.Count > 0 ? Proveedor.FirstOrDefault().Split('|')[2] : "",
                            razonSocialProveedor = Proveedor.Count > 0 ? Proveedor.FirstOrDefault().Split('|')[3] : "",
                            tipoDocumentoComprador = Comprador.Count > 0 ? Comprador.FirstOrDefault().Split('|')[1].Trim() : "",
                            tipoDocumentoCompradorText = Comprador.Count > 0 ? UTilidades.ObtenerTipoDocumentoText(Comprador.FirstOrDefault().Split('|')[1].Trim()) : "",
                            nroDocumentoComprador = Comprador.Count > 0 ? Comprador.FirstOrDefault().Split('|')[2].Trim() : "",
                            razonSocialComprador = Comprador.Count > 0 ? Comprador.FirstOrDefault().Split('|')[3].Trim() : "",
                            idmotivoTraslado = eCabecera[7].Trim(),
                            motivoTraslado = eCabecera[7].Trim(),
                            motivoTrasladoText = UTilidades.CodigoMotivoTrasladoText(eCabecera[7].Trim()),
                            motivoTrasladoExtra = eCabecera.Length > 22 ? eCabecera[22].Trim() : "",
                            unidadPesoBrutoSeleccionados = "KGM",
                            pesoBrutoSeleccionados = 0,
                            sustentoPesoBrutoSeleccionados = null,
                            unidadPesoBruto = "KGM",
                            pesoBruto = eCabecera[18] == "" ? 0 : Convert.ToDecimal(eCabecera[18]),
                            numeroBultos = eCabecera.Length > 21 && !string.IsNullOrWhiteSpace(eCabecera[21])
                                    ? Convert.ToInt32(Convert.ToDecimal(eCabecera[21], CultureInfo.InvariantCulture))
                                    : 0,
                            modalidadTraslado = eCabecera[8].Trim(),
                            modalidadTrasladoText = eCabecera[8].Trim() == "02" ? "Transporte privado" : "Transporte público",
                            fechaInicioTrasladoPrivado = eCabecera[9],
                            fechaEntregaBienesEmpresaTransporte = eCabecera[10],
                            esTransbordoProgramado = eCabecera.Length > 23 ? (string.IsNullOrWhiteSpace(eCabecera[23]) ? null : eCabecera[23].Trim() == "1" ? "SUNAT_Envio_IndicadorTransbordoProgramado" : null) : null,
                            esTransladoM1L = (eCabecera.Length > 24 && eCabecera[24].Trim() == "1")
                            ? "SUNAT_Envio_IndicadorTrasladoVehiculoM1L"
                            : "",
                            esRetornoEnvaseVacio = null,
                            esRetornoVehiculoVacio = null,
                            esTrasladoTotalDAM = DocumentoRelacionado.Count > 0 && (DocumentoRelacionado.FirstOrDefault().Split('|')[1].Trim() == "01" ||
                                                    DocumentoRelacionado.FirstOrDefault().Split('|')[1].Trim() == "03" || DocumentoRelacionado.FirstOrDefault().Split('|')[1].Trim() == "09")
                                                ? "" : DocumentoRelacionado.Count > 0 ? "SUNAT_Envio_IndicadorTrasladoTotalDAMoDS" : "",


                            esRegistroVehiculo = eCabecera[8].Trim() == "01" && indicador_vehiculo_conductor == "1" ? "SUNAT_Envio_IndicadorVehiculoConductoresTransp" : null,
                            registroMTC = null,
                            transportistaNumeroAutorizacion = null,
                            transportistaCodigoEntidadAutorizadora = null,
                            tipoPuertoEmbarque = GMIMP.Count > 0 ? GMIMP.FirstOrDefault().Split('|')[3].Trim() : "",
                            codPuertoEmbarque = GMIMP.Count > 0 ? GMIMP.FirstOrDefault().Split('|')[2].Trim() : "",
                            PuertoEmbarque = GMIMP.Count > 0 ? new brConsultar().Catalogo_63(GMIMP.FirstOrDefault().Split('|')[2].Trim()) : "",
                            tipoAeroPuertoEmbarque = null,
                            codAeroPuertoEmbarque = null,
                            AeroPuertoEmbarque = null,
                            numeroContenedor = GMIMP.Count > 0 ? GMIMP.FirstOrDefault().Split('|')[1].Trim() : "",
                            numeroPrecinto = GMIMP.Count > 0 ? GMIMP.FirstOrDefault().Split('|')[4].Trim() : "",
                            comentario = null,
                            idFormatoRepresentacionImpresa = "",

                            configuracion = new Configuracion
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
                                VistaPdf = configuracion.CSuc_VistaPdf + "GuiaRV2",
                                Detra027OrigDestPdf = configuracion.CSuc_Detra027OrigDestPdf,
                                ComentarioLegalExportacion = configuracion.CSuc_ComentarioLegalExportacion,
                                PorcentajeIGV = configuracion.CSuc_PorcentajeIGV,
                                LogoPDF = configuracion.Form_Icono,
                            },
                            tipoDocEmisor = "6",
                            rucEmisor = eCabecera[3].ToString(),
                            razonsocialEmisor = configuracion.Emis_RazonSocial,
                            direccionEmisor = configuracion.Emis_Direccion,
                            emailEmisor = configuracion.Emis_Correo,
                            telefonoEmisor = configuracion.Emis_Telefono,
                            faxEmisor = null,
                            distritoIdEmisor = null,
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
                                idRubro = 0,
                                tipoSucursal = null,
                            },
                            rutapfxEmisor = configuracion.Emis_RutaPFX,
                            clavepfxEmisor = configuracion.Emis_ClavePFX,
                            usuarioSunatEmisor = eCabecera[2].Trim().ToString() == "09" ? "20600705785RRHH2018" : configuracion.Emis_UsuarioSunat,
                            claveSunatEmisor = eCabecera[2].Trim().ToString() == "09" ? "lima123*" : configuracion.Emis_ClaveSunat,
                            valida = eCabecera[2].Trim().ToString() == "09" ? "20131312955" : configuracion.Emis_OSEBalanceado,
                            fileLogoPDFEmisor = configuracion.Form_Icono,
                            vUbl = "2.1",
                            vCustomID = "2.0",
                            usuario = eCabecera[16].ToString(),
                            Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                        };
                    }
                    #endregion

                    if (line[0].Trim().ToUpper() == "GDTP")
                    {
                        oGuiaRemitente.eRemitente.tipoDocumentoEmpresaTransporte = line[1].Trim();
                        oGuiaRemitente.eRemitente.nroDocumentoEmpresaTransporte = line[2].Trim();
                        oGuiaRemitente.eRemitente.razonSocialEmpresaTransporte = line[3].Trim();
                    }
                    if (line[0].Trim().ToUpper() == "DESTINATARIO")
                    {
                        oGuiaRemitente.eRemitente.tipoDocumentoDestinatario = line[1].Trim();
                        oGuiaRemitente.eRemitente.nroDocumentoDestinatario = line[2].Trim();
                        oGuiaRemitente.eRemitente.tipoDocumentoDestinatarioText = UTilidades.ObtenerTipoDocumentoText(line[1].Trim());
                        oGuiaRemitente.eRemitente.razonSocialDestinatario = line[3];
                        oGuiaRemitente.eRemitente.email =
                            (line.Length > 4 && !string.IsNullOrWhiteSpace(line[4]))
                            ? string.Join(";", line[4].Split(';')
                                .Select(e => e.Trim())
                                .Where(e => { try { return new MailAddress(e).Address == e; } catch { return false; } }))
                            : "";
                    }
                    if (line[0].Trim().ToUpper() == "GPPYL")
                    {
                        oGuiaRemitente.eRemitente.ubigeoPuntoPartida = line[1].Trim();
                        oGuiaRemitente.eRemitente.direccionPuntoPartida = line[2].Trim();
                        oGuiaRemitente.eRemitente.ubigeoPuntoPartidaText = new brConsultar().ObtenerUbigeo(line[1].Trim());
                        oGuiaRemitente.eRemitente.ubigeoPuntoLlegada = line[3].Trim();
                        oGuiaRemitente.eRemitente.direccionPuntoLlegada = line.Length > 4 ? line[4].Trim() : "";
                        oGuiaRemitente.eRemitente.ubigeoPuntoLlegadaText = new brConsultar().ObtenerUbigeo(line[3].Trim());
                        oGuiaRemitente.eRemitente.rucPuntoPartida = line.Length > 5 ? line[5].Trim() : "";
                        oGuiaRemitente.eRemitente.codigoPuntoPartida = line.Length > 6 ? line[6].Trim() : "";
                        oGuiaRemitente.eRemitente.rucPuntoLlegada = line.Length > 8 ? line[8].Trim() : "";
                        oGuiaRemitente.eRemitente.codigoPuntoLlegada = line.Length > 9 ? line[9].Trim() : "";
                    }
                    //if (line[0].Trim().ToUpper() == "GDOCREF")
                    //{
                    //    DocRel.Add(new GuiaRemisionDocumentoRelacionado()
                    //    {
                    //        codigoDocRel = line[1].Trim(),
                    //        nroDocRel = line[2].Trim(),
                    //        rucEmisor = line[3].Trim(),
                    //        rucEmisorDocumentoRelacionado = line[3].Trim(),
                    //        docRel = new brConsultar().Catalogo_61(line[1].Trim()),
                    //        tipoDocEmisor = "6"
                    //    });
                    //}
                    if (line[0].Trim().ToUpper() == "ORDENCOMPRA")
                    {
                        OrdenCompra.Add(new beGuiaRemisionOrdenCompra
                        {
                            accion = 1,
                            IdEmisor = eCabecera[3].ToString(),
                            serie = eCabecera[14].ToString() == "" ? "" : eCabecera[14].ToString(),
                            numero = eCabecera[15].Trim() == "" ? "" : eCabecera[15].Trim(),
                            ordenCompra = line[1].Trim()
                        });
                    }

                    #region CONDUCTOR Y VEHICULO
                    if (line[0].ToUpper() == "CONDUCTOR")
                    {
                        int conductor = 1;
                        Conductor.Add(new GuiaRemisionConductor()
                        {
                            itemConductor = conductor,
                            tipoConductor = !string.IsNullOrWhiteSpace(line[1]) ? char.ToUpper(line[1][0]) + line[1].Substring(1).ToLower() : string.Empty,
                            nroDocConductor = line[3],
                            tipoDocConductor = line[2],
                            nombresConductor = line[4],
                            apellidosConductor = line[5],
                            licenciaConductor = line[6]
                        });

                    }
                    oGuiaRemitente.lConductor = Conductor;

                    if (line[0].ToUpper() == "VEHICULO")
                    {
                        int vehiculo = 1;
                        Vehiculo.Add(new GuiaRemisionVehiculo()
                        {
                            itemVehiculo = vehiculo,
                            nroPlaca = line[1],
                            TUCE = line.Length > 2 ? line[2] : "",
                            NumeroAutorizacion = line.Length > 3 ? line[3] : "",
                            CodigoEntidadAutorizadora = line.Length > 4 ? line[4] : ""
                        });
                    }

                    oGuiaRemitente.lVehiculo = Vehiculo;
                    #endregion

                }

                if (DocumentoRelacionado.Count > 0)
                {
                    int i = 1;
                    foreach (var item in DocumentoRelacionado)
                    {
                        var line = item.Split('|');
                        DocRel.Add(new GuiaRemisionDocumentoRelacionado()
                        {
                            itemDocRel = i,
                            codigoDocRel = line[1].Trim(),
                            nroDocRel = line[2].Trim(),
                            rucEmisor = line[3].Trim(),
                            rucEmisorDocumentoRelacionado = line[3].Trim(),
                            docRel = new brConsultar().Catalogo_61(line[1].Trim()),
                            tipoDocEmisor = "6"
                        });
                        i++;
                    }
                }

                oGuiaRemitente.lDocRel = DocRel.Count > 0 ? DocRel : new List<GuiaRemisionDocumentoRelacionado>();
                oGuiaRemitente.lOrdenCompra = OrdenCompra.Count > 0 ? OrdenCompra : new List<beGuiaRemisionOrdenCompra>();
                #region CampoAdicional
                int z = 1;
                var valoresCA = new brConsultar().ListarCampoAdicional(eCabecera[3].Trim(), configuracion.Rubr_IdRubro.ToString(), eCabecera[2].Trim(), false);
                if (CampoAdicional.Count > 0)
                {
                    foreach (var itemx in CampoAdicional)
                    {
                        var campoAdicional = itemx.Split('|')[1];
                        if (!string.IsNullOrEmpty(campoAdicional) && valoresCA != null)
                        {
                            var valor = campoAdicional.Split(':');
                            var valorCampo = string.Join(":", valor.Skip(1));

                            foreach (var item in valoresCA)
                            {
                                if (valor[0].Trim().ToLower() == item.IdCampoAdicional.ToLower())
                                {
                                    oCampoAdicional.Add(new EmisorCampoAdicionalRegistro
                                    {
                                        accion = 1,
                                        idEmisor = eCabecera[3].Trim(),
                                        idRubro = configuracion.Rubr_IdRubro,
                                        idCampoAdicional = item.IdCampoAdicional,
                                        tipoDocumento = eCabecera[2].Trim(),
                                        serie = eCabecera[14].Trim(),
                                        numero = Convert.ToInt32(eCabecera[15].Trim()),
                                        index = z,
                                        titulo = item.Titulo,
                                        valor = valorCampo,
                                        esDetalle = item.EsDetalle,
                                        enXML = item.EnXML,
                                        enRepresentacionImpresa = item.EnRepresentacionImpresa
                                    });
                                    oGuiaRemitente.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<EmisorCampoAdicionalRegistro>();
                                }
                                z++;
                            }
                        }
                    }
                }
                else
                {
                    oGuiaRemitente.lCampoAdicional = new List<EmisorCampoAdicionalRegistro>();
                }
                #endregion

                #region DETALLE
                if (detalleLines.Count > 0)
                {
                    foreach (var item in detalleLines)
                    {
                        //var UnidadMedidaText = new brConsultar().UnidaMedidaText(line[3]);
                        var parts = item.Split('|');
                        var unidadMedidaText = new brConsultar().UnidaMedidaText(parts[3].Trim());
                        Detalle.Add(new GuiaRemisionDetalle()
                        {
                            itemBien = index,
                            itemRef = null,
                            cantidadBien = string.IsNullOrWhiteSpace(parts[4]) ? 0 : Convert.ToDecimal(parts[4], CultureInfo.InvariantCulture),
                            codigoSUNAT = parts[2].Trim(),
                            codigoBien = parts[1].Trim(),
                            descripcionBien = parts[5].Trim(),
                            unidadMedida = parts[3].Trim(),
                            precioUnitario = 0,
                            valorVenta = 0,
                            afectacionIgv = null,
                            codigoconcepto = parts.Length > 6 ? parts[6].Trim() : "",
                            nombreconcepto = parts.Length > 6 && !string.IsNullOrEmpty(parts[6].Trim()) ? new brConsultar().Catalogo_55(parts[6].Trim()) : "",
                            partidaArancelaria = parts.Length > 7 ? parts[7].Trim() : "",
                            numeroDAM = parts.Length > 9 ? parts[9].Trim() : "",
                            serieDAM = parts.Length > 10 ? parts[10].Trim() : "",
                            unidadMedidaText = unidadMedidaText.Item1,
                            UnidadAbreviatura = unidadMedidaText.Item2,
                            bienNormalizado = parts.Length > 8 ? (parts[8].Trim() == "0" ? false : true) : false,
                            pesounitario = parts.Length > 11
                                        ? string.IsNullOrWhiteSpace(parts[11].Trim())
                                            ? 0m
                                            : Convert.ToDecimal(parts[11].Trim())
                                        : 0m,

                            pesototal = parts.Length > 12
                                        ? string.IsNullOrWhiteSpace(parts[12].Trim())
                                            ? 0m
                                            : Convert.ToDecimal(parts[12].Trim())
                                        : 0m,
                        });
                        index++;
                    }
                }
                oGuiaRemitente.lDetalleBien = Detalle;
                #endregion
            }
            catch (Exception ex)
            {
                oGuiaRemitente = null;
                await LogAsync("DescomponerGuiaRemitenteV2", ex);
            }


            return oGuiaRemitente;
        }
    }
}
