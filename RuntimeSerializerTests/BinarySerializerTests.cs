using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuntimeSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeSerializer.Tests
{
    public class TestClass1
    {
        public byte Value0 { get; set; } // 1
        public sbyte Value1 { get; set; }
        public char Value2 { get; set; } // 2
        public Int16 Value3 { get; set; } // 2
        public Int32 Value4 { get; set; } // 4
        public Int64 Value5 { get; set; } // 8
        public UInt16 Value6 { get; set; } // 2
        public UInt32 Value7 { get; set; } // 4
        public UInt64 Value8 { get; set; } // 8
        public float Value9 { get; set; } // 4
        public double Value10 { get; set; } // 8
        public decimal Value11 { get; set; } // 16
        public DateTime Value12 { get; set; } // 8
        public DateTimeOffset Value13 { get; set; } // 16
        public Guid Value14 { get; set; } // 16
    }

    public class TestClass2
    {
        public byte? Value0 { get; set; }
        public sbyte? Value1 { get; set; }
        public char? Value2 { get; set; }
        public Int16? Value3 { get; set; }
        public Int32? Value4 { get; set; }
        public Int64? Value5 { get; set; }
        public UInt16? Value6 { get; set; }
        public UInt32? Value7 { get; set; }
        public UInt64? Value8 { get; set; }
        public float? Value9 { get; set; }
        public double? Value10 { get; set; }
        public decimal? Value11 { get; set; }
        public DateTime? Value12 { get; set; }
        public DateTimeOffset? Value13 { get; set; }
        public Guid? Value14 { get; set; }
        public string? Value15 { get; set; }
    }

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


    [TestClass()]
    public class BinarySerializerTests
    {
        [TestMethod()]
        public void Serialize_Test()
        {
            var t1 = new TestClass1()
            {
                Value0 = 254,
                Value1 = -1,
                Value2 = 'a',
                Value3 = 123,
                Value4 = 14343,
                Value5 = 2232,
                Value6 = 3929,
                Value7 = 2392992,
                Value8 = 239829,
                Value9 = 39829892,
                Value10 = 389929,
                Value11 = 298299,
                Value12 = DateTime.UtcNow,
                Value13 = DateTimeOffset.UtcNow,
                Value14 = Guid.NewGuid()
            };

            var outStream = new MemoryStream();

            var n = BinarySerializer.Serialize(t1, outStream);

            Assert.AreEqual(n, 99);
        }

        [TestMethod()]
        public void Serialize_Deserialize_Test()
        {
            var t1 = new TestClass1()
            {
                Value0 = 254,
                Value1 = -1,
                Value2 = 'a',
                Value3 = 123,
                Value4 = 14343,
                Value5 = 2232,
                Value6 = 3929,
                Value7 = 2392992,
                Value8 = 239829,
                Value9 = 39829892,
                Value10 = 389929,
                Value11 = 298299,
                Value12 = DateTime.UtcNow,
                Value13 = DateTimeOffset.UtcNow,
                Value14 = Guid.NewGuid()
            };

            var outStream = new MemoryStream();

            var n = BinarySerializer.Serialize(t1, outStream);

            outStream.Position = 0;

            var t2 = BinarySerializer.DeSerialize<TestClass1>(outStream);

            Assert.AreEqual(t1.Value0, t2.Value0);
            Assert.AreEqual(t1.Value1, t2.Value1);
            Assert.AreEqual(t1.Value2, t2.Value2);
            Assert.AreEqual(t1.Value3, t2.Value3);
            Assert.AreEqual(t1.Value4, t2.Value4);
            Assert.AreEqual(t1.Value5, t2.Value5);
            Assert.AreEqual(t1.Value6, t2.Value6);
            Assert.AreEqual(t1.Value7, t2.Value7);
            Assert.AreEqual(t1.Value8, t2.Value8);
            Assert.AreEqual(t1.Value9, t2.Value9);
            Assert.AreEqual(t1.Value10, t2.Value10);
            Assert.AreEqual(t1.Value11, t2.Value11);
            Assert.AreEqual(t1.Value12, t2.Value12);
            Assert.AreEqual(t1.Value13, t2.Value13);
        }

        [TestMethod()]
        public void Serialize_Deserialize_Struct_Test()
        {
            var t1 = new TestStruct()
            {
                Value0 = 254,
                Value1 = -1,
                Value2 = 'a',
                Value3 = 123,
                Value4 = 14343,
                Value5 = 2232,
                Value6 = 3929,
                Value7 = 2392992,
                Value8 = 239829,
                Value9 = 39829892,
                Value10 = 389929,
                Value11 = 298299,
                Value12 = DateTime.UtcNow,
                Value13 = DateTimeOffset.UtcNow,
                Value14 = Guid.NewGuid()
            };

            var outStream = new MemoryStream();

            var n = BinarySerializer.Serialize(t1, outStream);

            outStream.Position = 0;

            var t2 = BinarySerializer.DeSerialize<TestStruct>(outStream);

            Assert.AreEqual(t1.Value0, t2.Value0);
            Assert.AreEqual(t1.Value1, t2.Value1);
            Assert.AreEqual(t1.Value2, t2.Value2);
            Assert.AreEqual(t1.Value3, t2.Value3);
            Assert.AreEqual(t1.Value4, t2.Value4);
            Assert.AreEqual(t1.Value5, t2.Value5);
            Assert.AreEqual(t1.Value6, t2.Value6);
            Assert.AreEqual(t1.Value7, t2.Value7);
            Assert.AreEqual(t1.Value8, t2.Value8);
            Assert.AreEqual(t1.Value9, t2.Value9);
            Assert.AreEqual(t1.Value10, t2.Value10);
            Assert.AreEqual(t1.Value11, t2.Value11);
            Assert.AreEqual(t1.Value12, t2.Value12);
            Assert.AreEqual(t1.Value13, t2.Value13);
        }


        [TestMethod()]
        public void Serialize_Nullables_Test()
        {
            var outStream = new MemoryStream();

            var t2 = new TestClass2()
            {
                Value0 = null,
                Value1 = null,
                Value2 = 'a',
                Value3 = 123,
                Value4 = 14343,
                Value5 = null,
                Value6 = 3929,
                Value7 = 2392992,
                Value8 = 239829,
                Value9 = null,
                Value10 = 389929,
                Value11 = 298299,
                Value12 = DateTime.UtcNow,
                Value13 = null,
                Value14 = Guid.NewGuid()
            };

            var n = BinarySerializer.Serialize(t2, outStream);
        }

        [TestMethod()]
        public void Serialize_Array_Test()
        {
            var outStream = new MemoryStream();

            var t1 = new TestClass1[1000];

            for(var i = 0; i < t1.Length; i++)
            {
                t1[i] = new TestClass1()
                {
                    Value0 = 254,
                    Value1 = -1,
                    Value2 = 'a',
                    Value3 = 123,
                    Value4 = 14343,
                    Value5 = 2232,
                    Value6 = 3929,
                    Value7 = 2392992,
                    Value8 = 239829,
                    Value9 = 39829892,
                    Value10 = 389929,
                    Value11 = 298299,
                    Value12 = DateTime.UtcNow,
                    Value13 = DateTimeOffset.UtcNow,
                    Value14 = Guid.NewGuid()
                };
            }

            var n = BinarySerializer.Serialize(t1, outStream);

            outStream.Position = 0;

            var t2 = BinarySerializer.DeSerialize<TestClass1[]>(outStream);

            Assert.AreEqual(t1.Length, t2.Length);

            for(var i = 0; i < t1.Length; i++)
            {
                Assert.AreEqual(t1[i].Value0, t2[i].Value0);
                Assert.AreEqual(t1[i].Value1, t2[i].Value1);
                Assert.AreEqual(t1[i].Value2, t2[i].Value2);
                Assert.AreEqual(t1[i].Value3, t2[i].Value3);
                Assert.AreEqual(t1[i].Value4, t2[i].Value4);
                Assert.AreEqual(t1[i].Value5, t2[i].Value5);
                Assert.AreEqual(t1[i].Value6, t2[i].Value6);
                Assert.AreEqual(t1[i].Value7, t2[i].Value7);
                Assert.AreEqual(t1[i].Value8, t2[i].Value8);
                Assert.AreEqual(t1[i].Value9, t2[i].Value9);
                Assert.AreEqual(t1[i].Value10, t2[i].Value10);
                Assert.AreEqual(t1[i].Value11, t2[i].Value11);
                Assert.AreEqual(t1[i].Value12, t2[i].Value12);
                Assert.AreEqual(t1[i].Value13, t2[i].Value13);
            }
        }
    }
}