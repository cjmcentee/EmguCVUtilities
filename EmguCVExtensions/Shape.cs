using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MathExtensions;

namespace EmguCVExtensions
{
    public class Shape<TColor, TDepth>
        where TColor : struct, IColor
        where TDepth : new()
    {
        ////////////////////////////////////////////////
        //                  Fields
        ////////////////////////////////////////////////
        public readonly Image<TColor, TDepth> SourceImage;
        internal readonly Contour Contour;

        ////////////////////////////////////////////////
        //                Properties
        ////////////////////////////////////////////////
        public double Area                  { get { return Contour.Area; } }
        public double Perimeter             { get { return Contour.Perimeter; } }
        public PointF Center                { get { return BoundingRectangle.CenterF(); } }
        public List<Point> Points           { get { return Contour.Points; } }
        public List<Point> Points_Local     { get { return Contour.Points_BoundingBoxTopLeft; } }
        public Rectangle BoundingRectangle  { get { return Contour.BoundingRectangle; } }

        public PointF ImageCenterDisplacement {
            get { return Center.RelativeTo(new PointF((float)SourceImage.Width/2, (float)SourceImage.Height/2)); }
        }


        ////////////////////////////////////////////////
        //                Constructors
        ////////////////////////////////////////////////
        public Shape(Image<TColor, TDepth> sourceImage, Contour contour) {
            this.SourceImage = sourceImage;
            this.Contour = contour;
        }

        public Shape(Image<TColor, TDepth> sourceImage, Image<Gray, byte> contourMask)
        {
            this.SourceImage = sourceImage;

            var contours = ContourProcessing.FindContours(contourMask);
            if (contours.Count > 1)
                throw new ArgumentException("Mask has more than one contour. Cannot construct shape.");
            this.Contour = contours.First();
        }

        public static List<Shape<TColor, TDepth>> FromMask<TContourDepth>(Image<TColor, TDepth> sourceImage, Image<Gray, TContourDepth> contourMask)
            where TContourDepth : new()
        {
            var contours = ContourProcessing.FindContours(contourMask);
            var shapes = from contour in contours
                         select new Shape<TColor, TDepth>(sourceImage, contour);
            return shapes.ToList();
        }


        ////////////////////////////////////////////////
        //                 Processing
        ////////////////////////////////////////////////
        public List<int> GetGrayScalePixelIntensities() {
            var croppedImage = SourceImage.GetSubRect(this.BoundingRectangle).Convert<Gray, TDepth>();

            var spotValues =
                from point in Points_Local
                let grayValue = croppedImage[point]
                select (int) grayValue.Intensity;

            return spotValues.ToList();
        }

        public List<int> GetNonShapeGrayScalePixelIntensities() {

            var croppedImage = SourceImage.GetSubRect(this.BoundingRectangle).Convert<Gray, TDepth>();

            var nonSpotValues = new List<int>();
            for (int x = 0; x < croppedImage.Width; x++) {
                for (int y = 0; y < croppedImage.Height; y++) {
                    Point pixel = new Point(x, y);
                    
                    if ( ! Points_Local.Contains(pixel)) {
                        Gray grayValue = croppedImage[pixel];
                        nonSpotValues.Add((int) grayValue.Intensity);
                    }
                }
            }

            return nonSpotValues.ToList();
        }

        public List<int> GetRelativePixelIntensities() {
            int medianNonSpotIntensity = GetGrayScalePixelIntensities().Median();

            var relativeSpotValues =
                from spotValue in GetGrayScalePixelIntensities()
                let relativeValue = spotValue - medianNonSpotIntensity
                select relativeValue;

            return relativeSpotValues.ToList();
        }

        public bool Contains(Point p) {
            return Contour.BoundingRectangle.Contains(p);
        }

        public bool IsNearCenter() {
            int minSideLength = Math.Min(SourceImage.Width, SourceImage.Height);
            return DistanceToCenter() < minSideLength/4;
        }

        public double DistanceToCenter() {
            PointF center = new PointF(SourceImage.Width/2, SourceImage.Height/2);
            PointF contourCenter = Contour.emguContour.GetMinAreaRect().center;

            return center.DistanceFrom(contourCenter);
        }

        public double CalculateCircleness() {
            return Contour.emguContour.MatchShapes(Contour.Circle.emguContour, CONTOURS_MATCH_TYPE.CV_CONTOUR_MATCH_I1);
        }

        public bool IsCircular() {
            return CalculateCircleness() < 0.1;
        }
    }
}
