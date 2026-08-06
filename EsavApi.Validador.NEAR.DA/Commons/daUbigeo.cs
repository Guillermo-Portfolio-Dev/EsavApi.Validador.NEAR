using System;
using System.Data;
using System.Data.SqlClient;

namespace EsavApi.Validador.NEAR.DA.Commons
{
    public class daUbigeo
    {
        public string fObtener(SqlConnection oConnection, String Ubigeo)
        {
            string Departamento = string.Empty;
            string Provincia = string.Empty;
            string Distrito = string.Empty;

            string Resultado = string.Empty;
            using (SqlCommand oCommand = new SqlCommand("dbo.uspTransaccion_Ubigeo_Buscar", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add("@Codigo_ubigeo", SqlDbType.VarChar).Value = Ubigeo;
                using (SqlDataReader odr = oCommand.ExecuteReader())
                {
                    if (odr.HasRows)
                    {
                        while (odr.Read())
                        {
                            Departamento = odr.IsDBNull(odr.GetOrdinal("Depa_Descripcion")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Depa_Descripcion")));
                            Provincia = odr.IsDBNull(odr.GetOrdinal("Prov_Descripcion")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Prov_Descripcion")));
                            Distrito = odr.IsDBNull(odr.GetOrdinal("Dist_Descripcion")) ? "" : Convert.ToString(odr.GetString(odr.GetOrdinal("Dist_Descripcion")));
                        }

                        Resultado = (Departamento ?? "") + " - " + (Provincia ?? "") + " - " + (Distrito ?? "");
                    }
                }
            }
            return Resultado;
        }
    }
}
