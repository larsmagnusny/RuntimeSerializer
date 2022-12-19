using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMPReader
{
    public struct BitmapDIBHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitsPerPixel;
        public uint Compression;
        public uint ImageSize;
        public uint XpixelsPerM;
        public uint YpixelsPerM;
        public uint ColorsUsed;
        public uint ImportantColors;
    }
}
