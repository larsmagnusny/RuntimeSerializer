namespace BMPReader
{
    /// <summary>
    /// Repeated NumColors times, when bits-per-pixel < 8
    /// </summary>
    public struct ColorTable
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Reserved;
    }
}
