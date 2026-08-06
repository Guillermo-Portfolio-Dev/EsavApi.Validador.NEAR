using EsavApi.Validador.NEAR.BE.Commons;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brStorage : brGenerico
    {
        public async Task<bool> GuardarRechazos(beRechazo oRechazo, string RucEmisor)
        {
            try
            {
                DynamicTableEntity register = CrearEntidadRechazo(oRechazo);
                var tablaNombre = $"Rechazos{RucEmisor}";
                bool guardado = await InsertarOActualizarTabla(TablaEsavdoc(tablaNombre), register);

                return guardado;
            }
            catch (StorageException ex)
            {
                LogError($"Error de almacenamiento: {ex.Message}", ex);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error al guardar rechazo: {ex.Message}", ex);
                return false;
            }
        }
        private DynamicTableEntity CrearEntidadRechazo(beRechazo oRechazo)
        {
            if (oRechazo == null)
            {
                throw new ArgumentNullException(nameof(oRechazo), "El objeto de rechazo no puede ser nulo.");
            }

            var entidad = new DynamicTableEntity
            {
                PartitionKey = DateTime.Now.ToString("yyyy-MM-dd"),
                //RowKey = $"{oRechazo.CodigoRechazo}-{oRechazo.RUC}-{oRechazo.Sede}-{oRechazo.Serie}-{oRechazo.Numero}",
                RowKey = $"{oRechazo.RUC}-{oRechazo.Sede}-{oRechazo.Serie}-{oRechazo.Numero}-{oRechazo.CodigoRechazo}"
            };

            entidad.Properties.Add("RUC", new EntityProperty(oRechazo.RUC ?? string.Empty));
            entidad.Properties.Add("Sede", new EntityProperty(oRechazo.Sede ?? string.Empty));
            entidad.Properties.Add("Serie", new EntityProperty(oRechazo.Serie ?? string.Empty));
            entidad.Properties.Add("Numero", new EntityProperty(oRechazo.Numero ?? string.Empty));
            entidad.Properties.Add("Documento", new EntityProperty($"{oRechazo.RUC}-{oRechazo.Sede}-{oRechazo.Serie}-{oRechazo.Numero}"));
            entidad.Properties.Add("Descripcion", new EntityProperty(oRechazo.Descripcion ?? string.Empty));
            entidad.Properties.Add("TipoDoc", new EntityProperty(oRechazo.TipoDoc ?? string.Empty));

            entidad.Properties.Add("FechaEmision", new EntityProperty(oRechazo.FechaEmision != DateTime.MinValue ? (DateTimeOffset)oRechazo.FechaEmision : DateTimeOffset.Now));
            entidad.Properties.Add("FechaTransferencia", new EntityProperty(oRechazo.FechaTransferencia != DateTime.MinValue ? (DateTimeOffset)oRechazo.FechaTransferencia : DateTimeOffset.Now));

            entidad.Properties.Add("Txt", new EntityProperty(oRechazo.Txt ?? string.Empty));
            entidad.Properties.Add("TipoMoneda", new EntityProperty(oRechazo.TipoMoneda ?? string.Empty));
            entidad.Properties.Add("codigo", new EntityProperty(oRechazo.CodigoRechazo ?? string.Empty));

            return entidad;
        }
        private void LogError(string mensaje, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}");
            Console.WriteLine($"Detalles: {ex}");
            Console.ResetColor();
        }
        public static CloudTable TablaEsavdoc(string tabla)
        {
            var account = CloudStorageAccount.Parse(ConnStorage);
            var client = account.CreateCloudTableClient();
            client.GetTableReference(tabla).CreateIfNotExists();
            return client.GetTableReference(tabla);
        }
        public async Task<bool> InsertarOActualizarTabla(CloudTable table, DynamicTableEntity dato)
        {
            bool data = false;
            try
            {
                _ = await table.CreateIfNotExistsAsync();
                Type _type = dato.GetType();
                var entity = new DynamicTableEntity();
                TableOperation insertOperation = TableOperation.InsertOrMerge(dato);
                table.ExecuteAsync(insertOperation).Wait();
                data = true;
            }
            catch (Exception ex)
            {
                _ = LogAsync("InsertarOActualizarTabla", ex);
            }
            return data;
        }
        public void EliminarRegistroTabla(CloudTable table, string partitionKey, string rowKey, string etag)
        {
            try
            {
                table.Execute(TableOperation.Delete(new TableEntity(partitionKey, rowKey) { ETag = etag }));
            }
            catch (Exception ex)
            {
                _ = LogAsync("EliminarRegistroTabla", ex);
            }
        }
        public async Task EliminarRegistrosPorColumnasAndRechazo(string tabla, string colTxt, string txt, string colSerie, string serie, string colNumero, string numero, List<beRechazo> codigosRechazo)
        {
            if (string.IsNullOrEmpty(tabla) || string.IsNullOrEmpty(colTxt) || string.IsNullOrEmpty(txt) || codigosRechazo == null)
            {
                throw new ArgumentException("Parámetros de entrada no válidos.");
            }

            try
            {
                var table = TablaEsavdoc(tabla);

                string filter1 = TableQuery.GenerateFilterCondition(colTxt, QueryComparisons.Equal, txt);
                string filter2 = TableQuery.GenerateFilterCondition(colSerie, QueryComparisons.Equal, serie);
                string filter3 = TableQuery.GenerateFilterCondition(colNumero, QueryComparisons.Equal, numero);

                string combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.Or, filter2);
                combinedFilter = TableQuery.CombineFilters(combinedFilter, TableOperators.And, filter3);

                TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(combinedFilter);
                var entities = table.ExecuteQuery(query);

                var codigosRechazoSet = new HashSet<string>(codigosRechazo.Select(cr => cr.CodigoRechazo));

                if (entities.Any())
                {
                    foreach (var entity in entities)
                    {
                        if (entity.Properties.TryGetValue("codigo", out var codigoProperty) && codigoProperty.StringValue != null)
                        {
                            string codigoDeRechazo = codigoProperty.StringValue;

                            if (!codigosRechazoSet.Contains(codigoDeRechazo) && codigoDeRechazo != "000")
                            {
                                var deleteOperation = TableOperation.Delete(entity);
                                await table.ExecuteAsync(deleteOperation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogAsync("EliminarRegistrosPorColumnas", ex);
            }
        }
        public async Task EliminarRegistrosPorRowKeyPrefix(
                string tabla, string rowkey, List<beRechazo> codigosRechazo, int diasRango = 6)
        {
            if (string.IsNullOrEmpty(tabla) || string.IsNullOrEmpty(rowkey))
            {
                throw new ArgumentException("Parámetros de entrada no válidos.");
            }

            try
            {
                var table = TablaEsavdoc(tabla);
                string rowKeyPrefix = $"{rowkey}-";

                var codigosRechazoSet = new HashSet<string>(codigosRechazo.Select(cr => cr.CodigoRechazo));

                for (int i = 0; i < diasRango; i++)
                {
                    string partitionKey = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");

                    // Filtro PartitionKey + RowKey prefix
                    string filterPartition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                    string filterRowStart = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyPrefix);
                    string filterRowEnd = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rowKeyPrefix + "~");

                    string combinedRowFilter = TableQuery.CombineFilters(filterRowStart, TableOperators.And, filterRowEnd);
                    string finalFilter = TableQuery.CombineFilters(filterPartition, TableOperators.And, combinedRowFilter);

                    var query = new TableQuery<DynamicTableEntity>().Where(finalFilter);

                    TableContinuationToken token = null;

                    do
                    {
                        var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                        foreach (var entity in segment.Results)
                        {
                            if (entity.Properties.TryGetValue("codigo", out var codigoProperty) &&
                                codigoProperty.StringValue != null)
                            {
                                string codigoDeRechazo = codigoProperty.StringValue;

                                if (!codigosRechazoSet.Contains(codigoDeRechazo) && codigoDeRechazo != "000")
                                {
                                    var deleteOperation = TableOperation.Delete(entity);
                                    await table.ExecuteAsync(deleteOperation);
                                }
                            }
                        }
                        token = segment.ContinuationToken;
                    } while (token != null);
                }
            }
            catch (Exception ex)
            {
                await LogAsync("EliminarRegistrosPorRowKeyPrefix", ex);
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public async Task EliminarRegistrosPorColumnasAndRechazo_(
    string tabla,
    string colTxt,
    string txt,
    string colSerie,
    string serie,
    string colNumero,
    string numero,
    List<beRechazo> codigosRechazo)
        {
            if (string.IsNullOrEmpty(tabla) || string.IsNullOrEmpty(colTxt) || string.IsNullOrEmpty(txt) || codigosRechazo == null)
                throw new ArgumentException("Parámetros de entrada no válidos.");

            try
            {
                var table = TablaEsavdoc(tabla);

                // Filtros separados por claridad
                string filter1 = TableQuery.GenerateFilterCondition(colTxt, QueryComparisons.Equal, txt);
                string filter2 = TableQuery.GenerateFilterCondition(colSerie, QueryComparisons.Equal, serie);
                string filter3 = TableQuery.GenerateFilterCondition(colNumero, QueryComparisons.Equal, numero);

                string combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And,
                                        TableQuery.CombineFilters(filter2, TableOperators.And, filter3));

                var query = new TableQuery<DynamicTableEntity>().Where(combinedFilter);

                // Obtener todas las entidades
                TableContinuationToken token = null;
                var allEntities = new List<DynamicTableEntity>();

                do
                {
                    var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                    token = segment.ContinuationToken;
                    allEntities.AddRange(segment.Results);
                } while (token != null);

                var codigosRechazoSet = new HashSet<string>(codigosRechazo.Select(cr => cr.CodigoRechazo));

                // Filtrar entidades que no deben eliminarse
                var entidadesParaEliminar = allEntities
                    .Where(entity =>
                        entity.Properties.TryGetValue("codigo", out var codigoProp) &&
                        codigoProp.StringValue != null &&
                        !codigosRechazoSet.Contains(codigoProp.StringValue) &&
                        codigoProp.StringValue != "000")
                    .ToList();

                if (!entidadesParaEliminar.Any())
                    return;

                // Agrupar por PartitionKey y eliminar en lotes de hasta 100
                var gruposPorPartition = entidadesParaEliminar.GroupBy(e => e.PartitionKey);

                foreach (var grupo in gruposPorPartition)
                {
                    var batchList = grupo
                        .Select((entidad, index) => new { entidad, index })
                        .GroupBy(x => x.index / 100)
                        .Select(g => g.Select(x => x.entidad).ToList());

                    foreach (var batchEntities in batchList)
                    {
                        var batchOperation = new TableBatchOperation();
                        foreach (var entidad in batchEntities)
                        {
                            batchOperation.Delete(entidad);
                        }

                        await table.ExecuteBatchAsync(batchOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                await LogAsync("EliminarRegistrosPorColumnas", ex);
                throw;
            }
        }
        public void EliminarRegistrosPorColumnas(string tabla, string column1, string value1, string column2 = null, string value2 = null)
        {
            try
            {
                var table = TablaEsavdoc(tabla);

                string filter1 = TableQuery.GenerateFilterCondition(column1, QueryComparisons.Equal, value1);
                string combinedFilter = filter1;

                if (!string.IsNullOrEmpty(column2) && !string.IsNullOrEmpty(value2))
                {
                    string filter2 = TableQuery.GenerateFilterCondition(column2, QueryComparisons.Equal, value2);
                    combinedFilter = TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
                }

                TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(combinedFilter);
                var entities = table.ExecuteQuery(query);

                foreach (var entity in entities)
                {
                    var deleteOperation = TableOperation.Delete(entity);
                    table.Execute(deleteOperation);
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("EliminarRegistrosPorDosColumnas", ex);
            }
        }
        public void EliminarRegistrosNoEnRechazosAsync(string tabla, List<beRechazo> rechazos)
        {
            try
            {
                var table = TablaEsavdoc(tabla);
                var allEntities = ObtenerRegistros(table);

                // Agrupar por número y serie en la lista de rechazos
                var rechazosPorGrupo = rechazos
                    .GroupBy(r => new { r.Serie, r.Numero })
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => r.CodigoRechazo).ToList()
                    );

                var registrosAEliminar = allEntities
                    .Where(entity =>
                    {
                        var codigo = entity.Properties["codigo"].StringValue;
                        var serie = entity.Properties["Serie"].StringValue;
                        var numero = entity.Properties["Numero"].StringValue;

                        if (rechazosPorGrupo.TryGetValue(new { Serie = serie, Numero = numero }, out var codigosValidos))
                        {
                            return !codigosValidos.Contains(codigo);
                        }

                        return false;
                    })
                    .ToList();

                foreach (var entity in registrosAEliminar)
                {
                    try
                    {
                        var deleteOperation = TableOperation.Delete(entity);
                        table.ExecuteAsync(deleteOperation);
                    }
                    catch (Exception deleteEx)
                    {
                        _ = LogAsync("Error al eliminar entidad", deleteEx);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("EliminarRegistrosNoEnRechazos", ex);
            }
        }
        public static List<DynamicTableEntity> ObtenerRegistros(CloudTable table)
        {
            var allEntities = new List<DynamicTableEntity>();
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>();
            TableContinuationToken token = null;

            do
            {
                var segment = table.ExecuteQuerySegmented(projectionQuery, token);
                token = segment.ContinuationToken;
                allEntities.AddRange(segment.Results);
            } while (token != null); // Continuar si hay más entidades

            return allEntities;
        }
        public async Task GuardarAnulacionEnStorage(
                string correlativoTxt, string tipoDoc, string[] lineas, string[] cabecera, string[] nametxt,DateTime fechaRecepcion)
        {
            if (nametxt == null || nametxt.Length == 0)
                throw new ArgumentException("El parámetro 'nametxt' no puede estar vacío.");

            string nombreArchivoTxt = nametxt.Last();

            var entidad = new DynamicTableEntity
            {
                PartitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                RowKey = nombreArchivoTxt
            };

            entidad.Properties.Add("TipoDoc", new EntityProperty(tipoDoc));
            entidad.Properties.Add("Lineas", new EntityProperty(string.Join("~", lineas)));
            entidad.Properties.Add("Cabecera", new EntityProperty(string.Join("|", cabecera)));
            entidad.Properties.Add("NameTxt", new EntityProperty(string.Join("\\", nametxt)));
            entidad.Properties.Add("FechaRegistro", new EntityProperty(DateTime.UtcNow.ToString("o")));

            var tabla = TablaEsavdoc("ANULACIONES");
            if (tabla != null)
            {
                await InsertarOActualizarTabla(tabla, entidad);
            }
            else
            {
                throw new Exception("No se pudo obtener la tabla ANULACIONES");
            }
        }

        public async Task<List<AnulacionEntity>> ObtenerAnulacionesPendientes()
        {
            var tabla = TablaEsavdoc("ANULACIONES");
            if (tabla == null)
                throw new Exception("No se pudo obtener la tabla ANULACIONES.");

            TableQuery<DynamicTableEntity> consulta = new TableQuery<DynamicTableEntity>();
            var resultados = await tabla.ExecuteQuerySegmentedAsync(consulta, null);

            List<AnulacionEntity> anulacionesPendientes = new List<AnulacionEntity>();

            foreach (var entidad in resultados.Results)
            {
                if (entidad.Properties.TryGetValue("FechaRegistro", out EntityProperty fechaProp) &&
                    DateTime.TryParse(fechaProp.StringValue, out DateTime fechaRegistro))
                {
                    if ((DateTime.UtcNow - fechaRegistro).TotalMinutes >= 45)
                    {
                        string correlativo = entidad.RowKey; // Usamos RowKey como correlativo

                        anulacionesPendientes.Add(new AnulacionEntity
                        {
                            Correlativo = correlativo,
                            FechaRecepcion = fechaRegistro,
                            EntidadOriginal = entidad
                        });
                    }
                }
            }

            return anulacionesPendientes;
        }
        public async Task EliminarAnulacionProcesada(DynamicTableEntity entidad)
        {
            var tabla = TablaEsavdoc("ANULACIONES");
            if (tabla != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entidad);
                await tabla.ExecuteAsync(deleteOperation);
                LogMensaje($"Anulación eliminada de la tabla: {entidad.RowKey}", ConsoleColor.Green);
            }
        }

    }
}
