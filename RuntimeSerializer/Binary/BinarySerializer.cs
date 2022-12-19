namespace RuntimeSerializer.Binary
{
    public class BinarySerializer
    {
        private static readonly Dictionary<Type, Delegate> _serializers = new();
        private static readonly Dictionary<Type, Delegate> _deserializers = new();

        public static void Materialize<T>()
        {
            var type = typeof(T);

            if (!_serializers.ContainsKey(type))
            {
                _serializers[type] = BinaryMaterializer.CreateSerializer<T>();
            }
            if (!_deserializers.ContainsKey(type))
            {
                _deserializers[type] = BinaryMaterializer.CreateDeserializer<T>();
            }
        }

        public static int Serialize<T>(T input, Stream stream)
        {
            var type = typeof(T);

            if (!_serializers.ContainsKey(type))
                Materialize<T>();

            var d = _serializers[type] as Func<T, Stream, int>;

            return d(input, stream);
        }

        public static T DeSerialize<T>(Stream stream)
        {
            var type = typeof(T);

            if (!_deserializers.ContainsKey(type))
                Materialize<T>();

            var d = _deserializers[type] as Func<Stream, T>;

            return d(stream);
        }
    }
}