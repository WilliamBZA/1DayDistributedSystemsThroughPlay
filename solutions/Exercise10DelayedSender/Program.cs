using Azure.Messaging.ServiceBus;
using Polly;
using Polly.RateLimit;
using System.Text.Json;

namespace Exercise10DelayedSender
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient("");
            var sender = client.CreateSender("thedrivein");
            var senderForThisProcessor = client.CreateSender("thedriveinvirtualdevice");

            var processor = client.CreateProcessor("thedriveinvirtualdevice", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 100
            });

            var immediateRetryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync(3);

            var delayMessagePolicy = Policy
                .Handle<Exception>()
                .FallbackAsync(async (context, cancellationToken) =>
                {
                    if (context.TryGetValue("OriginalMessageEventArgs", out var obj) && obj is ProcessMessageEventArgs originalArgs)
                    {
                        var retryMessage = new ServiceBusMessage(originalArgs.Message.Body);
                        retryMessage.ContentType = originalArgs.Message.ContentType;
                        retryMessage.ApplicationProperties["retry-attempt"] = originalArgs.Message.DeliveryCount + 1;

                        // delay by 10 seconds more each time
                        var scheduledTime = DateTimeOffset.UtcNow.AddSeconds((originalArgs.Message.DeliveryCount) * 10);
                        await senderForThisProcessor.ScheduleMessageAsync(retryMessage, scheduledTime);

                        // Acknowledge the original message because we've resent it
                        await originalArgs.CompleteMessageAsync(originalArgs.Message);
                    }
                }, (e, t) =>
                {
                    return Task.CompletedTask;
                } );

            var combinedPolicy = Policy.WrapAsync(delayMessagePolicy, immediateRetryPolicy);

            processor.ProcessMessageAsync += async (arg) =>
            {
                await combinedPolicy.ExecuteAsync(async (context) =>
                {
                    context.TryAdd("OriginalMessageEventArgs", arg);
                    Console.WriteLine($"Message received: {arg.Message.Body} with delivery count: {arg.Message.DeliveryCount}");

                    throw new Exception("This will always fail");

                    await arg.CompleteMessageAsync(arg.Message);
                }, new Context());
            };

            processor.ProcessErrorAsync += (arg) =>
            {
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();

            //await SendSerializedMessage(sender);
            //await SendPlainDelayedMessages(senderForThisProcessor);

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        private static async Task SendSerializedMessage(ServiceBusSender sender)
        {
            var futureTime = DateTimeOffset.UtcNow.AddMinutes(1);
            var messageBody = JsonSerializer.Serialize(new ToggleLEDs { ToggleSource = "Scheduled Message" });
            var message = new ServiceBusMessage(messageBody);
            await sender.ScheduleMessageAsync(message, futureTime);
        }

        private static async Task SendPlainDelayedMessages(ServiceBusSender sender)
        {
            for (int i = 0; i < 5; i++)
            {
                var delay = TimeSpan.FromSeconds(10 * i);
                var scheduledTime = DateTimeOffset.UtcNow.Add(delay);
                var msg = new ServiceBusMessage($"Scheduled message {i}");
                await sender.ScheduleMessageAsync(msg, scheduledTime);
            }
        }
    }
}
