using Emgu.CV;
using MathExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
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
    }
}
