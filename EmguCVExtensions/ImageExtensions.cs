using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using MathExtensions;

namespace EmguCVExtensions
{
    public static class ImageExtensions
    {
        private const int DRAW_FILL = -1;

        public static Rectangle Rectangle<TDepth, TColor>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Rectangle(new Point(), image.Size);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, PointF point, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (thickness < 1)
                throw new ArgumentException("Thickness of point to be drawn cannot be less than 1.");

            CircleF circle = new CircleF(point, thickness);
            image.Draw(circle, color, DRAW_FILL);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, Point point, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (thickness < 1)
                throw new ArgumentException("Thickness of point to be drawn cannot be less than 1.");

            CircleF circle = new CircleF(point, thickness);
            image.Draw(circle, color, DRAW_FILL);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, RotatedRectangle rect, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var top    = new LineSegment2DF(rect.TopRight, rect.TopLeft);
            var left   = new LineSegment2DF(rect.TopLeft, rect.BottomLeft);
            var bottom = new LineSegment2DF(rect.BottomLeft, rect.BottomRight);
            var right  = new LineSegment2DF(rect.BottomRight, rect.TopRight);

            image.Draw(top, color, thickness);
            image.Draw(left, color, thickness);
            image.Draw(bottom, color, thickness);
            image.Draw(right, color, thickness);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, List<Contour> contours, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            foreach (Contour contour in contours)
                image.Draw(contour.emguContour, color, thickness);
        }

        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, Contour contour, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            image.Draw(contour.emguContour, color, thickness);
        }

        public static void Draw<TDepth, TColor, TShapeDepth>(this Image<TColor, TDepth> image, List<Shape<TShapeDepth>> shapes, TColor color)
            where TColor      : struct, IColor
            where TDepth      : new()
            where TShapeDepth : new()
        {
            foreach (Shape<TShapeDepth> shape in shapes)
                image.Draw(shape.Contour, color, DRAW_FILL);
        }

        public static void Draw<TDepth, TColor, TShapeDepth>(this Image<TColor, TDepth> image, Shape<TShapeDepth> shape, TColor color)
            where TColor      : struct, IColor
            where TDepth      : new()
            where TShapeDepth : new()
        {
            image.Draw(shape.Contour, color, DRAW_FILL);
        }

        public static Point MaxLocation<TDepth>(this Image<Gray, TDepth> image)
            where TDepth : new()
        {
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            return maxLocations[0];
        }

        public static Point MinLocation<TDepth>(this Image<Gray, TDepth> image)
            where TDepth : new()
        {
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            return minLocations[0];
        }

        public static double MaxValue<TDepth>(this Image<Gray, TDepth> image)
            where TDepth : new()
        {
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            return maxValues[0];
        }

        public static double MinValue<TDepth>(this Image<Gray, TDepth> image)
            where TDepth : new()
        {
            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            return minValues[0];
        }

        public static Image<TColor, TDepth> GetSubRectangle<TDepth, TColor>(this Image<TColor, TDepth> image, Rectangle rectangle, SelectionConstraint constraint)
            where TColor : struct, IColor
            where TDepth : new()
        {
            switch (constraint) {
                case SelectionConstraint.EXTEND:
                    throw new NotImplementedException();
                    
                case SelectionConstraint.CONTAIN:
                default:
                    rectangle.Intersect(image.Rectangle());
                    return image.GetSubRect(rectangle);
            }
        }
    }

    public enum SelectionConstraint
    {
        CONTAIN,
        EXTEND
    }
}
