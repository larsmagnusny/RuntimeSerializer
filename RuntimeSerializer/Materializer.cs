using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RuntimeSerializer
{
    public static class Materializer
    {
#pragma warning disable CS8604 // Possible null reference argument.
        private static Dictionary<Type, MethodInfo> _methodInfosRead = new Dictionary<Type, MethodInfo>()
        {
            { typeof(byte), typeof(SerializationHelpers).GetMethod("ReadByte") },
            { typeof(sbyte), typeof(SerializationHelpers).GetMethod("ReadSByte") },
            { typeof(char), typeof(SerializationHelpers).GetMethod("ReadChar") },
            { typeof(short), typeof(SerializationHelpers).GetMethod("ReadInt16") },
            { typeof(ushort), typeof(SerializationHelpers).GetMethod("ReadUInt16") },
            { typeof(int), typeof(SerializationHelpers).GetMethod("ReadInt32") },
            { typeof(uint), typeof(SerializationHelpers).GetMethod("ReadUInt32") },
            { typeof(long), typeof(SerializationHelpers).GetMethod("ReadInt64") },
            { typeof(ulong), typeof(SerializationHelpers).GetMethod("ReadUInt64") },
            { typeof(float), typeof(SerializationHelpers).GetMethod("ReadSingle") },
            { typeof(double), typeof(SerializationHelpers).GetMethod("ReadDouble") },
            { typeof(decimal), typeof(SerializationHelpers).GetMethod("ReadDecimal") },
            { typeof(DateTime), typeof(SerializationHelpers).GetMethod("ReadDateTime") },
            { typeof(DateTimeOffset), typeof(SerializationHelpers).GetMethod("ReadDateTimeOffset") },
            { typeof(Guid), typeof(SerializationHelpers).GetMethod("ReadGuid") }
        };
        private static Dictionary<Type, MethodInfo> _methodInfosWrite = new Dictionary<Type, MethodInfo>()
        {
            { typeof(byte), typeof(SerializationHelpers).GetMethod("WriteByte") },
            { typeof(sbyte), typeof(SerializationHelpers).GetMethod("WriteSByte") },
            { typeof(char), typeof(SerializationHelpers).GetMethod("WriteChar") },
            { typeof(short), typeof(SerializationHelpers).GetMethod("WriteInt16") },
            { typeof(ushort), typeof(SerializationHelpers).GetMethod("WriteUInt16") },
            { typeof(int), typeof(SerializationHelpers).GetMethod("WriteInt32") },
            { typeof(uint), typeof(SerializationHelpers).GetMethod("WriteUInt32") },
            { typeof(long), typeof(SerializationHelpers).GetMethod("WriteInt64") },
            { typeof(ulong), typeof(SerializationHelpers).GetMethod("WriteUInt64") },
            { typeof(float), typeof(SerializationHelpers).GetMethod("WriteSingle") },
            { typeof(double), typeof(SerializationHelpers).GetMethod("WriteDouble") },
            { typeof(decimal), typeof(SerializationHelpers).GetMethod("WriteDecimal") },
            { typeof(DateTime), typeof(SerializationHelpers).GetMethod("WriteDateTime") },
            { typeof(DateTimeOffset), typeof(SerializationHelpers).GetMethod("WriteDateTimeOffset") },
            { typeof(Guid), typeof(SerializationHelpers).GetMethod("WriteGuid") }
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
                var elementType = t.GetElementType();

                var genericSerializeMethod = typeof(BinarySerializer).GetMethod("Serialize");

                var serializeMethod = genericSerializeMethod.MakeGenericMethod(elementType);

                var lengthProperty = t.GetProperty("Length");

                var i = Expression.Parameter(typeof(int), "i");

                var breakLabel = Expression.Label("break");
                var loop = Expression.Loop(
                    Expression.Block(
                        Expression.IfThenElse(
                            Expression.LessThan(i, Expression.Property(inputVar, lengthProperty)),
                            Expression.Block(
                                Expression.AddAssign(byteCounterExp, Expression.Call(serializeMethod, Expression.ArrayIndex(inputVar, i), parameters[1])),
                                Expression.PostIncrementAssign(i)), 
                            Expression.Break(breakLabel))), breakLabel);

                expressions.Add(WriteArrayLength(parameters[1], Expression.Property(inputVar, lengthProperty), byteCounterExp));
                expressions.Add(loop);
                expressions.Add(byteCounterExp);

                var b = Expression.Block(new[] { byteCounterExp, inputVar, i }, expressions.ToArray());

                var l = Expression.Lambda<Func<T, Stream, int>>(b, parameters);

                return l.Compile();
            }

            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(o => o.MemberType == MemberTypes.Property || o.MemberType == MemberTypes.Field).ToArray();

            // Create instance
            var addExp = Expression.Add(byteCounterExp, Expression.Constant(1));

            foreach(var member in members)
            {
                Type? memberType = default;

                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;

                var underlyingType = Nullable.GetUnderlyingType(memberType);

                if (underlyingType != null)
                {
                    expressions.AddRange(Write(parameters[1], inputVar, member, underlyingType, byteCounterExp));
                    continue;
                }

                expressions.AddRange(Write(parameters[1], inputVar, member, memberType, byteCounterExp));
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

            if (t.IsSZArray)
            {
                var elementType = t.GetElementType();

                var expressions = new List<Expression>();

                var genericDeSerializeMethod = typeof(BinarySerializer).GetMethod("DeSerialize");

                var deSerializeMethod = genericDeSerializeMethod.MakeGenericMethod(elementType);

                var lengthVar = Expression.Variable(typeof(int), "arrayLength");
                var assignArrayLenVar = Expression.Assign(lengthVar, ReadArrayLength(parameter));

                var retVar = Expression.Variable(t, "retVar");
                var assignRetVar = Expression.Assign(retVar, Expression.NewArrayBounds(elementType, lengthVar));

                var i = Expression.Parameter(typeof(int), "i");

                var breakLabel = Expression.Label("break");
                var loop = Expression.Loop(
                    Expression.Block(
                        Expression.IfThenElse(
                            Expression.LessThan(i, lengthVar),
                            Expression.Block(
                                Expression.Assign(Expression.ArrayAccess(retVar, i), Expression.Call(deSerializeMethod, parameter)),
                                Expression.PostIncrementAssign(i)),
                            Expression.Break(breakLabel))), breakLabel);

                expressions.Add(assignArrayLenVar);
                expressions.Add(assignRetVar);
                expressions.Add(loop);
                expressions.Add(retVar);

                var b = Expression.Block(new[] { retVar, lengthVar, i }, expressions.ToArray());

                var l = Expression.Lambda<Func<Stream, T>>(b, parameter);

                return l.Compile();
            }

            var members = t.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(o => o.MemberType == MemberTypes.Property || o.MemberType == MemberTypes.Field).ToArray();

            var ctor = Expression.New(t);

            var memberAssignments = new List<MemberAssignment>(members.Length);

            foreach (var member in members)
            {
                Type? memberType = default;

                if (member.MemberType == MemberTypes.Property)
                    memberType = ((PropertyInfo)member).PropertyType;
                if (member.MemberType == MemberTypes.Field)
                    memberType = ((FieldInfo)member).FieldType;

                var underlyingType = Nullable.GetUnderlyingType(memberType);

                if(underlyingType != null)
                {
                    memberAssignments.Add(Read(parameter, member, memberType));
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
