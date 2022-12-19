using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeSerializer
{
    public class SerializationHelpers
    {
        #region Writers
        public static int WriteByte(Stream stream, byte value)
        {
            stream.WriteByte(value);

            return 1;
        }
        public static int WriteSByte(Stream stream, sbyte value)
        {
            stream.WriteByte((byte)value);

            return 1;
        }
        public static int WriteChar(Stream stream, char value)
        {
            var s = sizeof(char);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, (short)value);
            stream.Write(buffer);

            return s;
        }
        public static int WriteInt16(Stream stream, short value)
        {
            var s = sizeof(short);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteInt32(Stream stream, int value)
        {
            var s = sizeof(int);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteInt64(Stream stream, long value)
        {
            var s = sizeof(long);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteUInt16(Stream stream, ushort value)
        {
            var s = sizeof(ushort);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteUInt32(Stream stream, uint value)
        {
            var s = sizeof(uint);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteUInt64(Stream stream, ulong value)
        {
            var s = sizeof(ulong);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteSingle(Stream stream, float value)
        {
            var s = sizeof(float);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteDouble(Stream stream, double value)
        {
            var s = sizeof(double);
            Span<byte> buffer = stackalloc byte[s];
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            stream.Write(buffer);

            return s;
        }

        public static int WriteDecimal(Stream stream, decimal value)
        {
            var byteCount = 0;
            var s = sizeof(int);

            Span<int> bits = stackalloc int[4];
            // (int)d.Low, (int)d.Mid, (int)d.High, d._flags
            Decimal.TryGetBits(value, bits, out var count);

            Span<byte> buffer = stackalloc byte[s];

            for(var i = 0; i < 4; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, bits[i]);
                stream.Write(buffer); 
                byteCount += s;
            }

            return byteCount;
        }

        public static int WriteDateTime(Stream stream, DateTime value)
        {
            return WriteInt64(stream, value.ToBinary());
        }

        public static int WriteDateTimeOffset(Stream stream, DateTimeOffset value)
        {
            return WriteInt64(stream, value.Ticks) + WriteInt64(stream, value.Offset.Ticks);
        }

        public static int WriteGuid(Stream stream, Guid value)
        {
            Span<byte> buffer = stackalloc byte[16];
            value.TryWriteBytes(buffer);
            stream.Write(buffer);

            return buffer.Length;
        }
        #endregion

        #region Readers
        public static byte ReadByte(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[1];
            stream.ReadExactly(buffer);
            return buffer[0];
        }
        public static sbyte ReadSByte(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[1];
            stream.ReadExactly(buffer);
            return (sbyte)buffer[0];
        }
        public static char ReadChar(Stream stream)
        {
            var s = sizeof(char);
            Span<byte> buffer = stackalloc byte[s];
            
            stream.ReadExactly(buffer);

            return (char)BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }
        public static short ReadInt16(Stream stream)
        {
            var s = sizeof(short);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        public static int ReadInt32(Stream stream)
        {
            var s = sizeof(int);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        public static long ReadInt64(Stream stream)
        {
            var s = sizeof(long);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        public static ushort ReadUInt16(Stream stream)
        {
            var s = sizeof(ushort);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        public static uint ReadUInt32(Stream stream)
        {
            var s = sizeof(uint);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        public static ulong ReadUInt64(Stream stream)
        {
            var s = sizeof(ulong);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        public static float ReadSingle(Stream stream)
        {
            var s = sizeof(float);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        public static double ReadDouble(Stream stream)
        {
            var s = sizeof(double);
            Span<byte> buffer = stackalloc byte[s];

            stream.ReadExactly(buffer);

            return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        public static decimal ReadDecimal(Stream stream)
        {
            var s = sizeof(int);
            Span<byte> buffer = stackalloc byte[s];
            Span<int> intBuf = stackalloc int[4];

            for (var i = 0; i < 4; i++)
            {
                stream.ReadExactly(buffer);

                intBuf[i] = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            }

            return new decimal(intBuf);
        }

        public static DateTime ReadDateTime(Stream stream)
        {
            return DateTime.FromBinary(ReadInt64(stream));
        }

        public static DateTimeOffset ReadDateTimeOffset(Stream stream)
        {
            var ticks = ReadInt64(stream);
            var offsetTicks = ReadInt64(stream);

            return new DateTimeOffset(ticks, TimeSpan.FromTicks(offsetTicks));
        }

        public static Guid ReadGuid(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[16];

            stream.ReadExactly(buffer);

            return new Guid(buffer);
        }
        #endregion
    }
}
