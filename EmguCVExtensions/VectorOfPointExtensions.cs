using System.Collections.Generic;
using System.Drawing;
using Emgu.CV.Util;

namespace EmguCVExtensions
{
    public static class VectorOfPointExtensions
    {
        public static List<Point> ToList(this VectorOfPoint self) {
            var points = new List<Point>(self.Size);

            for (int i = 0; i < self.Size; i++)
                points.Add(self[i]);

            return points;
        }
    }
}
