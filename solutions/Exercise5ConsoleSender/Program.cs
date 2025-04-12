using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Transactions;

namespace Exercise4ConsoleSender
{
    public class Program
    {
        public static async Task Main()
        {
            var client = new ServiceBusClient("connectionstring");

            /* 
            // Send a message to 2 separate queues using a transaction
            // Create sender for the first queue, i.e. the device queue
            var senderToNanoFramework = client.CreateSender("thedrivein");

            // Create sender for second queue, i.e. the second console application you just created
            var senderToSecondReceiverConsoleApp = client.CreateSender("thedriveinvirtualdevice");

            // Create a message
            var toggleMessage = new ToggleLEDs
            {
                ToggleSource = "Console Application"
            };
            string jsonMessage = JsonSerializer.Serialize(toggleMessage);

            var message = new ServiceBusMessage(jsonMessage)
            {
                ContentType = "application/json"
            };

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await senderToNanoFramework.SendMessageAsync(message);
                await senderToSecondReceiverConsoleApp.SendMessageAsync(new ServiceBusMessage(message.Body));

                transaction.Complete();
            }
            */

            // Send a message to a topic
            // Create sender for the topic
            // Use your team's topic as the name
            var topicSender = client.CreateSender("TheDriveInLedToggleTopic");

            // Create a message
            var toggleMessage = new ToggleLEDs
            {
                ToggleSource = "Console Application"
            };
            string jsonMessage = JsonSerializer.Serialize(toggleMessage);

            var message = new ServiceBusMessage(jsonMessage)
            {
                ContentType = "application/json"
            };

            Console.WriteLine("Sending message to topic...");

            // Send message to the topic
            await topicSender.SendMessageAsync(new ServiceBusMessage(message.Body));

            Console.WriteLine("Message sent to the topic!");
        }
    }
}