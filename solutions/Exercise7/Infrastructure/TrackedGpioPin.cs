using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace Exercise7
{
    public class TrackedGpioPin(GpioPin pin, bool isHigh = false)
    {
        public void Toggle()
        {
            pin.Write(isHigh ? PinValue.Low : PinValue.High);
            isHigh = !isHigh;
        }

        public void Write(PinValue value)
        {
            isHigh = value == PinValue.High;
            pin.Write(value);
        }
    }
}