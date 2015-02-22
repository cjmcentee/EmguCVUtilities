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
    // Wrapper class for the retardation that is emgu's CV's uncopyable, unindexable contour class
    // The emgu class "Contour<Point>" is a pointer to a native contour object, the only way to
    // access the other contours is into increment the pointer through the list
    // But if we want to use any, ANY, of C#'s list features, we have to copy each contour
    // into a List<Contour<Point>>. But this can't be done because certain memory values
    // of the contour don't maintain their state when you push in new points (specifically
    // it's impossible to copy the BoundingRectangle even when all the points copy over appropriately)
    // Fucking ridiculous.
    // Fuck emgucv
    public class Contour
    {
        ////////////////////////////////////////////////
        //                  Constants
        ////////////////////////////////////////////////
        public  static readonly Contour Circle = GenerateCircle();

        private const string CIRCLE_FILE_NAME = "circle.tif";
        private static Contour GenerateCircle() {
            var image = new Image<Gray, byte>(CIRCLE_FILE_NAME);
            List<Contour> contours = ContourProcessing.FindContours(image);
            return contours[0];
        }

        ////////////////////////////////////////////////
        //                  Fields
        ////////////////////////////////////////////////
        internal readonly Contour<Point> emguContour;
        internal readonly Point offset;

        public Rectangle BoundingRectangle { get; private set; }


        ////////////////////////////////////////////////
        //                Properties
        ////////////////////////////////////////////////
        public double Area {
            get { return emguContour.Area; }
        }

        public double Perimeter {
            get { return emguContour.Perimeter; }
        }

        public PointF Center {
            get { return new PointF(offset.X + BoundingRectangle.CenterF().X,
                                    offset.Y + BoundingRectangle.CenterF().Y); }
        }

        public List<Point> Points {
            get {
                return emguContour.ToList();
            }
        }
        public List<Point> Points_BoundingBoxTopLeft { 
            get { 
                return (from point in emguContour
                        let point_local = point.RelativeTo(BoundingRectangle.TopLeft())
                        select point_local).ToList();
            }
        }
        


        ////////////////////////////////////////////////
        //              Constructors
        ////////////////////////////////////////////////
        public Contour(Contour<Point> emguContour, Point offset=new Point()) {
            this.offset = offset;
            this.BoundingRectangle = emguContour.BoundingRectangle.OffsetBy(offset);

            this.emguContour = new Contour<Point>(new MemStorage());
            foreach (Point p in emguContour)
                this.emguContour.Push(p.OffsetBy(offset));
        }

        public Contour(Contour contour, Point offset=new Point()) {
            this.offset = offset;
            this.BoundingRectangle = contour.BoundingRectangle.OffsetBy(offset);

            this.emguContour = new Contour<Point>(new MemStorage());
            foreach (Point p in contour.Points)
                this.emguContour.Push(p.OffsetBy(offset));
        }


        ////////////////////////////////////////////////
        //                  Methods
        ////////////////////////////////////////////////
        public bool Contains(Point p) {
            return BoundingRectangle.Contains(p);
        }

        public double CalculateCircleness() {
            return emguContour.MatchShapes(Circle.emguContour, CONTOURS_MATCH_TYPE.CV_CONTOUR_MATCH_I1);
        }

        public bool IsCircular() {
            return CalculateCircleness() < 0.1;
        }

        public Contour Offset(Point offsetAmount) {
            return new Contour(this, offsetAmount);
        }
    }
}
