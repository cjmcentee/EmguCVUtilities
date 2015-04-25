using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using MathExtensions;

namespace EmguCVExtensions
{
    public static class ContourProcessing
    {
        internal static VectorOfVectorOfPoint FindContours<TColor, TDepth>(Image<TColor, TDepth> image, Rectangle searchArea)
            where TColor : struct, IColor
            where TDepth : new()
        {
            Rectangle searchImageArea = searchArea.IntersectWith(image.Rectangle());
            Image<TColor, TDepth> searchImage = image.GetSubRect(searchImageArea);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(searchImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            return contours;
        }

        internal static VectorOfVectorOfPoint FindContours<TColor, TDepth>(Image<TColor, TDepth> image)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return FindContours(image, image.Rectangle());
        }
    }
}
