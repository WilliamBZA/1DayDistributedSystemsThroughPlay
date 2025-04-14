using Azure.Messaging.ServiceBus;

namespace Exercise8ConsoleReceiver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new ServiceBusClient("connectionstring");

            var sender = client.CreateSender("thedrivein");
            await sender.SendMessageAsync(new ServiceBusMessage(""));

            var processor = client.CreateProcessor("thedrivein", new ServiceBusProcessorOptions
            {
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10),
                MaxConcurrentCalls = 10
            });

            processor.ProcessMessageAsync += async (args) =>
            {
                Console.WriteLine("Starting long processing...");
                await Task.Delay(TimeSpan.FromSeconds(600));
                Console.WriteLine("Finished!");

                await args.CompleteMessageAsync(args.Message);
            };

            processor.ProcessErrorAsync += (args) => { return Task.CompletedTask; };

            await processor.StartProcessingAsync();

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}