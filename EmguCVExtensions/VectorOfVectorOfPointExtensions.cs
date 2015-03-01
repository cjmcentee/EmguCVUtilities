using System.Collections.Generic;
using System.Drawing;
using Emgu.CV.Util;

namespace EmguCVExtensions
{
    public static class VectorOfVectorOfPointExtensions
    {
        public static List<VectorOfPoint> ToList(this VectorOfVectorOfPoint self) {
            var points = new List<VectorOfPoint>(self.Size);

            for (int i = 0; i < self.Size; i++)
                points.Add(self[i]);

            return points;
        }
    }
}
