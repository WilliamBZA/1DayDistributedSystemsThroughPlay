using Azure.Messaging.ServiceBus;
using System.Diagnostics;

namespace Exercise7BatchSender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient("");
            var sender = client.CreateSender("thedriveinvirtualdevice");

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            /*
            // Non-batched sends
            for (var i = 0; i < 100; i++)
            {
                await sender.SendMessageAsync(new ServiceBusMessage($"Test message {i}"));
            }
            */

            // Batched sends
            var batch = await sender.CreateMessageBatchAsync();
            for (var i = 0; i < 100; i++)
            {
                if (!batch.TryAddMessage(new ServiceBusMessage($"Test message {i}")))
                {
                    await sender.SendMessagesAsync(batch);
                    batch = await sender.CreateMessageBatchAsync();
                }
            }

            await sender.SendMessagesAsync(batch);
            stopwatch.Stop();

            Console.WriteLine($"It took {stopwatch.Elapsed.TotalMilliseconds}ms to send 100 messages");
        }
    }
}