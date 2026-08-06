using Microsoft.Azure.Cosmos.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brGenerico
    {
        private static string _CadenaEsavDocConfig = ConfigurationManager.ConnectionStrings["dbEsavDocApi"].ConnectionString;
        protected internal SqlConnection connApi = new SqlConnection(_CadenaEsavDocConfig);
        private static string _CadenaEsavDocConfig_ = ConfigurationManager.ConnectionStrings["dbEsavDoc"].ConnectionString;
        protected internal SqlConnection conn = new SqlConnection(_CadenaEsavDocConfig_);

        public static string ruta = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["RutaLog"];
        public static string rutaSinJson = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["RutaSinJson"];
        protected static string ConnStorage = ConfigurationManager.AppSettings["ConnStorage"];
        protected static string ContenedorStorage = ConfigurationManager.AppSettings["ContenedorStorage"];
        protected static string RutaStorage = ConfigurationManager.AppSettings["BaseStorage"];
        protected static string RutaStorageJson = ConfigurationManager.AppSettings["BaseStorageJson"];

        protected static async Task LogAsync(string titulo, Exception ex, string getType = "")
        {
            using (StreamWriter log = new StreamWriter(ruta, true))
            {
                await log.WriteLineAsync($"-------------{titulo} ---------------");
                await log.WriteLineAsync(DateTime.Now.ToString());
                await log.WriteLineAsync(getType + "-" + ex.TargetSite.Name);
                await log.WriteLineAsync(ex.Message);
                await log.WriteLineAsync(ex.StackTrace);
                await log.WriteLineAsync("---------------------------------------");
            }
        }
        protected static async Task GuardarLogTxt(string titulo, string getType = "")
        {
            using (StreamWriter log = new StreamWriter(rutaSinJson, true))
            {
                await log.WriteLineAsync($"-------------{titulo} ---------------");
                await log.WriteLineAsync(DateTime.Now.ToString());
                await log.WriteLineAsync($"TipoDoc: {getType}");
                await log.WriteLineAsync("---------------------------------------");
            }
        }
        private CloudBlobContainer ObtenerContenedorEsavDoc()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(ConnStorage);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(ContenedorStorage);
            var returnData = container.CreateIfNotExistsAsync();
            while (returnData.Status != TaskStatus.RanToCompletion)
            {
                Thread.Sleep(500);
            }
            return container;
        }
        public bool DescargaArchivo(Stream outPutStream, string carpeta, string archivo)
        {
            var container = ObtenerContenedorEsavDoc();
            container.FetchAttributes();

            var folder = container.GetDirectoryReference($"");
            var blockBlob = folder.GetBlockBlobReference(archivo);
            try
            {
                if (!blockBlob.Exists())
                {
                    return false;
                }

                blockBlob.FetchAttributes();
                int bufferLength = 1 * 1024 * 1024;
                long blobRemainingLength = blockBlob.Properties.Length;
                Queue<KeyValuePair<long, long>> queues = new Queue<KeyValuePair<long, long>>();
                long offset = 0;

                while (blobRemainingLength > 0)
                {
                    long chunkLength = (long)Math.Min(bufferLength, blobRemainingLength);
                    queues.Enqueue(new KeyValuePair<long, long>(offset, chunkLength));
                    offset += chunkLength;
                    blobRemainingLength -= chunkLength;
                }
                Parallel.ForEach(queues,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 10
                    }, (queue) =>
                    {
                        using (var ms = new MemoryStream())
                        {
                            blockBlob.DownloadRangeToStream(ms, queue.Key, queue.Value);
                            lock (outPutStream)
                            {
                                outPutStream.Position = queue.Key;
                                var bytes = ms.ToArray();
                                outPutStream.Write(bytes, 0, bytes.Length);
                            }
                        }
                    });

                return true;
            }
            catch (Exception ex)
            {
                _ = LogAsync($"DescargaBlob - [{RutaStorage}/{carpeta}/{archivo}]", ex);
                return false;
            }
        }
        public void LogMensaje(string mensaje, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} => {mensaje}");
            Console.ResetColor();
        }
        public async Task<Uri> SubirJson(string carpeta, string nombre, Stream data)
        {
            try
            {
                var container = ObtenerContenedorEsavDoc();
                var folder = container.GetDirectoryReference($"{RutaStorageJson}/{carpeta}");
                var blockBlob = folder.GetBlockBlobReference(nombre);

                var upload = blockBlob.UploadFromStreamAsync(data);
                while (upload.Status != TaskStatus.RanToCompletion)
                {
                    Thread.Sleep(500);
                }

                return blockBlob.Uri;
            }
            catch (Exception ex)
            {
                await LogAsync($"SubirBlob [{RutaStorageJson}/{carpeta}/{nombre}]", ex);
                return new Uri("");
            }
        }
        public async Task EnviarCola(string ColaMensaje, string tipodoc)
        {
            try
            {
                Azure.Storage.Queues.QueueClient queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"api-bus-{tipodoc}");
                await queue.CreateIfNotExistsAsync();
                queue.SendMessage(Convert.ToBase64String(Encoding.UTF8.GetBytes(ColaMensaje)));
            }
            catch (Exception ex)
            {
                _ = LogAsync($"EnviarBus_cola", ex);
            }
        }
        public CloudTable TablaEsavDoc(string tabla)
        {
            var account = Microsoft.Azure.Cosmos.Table.CloudStorageAccount.Parse(ConnStorage);
            var client = account.CreateCloudTableClient();
            return client.GetTableReference(tabla);
        }
        public List<DynamicTableEntity> ListarTabla(CloudTable table, string columna, string key, string columna2 = null, string key2 = null)
        {
            table.CreateIfNotExists();
            List<DynamicTableEntity> resp = new List<DynamicTableEntity>();
            try
            {
                var condition = TableQuery.GenerateFilterCondition(columna, Microsoft.WindowsAzure.Storage.Table.QueryComparisons.Equal, key);

                var query = new TableQuery<DynamicTableEntity>().Where(condition);
                if (columna2 != null)
                {
                    var condition2 = TableQuery.GenerateFilterCondition(columna2, Microsoft.WindowsAzure.Storage.Table.QueryComparisons.Equal, key2);
                    query = new TableQuery<DynamicTableEntity>().Where(
                        TableQuery.CombineFilters(
                            condition,
                            TableOperators.And,
                            condition2
                            ));
                }
                else
                {

                }

                var token = new TableContinuationToken();

                do
                {
                    var response = table.ExecuteQuerySegmented(query, token);
                    token = response.ContinuationToken;
                    resp.AddRange(response.Results);
                } while (token != null);
                //var lst = table.ExecuteQuerySegmentedAsync(query, token).Result;
                return resp;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public bool InsertarOActualizarTabla(CloudTable table, DynamicTableEntity dato)
        {
            bool data = false;
            try
            {
                table.CreateIfNotExists();
                Type _type = dato.GetType();
                var entity = new DynamicTableEntity();
                TableOperation insertOperation = TableOperation.InsertOrMerge(dato);
                table.Execute(insertOperation);
                data = true;
            }
            catch (Exception ex)
            {
                _ = LogAsync("InsertarOActualizarTabla", ex);
            }
            return data;
        }        
    }
}
