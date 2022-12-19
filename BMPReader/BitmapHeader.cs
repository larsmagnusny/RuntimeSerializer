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
