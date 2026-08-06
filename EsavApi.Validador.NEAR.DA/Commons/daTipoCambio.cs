using System;
using System.Data;
using System.Data.SqlClient;

namespace EsavApi.Validador.NEAR.DA
{
    public class daTipoCambio
    {
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
    }
}
