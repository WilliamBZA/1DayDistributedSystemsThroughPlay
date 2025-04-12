using Iot.Device.Ssd13xx;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise4
{
    public class Screen
    {
        public Screen(Ssd1306 oled)
        {
            _oled = oled;
            _oled.ClearScreen();
            _oled.Font = new BasicFont();
        }

        public void Write(string message)
        {
            var lines = message.Split('\n');
            var startY = 2;
            foreach (var line in lines)
            {
                _oled.DrawString(2, startY, line, 1, true);
                startY += 14;
            }

            _oled.Display();
        }

        public void WriteBottom(string message)
        {
            _oled.DrawString(2, 50, message, 1, true);
            _oled.Display();
        }

        public void WriteLarge(string message)
        {
            _oled.DrawString(2, 32, message, 2, true);
            _oled.Display();
        }

        public void ClearScreen()
        {
            _oled.ClearScreen();
        }

        public void Marquee(string message)
        {
            // Todo: Create a marquee scroller
            // Hint: You can create a Thread class here and use the thread to move the text
        }

        Ssd1306 _oled;
    }
}
