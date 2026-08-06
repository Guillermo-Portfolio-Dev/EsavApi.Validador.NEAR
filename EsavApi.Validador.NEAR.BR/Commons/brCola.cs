using Azure;
using Azure.Storage.Queues.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsavApi.Validador.NEAR.BR.Commons
{
    public class brCola : brGenerico
    {
        public QueueMessage[] RecepcionCola(string ColaNombre)
        {
            QueueMessage[] Cola = null;
            try
            {
                Azure.Storage.Queues.QueueClient queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"{ColaNombre.ToLower()}");
                if (queue.Exists())
                {
                    Response<QueueMessage[]> response = queue.ReceiveMessages(maxMessages: 20);
                    Cola = response.Value;

                    foreach (QueueMessage message in Cola)
                    {
                        queue.DeleteMessage(message.MessageId, message.PopReceipt);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = LogAsync("RecepcionCola", ex);
            }
            return Cola;
        }
        public async Task<QueueMessage[]> RecepcionColaAsync(string colaNombre, int maxMessages = 20)
        {
            QueueMessage[] cola = Array.Empty<QueueMessage>();

            try
            {
                var queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"{colaNombre.ToLower()}");

                if (await queue.ExistsAsync().ConfigureAwait(false))
                {
                    await Task.Delay(2000).ConfigureAwait(false);

                    var response = await queue.ReceiveMessagesAsync(maxMessages).ConfigureAwait(false);
                    cola = response.Value ?? Array.Empty<QueueMessage>();

                    LogMensaje($"Mensajes recibidos de la cola '{colaNombre}': {cola.Length}", ConsoleColor.Magenta);

                    if (cola.Any())
                    {
                        Parallel.ForEach(cola, message =>
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    await LogAsync($"Error al eliminar mensaje con ID {message.MessageId} de la cola {colaNombre}", ex).ConfigureAwait(false);
                                }
                            }).Wait();
                        });
                    }
                    else
                    {
                        LogMensaje($"No se encontraron mensajes en la cola '{colaNombre}'", ConsoleColor.Gray);
                    }
                }
                else
                {
                    LogMensaje($"La cola '{colaNombre}' no existe", ConsoleColor.Gray);
                }
            }
            catch (Exception ex)
            {
                await LogAsync("RecepcionCola", ex).ConfigureAwait(false);
            }

            return cola;
        }
        public async Task<QueueMessage[]> RecepcionColaAsync2(string colaNombre, int maxMessages = 8)
        {
            QueueMessage[] cola = Array.Empty<QueueMessage>();

            try
            {
                var queue = new Azure.Storage.Queues.QueueClient(ConnStorage, colaNombre.ToLower());

                if (!await queue.ExistsAsync().ConfigureAwait(false))
                {
                    LogMensaje($"La cola '{colaNombre}' no existe", ConsoleColor.Gray);
                    return cola;
                }

                await Task.Delay(1000).ConfigureAwait(false);

                var response = await queue.ReceiveMessagesAsync(maxMessages).ConfigureAwait(false);
                cola = response.Value ?? Array.Empty<QueueMessage>();

                LogMensaje($"Mensajes recibidos de la cola '{colaNombre}': {cola.Length}", ConsoleColor.Magenta);

                if (!cola.Any())
                {
                    LogMensaje($"No se encontraron mensajes en la cola '{colaNombre}'", ConsoleColor.Gray);
                }
            }
            catch (Exception ex)
            {
                await LogAsync("RecepcionCola", ex).ConfigureAwait(false);
            }

            return cola;
        }
        public async Task EnviarRegistroBD(string ColaMensaje, string tipodoc)
        {
            try
            {
                Azure.Storage.Queues.QueueClient queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"registro-db-{tipodoc}");
                await queue.CreateIfNotExistsAsync();
                queue.SendMessage(Convert.ToBase64String(Encoding.UTF8.GetBytes(ColaMensaje)));
            }
            catch (Exception ex)
            {
                _ = LogAsync($"Enviar_cola_DB", ex);
            }
        }
        public async Task EliminarMensajeColaAsync(string colaNombre, string messageId, string popReceipt)
        {
            try
            {
                var queue = new Azure.Storage.Queues.QueueClient(ConnStorage, $"{colaNombre.ToLower()}");

                if (await queue.ExistsAsync().ConfigureAwait(false))
                {
                    await queue.DeleteMessageAsync(messageId, popReceipt).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                //await LogAsync($"Error al eliminar mensaje {messageId} de la cola {colaNombre}", ex).ConfigureAwait(false);
            }
        }

    }
}
