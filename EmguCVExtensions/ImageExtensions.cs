using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmguCVExtensions
{
    public static class ImageExtensions
    {
        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, List<Contour> contours, TColor color, int thickness)
            where TColor : struct, global::Emgu.CV.IColor
            where TDepth : new()
        {
            foreach (Contour contour in contours)
                image.Draw(contour.emgu, color, thickness);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, Contour contour, TColor color, int thickness)
            where TColor : struct, global::Emgu.CV.IColor
            where TDepth : new()
        {
            image.Draw(contour.emgu, color, thickness);
        }
    }
}
