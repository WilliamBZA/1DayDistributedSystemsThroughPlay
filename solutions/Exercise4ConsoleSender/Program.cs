using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Exercise4ConsoleSender
{
    public class Program
    {
        public static async Task Main()
        {
            var client = new ServiceBusClient("connection string");
            var sender = client.CreateSender("thedrivein");

            var messageBody = JsonSerializer.Serialize(new ToggleLEDs { ToggleSource = "ConsoleSender" });

            var message = new ServiceBusMessage(messageBody);
            message.ApplicationProperties["MessageType"] = "ToggleLEDs";

            await sender.SendMessageAsync(message);
            Console.WriteLine("Message sent!");
        }
    }
}