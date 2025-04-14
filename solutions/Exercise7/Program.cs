using Amqp;
using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using System.Text;
using nanoFramework.Json;

namespace Exercise7
{
    public class Program
    {
        static Screen screen;
        static string ipAddress = "0.0.0.0";
        static TrackedGpioPin redLed;
        static TrackedGpioPin blueLed;
        static LedHandlers ledHandler;

        public static void Main()
        {
            ConfigurePins();
            InitializeScreen();
            ConnectToWiFi();
            StartServiceBusReceiver();

            var currentDate = DateTime.UtcNow;
            Debug.WriteLine($"You have successfully deployed your first NanoFramework application at {currentDate.ToString()}!");

            using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(FileController), typeof(LedController) }))
            {
                server.Start();

                screen.Write($"Started\n{currentDate.ToString()}\n{ipAddress}");

                Thread.Sleep(Timeout.Infinite);
            }
        }

        private static void StartServiceBusReceiver()
        {
            var connection = new Connection(new Address("amqp connection string"));
            var session = new Session(connection);
            var receiverLink = new ReceiverLink(session, "Esp32 receiver Link", "thedrivein");
            var sender = new SenderLink(session, "ESP32 sender link", "thedriveinconsole");

            receiverLink.Start(5, (receiver, message) =>
            {
                // Extraxt actual message from message body
                var bytes = (byte[])message.Body;
                var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

                var messageType = message.ApplicationProperties["MessageType"] as string;
                Console.WriteLine($"Message Type: {messageType}");

                // Toggle the LEDs here
                if (messageType == "ToggleLEDs")
                {
                    var toggle = (ToggleLEDs)JsonConvert.DeserializeObject(json, typeof(ToggleLEDs));
                    ledHandler.HandleToggleLEDs(toggle, sender);
                }
                else if (messageType == "TurnLEDsOn")
                {
                    var turnLEDsOn = (TurnLEDsOn)JsonConvert.DeserializeObject(json, typeof(TurnLEDsOn));
                    ledHandler.HandleTurnLEDsOn(turnLEDsOn, sender);
                }
                else
                {
                    // We don't know the message type. What should we do with this message?
                    Console.WriteLine("Unknown message type.");
                }

                // Accept the message, telling Azure it can be removed from the queue
                receiver.Accept(message);
            });
        }

        private static void ToggleRemoteLeds()
        {
            // replace IP address with other device's IP
            using var request = WebRequest.Create("http://192.168.1.113/api/toggleleds");
            request.Method = "GET";

            using var response = request.GetResponse();
        }

        private static void ConnectToWiFi()
        {
            var connected = WifiNetworkHelper.ConnectDhcp("introtomessaging", "IsY2TPxx0TI9", requiresDateTime: true);
            if (connected)
            {
                ipAddress = IPGlobalProperties.GetIPAddress().ToString();
                Console.WriteLine($"Connected to WiFi. IP Address {ipAddress}");
            }
            else
            {
                screen.WriteLarge("FAIL");
            }
        }

        private static void InitializeScreen()
        {
            Ssd1306 oledscreen = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64, DisplayOrientation.Landscape180);
            screen = new Screen(oledscreen);
        }

        private static void ConfigurePins()
        {
            Configuration.SetPinFunction(Gpio.IO15, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

            var gpioController = new GpioController();

            var buttonOnePushed = false;
            var buttonTwoPushed = false;

            var button = gpioController.OpenPin(22, PinMode.InputPullDown);
            button.DebounceTimeout = TimeSpan.FromMilliseconds(100);
            button.ValueChanged += (s, e) => {
                buttonOnePushed = e.ChangeType == PinEventTypes.Rising;

                if (buttonOnePushed && buttonTwoPushed)
                {
                    ToggleLEDs();
                }
            };

            var secondButton = gpioController.OpenPin(25, PinMode.InputPullDown);
            secondButton.DebounceTimeout = TimeSpan.FromMilliseconds(100);
            secondButton.ValueChanged += (s, e) =>
            {
                buttonTwoPushed = e.ChangeType == PinEventTypes.Rising;

                if (buttonOnePushed && buttonTwoPushed)
                {
                    ToggleLEDs();
                }
            };

            redLed = new TrackedGpioPin(gpioController.OpenPin(21, PinMode.Output));
            blueLed = new TrackedGpioPin(gpioController.OpenPin(2, PinMode.Output));

            blueLed.Write(PinValue.Low);
            redLed.Write(PinValue.Low);

            ledHandler = new LedHandlers(redLed, blueLed);
        }

        public static void ToggleLEDs()
        {
            redLed.Toggle();
            blueLed.Toggle();
        }
    }
}