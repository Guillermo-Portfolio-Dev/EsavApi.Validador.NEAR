using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.DA.Commons
{
    public class daDocumentoEmitidos
    {
        public async Task<bool> UpdateStockDocumentoEmitido(SqlConnection oConnection, string IdEmisor)
        {
            bool TransaccionCorrecta = false;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_DescontarCantidadDocumentos", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@p_IdEmisor", IdEmisor);

                int FilaAfectadas = await oCommand.ExecuteNonQueryAsync();
                if (FilaAfectadas != -1) TransaccionCorrecta = true;
            }
            return TransaccionCorrecta;
        }
    }
}
