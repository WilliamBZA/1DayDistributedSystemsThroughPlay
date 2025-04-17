using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace Exercise0
{
    public class Program
    {
        private static Screen screen;

        public static void Main()
        {
            Configuration.SetPinFunction(Gpio.IO15, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

            Ssd1306 oledscreen = new Ssd1306(I2cDevice.Create(new I2cConnectionSettings(1, Ssd1306.DefaultI2cAddress)), Ssd13xx.DisplayResolution.OLED128x64, DisplayOrientation.Landscape180);
            screen = new Screen(oledscreen);
            screen.Write("Deploy something\nto me");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}