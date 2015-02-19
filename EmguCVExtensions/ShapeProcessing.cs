using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguCVExtensions
{
    public static class ShapeProcessing
    {
        public static List<Shape<TDepth>> FindShapes<TDepth>(Image<Gray, byte> shapeImage,  Image<Gray, TDepth> image)
            where TDepth : new()
        {
            return FindShapes(shapeImage, image, image.Rectangle());
        }

        public static List<Shape<TDepth>> FindShapes<TDepth>(Image<Gray, byte> shapeImage, Image<Gray, TDepth> masterImage, Rectangle searchArea)
            where TDepth : new()
        {
            var contours = ContourProcessing.FindContours(shapeImage, searchArea);
            var shapes = from contour in contours
                         select new Shape<TDepth>(masterImage, contour);

            return shapes.ToList();
        }
    }
}
