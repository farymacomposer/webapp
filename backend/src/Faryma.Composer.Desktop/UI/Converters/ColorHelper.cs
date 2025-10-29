using Windows.UI;

namespace Faryma.Composer.Desktop.UI
{
    public static class ColorHelper
    {
        public static Color FromHsl(double h, double s, double l)
        {
            double red;
            double green;
            double blue;

            h /= 360.0;
            s /= 100.0;
            l /= 100.0;

            if (Math.Abs(s - 0.0) < 0.00001)
            {
                red = l;
                green = l;
                blue = l;
            }
            else
            {
                double var2;

                if (l < 0.5)
                {
                    var2 = l * (1 + s);
                }
                else
                {
                    var2 = l + s - (s * l);
                }

                double var1 = (2 * l) - var2;

                red = Hue2Rgb(var1, var2, h + (1.0 / 3.0));
                green = Hue2Rgb(var1, var2, h);
                blue = Hue2Rgb(var1, var2, h - (1.0 / 3.0));
            }

            byte nRed = Convert.ToByte(red * 255);
            byte nGreen = Convert.ToByte(green * 255);
            byte nBlue = Convert.ToByte(blue * 255);

            return Color.FromArgb(255, nRed, nGreen, nBlue);
        }

        private static double Hue2Rgb(double v1, double v2, double vH)
        {
            if (vH < 0)
            {
                vH++;
            }

            if (vH > 1)
            {
                vH--;
            }

            if (6 * vH < 1)
            {
                return v1 + ((v2 - v1) * 6 * vH);
            }

            if (2 * vH < 1)
            {
                return v2;
            }

            if (3 * vH < 2)
            {
                return v1 + ((v2 - v1) * ((2.0 / 3.0) - vH) * 6);
            }

            return v1;
        }
    }
}