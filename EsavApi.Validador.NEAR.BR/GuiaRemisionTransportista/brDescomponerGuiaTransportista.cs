using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.GuiaRemision;
using EsavApi.Validador.NEAR.BE.GuiaRemisionTransportista;
using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.UTIL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.GuiaRemisionTransportista
{
    public class brDescomponerGuiaTransportista : brGenerico
    {
        public async Task<beGuiaTransportistaObj> DescomponerGuiaTransportista(string[] lineas)
        {
            beGuiaTransportistaObj oGuiaTransportista = new beGuiaTransportistaObj();
            List<GuiaRemisionConductor> Conductor = new List<GuiaRemisionConductor>();
            List<GuiaRemisionVehiculo> Vehiculo = new List<GuiaRemisionVehiculo>();
            List<GuiaRemisionDocumentoRelacionado> DocRel = new List<GuiaRemisionDocumentoRelacionado>();
            List<beGuiaRemisionOrdenCompra> OrdenCompra = new List<beGuiaRemisionOrdenCompra>();
            List<GuiaTrasportistaDetalle> Detalle = new List<GuiaTrasportistaDetalle>();
            List<EmisorCampoAdicionalRegistro> oCampoAdicional = new List<EmisorCampoAdicionalRegistro>();

            int index = 1;
            string[] eCabecera = lineas[0].Split('|');
            var configuracion = await new brConfiguracion().Consultar(eCabecera[3].ToString(), eCabecera[4].ToString(), eCabecera[2].Trim());
            var CampoAdicional = lineas.Where(x => x.ToUpper().StartsWith("CAMPOADICIONAL")).ToList();

            try
            {
                for (int j = 0; j < lineas.Length; j++)
                {
                    var line = lineas[j].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    #region CABECERA

                    if (line[0] == "210")
                    {
                        oGuiaTransportista.eTransportista = new beGuiaTransportista
                        {
                            accion = 1,
                            serie = eCabecera[11].ToString() == "" ? "" : eCabecera[11].ToString(),
                            numero = int.Parse(eCabecera[12]) == 0 ? 0 : int.Parse(eCabecera[12]),
                            fechaEmision = eCabecera[5].ToString(),
                            horaEmision = DateTime.Parse(eCabecera[5].ToString()).ToString("HH:mm:ss"),
                            tipoDocEmision = eCabecera[2].ToString(),
                            IdSucursal = configuracion.Sucu_IdSucursal,
                            tieneDocBaja = eCabecera[8] == "1" ? true : false,
                            //observaciones = eCabecera[19],
                            EntidadAutorizadora = null,
                            tipoDocumentoProveedor = "",
                            tipoDocumentoProveedorText = "",
                            nroDocumentoProveedor = "",
                            razonSocialProveedor = "",
                            tipoDocumentoComprador = "",
                            tipoDocumentoCompradorText = "",
                            nroDocumentoComprador = "",
                            razonSocialComprador = "",
                            idmotivoTraslado = null,
                            //motivoTraslado = eCabecera[7],
                            esTrasladoTotal = string.IsNullOrWhiteSpace(eCabecera[16]) ? null : eCabecera[16] == "1" ? "SUNAT_Envio_IndicadorTrasladoTotal" : eCabecera[16].Trim(),
                            motivoTrasladoText = "",
                            motivoTrasladoExtra = null,
                            unidadPesoBrutoSeleccionados = "KGM",
                            pesoBrutoSeleccionados = 0,
                            sustentoPesoBrutoSeleccionados = null,
                            unidadPesoBruto = "KGM",
                            pesoBruto = eCabecera[15] == "" ? 0 : Convert.ToDecimal(eCabecera[15]),
                            esPagadorFlete = string.IsNullOrWhiteSpace(eCabecera[22]) ? null : eCabecera[22] == "1" ? "SUNAT_Envio_IndicadorPagadorFlete_Tercero" : null,
                            esTransporteSubcontratado = string.IsNullOrWhiteSpace(eCabecera[17]) ? null : eCabecera[17] == "1" ? "SUNAT_Envio_IndicadorTrasporteSubcontratado" : eCabecera[17].Trim(),
                            numeroBultos = 0,
                            modalidadTraslado = "",
                            modalidadTrasladoText = "",
                            fechaInicioTrasladoPrivado = eCabecera[7],
                            //fechaEntregaBienesEmpresaTransporte = eCabecera[10],
                            esTransbordoProgramado = string.IsNullOrWhiteSpace(eCabecera[18]) ? null : eCabecera[18].Trim() == "1" ? "SUNAT_Envio_IndicadorTransbordoProgramado" : null,
                            registroMTC = null,
                            transportistaNumeroAutorizacion = null,
                            transportistaCodigoEntidadAutorizadora = null,
                            tipoPuertoEmbarque = null,
                            codPuertoEmbarque = null,
                            PuertoEmbarque = null,
                            tipoAeroPuertoEmbarque = null,
                            codAeroPuertoEmbarque = null,
                            AeroPuertoEmbarque = null,
                            numeroContenedor = null,
                            comentario = null,
                            idFormatoRepresentacionImpresa = "STANDARD",

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
                                VistaPdf = configuracion.CSuc_VistaPdf + "GuiaT",
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
                                correoSucursal = null,
                                telefonoSucursal = null,
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
                            usuarioSunatEmisor = configuracion.Emis_UsuarioSunat,
                            claveSunatEmisor = configuracion.Emis_ClaveSunat,
                            valida = configuracion.Emis_OSEBalanceado,
                            fileLogoPDFEmisor = configuracion.Form_Icono,
                            vUbl = "2.1",
                            vCustomID = "2.0",
                            usuario = eCabecera[13].ToString(),
                            Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),

                        };
                    }
                    #endregion

                    if (line[0].ToUpper() == "GDTP")
                    {
                        oGuiaTransportista.eTransportista.tipoDocumentoEmpresaTransporte = line[1];
                        oGuiaTransportista.eTransportista.nroDocumentoEmpresaTransporte = line[2];
                        oGuiaTransportista.eTransportista.razonSocialEmpresaTransporte = line[3];
                    }
                    if (line[0].ToUpper() == "DESTINATARIO")
                    {
                        oGuiaTransportista.eTransportista.tipoDocumentoDestinatario = line[1];
                        oGuiaTransportista.eTransportista.nroDocumentoDestinatario = line[2];
                        oGuiaTransportista.eTransportista.razonSocialDestinatario = line[3];
                        oGuiaTransportista.eTransportista.email = line[4];
                    }
                    if (line[0].ToUpper() == "REMITENTE")
                    {
                        oGuiaTransportista.eTransportista.Ca06_IdRemitente = line[1];
                        oGuiaTransportista.eTransportista.Remi_idRemitente = line[2];
                        oGuiaTransportista.eTransportista.RazonSocialRemitente = line[3];
                    }
                    if (line[0].ToUpper() == "SUBCONTRATO")
                    {
                        oGuiaTransportista.eTransportista.idSubContrata = string.IsNullOrWhiteSpace(eCabecera[17]) ? null : eCabecera[17] == "1" ? "SUNAT_Envio_IndicadorTrasporteSubcontratado" : eCabecera[17].Trim();
                        oGuiaTransportista.eTransportista.tipoDocumentSubContrata = line[1].Trim();
                        oGuiaTransportista.eTransportista.tipoDocumentoSubContrataText = UTilidades.ObtenerTipoDocumentoText(line[1].Trim());
                        oGuiaTransportista.eTransportista.nroDocumentoSubContrata = line[2];
                        oGuiaTransportista.eTransportista.razonSocialSubContrata = line[3];
                    }
                    if (line[0].ToUpper() == "GPPYL")
                    {
                        oGuiaTransportista.eTransportista.ubigeoPuntoPartida = line[1];
                        oGuiaTransportista.eTransportista.direccionPuntoPartida = line[2];
                        oGuiaTransportista.eTransportista.ubigeoPuntoLlegada = line[3];
                        oGuiaTransportista.eTransportista.direccionPuntoLlegada = line[4];
                        oGuiaTransportista.eTransportista.rucPuntoPartida = line[5];
                        oGuiaTransportista.eTransportista.codigoPuntoPartida = line[6];
                        oGuiaTransportista.eTransportista.rucPuntoLlegada = line.Length > 8 ? line[8] : "";
                        oGuiaTransportista.eTransportista.codigoPuntoLlegada = line.Length > 9 ? line[9] : "";
                    }
                    if (line[0].ToUpper() == "PAGOSERVICIO")
                    {
                        oGuiaTransportista.eTransportista.tipoDocumentoComprador = line[1].Trim();
                        oGuiaTransportista.eTransportista.tipoDocumentoCompradorText = UTilidades.ObtenerTipoDocumentoText(line[1].Trim());
                        oGuiaTransportista.eTransportista.nroDocumentoComprador = line[2].Trim();
                        oGuiaTransportista.eTransportista.razonSocialComprador = line[3].Trim();
                    }


                    if (line[0].Trim().ToUpper() == "GDOCREF")
                    {
                        int i = 1;
                        DocRel.Add(new GuiaRemisionDocumentoRelacionado()
                        {
                            itemDocRel = i,
                            codigoDocRel = line[1],
                            nroDocRel = line[2],
                            rucEmisor = line[3],
                            tipoDocEmisor = line.Length > 3 ? line[4].Trim() : "",
                            docRel = new brConsultar().Catalogo_61(line[1].Trim())
                        });
                        i++;
                    }

                    DocRel = DocRel.Count == 0 ? new List<GuiaRemisionDocumentoRelacionado>() : DocRel;
                    oGuiaTransportista.lDocRel = DocRel;

                    if (line[0].ToUpper() == "ORDENCOMPRA")
                    {
                        OrdenCompra.Add(new beGuiaRemisionOrdenCompra()
                        {
                            ordenCompra = line[1].Trim()
                        });
                    }

                    OrdenCompra = OrdenCompra.Count == 0 ? new List<beGuiaRemisionOrdenCompra>() : OrdenCompra;
                    oGuiaTransportista.lOrdenCompra = OrdenCompra;

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
                                        oGuiaTransportista.lCampoAdicional = oCampoAdicional.Count > 0 ? oCampoAdicional : new List<EmisorCampoAdicionalRegistro>();
                                    }
                                    z++;
                                }
                            }
                        }
                    }
                    else
                    {
                        oGuiaTransportista.lCampoAdicional = new List<EmisorCampoAdicionalRegistro>();
                    }
                    #endregion

                    #region CONDUCTOR Y VEHICULO
                    if (line[0].ToUpper() == "CONDUCTOR")
                    {
                        int conductor = 1;
                        Conductor.Add(new GuiaRemisionConductor()
                        {
                            itemConductor = conductor,
                            tipoConductor = line[1].Trim().ToUpper() == "PRINCIPAL" ? "Principal" : "Secundario",
                            nroDocConductor = line[3],
                            tipoDocConductor = line[2],
                            nombresConductor = line[4],
                            apellidosConductor = line[5],
                            licenciaConductor = line[6]
                        });
                        conductor++;
                    }
                    oGuiaTransportista.lConductor = Conductor;

                    if (line[0].ToUpper() == "VEHICULO")
                    {
                        int vehiculo = 1;
                        Vehiculo.Add(new GuiaRemisionVehiculo()
                        {
                            itemVehiculo = vehiculo,
                            nroPlaca = line[1],
                            TUCE = line[2],
                            NumeroAutorizacion = line[3],
                            CodigoEntidadAutorizadora = line[4]
                        });
                    }

                    oGuiaTransportista.lVehiculo = Vehiculo;
                    #endregion

                    #region DETALLE
                    if (Regex.IsMatch(line[0], @"^\d+") && line.Length <= 9)
                    {
                        var UnidadMedidaText = new brConsultar().UnidaMedidaText(line[3]);
                        Detalle.Add(new GuiaTrasportistaDetalle()
                        {
                            itemBien = index,
                            cantidadBien = line[4] == "" ? 0 : Convert.ToDecimal(line[4]),
                            codigoSUNAT = line[2],
                            codigoBien = line[1],
                            descripcionBien = line[5],
                            unidadMedida = line[3],
                            precioUnitario = 0,
                            valorVenta = 0,
                            pesototal = string.IsNullOrWhiteSpace(line[7]) ? 0 : Convert.ToDecimal(line[7].Trim()),
                            pesounitario = string.IsNullOrWhiteSpace(line[6]) ? 0 : Convert.ToDecimal(line[6].Trim()),
                            afectacionIgv = null,
                            codigoconcepto = line[6],
                            nombreconcepto = null,
                            partidaArancelaria = line.Length > 7 ? line[7] : "",
                            numeroDAM = line.Length > 9 ? line[9] : "",
                            serieDAM = line.Length > 10 ? line[10] : "",
                            unidadMedidaText = UnidadMedidaText.Item1,
                            bienNormalizado = line.Length > 8 && !string.IsNullOrWhiteSpace(line[8]) && (line[8].Trim() == "1" || bool.Parse(line[8].Trim().ToLower()))
                        });
                        index++;
                    }
                    oGuiaTransportista.lDetalleBien = Detalle;
                    #endregion

                }
            }
            catch (Exception ex)
            {
                oGuiaTransportista = null;
                await LogAsync("DescomponerGuiaTransportista", ex);
            }


            return oGuiaTransportista;
        }
    }
}
