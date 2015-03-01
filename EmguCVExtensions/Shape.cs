using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathExtensions;

namespace EmguCVExtensions
{
    public class Shape<TColor, TDepth>
        where TColor : struct, IColor
        where TDepth : new()
    {
        ////////////////////////////////////////////////
        //                  Constants
        ////////////////////////////////////////////////
        public  static readonly Shape<Gray, byte> Circle = GenerateCircle();

        private const string CIRCLE_FILE_NAME = "circle.tif";
        private static Shape<Gray, byte> GenerateCircle() {
            var image = new Image<Gray, byte>(CIRCLE_FILE_NAME);
            var shapes = ContourProcessing.FindContours(image)       .ToList()
                        .Select(c => new Shape<Gray, byte>(image, c)).ToList();
            return shapes[0];
        }

        private const bool SHAPE_IS_CLOSED = true;

        ////////////////////////////////////////////////
        //                  Fields
        ////////////////////////////////////////////////
        internal readonly VectorOfPoint emguContour;
        internal readonly Point offset;
        
        public readonly Image<TColor, TDepth> SourceImage;

        public Rectangle BoundingRectangle { get; private set; }


        ////////////////////////////////////////////////
        //                Properties
        ////////////////////////////////////////////////
        public double Area              { get { return CvInvoke.ContourArea(emguContour); } }
        public double Perimeter         { get { return CvInvoke.ArcLength(emguContour, SHAPE_IS_CLOSED); } }
        public PointF Center            { get { return new PointF(offset.X + BoundingRectangle.CenterF().X,
                                                                  offset.Y + BoundingRectangle.CenterF().Y); } }
        public List<Point> Points       { get { return emguContour.ToList(); } }
        public List<Point> Points_Local { 
            get { 
                return (from point in Points
                        let point_local = point.RelativeTo(BoundingRectangle.TopLeft())
                        select point_local).ToList();
            }
        }

        public PointF ImageCenterDisplacement {
            get { return Center.RelativeTo(new PointF((float)SourceImage.Width/2, (float)SourceImage.Height/2)); }
        }


        ////////////////////////////////////////////////
        //                Constructors
        ////////////////////////////////////////////////
        public Shape(Image<TColor, TDepth> sourceImage, VectorOfPoint inputContour, Point offset=new Point()) {
            this.offset = offset;
            this.SourceImage = sourceImage;
            this.BoundingRectangle = CvInvoke.BoundingRectangle(inputContour);

            var offsetPoints = from point in inputContour.ToList()
                               let offsetPoint = point.OffsetBy(offset)
                               select offsetPoint;
            this.emguContour = new VectorOfPoint(offsetPoints.ToArray());
        }

        public static List<Shape<TColor, TDepth>> FromMask<TContourDepth>(Image<TColor, TDepth> sourceImage, Image<Gray, TContourDepth> contourMask)
            where TContourDepth : new()
        {
            VectorOfVectorOfPoint contours = ContourProcessing.FindContours(contourMask);
            var shapes = from contour in contours.ToList()
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
            return this.BoundingRectangle.Contains(p);
        }

        public bool IsNearCenter() {
            int minSideLength = Math.Min(SourceImage.Width, SourceImage.Height);
            return DistanceToCenter() < minSideLength/4;
        }

        public double DistanceToCenter() {
            PointF center = new PointF(SourceImage.Width/2, SourceImage.Height/2);
            PointF contourCenter = CvInvoke.MinAreaRect(this.emguContour).Center;

            return center.DistanceFrom(contourCenter);
        }

        public double CalculateCircleness() {
            return CvInvoke.MatchShapes(Circle.emguContour, this.emguContour, ContoursMatchType.I1);
        }

        public bool IsCircular() {
            return CalculateCircleness() < 0.1;
        }

        public Shape<TColor, TDepth> OffsetBy(Point offsetAmount) {
            return new Shape<TColor, TDepth>(this.SourceImage, this.emguContour, offsetAmount);
        }
    }
}
