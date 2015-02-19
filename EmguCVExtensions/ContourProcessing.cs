using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using MathExtensions;

namespace EmguCVExtensions
{
    public static class ContourProcessing
    {
        public static List<Contour> ContoursToList(Contour<Point> contour) {
            return ContoursToList(contour, new Point());
        }

        public static List<Contour> ContoursToList(Contour<Point> contour, Point offset) {
            var allContours = new List<Contour>();
            while (contour != null) {
                Contour c = new Contour(contour, offset);
                allContours.Add(c);
                contour = contour.HNext;
            }

            return allContours;
        }

        public static List<Contour> FindContours<TColor, TDepth>(Image<TColor, TDepth> image, Rectangle searchArea)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var searchImageArea = searchArea.IntersectWith(image.Rectangle());
            var searchImage = image.GetSubRect(searchImageArea);

            Contour<Point> contours = searchImage.FindContours();

            Point offset = searchImageArea.TopLeft();
            var contourList = ContoursToList(contours, offset);

            return contourList;
        }

        public static List<Contour> FindContours<TColor, TDepth>(Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return FindContours(image, image.Rectangle());
        }
    }
}
