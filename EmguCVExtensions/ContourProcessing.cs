using Emgu.CV;
using MathExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmguCVExtensions
{
    public static class ContourProcessing
    {
        public static List<Contour> ContoursToList(Contour<Point> contour, int imageWidth, int imageHeight) {
            var allContours = new List<Contour>();
            while (contour != null) {
                allContours.Add(new Contour(contour, imageWidth, imageHeight));
                contour = contour.HNext;
            }

            return allContours;
        }

        public static List<Contour> FindContours<TDepth, TColor>(Image<TColor, TDepth> image)
            where TColor : struct, global::Emgu.CV.IColor
            where TDepth : new()
        {
            Contour<Point> contours = image.FindContours();

            int width  = image.Width;
            int height = image.Height;
            var contourList = ContoursToList(contours, width, height);

            return contourList;
        }
    }
}
