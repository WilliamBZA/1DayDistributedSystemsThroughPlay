using Azure.Messaging.ServiceBus;
using Polly;
using Polly.RateLimit;
using System.Text.Json;
using System.Threading.Tasks;

namespace Exercise9ConsoleReceiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Use Exercise8BatchSender to fill the queue
            var client = new ServiceBusClient("");
            var processor = client.CreateProcessor("thedriveinvirtualdevice", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 100
            });

            var rateLimitPolicy = Policy.RateLimitAsync(20, TimeSpan.FromSeconds(10));

            var retryOnRateLimit = Policy
                .Handle<RateLimitRejectedException>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(20));
            
            var combinedPolicy = Policy.WrapAsync(retryOnRateLimit, rateLimitPolicy);

            processor.ProcessMessageAsync += async (arg) =>
            {
                await combinedPolicy.ExecuteAsync(() =>
                {
                    Console.WriteLine($"Message received: {arg.Message.Body} with delivery count: {arg.Message.DeliveryCount}");
                    return Task.CompletedTask;
                });
            };

            processor.ProcessErrorAsync += (arg) =>
            {
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}
