using System.Diagnostics;
using System.Runtime.InteropServices;
using BMPReader;
using RuntimeSerializer.Binary;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        BinarySerializer.Materialize<BitmapHeader>();
        BinarySerializer.Materialize<BitmapDIBHeader>();
        BinarySerializer.Materialize<ColorTable>();
        BinarySerializer.Materialize<BitmapPixel>();

        Stopwatch sw = Stopwatch.StartNew();
        var fileStream = File.OpenRead("C:\\tmp\\test.bmp");

        var header = BinarySerializer.DeSerialize<BitmapHeader>(fileStream);
        var dibHeader = BinarySerializer.DeSerialize<BitmapDIBHeader>(fileStream);

        var numColors = 0;

        switch (dibHeader.BitsPerPixel)
        {
            case 1:
                numColors = 1;
                break;
            case 4:
                numColors = 16; 
                break;
            case 8:
                numColors = 256;
                break;
            case 16:
                numColors = 65536;
                break;
            case 24:
                numColors = 16000000;
                break;
        }

        if (dibHeader.BitsPerPixel < 8)
        {
            var colors = new ColorTable[numColors];
            for (var i = 0; i < numColors; i++)
            {
                colors[i] = BinarySerializer.DeSerialize<ColorTable>(fileStream);
            }
        }

        var bitmapData = new byte[3*dibHeader.Height*dibHeader.Width];

        var span = new Span<byte>(bitmapData);

        fileStream.Position = header.DataOffset;

        fileStream.ReadExactly(span);

        Console.WriteLine($"Read bmp file in {sw.Elapsed.TotalMilliseconds:F2} ms");
    }
}