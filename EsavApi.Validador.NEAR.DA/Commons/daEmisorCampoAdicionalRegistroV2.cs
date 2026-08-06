using EsavApi.Validador.NEAR.BE.Commons;
using System.Data;
using System.Data.SqlClient;

namespace EsavApi.Validador.NEAR.DA
{
    public class daEmisorCampoAdicionalRegistroV2
    {
        public bool Guardar(SqlConnection oConnection, EmisorCampoAdicionalRegistro oFactura)
        {
            bool TransaccionCorrecta = true;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_EmisorCampoAdicionalRegistro_Guardar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@Accion", 1);
                if (oFactura.idEmisor != null) oCommand.Parameters.AddWithValue("@Emis_IdEmisor", oFactura.idEmisor);
                if (oFactura.idRubro != 0) oCommand.Parameters.AddWithValue("@Rubr_IdRubro", oFactura.idRubro);
                if (oFactura.idCampoAdicional != null) oCommand.Parameters.AddWithValue("@CAdi_IdCampoAdicional", oFactura.idCampoAdicional);
                if (oFactura.tipoDocumento != null) oCommand.Parameters.AddWithValue("@Ca01_Id", oFactura.tipoDocumento);
                if (oFactura.serie != null) oCommand.Parameters.AddWithValue("@ECAR_Serie", oFactura.serie);
                if (oFactura.numero != null) oCommand.Parameters.AddWithValue("@ECAR_Numero", oFactura.numero);
                if (oFactura.index != 0) oCommand.Parameters.AddWithValue("@ECAR_Index", oFactura.index);
                if (oFactura.titulo != null) oCommand.Parameters.AddWithValue("@ECAR_Titulo", oFactura.titulo);
                if (oFactura.valor != null) oCommand.Parameters.AddWithValue("@ECAR_Valor", oFactura.valor);
                oCommand.Parameters.AddWithValue("@ECAR_EsDetalle", oFactura.esDetalle);
                oCommand.Parameters.AddWithValue("@ECAR_EnXML", oFactura.enXML);
                oCommand.Parameters.AddWithValue("@ECAR_EnRepresentacionImpresa", oFactura.enRepresentacionImpresa);

                int FilasAfectadas = oCommand.ExecuteNonQuery();
                if (FilasAfectadas == -1) TransaccionCorrecta = false;
            }
            return TransaccionCorrecta;
        }
    }
}
