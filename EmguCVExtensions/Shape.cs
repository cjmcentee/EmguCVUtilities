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
        
        public readonly Image<TColor, TDepth> SourceImage;

        public Rectangle BoundingRectangle { get; private set; }


        ////////////////////////////////////////////////
        //                Properties
        ////////////////////////////////////////////////
        public double Area              { get { return CvInvoke.ContourArea(emguContour); } }
        public double Perimeter         { get { return CvInvoke.ArcLength(emguContour, SHAPE_IS_CLOSED); } }
        public PointF Center            { get { return new PointF(BoundingRectangle.CenterF().X,
                                                                  BoundingRectangle.CenterF().Y); } }
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

        public Image<Gray, byte> Mask {
            get {
                Image<Gray, byte> mask = new Image<Gray, byte>(BoundingRectangle.Size);
                mask.Draw(Points_Local.ToArray(), ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);
                return mask;
            }
        } 


        ////////////////////////////////////////////////
        //                Constructors
        ////////////////////////////////////////////////
        public Shape(Image<TColor, TDepth> sourceImage, VectorOfPoint inputContour, Point offset=new Point()) {
            this.SourceImage = sourceImage;
            this.BoundingRectangle = CvInvoke.BoundingRectangle(inputContour).OffsetBy(offset);

            var offsetPoints = from point in inputContour.ToList()
                               let offsetPoint = point.OffsetBy(offset)
                               select offsetPoint;
            this.emguContour = new VectorOfPoint(offsetPoints.ToArray());
        }

        public static List<Shape<TColor, TDepth>> FromMask<TContourDepth>(Image<TColor, TDepth> sourceImage, Image<Gray, TContourDepth> contourMask, Point offset=new Point())
            where TContourDepth : new()
        {
            VectorOfVectorOfPoint contours = ContourProcessing.FindContours(contourMask);
            var shapes = from contour in contours.ToList()
                         select new Shape<TColor, TDepth>(sourceImage, contour, offset);
            return shapes.ToList();
        }

        public Shape<TColor, TDepth> Copy() {
            return new Shape<TColor, TDepth>(SourceImage, emguContour);
        }


        ////////////////////////////////////////////////
        //                 Properties
        ////////////////////////////////////////////////
        public Image<Gray, byte> CreateOversizeMask(int widthExcess, int heightExcess) {
            Image<Gray, byte> oversizedMask = new Image<Gray, byte>(BoundingRectangle.Size.Width + widthExcess*2, BoundingRectangle.Size.Height + heightExcess*2);

            var points_oversize = from point in Points_Local
                                  let point_oversized = point.OffsetBy(widthExcess, heightExcess)
                                  select point_oversized;
            oversizedMask.Draw(points_oversize.ToArray(), ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);
            return oversizedMask;
        } 


        ////////////////////////////////////////////////
        //               Pixel Analysis
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


        ////////////////////////////////////////////////
        //                 Shape Analysis
        ////////////////////////////////////////////////
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


        ////////////////////////////////////////////////
        //                 Arithmetic
        ////////////////////////////////////////////////
        public Shape<TColor, TDepth> OffsetBy(Point offsetAmount) {
            return new Shape<TColor, TDepth>(this.SourceImage, this.emguContour, offsetAmount);
        }


        ////////////////////////////////////////////////
        //             Boolean Operations
        ////////////////////////////////////////////////
        public bool Contains(Point p) {
            return this.BoundingRectangle.Contains(p);
        }
        
        public bool IntersectsAny<TColor2, TDepth2>(IEnumerable<Shape<TColor2, TDepth2>> otherShapes)
            where TColor2 : struct, IColor
            where TDepth2 : new()
        {
            // To calculate intersection, draw both shapes two images
            // intersect the images, and look see if anything remains
            Point offsetPoint = this.BoundingRectangle.TopLeft().Negate();

            using (Image<Gray, byte> thisShapeImage   = new Image<Gray, byte>(this.BoundingRectangle.Size))
            using (Image<Gray, byte> otherShapesImage = new Image<Gray, byte>(this.BoundingRectangle.Size)) {

                var thisOffsetShape    = this.OffsetBy(offsetPoint);
                var otherOffsetsShapes = from other in otherShapes
                                         let otherOffset = other.OffsetBy(offsetPoint)
                                         select otherOffset;

                thisShapeImage  .Draw(thisOffsetShape,    ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);
                otherShapesImage.Draw(otherOffsetsShapes, ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);

                var intersectionImage = thisShapeImage.And(otherShapesImage);
                int numberNonZeroPixels = intersectionImage.CountNonzero().First(); // Only 1 channel to count, so take first in array

                bool shapesIntersect = numberNonZeroPixels > 0;

                return shapesIntersect;
            }
        }

        public bool Intersects<TColor2, TDepth2>(Shape<TColor2, TDepth2> other)
            where TColor2 : struct, IColor
            where TDepth2 : new()
        {
            // If bounding rectangles don't intersect, their contained objects certainly don't
            if ( ! this.BoundingRectangle.IntersectsWith(other.BoundingRectangle))
                return false;

            // To calculate intersection, draw both shapes two images
            // intersect the images, and look see if anything remains
            Rectangle bothShapesBoundingRectangle = this.BoundingRectangle.InflateToContain(other.BoundingRectangle);
            Point offsetPoint = bothShapesBoundingRectangle.TopLeft().Negate();

            using (Image<Gray, byte> thisShapeImage  = new Image<Gray, byte>(bothShapesBoundingRectangle.Size))
            using (Image<Gray, byte> otherShapeImage = new Image<Gray, byte>(bothShapesBoundingRectangle.Size)) {

                var thisOffsetShape  = this .OffsetBy(offsetPoint);
                var otherOffsetShape = other.OffsetBy(offsetPoint);

                thisShapeImage .Draw(thisOffsetShape,  ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);
                otherShapeImage.Draw(otherOffsetShape, ImageExtensions.DRAW_WHITE, ImageExtensions.DRAW_FILL);

                var intersectionImage = thisShapeImage.And(otherShapeImage);
                int numberNonZeroPixels = intersectionImage.CountNonzero().First(); // Only 1 channel to count, so take first in array

                bool shapesIntersect = numberNonZeroPixels > 0;

                return shapesIntersect;
            }
        }


        ////////////////////////////////////////////////
        //          Morphological Operations
        ////////////////////////////////////////////////
        public List<Shape<TColor, TDepth>> Erode(int pixelsErosion) {

            var oversizedMask = CreateOversizeMask(pixelsErosion, pixelsErosion);
            Point oversizedMaskOffset = BoundingRectangle.TopLeft().OffsetBy(-pixelsErosion, -pixelsErosion);

            var erodedMask = oversizedMask.ErodeBy(pixelsErosion);
            var shapes = FromMask(SourceImage, erodedMask, oversizedMaskOffset);
            return shapes;
        }

        public Shape<TColor, TDepth> Dilate(int pixelsDilation) {

            var oversizedMask = CreateOversizeMask(pixelsDilation, pixelsDilation);
            Point oversizedMaskOffset = BoundingRectangle.TopLeft().OffsetBy(-pixelsDilation, -pixelsDilation);

            var dilatedMask = oversizedMask.DilateBy(pixelsDilation);
            var shapes = FromMask(SourceImage, dilatedMask, oversizedMaskOffset);

            if (shapes.Count == 0)
                return this.Copy();
            else
                return shapes[0]; // Dilation never generates more than one shape
        }

        public List<Shape<TColor, TDepth>> Open(int pixelsOpenDistance) {

            var oversizedMask = CreateOversizeMask(pixelsOpenDistance, pixelsOpenDistance);
            Point oversizedMaskOffset = BoundingRectangle.TopLeft().OffsetBy(-pixelsOpenDistance, -pixelsOpenDistance);

            var dilatedMask = oversizedMask.OpenBy(pixelsOpenDistance);
            var shapes = FromMask(SourceImage, dilatedMask, oversizedMaskOffset);

            return shapes;
        }

        public Shape<TColor, TDepth> Close(int pixelsCloseDistance) {

            var oversizedMask = CreateOversizeMask(pixelsCloseDistance, pixelsCloseDistance);
            Point oversizedMaskOffset = BoundingRectangle.TopLeft().OffsetBy(-pixelsCloseDistance, -pixelsCloseDistance);

            var dilatedMask = oversizedMask.CloseBy(pixelsCloseDistance);
            var shapes = FromMask(SourceImage, dilatedMask, oversizedMaskOffset);

            if (shapes.Count == 0)
                return this.Copy();
            else
                return shapes[0]; // Close never generates more than one shape
        }
    }
}
