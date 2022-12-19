using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace RuntimeSerializer.Binary
{
    public static class BinaryMaterializer
    {
#pragma warning disable CS8604 // Possible null reference argument.
        private static Dictionary<Type, MethodInfo> _methodInfosRead = new Dictionary<Type, MethodInfo>()
        {
            { typeof(bool), typeof(BinarySerializationHelpers).GetMethod("ReadBool") },
            { typeof(byte), typeof(BinarySerializationHelpers).GetMethod("ReadByte") },
            { typeof(sbyte), typeof(BinarySerializationHelpers).GetMethod("ReadSByte") },
            { typeof(char), typeof(BinarySerializationHelpers).GetMethod("ReadChar") },
            { typeof(short), typeof(BinarySerializationHelpers).GetMethod("ReadInt16") },
            { typeof(ushort), typeof(BinarySerializationHelpers).GetMethod("ReadUInt16") },
            { typeof(int), typeof(BinarySerializationHelpers).GetMethod("ReadInt32") },
            { typeof(uint), typeof(BinarySerializationHelpers).GetMethod("ReadUInt32") },
            { typeof(long), typeof(BinarySerializationHelpers).GetMethod("ReadInt64") },
            { typeof(ulong), typeof(BinarySerializationHelpers).GetMethod("ReadUInt64") },
            { typeof(float), typeof(BinarySerializationHelpers).GetMethod("ReadSingle") },
            { typeof(double), typeof(BinarySerializationHelpers).GetMethod("ReadDouble") },
            { typeof(decimal), typeof(BinarySerializationHelpers).GetMethod("ReadDecimal") },
            { typeof(DateTime), typeof(BinarySerializationHelpers).GetMethod("ReadDateTime") },
            { typeof(DateTimeOffset), typeof(BinarySerializationHelpers).GetMethod("ReadDateTimeOffset") },
            { typeof(Guid), typeof(BinarySerializationHelpers).GetMethod("ReadGuid") },
            { typeof(string), typeof(BinarySerializationHelpers).GetMethod("ReadString") }
        };
        private static Dictionary<Type, MethodInfo> _methodInfosWrite = new Dictionary<Type, MethodInfo>()
        {
            { typeof(bool), typeof(BinarySerializationHelpers).GetMethod("WriteBool") },
            { typeof(byte), typeof(BinarySerializationHelpers).GetMethod("WriteByte") },
            { typeof(sbyte), typeof(BinarySerializationHelpers).GetMethod("WriteSByte") },
            { typeof(char), typeof(BinarySerializationHelpers).GetMethod("WriteChar") },
            { typeof(short), typeof(BinarySerializationHelpers).GetMethod("WriteInt16") },
            { typeof(ushort), typeof(BinarySerializationHelpers).GetMethod("WriteUInt16") },
            { typeof(int), typeof(BinarySerializationHelpers).GetMethod("WriteInt32") },
            { typeof(uint), typeof(BinarySerializationHelpers).GetMethod("WriteUInt32") },
            { typeof(long), typeof(BinarySerializationHelpers).GetMethod("WriteInt64") },
            { typeof(ulong), typeof(BinarySerializationHelpers).GetMethod("WriteUInt64") },
            { typeof(float), typeof(BinarySerializationHelpers).GetMethod("WriteSingle") },
            { typeof(double), typeof(BinarySerializationHelpers).GetMethod("WriteDouble") },
            { typeof(decimal), typeof(BinarySerializationHelpers).GetMethod("WriteDecimal") },
            { typeof(DateTime), typeof(BinarySerializationHelpers).GetMethod("WriteDateTime") },
            { typeof(DateTimeOffset), typeof(BinarySerializationHelpers).GetMethod("WriteDateTimeOffset") },
            { typeof(Guid), typeof(BinarySerializationHelpers).GetMethod("WriteGuid") },
            { typeof(string), typeof(BinarySerializationHelpers).GetMethod("WriteString") }
        };
#pragma warning restore CS8604 // Possible null reference argument.

        #region Writers
        public static Expression WriteArrayLength(ParameterExpression streamParameter, Expression value, Expression byteCounter)
        {
            var methodInfo = _methodInfosWrite[typeof(int)];

            return Expression.AddAssign(byteCounter, Expression.Call(null, methodInfo, streamParameter, value));
        }

        public static Expression ReadArrayLength(ParameterExpression streamParameter)
        {
            var methodInfo = _methodInfosRead[typeof(int)];

            return Expression.Call(null, methodInfo, streamParameter);
        }

        public static Expression[] WriteByte(ParameterExpression streamParameter, Expression input, MemberInfo member, Type type, Expression byteCounter)
        {
            var methodInfo = _methodInfosWrite[type];

            return new Expression[] {
                Expression.Call(streamParameter, methodInfo, Expression.MakeMemberAccess(input, member)),
                Expression.PostIncrementAssign(byteCounter)
            };
        }
        public static Expression[] Write(ParameterExpression streamParameter, Expression input, MemberInfo member, Type type, Expression byteCounter)
        {
            var methodInfo = _methodInfosWrite[type];

            return new Expression[] {
                Expression.AddAssign(byteCounter, Expression.Call(null, methodInfo, streamParameter, Expression.MakeMemberAccess(input, member)))
            };
        }
        public static Expression[] WriteNullable(ParameterExpression streamParameter, Expression input, MemberInfo member, Type type, Type underlyingType, Expression byteCounter)
        {
            var methodInfo = _methodInfosWrite[underlyingType];
            var writeBoolMethod = _methodInfosWrite[typeof(bool)];

            var memberAccess = Expression.MakeMemberAccess(input, member);
            var hasValueMethodInfo = type.GetProperty("HasValue");
            var valueProperty = type.GetProperty("Value");

            return new Expression[] {
                Expression.AddAssign(byteCounter, Expression.Call(null, writeBoolMethod, streamParameter, Expression.Property(memberAccess, hasValueMethodInfo))),
                Expression.AddAssign(byteCounter, Expression.Condition(Expression.Property(memberAccess, hasValueMethodInfo), Expression.Call(null, methodInfo, streamParameter, Expression.Property(memberAccess, valueProperty)), Expression.Constant(0)))
            };
        }
        #endregion
        #region Readers
        public static MemberAssignment ReadByte(ParameterExpression streamParameter, MemberInfo member, Type type)
        {
            var methodInfo = _methodInfosRead[type];

            return Expression.Bind(member, Expression.Convert(Expression.Call(streamParameter, methodInfo), typeof(byte)));
        }
        public static MemberAssignment Read(ParameterExpression streamParameter, MemberInfo member, Type type)
        {
            var methodInfo = _methodInfosRead[type];

            return Expression.Bind(member, Expression.Call(null, methodInfo, streamParameter));
        }

        public static MemberAssignment ReadNullable(ParameterExpression streamParameter, MemberInfo member, Type type, Type underlyingType)
        {
            var methodInfo = _methodInfosRead[underlyingType];
            var readBoolInfo = _methodInfosRead[typeof(bool)];

            var readboolExp = Expression.Call(null, readBoolInfo, streamParameter);

            return Expression.Bind(member, Expression.Condition(readboolExp, Expression.Convert(Expression.Call(null, methodInfo, streamParameter), type), Expression.Default(type)));
        }

        private static List<Expression> SerializeArrayExpression(Type type, ParameterExpression i, Expression inputVar, ParameterExpression stream, Expression byteCounter)
        {
            var expressions = new List<Expression>();
            var elementType = type.GetElementType();

            var genericSerializeMethod = typeof(BinarySerializer).GetMethod("Serialize");

            var serializeMethod = genericSerializeMethod.MakeGenericMethod(elementType);

            var lengthProperty = type.GetProperty("Length");


            var breakLabel = Expression.Label("break");
            var loop = Expression.Loop(
                Expression.Block(
                    Expression.IfThenElse(
                        Expression.LessThan(i, Expression.Property(inputVar, lengthProperty)),
                        Expression.Block(
                            Expression.AddAssign(byteCounter, Expression.Call(serializeMethod, Expression.ArrayIndex(inputVar, i), stream)),
                            Expression.PostIncrementAssign(i)),
                        Expression.Break(breakLabel))), breakLabel);

            expressions.Add(WriteArrayLength(stream, Expression.Property(inputVar, lengthProperty), byteCounter));
            expressions.Add(loop);

            return expressions;
        }

        public static List<Expression> DeSerializeArrayExpression(Type type, Expression retVar, Expression lengthVar, ParameterExpression i, ParameterExpression stream)
        {
            var elementType = type.GetElementType();

            var expressions = new List<Expression>();

            var genericDeSerializeMethod = typeof(BinarySerializer).GetMethod("DeSerialize");

            var deSerializeMethod = genericDeSerializeMethod.MakeGenericMethod(elementType);

            var assignArrayLenVar = Expression.Assign(lengthVar, ReadArrayLength(stream));

            var assignRetVar = Expression.Assign(retVar, Expression.NewArrayBounds(elementType, lengthVar));

            var breakLabel = Expression.Label("break");
            var loop = Expression.Loop(
                Expression.Block(
                    Expression.IfThenElse(
                        Expression.LessThan(i, lengthVar),
            Expression.Block(
                            Expression.Assign(Expression.ArrayAccess(retVar, i), Expression.Call(deSerializeMethod, stream)),
                            Expression.PostIncrementAssign(i)),
                        Expression.Break(breakLabel))), breakLabel);

            expressions.Add(assignArrayLenVar);
            expressions.Add(assignRetVar);
            expressions.Add(loop);
            return expressions;
        }

        #endregion
        public static Func<T, Stream, int> CreateSerializer<T>()
        {
            var t = typeof(T);

            var parameters = new ParameterExpression[]{
                Expression.Parameter(t, "input"),
                Expression.Parameter(typeof(Stream), "output")
            };

            var byteCounterExp = Expression.Variable(typeof(int), "byteCounter");
            var inputVar = Expression.Variable(t, "inputVar");
            var assignInputVar = Expression.Assign(inputVar, Expression.Convert(parameters[0], t));
            
            var expressions = new List<Expression>()
            {
                assignInputVar
            };

            if (t.IsSZArray)
            {
                var i = Expression.Parameter(typeof(int), "i");
                expressions.AddRange(SerializeArrayExpression(t, i, inputVar, parameters[1], byteCounterExp));
                expressions.Add(byteCounterExp);

                var b = Expression.Block(new[] { byteCounterExp, inputVar, i }, expressions.ToArray());
                var l = Expression.Lambda<Func<T, Stream, int>>(b, parameters);

                return l.Compile();
            }

            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(o => o.MemberType == MemberTypes.Property || o.MemberType == MemberTypes.Field).ToArray();

            // Create instance
            var addExp = Expression.Add(byteCounterExp, Expression.Constant(1));

            var arrayMembers = new List<(MemberInfo mi, Type t)>();

            foreach (var member in members)
            {
                Type? memberType = default;

                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;

                var underlyingType = Nullable.GetUnderlyingType(memberType);

                if (underlyingType != null)
                {
                    expressions.AddRange(WriteNullable(parameters[1], inputVar, member, memberType, underlyingType, byteCounterExp));
                    continue;
                }

                if (memberType.IsSZArray)
                {
                    arrayMembers.Add((member, memberType));
                    continue;
                }

                expressions.AddRange(Write(parameters[1], inputVar, member, memberType, byteCounterExp));
            }

            foreach(var (memberInfo, memberType) in arrayMembers)
            {
                var genericSerializeMethod = typeof(BinarySerializer).GetMethod("Serialize");
                var serializeMethod = genericSerializeMethod.MakeGenericMethod(memberType);

                var e = Expression.AddAssign(byteCounterExp, Expression.Call(null, serializeMethod, Expression.MakeMemberAccess(inputVar, memberInfo), parameters[1]));

                expressions.Add(e);
            }

            expressions.Add(byteCounterExp);

            var body = Expression.Block(new[] { byteCounterExp, inputVar }, expressions.ToArray());

            var lamda = Expression.Lambda<Func<T, Stream, int>>(body, parameters);

            return lamda.Compile();
        }

        public static Func<Stream, T> CreateDeserializer<T>()
        {
            var t = typeof(T);

            var parameter = Expression.Parameter(typeof(Stream), "stream");

            var expressions = new List<Expression>();

            var retVar = Expression.Variable(t, "retVar");

            if (t.IsSZArray)
            {
                var lengthVar = Expression.Variable(typeof(int), "arrayLength");
                var i = Expression.Parameter(typeof(int), "i");
                expressions.AddRange(DeSerializeArrayExpression(t, retVar, lengthVar, i, parameter));
                expressions.Add(retVar);

                var b = Expression.Block(new[] { retVar, lengthVar, i }, expressions.ToArray());

                var l = Expression.Lambda<Func<Stream, T>>(b, parameter);

                return l.Compile();
            }

            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(o => o.MemberType == MemberTypes.Property || o.MemberType == MemberTypes.Field).ToArray();

            var ctor = Expression.New(t);

            var memberAssignments = new List<MemberAssignment>(members.Length);
            var arrayMembers = new List<(MemberInfo mi, Type memberType)>();

            foreach (var member in members)
            {
                Type? memberType = default;

                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;

                var underlyingType = Nullable.GetUnderlyingType(memberType);

                if (underlyingType != null)
                {
                    memberAssignments.Add(ReadNullable(parameter, member, memberType, underlyingType));
                    continue;
                }

                if (memberType.IsSZArray)
                {
                    var genericDeSerializeMethod = typeof(BinarySerializer).GetMethod("DeSerialize");
                    var deSerializeMethod = genericDeSerializeMethod.MakeGenericMethod(memberType);

                    var bind = Expression.Bind(member, Expression.Call(null, deSerializeMethod, parameter));

                    memberAssignments.Add(bind);
                    continue;
                }

                memberAssignments.Add(Read(parameter, member, memberType));
            }

            var memberInit = Expression.MemberInit(ctor, memberAssignments);

            var lamda = Expression.Lambda<Func<Stream, T>>(memberInit, parameter);

            return lamda.Compile();
        }
    }
}
