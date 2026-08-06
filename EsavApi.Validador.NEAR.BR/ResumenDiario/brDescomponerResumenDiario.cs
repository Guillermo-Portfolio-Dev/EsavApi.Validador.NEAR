using EsavApi.Validador.BR;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.ResumenDiario;
using EsavApi.Validador.NEAR.BR.Commons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.ResumenDiario
{
    public class brDescomponerResumenDiario : brGenerico
    {
        public async Task<beResumenDiarioObj> DescomponerResumenDiario(string[] lineas)
        {
            beResumenDiarioObj oAnulacion = new beResumenDiarioObj();
            List<beResumenDiarioDetalle> Detalle = new List<beResumenDiarioDetalle>();

            try
            {
                string[] eCabecera = lineas[0].Split('|');
                var configuracion = await new brConfiguracion().Consultar(eCabecera[3], eCabecera[4].ToString());
                var comunicacionBaja = new brConsultar().ComunicacionBajaObtener(eCabecera[3], eCabecera[2], eCabecera[6], eCabecera[7]);

                for (int i = 0; i < lineas.Length; i++)
                {
                    var line = lineas[i].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    var fechaFormat = Convert.ToDateTime(eCabecera[5].Trim()).ToString("dd/MM/yyyy HH:mm:ss");

                    #region CABECERA
                    oAnulacion.eCabBaja = new beResumenDiario
                    {
                        Accion = 1,
                        vUbl = "2.0",
                        vCustomID = "1.1",
                        //serie = DateTime.ParseExact(eCabecera[5], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        serie = DateTime.Now.ToString("yyyyMMdd"),
                        numero = int.Parse($"{DateTime.Now.Millisecond}{new Random().Next(1, 9)}"),
                        tipoDocEmision = eCabecera[2] == "03" ? "RC" :
                                         eCabecera[2] == "07" && eCabecera[6].StartsWith("B") ? "RC" :
                                         eCabecera[2] == "07" && eCabecera[6].StartsWith("F") ? "RA" :
                                         eCabecera[2] == "08" && eCabecera[6].StartsWith("B") ? "RC" :
                                         eCabecera[2] == "08" && eCabecera[6].StartsWith("F") ? "RA" :
                                         eCabecera[2] == "40" || eCabecera[2] == "20" ? "RR" : "",
                        fechaEmision = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        FechaDocumento = DateTime.ParseExact(fechaFormat, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"),
                        usuario = eCabecera[8],
                        Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        fechaBaja = DateTime.Now.ToString("dd/MM/yyyy"),
                        horaBaja = DateTime.Now.ToString("HH:mm:ss"),
                        codigoTipoDocElec = eCabecera[2],
                        Configuracion = new Configuracion
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
                            VistaPdf = configuracion.CSuc_VistaPdf + "Boleta",
                            Detra027OrigDestPdf = configuracion.CSuc_Detra027OrigDestPdf,
                            ComentarioLegalExportacion = configuracion.CSuc_ComentarioLegalExportacion,
                            PorcentajeIGV = configuracion.CSuc_PorcentajeIGV,
                            LogoPDF = configuracion.Form_Icono,
                            ComentarioLegal = configuracion.CSuc_ComentarioLegal,
                            CuentaCorriente = configuracion.CSuc_CuentaCorriente,
                        },
                        tipoDocEmisor = "6",
                        rucEmisor = eCabecera[3].ToString().Trim(),
                        razonsocialEmisor = configuracion.Emis_RazonSocial,
                        direccionEmisor = configuracion.Emis_Direccion,
                        emailEmisor = configuracion.Emis_Correo,
                        telefonoEmisor = configuracion.Emis_Telefono,
                        faxEmisor = null,
                        distritoIdEmisor = null,
                        distritoEmisor = configuracion.Dist_Descripcion,
                        provinciaIdEmisor = null,
                        provinciaEmisor = configuracion.Dist_Descripcion,
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
                    };
                    #endregion

                    #region DETALLE
                    Detalle.Add(new beResumenDiarioDetalle
                    {
                        Accion = 1,
                        rucEmisor = eCabecera[3],
                        codigoTipoDocElec = eCabecera[2],
                        NumeroResumen = comunicacionBaja.ResumenNumero,
                        NroItem = 1,
                        //IdSerie = DateTime.ParseExact(eCabecera[5], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        IdSerie = DateTime.Now.ToString("yyyyMMdd"),
                        serie = eCabecera[6].Trim(),
                        numero = Convert.ToInt32(eCabecera[7]),
                        tipoDocCliente = comunicacionBaja.TipoDocumentoIdentidadCliente,
                        nroDocCliente = comunicacionBaja.NroDocumentoIdentidadCliente,
                        codigoTipoDocElecReferencia = comunicacionBaja.CodigoTipoDocumentoReferencia == "" ? null : comunicacionBaja.CodigoTipoDocumentoReferencia,
                        serieReferencia = comunicacionBaja.SerieReferencia == "" ? null : comunicacionBaja.SerieReferencia,
                        numeroReferencia = comunicacionBaja.NumeroReferencia,
                        Moneda = comunicacionBaja.Moneda,
                        idGravado = "01",
                        gravado = comunicacionBaja.Gravado,
                        idExonerado = "02",
                        idInafecto = "03",
                        inafecto = comunicacionBaja.Inafecto,
                        indicadorOtrosCargos = comunicacionBaja.IndicadorOtrosCargos,
                        otrosCargos = comunicacionBaja.OtrosCargos,
                        exonerado = comunicacionBaja.Exonerado,
                        idGratuito = "05",
                        gratuito = comunicacionBaja.Gratuito,
                        idExportacion = "04",
                        exportacion = comunicacionBaja.Exportacion,
                        idIgv = "1000",
                        nombreIgv = "IGV",
                        codigoIgv = "VAT",
                        igv = comunicacionBaja.Igv,
                        idIsc = "2000",
                        nombreIsc = "ISC",
                        codigoIsc = "EXC",
                        idICBPER = "7152",
                        nombreICBPER = "ICBPER",
                        codigoICBPER = "OTH",
                        ICBPER = comunicacionBaja.Icbper,
                        idOth = "9999",
                        nombreOth = "OTH",
                        codigoOth = "OTH",
                        oth = comunicacionBaja.Oth,
                        descuento = comunicacionBaja.Descuento,
                        indicadorDescuento = comunicacionBaja.IndicadorDescuento,
                        importeTotal = (decimal)comunicacionBaja.ImporteTotal,
                        estado = 3,
                        fechaEmision = DateTime.ParseExact(Convert.ToDateTime(comunicacionBaja.Fecha).ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy"),
                        //tipoDocEmision = eCabecera[2] == "01" ? "RA" : eCabecera[2] == "03" ? "RC" : eCabecera[2] == "07" ? "RA" : eCabecera[2] == "08" ? "RC" : eCabecera[2] == "40" || eCabecera[2] == "20" ? "RR" : "",
                        tipoDocEmision = null,

                    });
                    oAnulacion.eDocBaja = Detalle;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("DescomponerResumenDiario", ex);
            }

            return oAnulacion;
        }
    }
}
