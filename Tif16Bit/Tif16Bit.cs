using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TifReader
{
    public class Tif16Bit
    {
        public static Image<Gray, UInt16> ReadTif(String filePath, int imageWidth, int imageHeight) {
            // Read as ummanaged object
            IntPtr imP = CvInvoke.cvLoadImage(filePath, Emgu.CV.CvEnum.LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_ANYDEPTH);

            // Copy to managed image object
            Image<Gray, UInt16> image = new Image<Gray, UInt16>(imageWidth, imageHeight);
            CvInvoke.cvCopy(imP, image, IntPtr.Zero);           
            CvInvoke.cvReleaseImage(ref imP);
            
            return image;
        }
    }
}