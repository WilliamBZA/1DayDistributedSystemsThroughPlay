using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace Exercise3
{
    public class Program
    {
        static Screen screen;
        static string ipAddress = "0.0.0.0";
        static TrackedGpioPin redLed;
        static TrackedGpioPin blueLed;

        public static void Main()
        {
            ConfigurePins();
            InitializeScreen();
            ConnectToWiFi();

            var currentDate = DateTime.UtcNow;
            Debug.WriteLine($"You have successfully deployed your first NanoFramework application at {currentDate.ToString()}!");

            using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(FileController) }))
            {
                server.Start();

                screen.Write($"Started\n{currentDate.ToString()}\n{ipAddress}");

                Thread.Sleep(Timeout.Infinite);
            }
        }

        private static void ConnectToWiFi()
        {
            var connected = WifiNetworkHelper.ConnectDhcp("introtomessaging", "IsY2TPxx0TI9");
            if (connected)
            {
                ipAddress = IPGlobalProperties.GetIPAddress().ToString();
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
        }

        private static void ToggleLEDs()
        {
            redLed.Toggle();
            blueLed.Toggle();
        }
    }
}