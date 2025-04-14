using Azure.Messaging.ServiceBus;
using System.Diagnostics;

namespace Exercise7ConsoleReceiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient("connectionstring");
            
            var stopWatch = new Stopwatch();
            //await Receive100MessagesUsingReceiver(client, stopWatch);
            //await Process100Messages(client, stopWatch);
            await Process100MessagesWithConcurrency(client, stopWatch, 10, 100);

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        private static async Task Receive100MessagesUsingReceiver(ServiceBusClient client, Stopwatch stopWatch)
        {
            var receiver = client.CreateReceiver("thedriveinvirtualdevice");

            // Sequential receiver
            stopWatch.Start();
            for (var i = 0; i < 100; i++)
            {
                var message = await receiver.ReceiveMessageAsync();
                Console.WriteLine($"Received: {message.Body.ToString()}");
            }
            stopWatch.Stop();
            Console.WriteLine($"It took {stopWatch.ElapsedMilliseconds}ms to process 100 messages...");
        }

        private static async Task Process100Messages(ServiceBusClient client, Stopwatch stopWatch)
        {
            var processor = client.CreateProcessor("thedriveinvirtualdevice");

            // Sequential processor
            var numberOfMessagesProcessed = 0;
            processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received: {body}");

                if (++numberOfMessagesProcessed == 100)
                {
                    stopWatch.Stop();
                    Console.WriteLine($"It took {stopWatch.ElapsedMilliseconds}ms to process 100 messages...");
                }

                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
            stopWatch.Start();
        }

        private static async Task Process100MessagesWithConcurrency(ServiceBusClient client, Stopwatch stopWatch, int concurrency, int prefetchCount)
        {
            var processor = client.CreateProcessor("thedriveinvirtualdevice", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = concurrency,
                PrefetchCount = prefetchCount
            });

            // Sequential processor
            var numberOfMessagesProcessed = 0;
            processor.ProcessMessageAsync += async args =>
            {
                var body = args.Message.Body.ToString();
                Console.WriteLine($"Received: {body}");

                if (++numberOfMessagesProcessed == 100)
                {
                    stopWatch.Stop();
                    Console.WriteLine($"It took {stopWatch.ElapsedMilliseconds}ms to process 100 messages...");
                }

                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += args =>
            {
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
            stopWatch.Start();
        }

    }
}