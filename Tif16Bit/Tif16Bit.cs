using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EmguCVExtensions;

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
                    var image = new Image<Gray, ushort>(new Bitmap(tif));
                    return image;
                }
            }
        }

        public static void WriteTif(Image<Gray, ushort> image, FileInfo tifFile) {
            WriteTif(image, tifFile.FullName);
        }

        public static void WriteTif(Image<Gray, ushort> image, String filePath) {

            using (BinaryWriter tifFile = new BinaryWriter(new FileStream(filePath, FileMode.Create))) {
                WriteTifHeader(tifFile);            // Writes 8 bytes

                IFDTags.WriteIFDHeader(tifFile);    // Writes 2 bytes
                IFDTags.WriteIFDTags(tifFile, image, 8 + 2);

                WriteImage(tifFile, image);
            }
        }

        private static void WriteTifHeader(BinaryWriter tifFile) {
            tifFile.Write((ushort) 0x4949);     // Little endian                                2 bytes
            tifFile.Write((ushort) 42);         // 42 DEC, Tiff binary ID                       2 bytes
            tifFile.Write((uint)    8);         // 8 bytes to the beginning of the IFD tags     4 bytes
        }

        private static void WriteImage(BinaryWriter tifFile, Image<Gray, ushort> image) {
            int numberPixels = image.NumberPixels();
            for (int i = 0; i < numberPixels; i++) {
                Gray pixel = image[new Point(i % image.Width, i / image.Width)];
                ushort pixelShade = (ushort) (pixel.Intensity / byte.MaxValue * ushort.MaxValue);
                tifFile.Write(pixelShade); // 2 bytes per pixel
            }
        }
    }
}