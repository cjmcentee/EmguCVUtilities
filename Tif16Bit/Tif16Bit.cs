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
        public static Image<Gray, ushort> ReadTif(FileInfo tifFile) {
            return ReadTif(tifFile.FullName);
        }

        public static Image<Gray, ushort> ReadTif(String tifFilePath) {
            using(FileStream stream = new FileStream(tifFilePath, FileMode.Open, FileAccess.Read))
            {
                using(Image tif = Image.FromStream(stream, false, false))
                {
                    int width = tif.Width;
                    int height = tif.Height;

                    // Read as ummanaged object
                    IntPtr imP = CvInvoke.cvLoadImage(tifFilePath, Emgu.CV.CvEnum.LOAD_IMAGE_TYPE.CV_LOAD_IMAGE_ANYDEPTH);

                    // Copy to managed image object
                    var image = new Image<Gray, ushort>(width, height);
                    CvInvoke.cvCopy(imP, image, IntPtr.Zero);           
                    CvInvoke.cvReleaseImage(ref imP);
            
                    return image;
                }
            }
        }
    }
}