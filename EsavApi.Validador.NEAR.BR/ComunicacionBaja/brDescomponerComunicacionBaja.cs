using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.ComunicacionBaja;
using EsavApi.Validador.NEAR.BR.Commons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.ComunicacionBaja
{
    public class brDescomponerComunicacionBaja : brGenerico
    {
        public async Task<beComunicacionBajaObj> DescomponerComunicacionBaja(string[] lineas)
        {
            beComunicacionBajaObj oAnulacion = new beComunicacionBajaObj();
            List<beComunicacionBajaDetalle> Detalle = new List<beComunicacionBajaDetalle>();

            try
            {
                string[] eCabecera = lineas[0].Split('|');
                var configuracion = await new brConfiguracion().Consultar(eCabecera[3].ToString(), eCabecera[4].ToString());
                var comunicacionBaja = new brConsultar().ComunicacionBajaObtener(eCabecera[3], eCabecera[2], eCabecera[6], eCabecera[7]);

                for (int i = 0; i < lineas.Length; i++)
                {
                    var line = lineas[i].Split('|');

                    if (line.Length == 0 || string.IsNullOrWhiteSpace(line[0])) continue;

                    var fechaFormat = Convert.ToDateTime(eCabecera[5].Trim()).ToString("dd/MM/yyyy HH:mm:ss");

                    #region CABECERA
                    oAnulacion.eCabBaja = new beComunicacionBaja
                    {
                        Accion = 1,
                        vUbl = "2.0",
                        vCustomID = "1.0",
                        //serie = DateTime.ParseExact(eCabecera[5], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        serie = DateTime.Now.ToString("yyyyMMdd"),
                        numero = int.Parse($"{DateTime.Now.Millisecond}{new Random().Next(1, 9)}"),
                        tipoDocEmision = eCabecera[2] == "01" ? "RA" :
                                                  eCabecera[2] == "03" ? "RC" :
                                                  eCabecera[2] == "07" && eCabecera[6].StartsWith("F") ? "RA" :
                                                  eCabecera[2] == "07" && eCabecera[6].StartsWith("B") ? "RC" :
                                                  eCabecera[2] == "08" && eCabecera[6].StartsWith("F") ? "RA" :
                                                  eCabecera[2] == "08" && eCabecera[6].StartsWith("B") ? "RC" :
                                                  eCabecera[2] == "40" || eCabecera[2] == "20" ? "RR" : "",
                        fechaEmision = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        usuario = eCabecera[8],
                        Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        FechaEmisionDocumentos = DateTime.ParseExact(fechaFormat, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"),
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
                    Detalle.Add(new beComunicacionBajaDetalle
                    {
                        Accion = 1,
                        rucEmisor = eCabecera[3],
                        Ca01_Id = eCabecera[2] == "01" ? "RA" :
                                                  eCabecera[2] == "03" ? "RC" :
                                                  eCabecera[2] == "07" && eCabecera[6].StartsWith("F") ? "RA" :
                                                  eCabecera[2] == "07" && eCabecera[6].StartsWith("B") ? "RC" :
                                                  eCabecera[2] == "08" && eCabecera[6].StartsWith("F") ? "RA" :
                                                  eCabecera[2] == "08" && eCabecera[6].StartsWith("B") ? "RC" :
                                                  eCabecera[2] == "40" || eCabecera[2] == "20" ? "RR" : "",
                        SerieCabecera = DateTime.ParseExact(fechaFormat, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                        NumeroCabecera = eCabecera[7],
                        codigoTipoDocElec = eCabecera[2],
                        serie = eCabecera[6].Trim(),
                        numero = Convert.ToInt32(eCabecera[7]),
                        motivoBaja = eCabecera[9].Trim(),
                        Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        usuario = eCabecera[8],
                        fechaEmision = DateTime.ParseExact(Convert.ToDateTime(comunicacionBaja.Fecha).ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy"),
                        tipoDocEmision = eCabecera[2]

                    });
                    oAnulacion.eDocBaja = Detalle;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("DescomponerComunicacionBaja", ex);
            }

            return oAnulacion;
        }
    }
}
