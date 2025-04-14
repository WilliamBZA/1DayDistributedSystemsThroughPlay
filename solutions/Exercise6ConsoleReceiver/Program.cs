using Azure;
using Azure.Messaging.ServiceBus;
using Polly;

namespace Exercise6ConsoleReceiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient("connection string");

            var receiver = client.CreateReceiver("thedriveinvirtualdevice");
            var message = await receiver.ReceiveMessageAsync();

            // Create a retry policy that will retry 3 times immediately
            var retryPolicy = Policy
                .Handle<Exception>() // You can be more specific here, e.g., Handle<HttpRequestException>()
                .Retry(3, (exception, retryCount) =>
                {
                    Console.WriteLine($"Retry {retryCount} due to {exception.Message}");
                });

            // Create a policy that will wait between retries with exponential backoff
            var waitAndRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
                    }
                );

            var dlqPolicy = Policy.Handle<Exception>()
                .Fallback(async () =>
                {
                    await receiver.DeadLetterMessageAsync(message);
                });

            var combinedPolicy = Policy.Wrap(dlqPolicy, waitAndRetryPolicy, retryPolicy);
            combinedPolicy.Execute(() => HandleMessage(message));

            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        static DateTime failUntil = DateTime.UtcNow.AddSeconds(95);
        private static void HandleMessage(ServiceBusReceivedMessage message)
        {
            Console.WriteLine("Attempting to process the message...");

            Console.WriteLine($"Time: {DateTime.UtcNow:HH:mm:ss}");
            if (DateTime.UtcNow < failUntil)
            {
                throw new Exception("Still failing...");
            }

            Console.WriteLine("Message handled successfully!");
        }
    }
}
