using EsavApi.Validador.NEAR.BE.Commons;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.DA
{
    public class daConfiguracion
    {
        public async Task<beConfiguracionEmisor> Obtener(SqlConnection conn, string ruc, string sede, string tipoDoc)
        {
            beConfiguracionEmisor config = new beConfiguracionEmisor();
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_ConfiguracionEmisor_Obtener", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Emis_IdEmisor", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@Sucursal", SqlDbType.VarChar).Value = sede;
                oCommand.Parameters.Add("@TipoDoc", SqlDbType.VarChar).Value = tipoDoc;
                using (SqlDataReader oDr = await oCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (await oDr.ReadAsync())
                        {
                            if (!DBNull.Value.Equals(oDr["Emis_IdEmisor"])) config.Emis_IdEmisor = (string)oDr["Emis_IdEmisor"];
                            if (!DBNull.Value.Equals(oDr["Emis_RazonSocial"])) config.Emis_RazonSocial = (string)oDr["Emis_RazonSocial"];
                            if (!DBNull.Value.Equals(oDr["Emis_Direccion"])) config.Emis_Direccion = (string)oDr["Emis_Direccion"];
                            if (!DBNull.Value.Equals(oDr["Emis_Correo"])) config.Emis_Correo = (string)oDr["Emis_Correo"];
                            if (!DBNull.Value.Equals(oDr["Emis_Telefono"])) config.Emis_Telefono = (string)oDr["Emis_Telefono"];
                            if (!DBNull.Value.Equals(oDr["Rubr_IdRubro"])) config.Rubr_IdRubro = (int)oDr["Rubr_IdRubro"];
                            if (!DBNull.Value.Equals(oDr["Emis_RutaPFX"])) config.Emis_RutaPFX = (string)oDr["Emis_RutaPFX"];
                            if (!DBNull.Value.Equals(oDr["Emis_ClavePFX"])) config.Emis_ClavePFX = (string)oDr["Emis_ClavePFX"];
                            if (!DBNull.Value.Equals(oDr["Emis_UsuarioSunat"])) config.Emis_UsuarioSunat = (string)oDr["Emis_UsuarioSunat"];
                            if (!DBNull.Value.Equals(oDr["Emis_ClaveSunat"])) config.Emis_ClaveSunat = (string)oDr["Emis_ClaveSunat"];
                            if (!DBNull.Value.Equals(oDr["Emis_OSEBalanceado"])) config.Emis_OSEBalanceado = (string)oDr["Emis_OSEBalanceado"];
                            if (!DBNull.Value.Equals(oDr["Sucu_IdSucursal"])) config.Sucu_IdSucursal = (string)oDr["Sucu_IdSucursal"];
                            if (!DBNull.Value.Equals(oDr["Sucu_CodigoLocalSunat"])) config.Sucu_CodigoLocalSunat = (string)oDr["Sucu_CodigoLocalSunat"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Nombre"])) config.Sucu_Nombre = (string)oDr["Sucu_Nombre"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Direccion"])) config.Sucu_Direccion = (string)oDr["Sucu_Direccion"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Ubigeo"])) config.Sucu_Ubigeo = (string)oDr["Sucu_Ubigeo"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Telefono"])) config.Sucu_Telefono = (string)oDr["Sucu_Telefono"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Correo"])) config.Sucu_Correo = (string)oDr["Sucu_Correo"];
                            if (!DBNull.Value.Equals(oDr["Sucu_Web"])) config.Sucu_Web = (string)oDr["Sucu_Web"];
                            if (!DBNull.Value.Equals(oDr["Anotacion"])) config.Anotacion = (string)oDr["Anotacion"];
                            if (!DBNull.Value.Equals(oDr["CSuc_CantidadDecimal"])) config.CSuc_CantidadDecimal = (int)oDr["CSuc_CantidadDecimal"];
                            if (!DBNull.Value.Equals(oDr["CEmi_CantidadDecimalDetalle"])) config.CEmi_CantidadDecimalDetalle = (int)oDr["CEmi_CantidadDecimalDetalle"];
                            if (!DBNull.Value.Equals(oDr["CSuc_ColorCss"])) config.CSuc_ColorCss = (string)oDr["CSuc_ColorCss"];
                            if (!DBNull.Value.Equals(oDr["CSuc_EstiloCss"])) config.CSuc_EstiloCss = (string)oDr["CSuc_EstiloCss"];
                            if (!DBNull.Value.Equals(oDr["CSuc_FormatoNumericoDetalle"])) config.CSuc_FormatoNumericoDetalle = (string)oDr["CSuc_FormatoNumericoDetalle"];
                            if (!DBNull.Value.Equals(oDr["CSuc_FormatoNumerico"])) config.CSuc_FormatoNumerico = (string)oDr["CSuc_FormatoNumerico"];
                            if (!DBNull.Value.Equals(oDr["CSuc_NombreFuncionDll"])) config.CSuc_NombreFuncionDll = (string)oDr["CSuc_NombreFuncionDll"];
                            if (!DBNull.Value.Equals(oDr["CSuc_NroResolucion"])) config.CSuc_NroResolucion = (string)oDr["CSuc_NroResolucion"];
                            if (!DBNull.Value.Equals(oDr["CSuc_ComentarioLegal"])) config.CSuc_ComentarioLegal = (string)oDr["CSuc_ComentarioLegal"];
                            if (!DBNull.Value.Equals(oDr["CSuc_CuentaCorriente"])) config.CSuc_CuentaCorriente = (string)oDr["CSuc_CuentaCorriente"];
                            if (!DBNull.Value.Equals(oDr["CSuc_ComentarioLegalExportacion"])) config.CSuc_ComentarioLegalExportacion = (string)oDr["CSuc_ComentarioLegalExportacion"];
                            if (!DBNull.Value.Equals(oDr["CSuc_PorcentajeIGV"])) config.CSuc_PorcentajeIGV = (decimal)oDr["CSuc_PorcentajeIGV"];
                            if (!DBNull.Value.Equals(oDr["Dist_Descripcion"])) config.Dist_Descripcion = (string)oDr["Dist_Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Prov_Descripcion"])) config.Prov_Descripcion = (string)oDr["Prov_Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Depa_Descripcion"])) config.Depa_Descripcion = (string)oDr["Depa_Descripcion"];
                            if (!DBNull.Value.Equals(oDr["CSuc_CuentaDetraccion"])) config.CSuc_CuentaDetraccion = (string)oDr["CSuc_CuentaDetraccion"];
                            if (!DBNull.Value.Equals(oDr["CSuc_Detra027OrigDestPdf"])) config.CSuc_Detra027OrigDestPdf = (bool)oDr["CSuc_Detra027OrigDestPdf"];
                            if (!DBNull.Value.Equals(oDr["Form_Icono"])) config.Form_Icono = (string)oDr["Form_Icono"];
                            if (!DBNull.Value.Equals(oDr["CSuc_VistaPdf"])) config.CSuc_VistaPdf = (string)oDr["CSuc_VistaPdf"];
                            if (!DBNull.Value.Equals(oDr["CEmi_PagoDetraccion"])) config.CEmi_PagoDetraccion = (string)oDr["CEmi_PagoDetraccion"];

                        }
                    }
                }
            }
            return config;
        }
    }
}
