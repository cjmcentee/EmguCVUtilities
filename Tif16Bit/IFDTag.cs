using System.Collections.Generic;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TifReader
{
    internal abstract class IFDTag
    {
        public const uint IFD_TAG_BYTES = 12; 

        public const ushort BYTE      = 1;
        public const ushort ASCII     = 2;
        public const ushort WORD      = 3;
        public const ushort DWORD     = 4;
        public const ushort RATIONAL  = 5;


        internal ushort ID;
        internal uint NumberValues = 1;

        public abstract void WriteToStream(BinaryWriter tifFile);
    }

    internal class WordIFDTag : IFDTag
    {
        internal ushort Word;

        internal WordIFDTag(ushort id, int word) 
            : this(id, (ushort) word) { }

        internal WordIFDTag(ushort id, ushort word) {
            this.ID = id;
            this.Word = word;
        }

        public override void WriteToStream(BinaryWriter tifFile) {
            tifFile.Write(ID);
            tifFile.Write(WORD);
            tifFile.Write(NumberValues);
            tifFile.Write(Word);
            tifFile.Write((ushort)0x00);
        }
    }

    internal class DWordIFDTag : IFDTag
    {
        internal uint DWord;

        internal DWordIFDTag(ushort id, int word) 
            : this(id, (uint) word) { }

        internal DWordIFDTag(ushort id, uint dword) {
            this.ID = id;
            this.DWord = dword;
        }

        public override void WriteToStream(BinaryWriter tifFile) {
            tifFile.Write(ID);
            tifFile.Write(DWORD);
            tifFile.Write(NumberValues);
            tifFile.Write(DWord);
        }
    }

    internal class RationalIFDTag : IFDTag
    {
        public const uint NUMBER_BYTES = 8;

        internal uint Offset, Numerator, Denominator;

        internal RationalIFDTag(ushort id, uint offset, uint numerator, uint denominator) {
            this.ID = id;
            this.Offset = offset;
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public override void WriteToStream(BinaryWriter tifFile) {
            tifFile.Write(ID);
            tifFile.Write(RATIONAL);
            tifFile.Write(NumberValues);
            tifFile.Write(Offset);
        }

        public void WriteRationalNumberToStream(BinaryWriter tifFile) {
            tifFile.Write(Numerator);
            tifFile.Write(Denominator);
        }
    }

    internal class IFDTags
    {
        public const uint NUMBER_IFD_TAGS = 13;
        public const uint ALL_IFD_TAGS_BYTES = NUMBER_IFD_TAGS * IFDTag.IFD_TAG_BYTES;

        const int BYTES_PER_USHORT = 2;

        const ushort DEFAULT_IMAGE = 0;
        const ushort USHORT_DEPTH = 16;
        const ushort NO_COMPRESSION = 1;
        const ushort GRAYSCALE = 1;
        const ushort ONE_SAMPLE_PER_PIXEL = 1;
        const ushort INCHES = 2;

        const ushort IMAGE_TYPE_TAG         = 0xFE;
        const ushort WIDTH_TAG              = 256;
        const ushort HEIGHT_TAG             = 257;
        const ushort BITS_PER_SAMPLE_TAG    = 258;
        const ushort COMPRESSION_TAG        = 259;
        const ushort CHANNELS_TAG           = 262;
        const ushort IMAGE_OFFSET_TAG       = 273;
        const ushort SAMPLES_PER_PIXEL_TAG  = 277;
        const ushort ROWS_PER_STRIP_TAG     = 278;
        const ushort BYTES_PER_STRIP_TAG    = 279;
        const ushort RESOLUTION_UNIT_TAG    = 296;
        const ushort X_RESOLUTION_TAG       = 282;
        const ushort Y_RESOLUTION_TAG       = 283;

        public static void WriteIFDHeader(BinaryWriter tifFile) {
            tifFile.Write((ushort)NUMBER_IFD_TAGS);  // How many bytes the IFD takes up      2 bytes
        } 

        public static void WriteIFDTags(BinaryWriter tifFile, Image<Gray, ushort> image, uint offset) {
            
            var rationalTags = new List<RationalIFDTag> {
                new RationalIFDTag(X_RESOLUTION_TAG, offset + ALL_IFD_TAGS_BYTES, 72, 1),
                new RationalIFDTag(Y_RESOLUTION_TAG, offset + ALL_IFD_TAGS_BYTES + RationalIFDTag.NUMBER_BYTES, 72, 1)
            };

            uint overflowBytes = (uint) rationalTags.Count * RationalIFDTag.NUMBER_BYTES;
            int  imageBytes = image.Height * image.Width * BYTES_PER_USHORT;

            var tags = new List<IFDTag> {
                new DWordIFDTag(IMAGE_TYPE_TAG,         DEFAULT_IMAGE),
                new WordIFDTag (WIDTH_TAG,              image.Width),
                new WordIFDTag (HEIGHT_TAG,             image.Height),
                new WordIFDTag (BITS_PER_SAMPLE_TAG,    USHORT_DEPTH),
                new WordIFDTag (COMPRESSION_TAG,        NO_COMPRESSION),
                new WordIFDTag (CHANNELS_TAG,           GRAYSCALE),
                new DWordIFDTag(IMAGE_OFFSET_TAG,       offset + ALL_IFD_TAGS_BYTES + overflowBytes), // Image comes after the IFD tags and tag data
                new WordIFDTag (SAMPLES_PER_PIXEL_TAG,  ONE_SAMPLE_PER_PIXEL),
                new WordIFDTag (ROWS_PER_STRIP_TAG,     image.Height),           // All rows in one strip
                new DWordIFDTag(BYTES_PER_STRIP_TAG,    imageBytes),             // All bytes in one strip
                new WordIFDTag (RESOLUTION_UNIT_TAG,    INCHES)
            };

            // Write IFD tag data
            foreach (var tag in tags)
                tag.WriteToStream(tifFile);
            foreach (var tag in rationalTags)
                tag.WriteToStream(tifFile);

            // Write the overflow data
            foreach (var tag in rationalTags)
                tag.WriteRationalNumberToStream(tifFile);
        }

    }
}
