using EsavApi.Validador.NEAR.BE.Boleta;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.Factura;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EsavApi.Validador.NEAR.DA
{
    public class daConsultar
    {
        public int Obtener(SqlConnection conn, string ruc, string sede, string serieRef, string numeroRef)
        {
            int existe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_NotaCredito_DocRef", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@RUC", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@SERIEREF", SqlDbType.VarChar).Value = serieRef;
                oCommand.Parameters.Add("@NUMEROREF", SqlDbType.VarChar).Value = numeroRef;
                oCommand.Parameters.Add("@SEDE", SqlDbType.VarChar).Value = sede;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["EXISTE"])) existe = (Int32)oDr["EXISTE"];
                        }
                    }
                }
            }
            return existe;
        }
        public decimal ObtenerImporte(SqlConnection conn, string ruc, string sede, string serieRef, string tipoDocRef, string numeroRef)
        {
            decimal importe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_NotaCredito_Importe", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@RUC", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@SERIEREF", SqlDbType.VarChar).Value = serieRef;
                oCommand.Parameters.Add("@NUMEROREF", SqlDbType.VarChar).Value = Convert.ToInt32(numeroRef);
                oCommand.Parameters.Add("@TIPODOCREF", SqlDbType.VarChar).Value = tipoDocRef;
                oCommand.Parameters.Add("@SEDE", SqlDbType.VarChar).Value = sede;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["importe"])) importe = (decimal)oDr["importe"];
                        }
                    }
                }
            }
            return importe;
        }
        public DateTime ObtenerDocReferencia(SqlConnection conn, string ruc, string sede, string serieRef, string numeroRef, string tipoDoc)
        {
            DateTime fecha = new DateTime();
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_DocumentoReferencia_validar", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar).Value = serieRef;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar).Value = Convert.ToInt32(numeroRef);
                oCommand.Parameters.Add("@TipoDoc", SqlDbType.VarChar).Value = tipoDoc;
                oCommand.Parameters.Add("@Sucursal", SqlDbType.VarChar).Value = sede;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["FechaDocRel"])) fecha = (DateTime)oDr["FechaDocRel"];
                        }
                    }
                }
            }
            return fecha;
        }
        public int ExisteDocumentoReferenciado(SqlConnection conn, string ruc, string sede, string serieRef, string numeroRef, string tipoDcoRef)
        {
            int existe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_Existe_DocRef", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Ruc", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@SerieRef", SqlDbType.VarChar).Value = serieRef;
                oCommand.Parameters.Add("@NumeroRef", SqlDbType.VarChar).Value = Convert.ToInt32(numeroRef).ToString();
                oCommand.Parameters.Add("@Sede", SqlDbType.VarChar).Value = sede;
                oCommand.Parameters.Add("@TipoDocRef", SqlDbType.VarChar).Value = tipoDcoRef;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["existe"])) existe = (Int32)oDr["existe"];
                        }
                    }
                }
            }
            return existe;
        }
        public int EstadoSunatDocReferencia(SqlConnection conn, string ruc, string serieRef, string numeroRef, string tipoDcoRef)
        {
            int existe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_EstadoSunat", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Ruc", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@SerieRef", SqlDbType.VarChar).Value = serieRef;
                oCommand.Parameters.Add("@NumeroRef", SqlDbType.VarChar).Value = Convert.ToInt32(numeroRef);
                oCommand.Parameters.Add("@TipoDocRef", SqlDbType.VarChar).Value = tipoDcoRef;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["estado"])) existe = (Int32)oDr["estado"];
                        }
                    }
                }
            }
            return existe;
        }
        public Tuple<string, string> UnidaMedidaText(SqlConnection conn, string unidad)
        {
            string descripcion = "";
            string abreviatura = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.usp_Servicio_Catalogo03_Text", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@ID", SqlDbType.VarChar).Value = unidad;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Abreviatura"])) abreviatura = (String)oDr["Abreviatura"];
                        }
                    }
                }
            }
            return new Tuple<string, string>(descripcion, abreviatura);
        }
        public beDataEmisor ConsultarDataEmisor(SqlConnection conn, string ruc, string serie, string usuario, string sucursal, string tipoDoc)
        {
            beDataEmisor de = new beDataEmisor();
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_DatosConfigurados", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar).Value = serie;
                oCommand.Parameters.Add("@Usuario", SqlDbType.VarChar).Value = usuario;
                oCommand.Parameters.Add("@Sucursal", SqlDbType.VarChar).Value = sucursal;
                oCommand.Parameters.Add("@TipoDoc", SqlDbType.VarChar).Value = tipoDoc;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["T_SERIE"])) de.serie = (Int32)oDr["T_SERIE"];
                            if (!DBNull.Value.Equals(oDr["T_SERIE_USUARIO"])) de.serieUsuario = (Int32)oDr["T_SERIE_USUARIO"];
                            if (!DBNull.Value.Equals(oDr["T_USUARIO"])) de.usuario = (Int32)oDr["T_USUARIO"];
                            if (!DBNull.Value.Equals(oDr["T_SUCURSAL"])) de.sucursal = (Int32)oDr["T_SUCURSAL"];
                            if (!DBNull.Value.Equals(oDr["T_SUCURSAL_USUARIO"])) de.sucursalUsuario = (Int32)oDr["T_SUCURSAL_USUARIO"];
                            if (!DBNull.Value.Equals(oDr["T_EMISOR"])) de.emisor = (Int32)oDr["T_EMISOR"];
                            if (!DBNull.Value.Equals(oDr["T_ESTADO"])) de.estado = (Int32)oDr["T_ESTADO"];
                        }
                    }
                }
            }
            return de;
        }
        public int ExisteRuc(SqlConnection conn, string ruc)
        {
            int existe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_ConsultarRuc", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@RUC", SqlDbType.VarChar).Value = ruc;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["existe"])) existe = (Int32)oDr["existe"];
                        }
                    }
                }
            }
            return existe;
        }
        public int ExisteDuplicado(SqlConnection conn, string ruc, string serie, string numero, string tipoDoc)
        {
            int existe = 0;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_ExisteDuplicado", conn))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Ruc", SqlDbType.VarChar).Value = ruc;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar).Value = serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar).Value = Convert.ToInt32(numero);
                oCommand.Parameters.Add("@TipoDoc", SqlDbType.VarChar).Value = tipoDoc;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            if (!DBNull.Value.Equals(oDr["existe"])) existe = (Int32)oDr["existe"];
                        }
                    }
                }
            }
            return existe;
        }
        public beDocumentoAdicional fComunicacionBaja(SqlConnection oConnection, String Ruc, String TipoDocumento, String Serie, String Numero)
        {
            beDocumentoAdicional _beDocumentoAdicional = new beDocumentoAdicional();

            using (SqlCommand oCommand = new SqlCommand("dbo.uspReporte_DocumentoComunicacionBaja_Obtener", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Ruc", SqlDbType.VarChar).Value = Ruc;
                oCommand.Parameters.Add("@Cat01", SqlDbType.VarChar).Value = TipoDocumento;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar).Value = Serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar).Value = Convert.ToInt32(Numero).ToString();
                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            _beDocumentoAdicional = new beDocumentoAdicional();
                            _beDocumentoAdicional.Codigo = odr.IsDBNull(odr.GetOrdinal("Codigo")) ? "" : Convert.ToString(odr.GetInt32(odr.GetOrdinal("Codigo"))).TrimEnd();
                            _beDocumentoAdicional.TipoDocumento = odr.IsDBNull(odr.GetOrdinal("Tipo")) ? "" : odr.GetString(odr.GetOrdinal("Tipo"));
                            _beDocumentoAdicional.CodigoTipoDocumento = odr.IsDBNull(odr.GetOrdinal("CodigoTipo")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("CodigoTipo"))).TrimEnd();
                            _beDocumentoAdicional.Serie = odr.IsDBNull(odr.GetOrdinal("Serie")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Serie"))).TrimEnd();
                            _beDocumentoAdicional.Numero = odr.IsDBNull(odr.GetOrdinal("Numero")) ? 0 : Convert.ToInt32(odr.GetString(odr.GetOrdinal("Numero")));
                            _beDocumentoAdicional.CodigoTipoDocumentoReferencia = odr.IsDBNull(odr.GetOrdinal("CodigoTipoReferencia")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("CodigoTipoReferencia"))).TrimEnd();
                            _beDocumentoAdicional.SerieReferencia = odr.IsDBNull(odr.GetOrdinal("SerieReferencia")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("SerieReferencia"))).TrimEnd();
                            _beDocumentoAdicional.NumeroReferencia = odr.IsDBNull(odr.GetOrdinal("NumeroReferencia")) ? 0 : Convert.ToInt64(odr.GetString(odr.GetOrdinal("NumeroReferencia")));
                            _beDocumentoAdicional.Fecha = odr.IsDBNull(odr.GetOrdinal("FechaEmision")) ? "" : odr.GetDateTime(odr.GetOrdinal("FechaEmision")).ToString("dd/MM/yyyy");
                            _beDocumentoAdicional.Moneda = odr.IsDBNull(odr.GetOrdinal("Moneda")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Moneda")));
                            _beDocumentoAdicional.IdGravado = odr.IsDBNull(odr.GetOrdinal("IdGravado")) ? "" : odr.GetString(odr.GetOrdinal("IdGravado"));
                            _beDocumentoAdicional.Gravado = odr.IsDBNull(odr.GetOrdinal("Gravado")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Gravado"));
                            _beDocumentoAdicional.IdInafecto = odr.IsDBNull(odr.GetOrdinal("IdInafecto")) ? "" : odr.GetString(odr.GetOrdinal("IdInafecto"));
                            _beDocumentoAdicional.Inafecto = odr.IsDBNull(odr.GetOrdinal("Inafecto")) ? 0 : Convert.ToDecimal(odr.GetDecimal(odr.GetOrdinal("Inafecto")));
                            _beDocumentoAdicional.IdExonerado = odr.IsDBNull(odr.GetOrdinal("IdExonerado")) ? "" : odr.GetString(odr.GetOrdinal("IdExonerado"));
                            _beDocumentoAdicional.Exonerado = odr.IsDBNull(odr.GetOrdinal("Exonerado")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Exonerado"));
                            _beDocumentoAdicional.IdGratuito = odr.IsDBNull(odr.GetOrdinal("IdGratuito")) ? "" : odr.GetString(odr.GetOrdinal("IdGratuito"));
                            _beDocumentoAdicional.Gratuito = odr.IsDBNull(odr.GetOrdinal("Gratuito")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Gratuito"));
                            _beDocumentoAdicional.IdExportacion = odr.IsDBNull(odr.GetOrdinal("IdExportacion")) ? "" : odr.GetString(odr.GetOrdinal("IdExportacion"));
                            _beDocumentoAdicional.Exportacion = odr.IsDBNull(odr.GetOrdinal("Exportacion")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Exportacion"));
                            _beDocumentoAdicional.IndicadorOtrosCargos = odr.IsDBNull(odr.GetOrdinal("IndicadorOtrosCargos")) ? false : odr.GetBoolean(odr.GetOrdinal("IndicadorOtrosCargos"));
                            _beDocumentoAdicional.OtrosCargos = odr.IsDBNull(odr.GetOrdinal("OtrosCargos")) ? 0 : odr.GetDecimal(odr.GetOrdinal("OtrosCargos"));
                            _beDocumentoAdicional.IndicadorDescuento = odr.IsDBNull(odr.GetOrdinal("IndicadorDescuento")) ? false : odr.GetBoolean(odr.GetOrdinal("IndicadorDescuento"));
                            _beDocumentoAdicional.Descuento = odr.IsDBNull(odr.GetOrdinal("Descuento")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Descuento"));
                            _beDocumentoAdicional.IdIgv = odr.IsDBNull(odr.GetOrdinal("IdIgv")) ? "" : odr.GetString(odr.GetOrdinal("IdIgv"));
                            _beDocumentoAdicional.NombreIgv = odr.IsDBNull(odr.GetOrdinal("NombreIgv")) ? "" : odr.GetString(odr.GetOrdinal("NombreIgv"));
                            _beDocumentoAdicional.CodigoIgv = odr.IsDBNull(odr.GetOrdinal("CodigoIgv")) ? "" : odr.GetString(odr.GetOrdinal("CodigoIgv"));
                            _beDocumentoAdicional.Igv = odr.IsDBNull(odr.GetOrdinal("Igv")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Igv"));
                            _beDocumentoAdicional.IdIsc = odr.IsDBNull(odr.GetOrdinal("IdIsc")) ? "" : odr.GetString(odr.GetOrdinal("IdIsc"));
                            _beDocumentoAdicional.NombreIsc = odr.IsDBNull(odr.GetOrdinal("NombreIsc")) ? "" : odr.GetString(odr.GetOrdinal("NombreIsc"));
                            _beDocumentoAdicional.CodigoIsc = odr.IsDBNull(odr.GetOrdinal("CodigoIsc")) ? "" : odr.GetString(odr.GetOrdinal("CodigoIsc"));
                            _beDocumentoAdicional.Isc = odr.IsDBNull(odr.GetOrdinal("Isc")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Isc"));
                            _beDocumentoAdicional.IdOth = odr.IsDBNull(odr.GetOrdinal("IdOth")) ? "" : odr.GetString(odr.GetOrdinal("IdOth"));
                            _beDocumentoAdicional.NombreOth = odr.IsDBNull(odr.GetOrdinal("NombreOth")) ? "" : odr.GetString(odr.GetOrdinal("NombreOth"));
                            _beDocumentoAdicional.CodigoOth = odr.IsDBNull(odr.GetOrdinal("CodigoOth")) ? "" : odr.GetString(odr.GetOrdinal("CodigoOth"));
                            _beDocumentoAdicional.Oth = odr.IsDBNull(odr.GetOrdinal("Oth")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Oth"));
                            //_beDocumentoAdicional.IdIcbper = odr.IsDBNull(odr.GetOrdinal("IdIcbp")) ? "" : odr.GetString(odr.GetOrdinal("IdIcbp"));
                            //_beDocumentoAdicional.NombreIcbper = odr.IsDBNull(odr.GetOrdinal("NombreIcbp")) ? "" : odr.GetString(odr.GetOrdinal("NombreIcbp"));
                            //_beDocumentoAdicional.CodigoIcbper = odr.IsDBNull(odr.GetOrdinal("CodigoIcbp")) ? "" : odr.GetString(odr.GetOrdinal("CodigoIcbp"));
                            //_beDocumentoAdicional.Icbper = odr.IsDBNull(odr.GetOrdinal("Icbp")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Icbp"));
                            _beDocumentoAdicional.ImporteTotal = odr.IsDBNull(odr.GetOrdinal("Importe")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Importe")));
                            _beDocumentoAdicional.Regimen = odr.IsDBNull(odr.GetOrdinal("Regimen")) ? "" : odr.GetString(odr.GetOrdinal("Regimen"));
                            _beDocumentoAdicional.PorcentajePercepcion = odr.IsDBNull(odr.GetOrdinal("PorcentajePercepcion")) ? 0 : odr.GetDecimal(odr.GetOrdinal("PorcentajePercepcion"));
                            _beDocumentoAdicional.BaseImponible = odr.IsDBNull(odr.GetOrdinal("BaseImponible")) ? 0 : odr.GetDecimal(odr.GetOrdinal("BaseImponible"));
                            _beDocumentoAdicional.ImportePercepcion = odr.IsDBNull(odr.GetOrdinal("ImportePercepcion")) ? 0 : odr.GetDecimal(odr.GetOrdinal("ImportePercepcion"));
                            _beDocumentoAdicional.ImporteCobrar = odr.IsDBNull(odr.GetOrdinal("ImporteCobrar")) ? 0 : odr.GetDecimal(odr.GetOrdinal("ImporteCobrar"));
                            _beDocumentoAdicional.TipoDocumentoIdentidadCliente = odr.IsDBNull(odr.GetOrdinal("IdTipoDocumentoCliente")) ? "" : odr.GetString(odr.GetOrdinal("IdTipoDocumentoCliente"));
                            _beDocumentoAdicional.NroDocumentoIdentidadCliente = odr.IsDBNull(odr.GetOrdinal("NroDocumentoCliente")) ? "" : odr.GetString(odr.GetOrdinal("NroDocumentoCliente"));
                            _beDocumentoAdicional.RazónSocialCliente = odr.IsDBNull(odr.GetOrdinal("RazonSocial")) ? "" : odr.GetString(odr.GetOrdinal("RazonSocial"));
                            _beDocumentoAdicional.Estado = odr.IsDBNull(odr.GetOrdinal("Estado")) ? true : Convert.ToBoolean(odr.GetInt32(odr.GetOrdinal("Estado")));

                        }
                    }
                }
            }
            return _beDocumentoAdicional;
        }
        public List<beCampoAdicional> fListar(SqlConnection oConnection, string Ruc, string IdRubro, string TipoDocumentoEmision, bool EsDetalle)
        {
            List<beCampoAdicional> lbeCampoAdicional = new List<beCampoAdicional>();

            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_EmisorCampoAdicional_Listar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;

                if (!string.IsNullOrEmpty(Ruc)) oCommand.Parameters.AddWithValue("@Ruc", Ruc);
                if (!string.IsNullOrEmpty(IdRubro)) oCommand.Parameters.AddWithValue("@IdRubro", IdRubro);
                if (!string.IsNullOrEmpty(TipoDocumentoEmision)) oCommand.Parameters.AddWithValue("@TipoDocumentoEmision", TipoDocumentoEmision);
                oCommand.Parameters.AddWithValue("@EsDetalle", EsDetalle);

                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            beCampoAdicional _beCampoAdicional = new beCampoAdicional();
                            _beCampoAdicional.IdCampoAdicional = odr.IsDBNull(odr.GetOrdinal("ECAd_IdCampoAdicional")) ? "" : (string)odr["ECAd_IdCampoAdicional"];
                            _beCampoAdicional.Descripcion = odr.IsDBNull(odr.GetOrdinal("ECAd_Descripcion")) ? "" : (string)odr["ECAd_Descripcion"];
                            _beCampoAdicional.Titulo = odr.IsDBNull(odr.GetOrdinal("ECAd_Titulo")) ? "" : (string)odr["ECAd_Titulo"];
                            _beCampoAdicional.Placeholder = odr.IsDBNull(odr.GetOrdinal("ECAd_Placeholder")) ? "" : (string)odr["ECAd_Placeholder"];
                            _beCampoAdicional.TipoDato = odr.IsDBNull(odr.GetOrdinal("ECAd_TipoDato")) ? "" : (string)odr["ECAd_TipoDato"];
                            _beCampoAdicional.Icono = odr.IsDBNull(odr.GetOrdinal("ECAd_Icono")) ? "" : (string)odr["ECAd_Icono"];
                            _beCampoAdicional.MinLength = odr.IsDBNull(odr.GetOrdinal("ECAd_MinLength")) ? 0 : (int)odr["ECAd_MinLength"];
                            _beCampoAdicional.MaxLength = odr.IsDBNull(odr.GetOrdinal("ECAd_MaxLength")) ? 0 : (int)odr["ECAd_MaxLength"];
                            _beCampoAdicional.Requerido = odr.IsDBNull(odr.GetOrdinal("ECAd_Requerido")) ? false : (bool)odr["ECAd_Requerido"];
                            _beCampoAdicional.Readonly = odr.IsDBNull(odr.GetOrdinal("ECAd_Readonly")) ? false : (bool)odr["ECAd_Readonly"];
                            _beCampoAdicional.ClassNameParent = odr.IsDBNull(odr.GetOrdinal("ECAd_ClassNameParent")) ? "" : (string)odr["ECAd_ClassNameParent"];
                            _beCampoAdicional.EsDetalle = odr.IsDBNull(odr.GetOrdinal("ECAd_EsDetalle")) ? false : (bool)odr["ECAd_EsDetalle"];
                            _beCampoAdicional.EnXML = odr.IsDBNull(odr.GetOrdinal("ECAd_EnXML")) ? false : (bool)odr["ECAd_EnXML"];
                            _beCampoAdicional.EnRepresentacionImpresa = odr.IsDBNull(odr.GetOrdinal("ECAd_EnRepresentacionImpresa")) ? false : (bool)odr["ECAd_EnRepresentacionImpresa"];
                            lbeCampoAdicional.Add(_beCampoAdicional);
                        }

                    }
                }
            }
            return lbeCampoAdicional;
        }
        public beFacturaAnticipo fOBtenerFacturaAnticipo(SqlConnection oConnection, String IdEmisor, String DocCliente, String TipoDocCliente, String Serie, String Numero)
        {
            beFacturaAnticipo _beFactura = null;

            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_FacturaAnticipo_Listar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar, 11).Value = IdEmisor;
                oCommand.Parameters.Add("@DocCliente", SqlDbType.VarChar, 11).Value = DocCliente;
                oCommand.Parameters.Add("@TipoDocCliente", SqlDbType.VarChar, 1).Value = TipoDocCliente;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar, 4).Value = Serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar, 8).Value = Convert.ToInt32(Numero).ToString();

                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            _beFactura = new beFacturaAnticipo();
                            _beFactura.IdEmisor = DBNull.Value.Equals(odr["IdEmisor"]) ? "" : (string)odr["IdEmisor"];
                            _beFactura.DocCliente = DBNull.Value.Equals(odr["DocCliente"]) ? "" : (string)odr["DocCliente"];
                            _beFactura.TipoDocCliente = DBNull.Value.Equals(odr["TipoDocCliente"]) ? "" : (string)odr["TipoDocCliente"];
                            _beFactura.FechaEmision = DBNull.Value.Equals(odr["FechaEmision"]) ? "" : ((DateTime)odr["FechaEmision"]).ToString("yyyy-MM-dd");
                            _beFactura.BaseImponible = DBNull.Value.Equals(odr["BaseImponible"]) ? 0M : (decimal)odr["BaseImponible"];
                            _beFactura.Gravado = DBNull.Value.Equals(odr["Gravado"]) ? 0M : (decimal)odr["Gravado"];
                            _beFactura.Exonerado = DBNull.Value.Equals(odr["Exonerado"]) ? 0M : (decimal)odr["Exonerado"];
                            _beFactura.Inafecto = DBNull.Value.Equals(odr["Inafecto"]) ? 0M : (decimal)odr["Inafecto"];
                            _beFactura.Exportacion = DBNull.Value.Equals(odr["Exportacion"]) ? 0M : (decimal)odr["Exportacion"];
                            _beFactura.IGV = DBNull.Value.Equals(odr["IGV"]) ? 0M : (decimal)odr["IGV"];
                            _beFactura.ISC = DBNull.Value.Equals(odr["ISC"]) ? 0M : (decimal)odr["ISC"];
                            _beFactura.OtroTributos = DBNull.Value.Equals(odr["OtroTributos"]) ? 0M : (decimal)odr["OtroTributos"];
                            _beFactura.ImporteTotal = DBNull.Value.Equals(odr["ImporteTotal"]) ? 0M : (decimal)odr["ImporteTotal"];
                            _beFactura.Moneda = DBNull.Value.Equals(odr["Moneda"]) ? "" : (string)odr["Moneda"];
                        }
                    }
                }
            }
            return _beFactura;
        }
        public beBoletaAnticipo fOBtenerBoletaAnticipo(SqlConnection oConnection, String IdEmisor, String DocCliente, String TipoDocCliente, String Serie, String Numero)
        {
            beBoletaAnticipo _beBoleta = null;

            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_BoletaAnticipo_Listar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar, 11).Value = IdEmisor;
                oCommand.Parameters.Add("@DocCliente", SqlDbType.VarChar, 11).Value = DocCliente;
                oCommand.Parameters.Add("@TipoDocCliente", SqlDbType.VarChar, 1).Value = TipoDocCliente;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar, 4).Value = Serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar, 8).Value = Convert.ToInt32(Numero).ToString();

                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            _beBoleta = new beBoletaAnticipo();
                            _beBoleta.IdEmisor = DBNull.Value.Equals(odr["IdEmisor"]) ? "" : (string)odr["IdEmisor"];
                            _beBoleta.DocCliente = DBNull.Value.Equals(odr["DocCliente"]) ? "" : (string)odr["DocCliente"];
                            _beBoleta.TipoDocCliente = DBNull.Value.Equals(odr["TipoDocCliente"]) ? "" : (string)odr["TipoDocCliente"];
                            _beBoleta.FechaEmision = DBNull.Value.Equals(odr["FechaEmision"]) ? "" : ((DateTime)odr["FechaEmision"]).ToString("yyyy-MM-dd");
                            _beBoleta.BaseImponible = DBNull.Value.Equals(odr["BaseImponible"]) ? 0M : (decimal)odr["BaseImponible"];
                            _beBoleta.Gravado = DBNull.Value.Equals(odr["Gravado"]) ? 0M : (decimal)odr["Gravado"];
                            _beBoleta.Exonerado = DBNull.Value.Equals(odr["Exonerado"]) ? 0M : (decimal)odr["Exonerado"];
                            _beBoleta.Inafecto = DBNull.Value.Equals(odr["Inafecto"]) ? 0M : (decimal)odr["Inafecto"];
                            _beBoleta.Exportacion = DBNull.Value.Equals(odr["Exportacion"]) ? 0M : (decimal)odr["Exportacion"];
                            _beBoleta.IGV = DBNull.Value.Equals(odr["IGV"]) ? 0M : (decimal)odr["IGV"];
                            _beBoleta.ISC = DBNull.Value.Equals(odr["ISC"]) ? 0M : (decimal)odr["ISC"];
                            _beBoleta.OtroTributos = DBNull.Value.Equals(odr["OtroTributos"]) ? 0M : (decimal)odr["OtroTributos"];
                            _beBoleta.ImporteTotal = DBNull.Value.Equals(odr["ImporteTotal"]) ? 0M : (decimal)odr["ImporteTotal"];
                            _beBoleta.Moneda = DBNull.Value.Equals(odr["Moneda"]) ? "" : (string)odr["Moneda"];
                        }
                    }
                }
            }
            return _beBoleta;
        }
        public decimal fObtener(SqlConnection oConnection, String Fecha, String Moneda)
        {
            DateTime _Fecha;
            if (!DateTime.TryParse(Fecha, out _Fecha)) { return 0; }

            decimal _tipoCambio = 0;

            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_TipoCambio_Obtener", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@fecha", Fecha);
                oCommand.Parameters.AddWithValue("@moneda", Moneda.Trim().ToUpper());
                oCommand.Parameters.Add("@ReturnVal", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                oCommand.ExecuteNonQuery();
                _tipoCambio = Convert.ToDecimal(oCommand.Parameters["@ReturnVal"].Value == DBNull.Value ? 0 : oCommand.Parameters["@ReturnVal"].Value);

            }
            return _tipoCambio;
        }
        public decimal fObtenerMontoPendiente(SqlConnection oConnection, String idEmisor, String Tipo, String Serie, String Numero)
        {

            decimal Monto = 0;

            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_Anticipo_ObtenerMontoPendiente", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@IdEmisor", SqlDbType.VarChar, 11).Value = idEmisor;
                oCommand.Parameters.Add("@Tipo", SqlDbType.VarChar, 11).Value = Tipo;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar, 4).Value = Serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar, 8).Value = Convert.ToInt32(Numero).ToString();

                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            Monto = DBNull.Value.Equals(odr["MontoPendienteActual"]) ? 0M : (decimal)odr["MontoPendienteActual"];

                        }
                    }
                }
            }
            return Monto;
        }
        public string Catalogo_61_Obtener(SqlConnection sql, String codigo)
        {
            List<beCatalogo> Text = new List<beCatalogo>();
            beCatalogo catalogo = new beCatalogo();
            string descripcion = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Catalogo61_Listar", sql))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            catalogo = new beCatalogo();
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) catalogo.Descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Id"])) catalogo.Codigo = (String)oDr["Id"];
                            Text.Add(catalogo);
                        }
                    }

                    descripcion = Text
                    .Where(x => x.Codigo == codigo)
                    .Select(x => x.Descripcion).FirstOrDefault();
                }
            }
            return descripcion;
        }
        public string Catalogo_54_Obtener(SqlConnection sql, String codigo)
        {
            List<beCatalogo> Text = new List<beCatalogo>();
            beCatalogo catalogo = new beCatalogo();
            string descripcion = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Catalogo54_Listar", sql))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            catalogo = new beCatalogo();
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) catalogo.Descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Id"])) catalogo.Codigo = (String)oDr["Id"];
                            Text.Add(catalogo);
                        }
                    }

                    descripcion = Text
                    .Where(x => x.Codigo == codigo)
                    .Select(x => x.Descripcion).FirstOrDefault();
                }
            }
            return descripcion;
        }
        public string Catalogo_55_Obtener(SqlConnection sql, String codigo)
        {
            List<beCatalogo> Text = new List<beCatalogo>();
            beCatalogo catalogo = new beCatalogo();
            string descripcion = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Catalogo55_Listar", sql))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            catalogo = new beCatalogo();
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) catalogo.Descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Id"])) catalogo.Codigo = (String)oDr["Id"];
                            Text.Add(catalogo);
                        }
                    }

                    descripcion = Text
                    .Where(x => x.Codigo == codigo)
                    .Select(x => x.Descripcion).FirstOrDefault();
                }
            }
            return descripcion;
        }
        public string Catalogo_63_Obtener(SqlConnection sql, String codigo)
        {
            List<beCatalogo> Text = new List<beCatalogo>();
            beCatalogo catalogo = new beCatalogo();
            string descripcion = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Catalogo63_Listar", sql))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            catalogo = new beCatalogo();
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) catalogo.Descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Id"])) catalogo.Codigo = (String)oDr["Id"];
                            Text.Add(catalogo);
                        }
                    }

                    descripcion = Text
                    .Where(x => x.Codigo == codigo)
                    .Select(x => x.Descripcion).FirstOrDefault();
                }
            }
            return descripcion;
        }
        public string Catalogo_65_Obtener(SqlConnection sql, String codigo)
        {
            List<beCatalogo> Text = new List<beCatalogo>();
            beCatalogo catalogo = new beCatalogo();
            string descripcion = "";
            using (SqlCommand oCommand = new SqlCommand("dbo.uspCatalogo_Catalogo65_Listar", sql))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                using (SqlDataReader oDr = oCommand.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (oDr.HasRows)
                    {
                        while (oDr.Read())
                        {
                            catalogo = new beCatalogo();
                            if (!DBNull.Value.Equals(oDr["Descripcion"])) catalogo.Descripcion = (String)oDr["Descripcion"];
                            if (!DBNull.Value.Equals(oDr["Id"])) catalogo.Codigo = (String)oDr["Id"];
                            Text.Add(catalogo);
                        }
                    }

                    descripcion = Text
                    .Where(x => x.Codigo == codigo)
                    .Select(x => x.Descripcion).FirstOrDefault();
                }
            }
            return descripcion;
        }
        public beDocumentoElectronico fObtener(SqlConnection oConnection, String RUC, String TipoDocumento, String Serie, String Numero)
        {

            beDocumentoElectronico _beDocumentoElectronico = new beDocumentoElectronico();
            beDocumentoElectronicoDetalle _beDocumentoElectronicoDetalle;
            beDocumentoElectronicoTotal _beDocumentoElectronicoTotal;
            beDocumentoElectronicoDetalleTotal _beDocumentoElectronicoDetalleTotal;
            List<beDocumentoElectronicoDetalleTotal> lbeDocumentoElectronicoDetalleTotal = new List<beDocumentoElectronicoDetalleTotal>();
            List<beDocumentoElectronicoTotal> lbeDocumentoElectronicoTotal = new List<beDocumentoElectronicoTotal>();
            beDocumentoElectronicoCargosDescuentosGlobales _beDocumentoElectronicoCargosDescuentosGlobales = new beDocumentoElectronicoCargosDescuentosGlobales();
            List<beDocumentoElectronicoCargosDescuentosGlobales> lbeDocumentoElectronicoCargosDescuentosGlobales = new List<beDocumentoElectronicoCargosDescuentosGlobales>();

            using (SqlCommand oCommand = new SqlCommand("dbo.uspReporte_DocumentoElectronico_Obtener", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@ruc", SqlDbType.VarChar).Value = RUC;
                oCommand.Parameters.Add("@TipoDocumento", SqlDbType.VarChar).Value = TipoDocumento;
                oCommand.Parameters.Add("@Serie", SqlDbType.VarChar).Value = Serie;
                oCommand.Parameters.Add("@Numero", SqlDbType.VarChar).Value = !string.IsNullOrEmpty(Numero) ? int.Parse(Numero) : 0;
                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            _beDocumentoElectronico = new beDocumentoElectronico();
                            _beDocumentoElectronico.Id = odr.IsDBNull(odr.GetOrdinal("Id")) ? 0 : odr.GetInt32(odr.GetOrdinal("Id"));
                            _beDocumentoElectronico.Serie = odr.IsDBNull(odr.GetOrdinal("Serie")) ? "" : odr.GetString(odr.GetOrdinal("Serie"));
                            _beDocumentoElectronico.Numero = odr.IsDBNull(odr.GetOrdinal("Numero")) ? 0 : Convert.ToInt32(odr.GetString(odr.GetOrdinal("Numero")));
                            _beDocumentoElectronico.FechaEmision = DBNull.Value.Equals(odr["FechaEmision"]) ? "" : ((DateTime)odr["FechaEmision"]).ToString("M/d/yyyy hh:mm:ss");
                            _beDocumentoElectronico.FechaVencimiento = DBNull.Value.Equals(odr["FechaVencimiento"]) ? "" : ((DateTime)odr["FechaVencimiento"]).ToString("M/d/yyyy hh:mm:ss");
                            _beDocumentoElectronico.Moneda = odr.IsDBNull(odr.GetOrdinal("Moneda")) ? "" : odr.GetString(odr.GetOrdinal("Moneda"));
                            _beDocumentoElectronico.ClienteRazonSocial = odr.IsDBNull(odr.GetOrdinal("ClienteRazonSocial")) ? "" : odr.GetString(odr.GetOrdinal("ClienteRazonSocial"));
                            _beDocumentoElectronico.ClienteDocumento = odr.IsDBNull(odr.GetOrdinal("ClienteDocumento")) ? "" : odr.GetString(odr.GetOrdinal("ClienteDocumento"));
                            _beDocumentoElectronico.ClienteTipoDocumento = odr.IsDBNull(odr.GetOrdinal("ClienteTipoDocumento")) ? "" : odr.GetString(odr.GetOrdinal("ClienteTipoDocumento"));
                            _beDocumentoElectronico.ClienteDireccion = odr.IsDBNull(odr.GetOrdinal("ClienteDireccion")) ? "" : odr.GetString(odr.GetOrdinal("ClienteDireccion"));
                            _beDocumentoElectronico.ClienteUbigeo = odr.IsDBNull(odr.GetOrdinal("ClienteUbigeo")) ? "" : odr.GetString(odr.GetOrdinal("ClienteUbigeo"));
                            _beDocumentoElectronico.ValorVenta = odr.IsDBNull(odr.GetOrdinal("ValorVenta")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("ValorVenta")));
                            _beDocumentoElectronico.PrecioVenta = odr.IsDBNull(odr.GetOrdinal("PrecioVenta")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("PrecioVenta")));
                            _beDocumentoElectronico.Descuento = odr.IsDBNull(odr.GetOrdinal("Descuento")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Descuento")));
                            _beDocumentoElectronico.OtroCargo = odr.IsDBNull(odr.GetOrdinal("OtroCargo")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("OtroCargo")));
                            _beDocumentoElectronico.Anticipo = odr.IsDBNull(odr.GetOrdinal("Anticipo")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Anticipo")));
                            _beDocumentoElectronico.ImporteTotal = odr.IsDBNull(odr.GetOrdinal("ImporteTotal")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("ImporteTotal")));
                        }
                        odr.NextResult();
                        _beDocumentoElectronico.Detalle = new List<beDocumentoElectronicoDetalle>();
                        while (odr.Read())
                        {
                            _beDocumentoElectronicoDetalle = new beDocumentoElectronicoDetalle();
                            _beDocumentoElectronicoDetalle.ID = odr.IsDBNull(odr.GetOrdinal("Id")) ? 0 : Convert.ToInt32(odr.GetString(odr.GetOrdinal("Id")));
                            _beDocumentoElectronicoDetalle.Cantidad = odr.IsDBNull(odr.GetOrdinal("Cantidad")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Cantidad"));
                            _beDocumentoElectronicoDetalle.Unidad = odr.IsDBNull(odr.GetOrdinal("Unidad")) ? "" : odr.GetString(odr.GetOrdinal("Unidad"));
                            _beDocumentoElectronicoDetalle.DescripcionUnidad = odr.IsDBNull(odr.GetOrdinal("DescripcionUnidad")) ? "" : odr.GetString(odr.GetOrdinal("DescripcionUnidad"));
                            _beDocumentoElectronicoDetalle.PrecioVentaUnitario = odr.IsDBNull(odr.GetOrdinal("PrecioVenta")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("PrecioVenta")));
                            _beDocumentoElectronicoDetalle.PrecioVenta = _beDocumentoElectronicoDetalle.PrecioVentaUnitario * (double)_beDocumentoElectronicoDetalle.Cantidad;
                            _beDocumentoElectronicoDetalle.TipoPrecio = odr.IsDBNull(odr.GetOrdinal("CodigoTipoPrecio")) ? "" : odr.GetString(odr.GetOrdinal("CodigoTipoPrecio"));
                            _beDocumentoElectronicoDetalle.Descripcion = odr.IsDBNull(odr.GetOrdinal("Descripcion")) ? "" : odr.GetString(odr.GetOrdinal("Descripcion"));
                            _beDocumentoElectronicoDetalle.CodigoProducto = odr.IsDBNull(odr.GetOrdinal("CodigoProducto")) ? "" : odr.GetString(odr.GetOrdinal("CodigoProducto"));
                            _beDocumentoElectronicoDetalle.CodigoProductoSunat = odr.IsDBNull(odr.GetOrdinal("CodigoSunat")) ? "" : odr.GetString(odr.GetOrdinal("CodigoSunat"));
                            _beDocumentoElectronicoDetalle.ConceptoTributario = odr.IsDBNull(odr.GetOrdinal("ConceptoTributario")) ? "" : odr.GetString(odr.GetOrdinal("ConceptoTributario"));
                            _beDocumentoElectronicoDetalle.CodigoConceptoTributario = odr.IsDBNull(odr.GetOrdinal("CodigoConceptoTributario")) ? "" : odr.GetString(odr.GetOrdinal("CodigoConceptoTributario"));
                            _beDocumentoElectronicoDetalle.BienServicioDetraccion = odr.IsDBNull(odr.GetOrdinal("BienServicioDetraccion")) ? "" : odr.GetString(odr.GetOrdinal("BienServicioDetraccion"));
                            _beDocumentoElectronicoDetalle.FechaInicio = odr.IsDBNull(odr.GetOrdinal("FechaInicio")) ? "" : odr.GetString(odr.GetOrdinal("FechaInicio"));
                            _beDocumentoElectronicoDetalle.ValorUnitario = odr.IsDBNull(odr.GetOrdinal("ValorUnitario")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("ValorUnitario")));
                            _beDocumentoElectronicoDetalle.Descuento = odr.IsDBNull(odr.GetOrdinal("Descuento")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Descuento")));
                            //_beDocumentoElectronicoDetalle.MontoDescuento = odr.IsDBNull(odr.GetOrdinal("MontoDescuento")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("MontoDescuento")));/*AGREGADO 24-04-2023*/
                            //_beDocumentoElectronicoDetalle.unidadMedida65 = odr.IsDBNull(odr.GetOrdinal("unidadMedida65")) ? "" : odr.GetString(odr.GetOrdinal("unidadMedida65"));/*AGREGADO 07-07-2023*/
                            //_beDocumentoElectronicoDetalle.OtroCargo = odr.IsDBNull(odr.GetOrdinal("OtroCargo")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("OtroCargo")));
                            //_beDocumentoElectronicoDetalle.PorcentajeOtroCargo = odr.IsDBNull(odr.GetOrdinal("OtroCargo")) ? 0 : odr.GetDecimal(odr.GetOrdinal("OtroCargo"));
                            //_beDocumentoElectronicoDetalle.CodigoDescuento = odr.IsDBNull(odr.GetOrdinal("CodigoDescuento")) ? "" : odr.GetString(odr.GetOrdinal("CodigoDescuento"));
                            //_beDocumentoElectronicoDetalle.CodigoOtroCargo = odr.IsDBNull(odr.GetOrdinal("CodigoOtroCargo")) ? "" : odr.GetString(odr.GetOrdinal("CodigoOtroCargo"));
                            //_beDocumentoElectronicoDetalle.OtroCargo = odr.IsDBNull(odr.GetOrdinal("PorcentajeOtroCargo")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("PorcentajeOtroCargo")));
                            //_beDocumentoElectronicoDetalle.PrecioUnitarioIcbper = odr.IsDBNull(odr.GetOrdinal("PrecioUnitarioIcbper")) ? 0 : odr.GetDecimal(odr.GetOrdinal("PrecioUnitarioIcbper"));
                            _beDocumentoElectronico.Detalle.Add(_beDocumentoElectronicoDetalle);
                        }

                        odr.NextResult();
                        lbeDocumentoElectronicoTotal = new List<beDocumentoElectronicoTotal>();
                        while (odr.Read())
                        {
                            _beDocumentoElectronicoTotal = new beDocumentoElectronicoTotal();
                            _beDocumentoElectronicoTotal.Tipo = odr.IsDBNull(odr.GetOrdinal("Tipo")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Tipo")));
                            _beDocumentoElectronicoTotal.Codigo = odr.IsDBNull(odr.GetOrdinal("Codigo")) ? "" : odr.GetString(odr.GetOrdinal("Codigo"));
                            _beDocumentoElectronicoTotal.Descripcion = odr.IsDBNull(odr.GetOrdinal("Descripcion")) ? "" : odr.GetString(odr.GetOrdinal("Descripcion"));
                            _beDocumentoElectronicoTotal.Monto = odr.IsDBNull(odr.GetOrdinal("Monto")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Monto")));
                            _beDocumentoElectronicoTotal.Moneda = odr.IsDBNull(odr.GetOrdinal("Moneda")) ? "" : odr.GetString(odr.GetOrdinal("Moneda"));
                            lbeDocumentoElectronicoTotal.Add(_beDocumentoElectronicoTotal);
                        }

                        odr.NextResult();

                        lbeDocumentoElectronicoDetalleTotal = new List<beDocumentoElectronicoDetalleTotal>();
                        while (odr.Read())
                        {
                            _beDocumentoElectronicoDetalleTotal = new beDocumentoElectronicoDetalleTotal();
                            _beDocumentoElectronicoDetalleTotal.Item = odr.IsDBNull(odr.GetOrdinal("Item")) ? 0 : odr.GetInt32(odr.GetOrdinal("Item"));
                            _beDocumentoElectronicoDetalleTotal.Monto = odr.IsDBNull(odr.GetOrdinal("Monto")) ? 0 : Convert.ToDouble(odr.GetDecimal(odr.GetOrdinal("Monto")));
                            _beDocumentoElectronicoDetalleTotal.Moneda = odr.IsDBNull(odr.GetOrdinal("Moneda")) ? "" : odr.GetString(odr.GetOrdinal("Moneda"));
                            _beDocumentoElectronicoDetalleTotal.CodigoTributo = odr.IsDBNull(odr.GetOrdinal("Codigo")) ? "" : odr.GetString(odr.GetOrdinal("Codigo"));
                            _beDocumentoElectronicoDetalleTotal.Afectacion = odr.IsDBNull(odr.GetOrdinal("Afectacion")) ? "" : odr.GetString(odr.GetOrdinal("Afectacion"));
                            _beDocumentoElectronicoDetalleTotal.DetalleAfectacion = odr.IsDBNull(odr.GetOrdinal("AFectacionDetalle")) ? "" : odr.GetString(odr.GetOrdinal("AFectacionDetalle"));
                            //_beDocumentoElectronicoDetalleTotal.ProcentajeImpuesto = odr.IsDBNull(odr.GetOrdinal("ProcentajeImpuesto")) ? 0 : odr.GetDecimal(odr.GetOrdinal("ProcentajeImpuesto"));
                            //_beDocumentoElectronicoDetalleTotal.NombreImpuesto = odr.IsDBNull(odr.GetOrdinal("NombreImpuesto")) ? "" : odr.GetString(odr.GetOrdinal("NombreImpuesto"));
                            //_beDocumentoElectronicoDetalleTotal.CodigoTipoImpuesto = odr.IsDBNull(odr.GetOrdinal("CodigoTipoImpuesto")) ? "" : odr.GetString(odr.GetOrdinal("CodigoTipoImpuesto"));
                            lbeDocumentoElectronicoDetalleTotal.Add(_beDocumentoElectronicoDetalleTotal);
                        }

                        odr.NextResult();

                        lbeDocumentoElectronicoCargosDescuentosGlobales = new List<beDocumentoElectronicoCargosDescuentosGlobales>();
                        while (odr.Read())
                        {
                            _beDocumentoElectronicoCargosDescuentosGlobales = new beDocumentoElectronicoCargosDescuentosGlobales();
                            _beDocumentoElectronicoCargosDescuentosGlobales.CodigoCargo = odr.IsDBNull(odr.GetOrdinal("CodigoCargo")) ? "" : odr.GetString(odr.GetOrdinal("CodigoCargo"));
                            _beDocumentoElectronicoCargosDescuentosGlobales.Porcentaje = odr.IsDBNull(odr.GetOrdinal("FactorNumerico")) ? 0 : odr.GetDecimal(odr.GetOrdinal("FactorNumerico"));
                            _beDocumentoElectronicoCargosDescuentosGlobales.Monto = odr.IsDBNull(odr.GetOrdinal("Monto")) ? 0 : odr.GetDecimal(odr.GetOrdinal("Monto"));
                            _beDocumentoElectronicoCargosDescuentosGlobales.BaseMonto = odr.IsDBNull(odr.GetOrdinal("BaseMonto")) ? 0 : odr.GetDecimal(odr.GetOrdinal("BaseMonto"));
                            lbeDocumentoElectronicoCargosDescuentosGlobales.Add(_beDocumentoElectronicoCargosDescuentosGlobales);
                        }


                        _beDocumentoElectronico.Gravado = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1001" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1001").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Exonerado = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1003" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1003").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Exportacion = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Inafecto = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1002" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1002").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Gratuito = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1004" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1004").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Percepcion = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2001" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2001").FirstOrDefault().Monto;
                        _beDocumentoElectronico.Detraccion = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2003" && x.Tipo == "T").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2003").FirstOrDefault().Monto;
                        _beDocumentoElectronico.IGV = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000" && x.Tipo == "I").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000").FirstOrDefault().Monto;
                        _beDocumentoElectronico.ISC = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2000" && x.Tipo == "I").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2000").FirstOrDefault().Monto;
                        _beDocumentoElectronico.OTH = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "9999" && x.Tipo == "I").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "9999").FirstOrDefault().Monto;
                        _beDocumentoElectronico.ICBPER = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "7152" && x.Tipo == "I").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "7152").FirstOrDefault().Monto;

                        _beDocumentoElectronico.GravadoCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1001" && x.Tipo == "T").FirstOrDefault() == null ? "1001" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1001").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.ExoneradoCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1003" && x.Tipo == "T").FirstOrDefault() == null ? "1003" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1003").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.ExportacionCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000" && x.Tipo == "T").FirstOrDefault() == null ? "1000" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.InafectoCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1002" && x.Tipo == "T").FirstOrDefault() == null ? "1002" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1002").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.GratuitoCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1004" && x.Tipo == "T").FirstOrDefault() == null ? "1004" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1004").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.PercepcionCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2001" && x.Tipo == "T").FirstOrDefault() == null ? "2001" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2001").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.DetraccionCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2003" && x.Tipo == "T").FirstOrDefault() == null ? "2003" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2003").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.IGVCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000" && x.Tipo == "I").FirstOrDefault() == null ? "1000" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "1000").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.ISCCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2000" && x.Tipo == "I").FirstOrDefault() == null ? "2000" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "2000").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.OTHCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "9999" && x.Tipo == "I").FirstOrDefault() == null ? "9999" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "9999").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.ICBPERCodigo = lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "7152" && x.Tipo == "I").FirstOrDefault() == null ? "7152" : lbeDocumentoElectronicoTotal.Where(x => x.Codigo == "7152").FirstOrDefault().Codigo;
                        _beDocumentoElectronico.CodigoOtroCargoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault().CodigoCargo;
                        _beDocumentoElectronico.CodigoDescuentoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "03" || x.CodigoCargo == "02").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "03" || x.CodigoCargo == "02").FirstOrDefault().CodigoCargo;
                        _beDocumentoElectronico.CodigoAnticipoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault().CodigoCargo;
                        _beDocumentoElectronico.MontoOtroCargoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault().Monto;
                        _beDocumentoElectronico.MontoDescuentoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "03" || x.CodigoCargo == "02").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "02" || x.CodigoCargo == "03").FirstOrDefault().Monto;
                        _beDocumentoElectronico.MontoAnticipoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault().Monto;
                        _beDocumentoElectronico.PorcentajeOtroCargoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault().Porcentaje;
                        _beDocumentoElectronico.PorcentajeDescuentoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "03" || x.CodigoCargo == "02").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "02" || x.CodigoCargo == "03").FirstOrDefault().Porcentaje;
                        _beDocumentoElectronico.PorcentajeAnticipoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault().Porcentaje;
                        _beDocumentoElectronico.MontoBaseOtroCargoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "50" || x.CodigoCargo == "49").FirstOrDefault().BaseMonto;
                        _beDocumentoElectronico.MontoBaseDescuentoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "03" || x.CodigoCargo == "02").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "02" || x.CodigoCargo == "03").FirstOrDefault().BaseMonto;
                        _beDocumentoElectronico.MontoBaseAnticipoGlobal = lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoCargosDescuentosGlobales.Where(x => x.CodigoCargo == "04" || x.CodigoCargo == "05" || x.CodigoCargo == "06").FirstOrDefault().BaseMonto;

                        foreach (beDocumentoElectronicoDetalle o in _beDocumentoElectronico.Detalle)
                        {
                            o.IGV = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault().Monto;
                            o.AfectacionIGV = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault().Afectacion;
                            o.ISC = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault().Monto;
                            o.OTH = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault().Monto;
                            o.ICBPER = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "7152").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "7152").FirstOrDefault().Monto;
                            o.IGVCodigo = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault() == null ? "1000" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault().CodigoTributo;
                            o.ISCCodigo = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault() == null ? "2000" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault().CodigoTributo;
                            o.OTHCodigo = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault() == null ? "9999" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault().CodigoTributo;
                            o.ICBPERCodigo = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "7152").FirstOrDefault() == null ? "7152" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "7152").FirstOrDefault().CodigoTributo;
                            o.SubAfectacionIGV = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault().DetalleAfectacion;
                            o.TipoISC = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault() == null ? "" : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault().DetalleAfectacion;
                            o.PorcentajeIgv = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "1000").FirstOrDefault().ProcentajeImpuesto;
                            o.PorcentajeIsc = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "2000").FirstOrDefault().ProcentajeImpuesto;
                            //o.PorcentajeOtroCargo = lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault() == null ? 0 : lbeDocumentoElectronicoDetalleTotal.Where(x => x.Item == o.ID && x.CodigoTributo == "9999").FirstOrDefault().ProcentajeImpuesto;

                        }

                        _beDocumentoElectronico.EstadoComprobante = true;
                    }
                    else
                    {
                        _beDocumentoElectronico.EstadoComprobante = false;
                    }
                }
            }
            return _beDocumentoElectronico;
        }
    }
}
