using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMPReader
{
    public struct BitmapHeader
    {
        public short Signature;
        public uint FileSize;
        public uint Reserved;
        public uint DataOffset;
    }
}
