using EsavApi.Validador.NEAR.BR.Commons;
using EsavApi.Validador.NEAR.DA;
using System;
using System.IO;

namespace EsavApi.Validador.BR
{
    public class brObtenerTipoCambio : brGenerico
    {
        public decimal Obtener(String Fecha, String Moneda)
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
                    using (StreamWriter log = new StreamWriter(ruta, true))
                    {
                        log.WriteLine(DateTime.Now.ToString());
                        log.WriteLine(ex.Message);
                        log.WriteLine(ex.StackTrace);
                        log.WriteLine("-----------------------------------");
                    }
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }


            return (cambio);
        }
    }
}
