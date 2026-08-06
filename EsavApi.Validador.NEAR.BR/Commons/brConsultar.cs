using EsavApi.Validador.NEAR.BE.Boleta;
using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.BE.Factura;
using EsavApi.Validador.NEAR.DA;
using EsavApi.Validador.NEAR.DA.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brConsultar : brGenerico
    {
        public int Consultar(string ruc, string sede, string serieRef, string numeroRef)
        {
            int existe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    existe = consultar.Obtener(connApi, ruc, sede, serieRef, numeroRef);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarNCre_Ref", ex);
            }

            return existe;
        }
        public decimal ConsultaImporte(string ruc, string sede, string serieRef, string tipoDocRef, string numeroRef)
        {
            decimal importe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    importe = consultar.ObtenerImporte(connApi, ruc, sede, serieRef, tipoDocRef, numeroRef);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultaImporte", ex);
            }

            return importe;
        }
        public DateTime ConsultarDocReferencia(string ruc, string sede, string serieRef, string numeroRef, string tipoDoc)
        {
            DateTime fecha = default;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    fecha = consultar.ObtenerDocReferencia(connApi, ruc, sede, serieRef, numeroRef, tipoDoc);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarDocReferencia", ex);
            }

            return fecha;
        }
        public DateTime ConsultarDocReferenciaValidar(string ruc, string sede, string serieRef, string numeroRef, string tipoDoc)
        {
            DateTime fecha = default;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    fecha = consultar.ObtenerDocReferencia(connApi, ruc, sede, serieRef, numeroRef, tipoDoc);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarDocReferenciaValidar", ex);
            }

            return fecha;
        }
        public int ExisteDocReferencia(string ruc, string sede, string serieRef, string numeroRef, string tipoDocRef)
        {
            int existe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    existe = consultar.ExisteDocumentoReferenciado(connApi, ruc, sede, serieRef, numeroRef, tipoDocRef);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync($"ConsultarDoc_Ref: {ruc}-{sede}-{serieRef}-{numeroRef}-{tipoDocRef}", ex);
            }

            return existe;
        }
        public int EstadoSunat(string ruc, string serieRef, string numeroRef, string tipoDocRef)
        {
            int existe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    existe = consultar.EstadoSunatDocReferencia(connApi, ruc, serieRef, numeroRef, tipoDocRef);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarDoc_Ref", ex);
            }

            return existe;
        }
        public Tuple<string, string> UnidaMedidaText(string unidad)
        {
            string descripcion = "";
            string abreviatura = "";
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    (descripcion, abreviatura) = consultar.UnidaMedidaText(connApi, unidad);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("UnidaMedidaText", ex);
            }

            return new Tuple<string, string>(descripcion, abreviatura);
        }
        public List<beCatalogo> ListarCodigoProductoSunat(string codigo)
        {
            List<beCatalogo> lista = null;
            try
            {
                //string NombreArchivo = Path.Combine(@"I:\Mi unidad\FACTURACION\PERU\EsavApi.Validador.NEAR\EsavApi.Validador.NEAR\", "DataFileNear", "UNSPSC_Spanish_v14.csv");
                string NombreArchivo = AppDomain.CurrentDomain.BaseDirectory + "DataFileNear/UNSPSC_Spanish_v14.csv";
                lista = File.ReadAllLines(NombreArchivo).Select(x => x.Split(';')).Where(x => x[0].StartsWith(codigo)).Select(x => new beCatalogo() { Codigo = x[0], Descripcion = x[1] }).Take(20).ToList();
            }
            catch (Exception ex)
            {
                _ = LogAsync("ListarCodigoProductoSunat", ex);
            }

            return lista;
        }
        public beDataEmisor ConsultarDataEmisor(string ruc, string serie, string usuario, string sucursal, string tipoDoc)
        {
            beDataEmisor de = new beDataEmisor();
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    de = consultar.ConsultarDataEmisor(connApi, ruc, serie, usuario, sucursal, tipoDoc);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarDataEmisor", ex);
            }

            return de;
        }
        public int ConsultarRuc(string ruc)
        {
            int existe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    existe = consultar.ExisteRuc(connApi, ruc);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ConsultarRuc", ex);
            }

            return existe;
        }
        public int ExisteDocDuplicado(string ruc, string serie, string numero, string tipoDoc)
        {
            int existe = 0;
            daConsultar consultar = new daConsultar();

            try
            {
                using (connApi)
                {
                    connApi.Open();
                    existe = consultar.ExisteDuplicado(connApi, ruc, serie, numero, tipoDoc);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ExisteDocDuplicado", ex);
            }

            return existe;
        }
        public beDocumentoAdicional ComunicacionBajaObtener(String Ruc, String TipoDocumento, String Serie, String Numero)
        {
            daConsultar odaDocumentoAdicional = new daConsultar();
            beDocumentoAdicional obeDocumento = new beDocumentoAdicional();
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    obeDocumento = odaDocumentoAdicional.fComunicacionBaja(connApi, Ruc, TipoDocumento, Serie, Numero);
                }
                catch (Exception ex)
                {
                    _ = LogAsync($"ComunicacionBajaObtener: {Ruc}-{TipoDocumento}-{Serie}-{Numero}", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }

            return (obeDocumento);
        }
        public List<beCampoAdicional> ListarCampoAdicional(string Ruc, string IdRubro, string TipoDocumentoEmision, bool EsDetalle)
        {
            daConsultar odaCampoAdicional = new daConsultar();
            List<beCampoAdicional> lbeCampoAdicional = new List<beCampoAdicional>();
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    lbeCampoAdicional = odaCampoAdicional.fListar(connApi, Ruc, IdRubro, TipoDocumentoEmision, EsDetalle);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("ListarCampoAdicional", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }


            return (lbeCampoAdicional);
        }
        public beBoletaAnticipo BoletaAnticipo_Obtener(String IdEmisor, String DocCliente, String TipoDocCliente, String Serie, String Numero)
        {
            beBoletaAnticipo obeBoleta = new beBoletaAnticipo();
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    obeBoleta = new daConsultar().fOBtenerBoletaAnticipo(connApi, IdEmisor, DocCliente, TipoDocCliente, Serie, Numero);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("BoletaAnticipo_Obtener", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }


            return obeBoleta;
        }
        public beFacturaAnticipo FacturaAnticipo_Obtener(String IdEmisor, String DocCliente, String TipoDocCliente, String Serie, String Numero)
        {
            beFacturaAnticipo obeBoleta = new beFacturaAnticipo();
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    obeBoleta = new daConsultar().fOBtenerFacturaAnticipo(connApi, IdEmisor, DocCliente, TipoDocCliente, Serie, Numero);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("FacturaAnticipo_Obtener", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }


            return obeBoleta;
        }
        public decimal ObtenerTipoCambio(String Fecha, String Moneda)
        {
            daTipoCambio odaTipoCambio = new daTipoCambio();
            decimal cambio = 0;
            using (conn)
            {
                try
                {
                    conn.Open();
                    cambio = odaTipoCambio.fObtener(conn, Fecha, Moneda);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("ObtenerTipoCambio", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }


            return (cambio);
        }
        public decimal ObtenerMontoPendienteAnticipo(String idEmisor, String Tipo, String Serie, String Numero)
        {
            daConsultar odaMontoPendiente = new daConsultar();
            decimal Monto = 0;
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    Monto = odaMontoPendiente.fObtenerMontoPendiente(connApi, idEmisor, Tipo, Serie, Numero);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("ObtenerMontoPendienteAnticipo", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }


            return (Monto);
        }
        public beRUC ObtenerRuc(String Ruc, String IdEmisor)
        {
            daRuc odaRUC = new daRuc();
            beRUC obeRUC = new beRUC();
            using (conn)
            {
                try
                {
                    conn.Open();
                    obeRUC = odaRUC.fObtener(conn, Ruc, IdEmisor);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("ObtenerRuc", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }



            return (obeRUC);
        }
        public string ObtenerUbigeo(String ubigeo)
        {
            daUbigeo odaUBIGEO = new daUbigeo();
            string ubi = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    ubi = odaUBIGEO.fObtener(conn, ubigeo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("ObtenerUbigeo", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }



            return (ubi);
        }
        public string Catalogo_61(String codigo)
        {
            daConsultar odaCatalogo_61 = new daConsultar();
            string catalog_61 = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    catalog_61 = odaCatalogo_61.Catalogo_61_Obtener(conn, codigo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("Catalogo_61", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return catalog_61;
        }
        public string Catalogo_54(String codigo)
        {
            daConsultar odaCatalogo_61 = new daConsultar();
            string catalog_61 = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    catalog_61 = odaCatalogo_61.Catalogo_54_Obtener(conn, codigo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("Catalogo_61", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return catalog_61;
        }
        public string Catalogo_55(String codigo)
        {
            daConsultar odaCatalogo_55 = new daConsultar();
            string catalog_55 = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    catalog_55 = odaCatalogo_55.Catalogo_55_Obtener(conn, codigo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("Catalogo_55", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return catalog_55;
        }
        public string Catalogo_63(String codigo)
        {
            daConsultar odaCatalogo_63 = new daConsultar();
            string catalog_63 = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    catalog_63 = odaCatalogo_63.Catalogo_63_Obtener(conn, codigo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("Catalogo_63", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return catalog_63;
        }
        public string Catalogo_65(String codigo)
        {
            daConsultar odaCatalogo_65 = new daConsultar();
            string catalog_65 = string.Empty;
            using (conn)
            {
                try
                {
                    conn.Open();
                    catalog_65 = odaCatalogo_65.Catalogo_65_Obtener(conn, codigo);
                }
                catch (Exception ex)
                {
                    _ = LogAsync("Catalogo_65", ex);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }

            return catalog_65;
        }
        public beDocumentoElectronico ObtenerDocElectronicoForNC(String Ruc, String TipoDocumento, String Serie, String Numero)
        {
            daConsultar odaDocumento = new daConsultar();
            beDocumentoElectronico lbeDocumento = new beDocumentoElectronico();
            using (connApi)
            {
                try
                {
                    connApi.Open();
                    if (TipoDocumento != "09" && TipoDocumento != "31")
                    {
                        lbeDocumento = odaDocumento.fObtener(connApi, Ruc, TipoDocumento, Serie, Numero);
                    }
                    //else { lbeDocumento = odaDocumento.GuiaObtener(conn, Ruc, TipoDocumento, Serie, Numero); }

                }
                catch (Exception ex)
                {
                    _ = LogAsync("ObtenerDocElectronico", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }


            return (lbeDocumento);
        }

        private TokenResponse GeneracionToken()
        {
            try
            {
                string clientId = "2b3e7650-d1d1-4849-8ff2-268cc5de807c";
                string clientSecret = "dn8zdq79lOOjYqi7KzVdqA==";
                HttpClient _httpClient = new HttpClient();
                string url = $"https://api-seguridad.sunat.gob.pe/v1/clientesextranet/{clientId}/oauth2/token";

                var parametros = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "https://api.sunat.gob.pe/v1/contribuyente/contribuyentes"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                HttpResponseMessage response = _httpClient.PostAsync(url, parametros).Result;

                response.EnsureSuccessStatusCode();

                string jsonResponse = response.Content.ReadAsStringAsync().Result;

                var tokenObj = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

                return tokenObj;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar token: {ex.Message}");
            }
        }
        private string ValidarComprobante(string ruc, object bodyRequest)
        {
            try
            {
                string token = GetToken();
                using (var _httpClient = new HttpClient())
                {
                    string url = $"https://api.sunat.gob.pe/v1/contribuyente/contribuyentes/{ruc}/validarcomprobante";

                    string jsonBody = System.Text.Json.JsonSerializer.Serialize(bodyRequest);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                    HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;

                    response.EnsureSuccessStatusCode();

                    string jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return jsonResponse;
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("ValidarComprobante", ex);
                return $"Error: {ex.Message}";
            }
        }

        private static string _token;
        private static DateTime _expiration;
        public string GetToken()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.Now < _expiration)
            {
                return _token;
            }
            var tokenResponse = GeneracionToken();
            _token = tokenResponse.access_token;
            _expiration = DateTime.Now.AddSeconds(tokenResponse.expires_in - 60);

            return _token;
        }

        public string SunatConsultaApi(string rucCliente, string tipoDoc, string serie, int numero, string fechaEmision, decimal monto)
        {
            try
            {
                string ruc = "20600705785"; //del q hace la consulta
                var requestBody = new
                {
                    numRuc = rucCliente,
                    codComp = tipoDoc,
                    numeroSerie = serie,
                    numero = numero,
                    fechaEmision = fechaEmision,
                    monto = monto
                };

                string resultado = ValidarComprobante(ruc, requestBody);

                using (JsonDocument doc = JsonDocument.Parse(resultado))
                {
                    JsonElement data = doc.RootElement.GetProperty("data");

                    string estadoCp = data.TryGetProperty("estadoCp", out JsonElement estadoCpElem)
                             ? estadoCpElem.GetString()
                             : null;

                    string estadoRuc = data.TryGetProperty("estadoRuc", out JsonElement estadoRucElem)
                        ? estadoRucElem.GetString()
                        : null;

                    string condDomiRuc = data.TryGetProperty("condDomiRuc", out JsonElement condDomiRucElem)
                        ? condDomiRucElem.GetString()
                        : null;

                    return estadoCp;
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("SunatConsultaApi", ex);
                return "";
            }
        }
    }
}
