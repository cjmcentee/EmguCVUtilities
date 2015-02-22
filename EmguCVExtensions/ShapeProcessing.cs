using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using MathExtensions;

namespace EmguCVExtensions
{
    public static class ShapeProcessing
    {
        public static List<Shape<TColor, TDepth>> FindShapes<TColor, TDepth>(Image<TColor, TDepth> masterImage, Image<Gray, byte> shapeImage)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (masterImage.Size != shapeImage.Size)
                throw new ArgumentException("Shape and master images must be the same size to search for shapes.");

            return FindShapes(masterImage, shapeImage, masterImage.Rectangle());
        }

        public static List<Shape<TColor, TDepth>> FindShapes<TColor, TDepth>(Image<TColor, TDepth> masterImage, Image<Gray, byte> shapeImage, Rectangle searchArea)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var contours = ContourProcessing.FindContours(shapeImage, searchArea);
            var shapes = from contour in contours
                         select new Shape<TColor, TDepth>(masterImage, contour);

            return shapes.ToList();
        }

        public static List<Shape<TColor, TDepth>> FindShapes<TColor, TDepth>(Image<TColor, TDepth> masterImage, Rectangle shapeInMaster, Image<Gray, byte> shapeSubImage)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var contours = ContourProcessing.FindContours(shapeSubImage);
            var offsetContours = from contour in contours
                                 let offsetContour = contour.Offset(shapeInMaster.TopLeft())
                                 select offsetContour;

            var shapes = from contour in offsetContours
                         select new Shape<TColor, TDepth>(masterImage, contour);
            return shapes.ToList();
        }
    }
}
