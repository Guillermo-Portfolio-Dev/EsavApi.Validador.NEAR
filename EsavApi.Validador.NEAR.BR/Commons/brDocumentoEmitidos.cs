using EsavApi.Validador.NEAR.BR.Commons;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace EsavApi.Validador.BR.Commons
{
    public class brDocumentoEmitidos : brGenerico
    {
        public async Task<bool> ProcesoStockDocumentos(SqlConnection oConnection, string TipoDocumento, string IdEmisor)
        {
            bool TransaccionCorrecta = false;
            var plan = new Plan();

            using (SqlCommand oCommand = new SqlCommand("dbo.uspServicio_ObtenerStockDocumentos", oConnection))
            {
                oCommand.CommandType = CommandType.StoredProcedure;
                oCommand.Parameters.Add(new SqlParameter("@p_IdEmisor", SqlDbType.VarChar)).Value = IdEmisor;

                using (SqlDataReader oDr = await oCommand.ExecuteReaderAsync())
                {
                    if (oDr.HasRows)
                    {
                        TransaccionCorrecta = true;
                        while (oDr.Read())
                        {
                            plan.TipoPlan = oDr.IsDBNull(oDr.GetOrdinal("TipoPlan")) ? null : oDr.GetString(oDr.GetOrdinal("TipoPlan"));
                            plan.FechaContratacion = oDr.IsDBNull(oDr.GetOrdinal("FechaContratacion")) ? "" : oDr.GetDateTime(oDr.GetOrdinal("FechaContratacion")).ToString("yyyy-MM-dd");
                            plan.FechaFinPlan = oDr.IsDBNull(oDr.GetOrdinal("FechaFinPlan")) ? "" : oDr.GetDateTime(oDr.GetOrdinal("FechaFinPlan")).ToString("yyyy-MM-dd");
                        }
                    }

                    if (!string.IsNullOrEmpty(plan.FechaContratacion) && !string.IsNullOrEmpty(plan.FechaFinPlan))
                    {
                        if (plan.TipoPlan != "I")
                        {
                            _ = AlmacenarCantidadDocumentos(IdEmisor, TipoDocumento, plan.FechaContratacion, plan.FechaFinPlan);
                        }
                    }
                }
            }

            return TransaccionCorrecta;
        }
        public async Task<bool> AlmacenarCantidadDocumentos(string IdEmisor, string tipoDocumento, string FechaContratacion, string FechaFinPlan)
        {
            bool status = false;

            try
            {
                var table = TablaEsavDoc($"STOCKPLAN{IdEmisor}API");
                System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("es-PE");

                var entidades = ListarTabla(table, "RowKey", $"{tipoDocumento}-{FechaContratacion.Replace("-", string.Empty)}");
                DynamicTableEntity entidad = entidades?.LastOrDefault();

                string partitionKey = IdEmisor;
                string rowKey = $"{tipoDocumento}-{FechaContratacion.Replace("-", string.Empty)}";

                var retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey);
                var result = await table.ExecuteAsync(retrieveOperation);

                if (result.Result != null)
                {
                    int nuevaCantidad = 0;

                    if (entidad.Properties.ContainsKey("Cantidad"))
                    {
                        var cantidadActual = entidad.Properties["Cantidad"].Int32Value ?? 0;
                        nuevaCantidad = cantidadActual + 1;
                    }
                    else
                    {
                        nuevaCantidad = 1;
                    }

                    status = await CrearYGuardarEntidad(table, partitionKey, rowKey, tipoDocumento, nuevaCantidad, FechaContratacion, FechaFinPlan);
                }
                else
                {
                    status = await CrearYGuardarEntidad(table, partitionKey, rowKey, tipoDocumento, 1, FechaContratacion, FechaFinPlan);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("AlmacenarCantidadDocumentos", ex);
            }

            return status;
        }
        private async Task<bool> CrearYGuardarEntidad(CloudTable table, string partitionKey, string rowKey, string tipoDocumento, int cantidad, string fechaContratacion, string fechaFinPlan)
        {
            try
            {
                DynamicTableEntity register = new DynamicTableEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey
                };

                register.Properties.Add("TipoDocumento", new EntityProperty(tipoDocumento));
                register.Properties.Add("Cantidad", new EntityProperty(cantidad));
                register.Properties.Add("FechaContratacion", new EntityProperty(fechaContratacion));
                register.Properties.Add("FechaFinPlan", new EntityProperty(fechaFinPlan));

                return InsertarOActualizarTabla(table, register);
            }
            catch (Exception ex)
            {
                await LogAsync("CrearYGuardarEntidad", ex);
                return false;
            }
        }
        private class Plan
        {
            public string TipoPlan { get; set; }
            public string FechaContratacion { get; set; }
            public string FechaFinPlan { get; set; }
        }
    }
}
