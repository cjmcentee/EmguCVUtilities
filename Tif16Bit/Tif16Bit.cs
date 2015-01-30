using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TifReader
{
    public class Tif16Bit
    {
        public static Image<Gray, UInt16> ReadTif(String filePath) {
            using(FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using(Image tif = Image.FromStream(stream, false, false))
                {
                    // This is only meant to open tif files, so we can expect width and height to be integers
                    int width = (int) tif.PhysicalDimension.Width;
                    int height = (int) tif.PhysicalDimension.Height;

                    // Read as ummanaged object
                    IntPtr imP = CvInvoke.cvLoadImage(filePath, Emgu.CV.CvEnum.LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_ANYDEPTH);

                    // Copy to managed image object
                    Image<Gray, UInt16> image = new Image<Gray, UInt16>(width, height);
                    CvInvoke.cvCopy(imP, image, IntPtr.Zero);           
                    CvInvoke.cvReleaseImage(ref imP);
            
                    return image;
                }
            }
            
            
        }
    }
}