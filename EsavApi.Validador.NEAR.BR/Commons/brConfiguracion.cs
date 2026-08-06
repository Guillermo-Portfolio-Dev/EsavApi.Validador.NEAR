using EsavApi.Validador.NEAR.BE.Commons;
using EsavApi.Validador.NEAR.DA;
using System;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brConfiguracion : brGenerico
    {
        public async Task<beConfiguracionEmisor> Consultar(string ruc, string sede, string tipoDoc = null)
        {
            beConfiguracionEmisor resultado = new beConfiguracionEmisor();
            daConfiguracion consultar = new daConfiguracion();
            using (connApi)
            {
                try
                {
                    await connApi.OpenAsync();
                    resultado = await consultar.Obtener(connApi, ruc, sede, tipoDoc);
                    connApi.Dispose();
                    connApi.Close();
                }
                catch (Exception ex)
                {

                    _ = LogAsync("ConsultarConfiguracionEmisor", ex);
                }
                finally
                {
                    if (connApi.State == System.Data.ConnectionState.Open) connApi.Close();
                }
            }

            return resultado;
        }
    }
}
