using EsavApi.Validador.NEAR.BE.Boleta;
using EsavApi.Validador.NEAR.BE.Factura;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.DA.Commons
{
    public class daCampoAdicionalDetalleRegistro
    {
        public async Task<bool> Guardar_03(SqlConnection oConnection, beBoletaDeliveryDetalle oCampoAdicionalDetalleRegistro, int accion, string IdEmisor, int IdRubro, string Ca01_Id, string Serie, string Numero)
        {
            bool TransaccionCorrecta = true;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_EmisorCampoAdicionalRegistroItem_Guardar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@Accion", accion);
                if (IdEmisor != null) oCommand.Parameters.AddWithValue("@Emis_IdEmisor", IdEmisor);
                if (IdRubro != 0) oCommand.Parameters.AddWithValue("@Rubr_IdRubro", IdRubro);
                if (Ca01_Id != null) oCommand.Parameters.AddWithValue("@Ca01_Id", Ca01_Id);
                if (Serie != null) oCommand.Parameters.AddWithValue("@ECAR_Serie", Serie);
                if (Numero != null) oCommand.Parameters.AddWithValue("@ECAR_Numero", Numero);
                if (oCampoAdicionalDetalleRegistro.index != 0) oCommand.Parameters.AddWithValue("@ECAR_Index", oCampoAdicionalDetalleRegistro.index);
                if (oCampoAdicionalDetalleRegistro.Item != null) oCommand.Parameters.AddWithValue("@ECAR_Item", oCampoAdicionalDetalleRegistro.Item);
                if (oCampoAdicionalDetalleRegistro.descripcionPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Titulo", oCampoAdicionalDetalleRegistro.descripcionPropiedad);
                if (oCampoAdicionalDetalleRegistro.idPropiedad != null) oCommand.Parameters.AddWithValue("@CAdi_IdCampoAdicional", oCampoAdicionalDetalleRegistro.idPropiedad);
                if (oCampoAdicionalDetalleRegistro.descripcionPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Name", oCampoAdicionalDetalleRegistro.descripcionPropiedad);
                if (oCampoAdicionalDetalleRegistro.valorPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Valor", oCampoAdicionalDetalleRegistro.valorPropiedad);
                oCommand.Parameters.AddWithValue("@ECAR_EnXML", oCampoAdicionalDetalleRegistro.enXML);
                oCommand.Parameters.AddWithValue("@ECAR_EnRepresentacionImpresa", oCampoAdicionalDetalleRegistro.enRepresentacionImpresa);

                int FilasAfectadas = await oCommand.ExecuteNonQueryAsync();
                if (FilasAfectadas == -1) TransaccionCorrecta = false;
            }
            return TransaccionCorrecta;
        }
        public bool Guardar_01(SqlConnection oConnection, beFacturaDeliveryDetalle oCampoAdicionalDetalleRegistro, int accion, string IdEmisor, int IdRubro, string Ca01_Id, string Serie, string Numero)
        {
            bool TransaccionCorrecta = true;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_EmisorCampoAdicionalRegistroItem_Guardar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.AddWithValue("@Accion", accion);
                if (IdEmisor != null) oCommand.Parameters.AddWithValue("@Emis_IdEmisor", IdEmisor);
                if (IdRubro != 0) oCommand.Parameters.AddWithValue("@Rubr_IdRubro", IdRubro);
                if (Ca01_Id != null) oCommand.Parameters.AddWithValue("@Ca01_Id", Ca01_Id);
                if (Serie != null) oCommand.Parameters.AddWithValue("@ECAR_Serie", Serie);
                if (Numero != null) oCommand.Parameters.AddWithValue("@ECAR_Numero", Numero);
                if (oCampoAdicionalDetalleRegistro.index != 0) oCommand.Parameters.AddWithValue("@ECAR_Index", oCampoAdicionalDetalleRegistro.index);
                if (oCampoAdicionalDetalleRegistro.Item != null) oCommand.Parameters.AddWithValue("@ECAR_Item", oCampoAdicionalDetalleRegistro.Item);
                if (oCampoAdicionalDetalleRegistro.descripcionPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Titulo", oCampoAdicionalDetalleRegistro.descripcionPropiedad);
                if (oCampoAdicionalDetalleRegistro.idPropiedad != null) oCommand.Parameters.AddWithValue("@CAdi_IdCampoAdicional", oCampoAdicionalDetalleRegistro.idPropiedad);
                if (oCampoAdicionalDetalleRegistro.descripcionPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Name", oCampoAdicionalDetalleRegistro.descripcionPropiedad);
                if (oCampoAdicionalDetalleRegistro.valorPropiedad != null) oCommand.Parameters.AddWithValue("@ECAR_Valor", oCampoAdicionalDetalleRegistro.valorPropiedad);
                oCommand.Parameters.AddWithValue("@ECAR_EnXML", oCampoAdicionalDetalleRegistro.enXML);
                oCommand.Parameters.AddWithValue("@ECAR_EnRepresentacionImpresa", oCampoAdicionalDetalleRegistro.enRepresentacionImpresa);

                int FilasAfectadas = oCommand.ExecuteNonQuery();
                if (FilasAfectadas == -1) TransaccionCorrecta = false;
            }
            return TransaccionCorrecta;
        }
    }
}
