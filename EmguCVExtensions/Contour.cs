﻿using Emgu.CV;
using MathExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessor
{
    // Wrapper class for the retardation that is Emgu CV's uncopyable, unindexable contour class
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
        public readonly Contour<Point> emgu;
        public readonly Rectangle BoundingRectangle;

        public double DimensionRatio {
            get { return (double)(BoundingRectangle.Width) / (double)(BoundingRectangle.Height); }
        }

        public double Diameter {
            get { return (BoundingRectangle.Width + BoundingRectangle.Height)/2; }
        }

        public double Area {
            get { return emgu.Area; }
        }

        public double Perimeter {
            get { return emgu.Perimeter; }
        }

        public PointF Center {
            get { return BoundingRectangle.CenterF(); }
        }

        public PointF ImageCenterDisplacement {
            get { return Center.RelativeTo(new PointF((float)ImageWidth/2, (float)ImageHeight/2)); }
        }

        public readonly int ImageWidth, ImageHeight;

        public Contour(Contour<Point> emguContour, int imageWidth, int imageHeight) {
            emgu = new Contour<Point>(new MemStorage());
            foreach (Point p in emguContour)
                emgu.Push(p);
            this.BoundingRectangle = emguContour.BoundingRectangle;

            this.ImageWidth = imageWidth;
            this.ImageHeight = imageHeight;
        }

        public bool Contains(Point p) {
            return BoundingRectangle.Contains(p);
        }

        public bool IsNearCenter() {
            int minSideLength = Math.Min(ImageWidth, ImageHeight);
            return DistanceToCenter() < minSideLength/4;
        }

        public double DistanceToCenter() {
            PointF center = new PointF(ImageWidth/2, ImageHeight/2);
            PointF contourCenter = emgu.GetMinAreaRect().center;

            return center.DistanceFrom(contourCenter);
        }

        public bool IsCircular() {
            // Test for bounding rectangle being approximately a square
            if (Math.Abs(BoundingRectangle.Width - BoundingRectangle.Height) < 3) {
                double radius = Diameter/2;
                double estimatedArea = Math.Pow(radius, 2) * Math.PI;
                double areaError = Area * 0.25;

                bool contourCircular = Math.Abs(estimatedArea - Area) < areaError;
                return contourCircular;
            }
            else
                return false;
        }
    }
}