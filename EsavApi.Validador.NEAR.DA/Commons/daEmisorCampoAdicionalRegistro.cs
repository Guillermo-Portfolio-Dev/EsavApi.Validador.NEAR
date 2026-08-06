using EsavApi.Validador.NEAR.BE.Commons;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.DA
{
    public class daEmisorCampoAdicionalRegistro
    {
        public async Task<bool> Guardar(SqlConnection oConnection, beEmisorCampoAdicionalRegistro oFactura)
        {
            bool TransaccionCorrecta = true;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_EmisorCampoAdicionalRegistro_Guardar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@Accion", oFactura.Accion);
                if (oFactura.IdEmisor != null) oCommand.Parameters.AddWithValue("@Emis_IdEmisor", oFactura.IdEmisor);
                if (oFactura.IdRubro != 0) oCommand.Parameters.AddWithValue("@Rubr_IdRubro", oFactura.IdRubro);
                if (oFactura.IdCampoAdicional != null) oCommand.Parameters.AddWithValue("@CAdi_IdCampoAdicional", oFactura.IdCampoAdicional);
                if (oFactura.Ca01_Id != null) oCommand.Parameters.AddWithValue("@Ca01_Id", oFactura.Ca01_Id);
                if (oFactura.Serie != null) oCommand.Parameters.AddWithValue("@ECAR_Serie", oFactura.Serie);
                if (oFactura.Numero != null) oCommand.Parameters.AddWithValue("@ECAR_Numero", oFactura.Numero);
                if (oFactura.Index != 0) oCommand.Parameters.AddWithValue("@ECAR_Index", oFactura.Index);
                if (oFactura.Titulo != null) oCommand.Parameters.AddWithValue("@ECAR_Titulo", oFactura.Titulo);
                if (oFactura.Valor != null) oCommand.Parameters.AddWithValue("@ECAR_Valor", oFactura.Valor);
                oCommand.Parameters.AddWithValue("@ECAR_EsDetalle", oFactura.EsDetalle);
                oCommand.Parameters.AddWithValue("@ECAR_EnXML", oFactura.EnXML);
                oCommand.Parameters.AddWithValue("@ECAR_EnRepresentacionImpresa", oFactura.EnRepresentacionImpresa);

                int FilasAfectadas = await oCommand.ExecuteNonQueryAsync();
                if (FilasAfectadas == -1) TransaccionCorrecta = false;
            }
            return TransaccionCorrecta;
        }
    }
}
