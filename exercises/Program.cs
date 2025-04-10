using nanoFramework.Hardware.Esp32;
using nanoFramework.Runtime.Native;
using nanoFramework.System.IO.FileSystem;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace Exercises
{
    public class Program
    {
        public static void Main()
        {
            var currentDate = DateTime.UtcNow;
            Debug.WriteLine($"You have successfully deployed your first NanoFramework application at {currentDate.ToString()}!");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
