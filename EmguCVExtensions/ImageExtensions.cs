﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MathExtensions;

namespace EmguCVExtensions
{
    public enum SelectionConstraint
    {
        CONTAIN,
        EXTEND
    }

    public static class ImageExtensions
    {
        ////////////////////////////////////////////////
        //                  Constants
        ////////////////////////////////////////////////
        internal const int DRAW_FILL = -1;
        internal static readonly Gray DRAW_WHITE = new Gray(byte.MaxValue);


        ////////////////////////////////////////////////
        //                 Properties
        ////////////////////////////////////////////////
        public static Rectangle Rectangle<TDepth, TColor>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return new Rectangle(new Point(), image.Size);
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

        public static Type Color<TColor, TDepth>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return typeof(TColor);
        }

        public static Type Depth<TColor, TDepth>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return typeof(TDepth);
        }

        public static int NumberPixels<TColor, TDepth>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return image.Width * image.Height;
        }

        private static double MaxValue<TDepth>() {
            double maxValue = 255;
            if (typeof(TDepth) == typeof(ushort)) {
                maxValue = ushort.MaxValue;
            }
            else if (typeof(TDepth) == typeof(byte)) {
                maxValue = byte.MaxValue;
            }
            else if (typeof(TDepth) == typeof(float)) {
                maxValue = float.MaxValue;
            }
            else if (typeof(TDepth) == typeof(double)) {
                maxValue = double.MaxValue;
            }
            else {
                throw new ArgumentException("The type " + typeof(TDepth) + " is not supported.");
            }

            return maxValue;
        }


        ////////////////////////////////////////////////
        //                Adjustments
        ////////////////////////////////////////////////
        public static Image<TColor, TDepth> ScaleLevels<TColor, TDepth>(this Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {

            using (var grayscaleImage = image.Convert<Gray, double>()) {
                double imageMax = grayscaleImage.MaxValue();
                double maxValue = MaxValue<TDepth>();
                double scaleFactor = maxValue / imageMax;

                using (var scaledImage = grayscaleImage.Convert(p => p * scaleFactor)) {
                    return scaledImage.Convert<TColor, TDepth>();
                }
            }
        }


        ////////////////////////////////////////////////
        //                Morphology
        ////////////////////////////////////////////////
        public static Image<TColor, TDepth> ErodeBy<TColor, TDepth>(this Image<TColor, TDepth> self, int pixelsErosion)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var erodedImage = self.MorphologyEx(MorphOp.Erode, Algorithms.CIRCLE(pixelsErosion), new Point(pixelsErosion/2, pixelsErosion/2), 1, BorderType.Default, Algorithms.MORPHOLOGY_BORDER);
            return erodedImage;
        }

        public static Image<TColor, TDepth> DilateBy<TColor, TDepth>(this Image<TColor, TDepth> self, int pixelsDilation)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var dilatedImage = self.MorphologyEx(MorphOp.Dilate, Algorithms.CIRCLE(pixelsDilation), new Point(pixelsDilation/2, pixelsDilation/2), 1, BorderType.Default, Algorithms.MORPHOLOGY_BORDER);
            return dilatedImage;
        }

        public static Image<TColor, TDepth> OpenBy<TColor, TDepth>(this Image<TColor, TDepth> self, int pixelsOpenDistance)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var openedImage = self.MorphologyEx(MorphOp.Open, Algorithms.CIRCLE(pixelsOpenDistance), new Point(pixelsOpenDistance/2, pixelsOpenDistance/2), 1, BorderType.Default, Algorithms.MORPHOLOGY_BORDER);
            return openedImage;
        }

        public static Image<TColor, TDepth> CloseBy<TColor, TDepth>(this Image<TColor, TDepth> self, int pixelsCloseDistance)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var closedImage = self.MorphologyEx(MorphOp.Close, Algorithms.CIRCLE(pixelsCloseDistance), new Point(pixelsCloseDistance/2, pixelsCloseDistance/2), 1, BorderType.Default, Algorithms.MORPHOLOGY_BORDER);
            return closedImage;
        }


        ////////////////////////////////////////////////
        //                  Drawing
        ////////////////////////////////////////////////
        /// Integer Point
        public static void Draw<TDepth>(this Image<Rgb, TDepth> image, Point point, Color color, int thickness)
            where TDepth : new() { image.Draw(point, new Rgb(color), thickness); }

        /// Integer Point
        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, Point point, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (thickness < 1)
                throw new ArgumentException("Thickness of point to be drawn cannot be less than 1.");

            CircleF circle = new CircleF(point, thickness);
            image.Draw(circle, color, DRAW_FILL);
        }

        /// Float Point
        public static void Draw<TDepth>(this Image<Rgb, TDepth> image, PointF point, Color color, int thickness)
            where TDepth : new() { image.Draw(point, new Rgb(color), thickness); }

        /// Float Point
        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, PointF point, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            if (thickness < 1)
                throw new ArgumentException("Thickness of point to be drawn cannot be less than 1.");

            CircleF circle = new CircleF(point, thickness);
            image.Draw(circle, color, DRAW_FILL);
        }

        /// Rectangle
        public static void Draw<TDepth>(this Image<Rgb, TDepth> image, Rectangle rectangle, Color color, int thickness)
            where TDepth : new()
        {
            image.Draw(rectangle, new Rgb(color), thickness);
        }
        
        /// Rotated Rectangle
        public static void Draw<TDepth>(this Image<Rgb, TDepth> image, RotatedRectangle rect, Color color, int thickness)
            where TDepth : new() { image.Draw(rect, new Rgb(color), thickness); }

        /// Rotated Rectangle
        public static void Draw<TDepth, TColor>(this Image<TColor, TDepth> image, RotatedRectangle rect, TColor color, int thickness)
            where TColor : struct, IColor
            where TDepth : new()
        {
            var top    = new LineSegment2DF(rect.TopRight,    rect.TopLeft    );
            var left   = new LineSegment2DF(rect.TopLeft,     rect.BottomLeft );
            var bottom = new LineSegment2DF(rect.BottomLeft,  rect.BottomRight);
            var right  = new LineSegment2DF(rect.BottomRight, rect.TopRight   );

            image.Draw(top,    color, thickness);
            image.Draw(left,   color, thickness);
            image.Draw(bottom, color, thickness);
            image.Draw(right,  color, thickness);
        }

        /// Shape
        public static void Draw<TDepth, TShapeColor, TShapeDepth>(this Image<Rgb, TDepth> image, Shape<TShapeColor, TShapeDepth> shape, Color color, int thickness)
            where TDepth      : new()
            where TShapeColor : struct, IColor
            where TShapeDepth : new() { image.Draw(shape, new Rgb(color), thickness); }

        /// Shape
        public static void Draw<TDepth, TColor, TShapeColor, TShapeDepth>(this Image<TColor, TDepth> image, Shape<TShapeColor, TShapeDepth> shape, TColor color, int thickness)
            where TColor      : struct, IColor
            where TDepth      : new()
            where TShapeColor : struct, IColor
            where TShapeDepth : new()
        {
            image.Draw(shape.Points.ToArray(), color, thickness);
        }

        /// List of Shapes
        public static void Draw<TDepth,TShapeColor, TShapeDepth>(this Image<Rgb, TDepth> image, IEnumerable<Shape<TShapeColor, TShapeDepth>> shapes, Color color, int thickness)
            where TDepth      : new()
            where TShapeColor : struct, IColor
            where TShapeDepth : new() { image.Draw(shapes, new Rgb(color), thickness); }

        /// List of Shapes
        public static void Draw<TDepth, TColor, TShapeColor, TShapeDepth>(this Image<TColor, TDepth> image, IEnumerable<Shape<TShapeColor, TShapeDepth>> shapes, TColor color, int thickness)
            where TColor      : struct, IColor
            where TDepth      : new()
            where TShapeColor : struct, IColor
            where TShapeDepth : new()
        {
            foreach (Shape<TShapeColor, TShapeDepth> shape in shapes)
                image.Draw(shape, color, thickness);
        }
    }
}
