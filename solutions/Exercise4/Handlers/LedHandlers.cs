using Amqp;
using Amqp.Framing;
using nanoFramework.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise4
{
    public class LedHandlers(TrackedGpioPin redLed, TrackedGpioPin blueLed)
    {
        public void HandleToggleLEDs(ToggleLEDs message, SenderLink sender)
        {
            Console.WriteLine($"Toggled by {message.ToggleSource}");

            redLed.Toggle();
            blueLed.Toggle();

            var ledsToggledMessage = new LEDsToggled();
            var jsonBody = JsonConvert.SerializeObject(ledsToggledMessage);
            var outgoingMessage = new Message(jsonBody);
            outgoingMessage.ApplicationProperties = new ApplicationProperties();
            outgoingMessage.ApplicationProperties["MessageType"] = "LEDsToggled";

            sender.Send(outgoingMessage, null, null);
        }
    }
}