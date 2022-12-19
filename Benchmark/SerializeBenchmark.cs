using System.Runtime.InteropServices;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using RuntimeSerializer.Binary;

namespace Benchmark
{
    [Serializable]
    public class TestClass1
    {
        public byte Value0 { get; set; }
        public sbyte Value1 { get; set; }
        public char Value2 { get; set; }
        public Int16 Value3 { get; set; }
        public Int32 Value4 { get; set; }
        public Int64 Value5 { get; set; }
        public UInt16 Value6 { get; set; }
        public UInt32 Value7 { get; set; }
        public UInt64 Value8 { get; set; }
        public float Value9 { get; set; }
        public double Value10 { get; set; }
        public decimal Value11 { get; set; }
        public DateTime Value12 { get; set; }
        public DateTimeOffset Value13 { get; set; }
        public Guid Value14 { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct
    {
        public byte Value0;
        public sbyte Value1;
        public char Value2;
        public Int16 Value3;
        public Int32 Value4;
        public Int64 Value5;
        public UInt16 Value6;
        public UInt32 Value7;
        public UInt64 Value8;
        public float Value9;
        public double Value10;
        public decimal Value11;
        public DateTime Value12;
        public DateTimeOffset Value13;
        public Guid Value14;
    }

    public class SerializeBenchmark
    {
        private TestClass1 test;
        private TestClass1[] testClasses;
        private TestStruct testStruct;
        private TestStruct[] testStructs;

        public SerializeBenchmark()
        {
            var random = new Random(42);

            test = new TestClass1
            {
                Value0 = (byte)random.Next(byte.MinValue, byte.MaxValue),
                Value1 = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue),
                Value2 = (char)random.Next(0, 255),
                Value3 = (short)random.Next(short.MinValue, short.MaxValue),
                Value4 = random.Next(int.MinValue, int.MaxValue),
                Value5 = random.NextInt64(long.MinValue, long.MaxValue),
                Value6 = (ushort)random.Next(ushort.MinValue, ushort.MaxValue),
                Value7 = (uint)random.Next(int.MinValue, int.MaxValue),
                Value8 = (ulong)random.NextInt64(long.MinValue, long.MaxValue),
                Value9 = random.NextSingle(),
                Value10 = random.NextDouble(),
                Value11 = new decimal(random.NextDouble()),
                Value12 = DateTime.UtcNow,
                Value13 = DateTimeOffset.UtcNow,
                Value14 = Guid.NewGuid(),
            };

            testClasses = new TestClass1[1000];

            for(int i = 0; i < testClasses.Length; i++)
            {
                testClasses[i] = new TestClass1
                {
                    Value0 = (byte)random.Next(byte.MinValue, byte.MaxValue),
                    Value1 = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue),
                    Value2 = (char)random.Next(0, 255),
                    Value3 = (short)random.Next(short.MinValue, short.MaxValue),
                    Value4 = random.Next(int.MinValue, int.MaxValue),
                    Value5 = random.NextInt64(long.MinValue, long.MaxValue),
                    Value6 = (ushort)random.Next(ushort.MinValue, ushort.MaxValue),
                    Value7 = (uint)random.Next(int.MinValue, int.MaxValue),
                    Value8 = (ulong)random.NextInt64(long.MinValue, long.MaxValue),
                    Value9 = random.NextSingle(),
                    Value10 = random.NextDouble(),
                    Value11 = new decimal(random.NextDouble()),
                    Value12 = DateTime.UtcNow,
                    Value13 = DateTimeOffset.UtcNow,
                    Value14 = Guid.NewGuid(),
                };
            }

            testStruct = new TestStruct
            {
                Value0 = (byte)random.Next(byte.MinValue, byte.MaxValue),
                Value1 = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue),
                Value2 = (char)random.Next(0, 255),
                Value3 = (short)random.Next(short.MinValue, short.MaxValue),
                Value4 = random.Next(int.MinValue, int.MaxValue),
                Value5 = random.NextInt64(long.MinValue, long.MaxValue),
                Value6 = (ushort)random.Next(ushort.MinValue, ushort.MaxValue),
                Value7 = (uint)random.Next(int.MinValue, int.MaxValue),
                Value8 = (ulong)random.NextInt64(long.MinValue, long.MaxValue),
                Value9 = random.NextSingle(),
                Value10 = random.NextDouble(),
                Value11 = new decimal(random.NextDouble()),
                Value12 = DateTime.UtcNow,
                Value13 = DateTimeOffset.UtcNow,
                Value14 = Guid.NewGuid(),
            };

            testStructs = new TestStruct[1000];

            for (int i = 0; i < testStructs.Length; i++)
            {
                testStructs[i] = new TestStruct
                {
                    Value0 = (byte)random.Next(byte.MinValue, byte.MaxValue),
                    Value1 = (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue),
                    Value2 = (char)random.Next(0, 255),
                    Value3 = (short)random.Next(short.MinValue, short.MaxValue),
                    Value4 = random.Next(int.MinValue, int.MaxValue),
                    Value5 = random.NextInt64(long.MinValue, long.MaxValue),
                    Value6 = (ushort)random.Next(ushort.MinValue, ushort.MaxValue),
                    Value7 = (uint)random.Next(int.MinValue, int.MaxValue),
                    Value8 = (ulong)random.NextInt64(long.MinValue, long.MaxValue),
                    Value9 = random.NextSingle(),
                    Value10 = random.NextDouble(),
                    Value11 = new decimal(random.NextDouble()),
                    Value12 = DateTime.UtcNow,
                    Value13 = DateTimeOffset.UtcNow,
                    Value14 = Guid.NewGuid(),
                };
            }

            BinarySerializer.Materialize<TestClass1>();
            BinarySerializer.Materialize<TestClass1[]>();
            BinarySerializer.Materialize<TestStruct>();
            BinarySerializer.Materialize<TestStruct[]>();
        }

        [Benchmark]
        public void Serialize_Deserialize()
        {
            using var mem = new MemoryStream();

            BinarySerializer.Serialize(test, mem);
            mem.Position = 0;
            var v = BinarySerializer.DeSerialize<TestClass1>(mem);
        }

        [Benchmark]
        public void Serialize_Deserialize_1000()
        {
            using var mem = new MemoryStream();

            BinarySerializer.Serialize(testClasses, mem);
            mem.Position = 0;
            var v = BinarySerializer.DeSerialize<TestClass1[]>(mem);
        }

        [Benchmark]
        public void Serialize_Struct_Deserialize()
        {
            using var mem = new MemoryStream();

            BinarySerializer.Serialize(testStruct, mem);
            mem.Position = 0;
            var v = BinarySerializer.DeSerialize<TestStruct>(mem);
        }

        [Benchmark]
        public void Serialize_Struct_Deserialize_1000()
        {
            using var mem = new MemoryStream();

            BinarySerializer.Serialize(testStructs, mem);
            mem.Position = 0;
            var v = BinarySerializer.DeSerialize<TestStruct[]>(mem);
        }

        [Benchmark]
        public void JsonSerialize_DeSerialize()
        {
            var json = JsonSerializer.Serialize(test);

            var v = JsonSerializer.Deserialize<TestClass1>(json);
        }

        [Benchmark] 
        public void JsonSerialize_DeSerialize_1000() {
            var json = JsonSerializer.Serialize(testClasses);
            var v = JsonSerializer.Deserialize<TestClass1[]>(json);
        }
    }
}
