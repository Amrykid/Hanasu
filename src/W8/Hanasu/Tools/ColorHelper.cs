using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Hanasu.Tools
{
    public static class ColorHelper
    {
        public static Color GetColorFromHexString(string hexValue)
        {
            //http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/b639cd8a-30c2-48cf-99be-559f34cbfa79

            //var a = Convert.ToByte((string)hexValue.Substring(0, 2), (int)16);
            //var r = Convert.ToByte(hexValue.Substring(2, 2), 16);
            //var g = Convert.ToByte(hexValue.Substring(4, 2), 16);
            //var b = Convert.ToByte(hexValue.Substring(6, 2), 16);
            //return Color.FromArgb(a, r, g, b);

            byte alpha;
            byte pos = 0;

            string hex = hexValue.ToString().Replace("#", "");

            if (hex.Length == 8)
            {
                alpha = System.Convert.ToByte(hex.Substring(pos, 2), 16);
                pos = 2;
            }
            else
            {
                alpha = System.Convert.ToByte("ff", 16);
            }

            byte red = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            pos += 2;
            byte green = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            pos += 2;
            byte blue = System.Convert.ToByte(hex.Substring(pos, 2), 16);

            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}
