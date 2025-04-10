using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace Exercise2
{
    public class Program
    {
        private static Screen screen;
        private static string ipAddress = "0.0.0.0";
        private static GpioPin redLed;
        private static GpioPin blueLed;

        public static void Main()
        {
            ConfigurePins();
            InitializeScreen();
            ConnectToWiFi();

            var currentDate = DateTime.UtcNow;
            Debug.WriteLine($"You have successfully deployed your first NanoFramework application at {currentDate.ToString()}!");

            screen.Write($"Started\n{currentDate.ToString()}\n{ipAddress}");

            Thread.Sleep(Timeout.Infinite);
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

            // Todo: Create a GPIO Controller and initialize the Red LED on PIN 21 and the Blue LED on PIN 4
        }

        private static void ToggleLEDs()
        {
            // Todo: Toggle the LEDs. If the Red LED is on, turn it off. If the Red LED is off, turn it on.
            // Same with the Blue LED.
        }
    }
}