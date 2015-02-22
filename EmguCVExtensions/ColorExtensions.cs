using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguCVExtensions
{
    public static class ColorExtensions
    {
        public static Rgb ToRgb(this Hsv self) {
            var hsvImage = new Image<Hsv, float>(new Size(1, 1));
            hsvImage[0, 0] = self;
            var rgbImage = hsvImage.Convert<Rgb, byte>();
            return rgbImage[0, 0];
        }

        public static Hsv ToHsv(this Rgb self) {
            var rgbImage = new Image<Rgb, float>(new Size(1, 1));
            rgbImage[0, 0] = self;
            var hsvImage = rgbImage.Convert<Hsv, byte>();
            return hsvImage[0, 0];
        }

        public static List<Hsv> SpanningHues(int numberColors) {
            // hue spans 0 - 180
            var hues = new List<double>(numberColors);
            for (int i = 0; i < numberColors; i++)
                hues.Add(i * 180.0/numberColors);

            var hsvColors = from hue in hues
                            let hsvColor = new Hsv(hue, 255, 255)
                            select hsvColor;
            return hsvColors.ToList();
        }
    }
}
