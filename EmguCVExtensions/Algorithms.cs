using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EmguCVExtensions
{
    public static class Algorithms
    {
        // Morphology structuring elements
        public static readonly Point KERNEL_CENTER = new Point(-1, -1);
        public static readonly MCvScalar MORPHOLOGY_BORDER = new MCvScalar(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue);

        public static Image<Gray, byte> SQUARE(int side) {
            using (Image<Gray, byte> zeroesImage = new Image<Gray, byte>(new Size(side, side))) {
                var onesImage = zeroesImage.Convert(b => (byte)1);
                return onesImage;
            }
        }

        public const string CIRCLE_IMAGE_FILE = "circle.tif";
        public static readonly Image<Gray, byte> CIRCLE_IMAGE = new Image<Gray, byte>(CIRCLE_IMAGE_FILE);

        public static Image<Gray, byte> CIRCLE(int radius) {

            if (radius <= 15) {
                return new Image<Gray, byte>("circle" + radius + ".tif");
            }
            else {
                double circleRadius = CIRCLE_IMAGE.Width;
                var scaledCircle = CIRCLE_IMAGE.Resize(radius / circleRadius, Inter.Nearest);
                return scaledCircle;    
            }
        }
    }
}
