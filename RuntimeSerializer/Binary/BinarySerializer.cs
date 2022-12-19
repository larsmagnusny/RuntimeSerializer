namespace RuntimeSerializer.Binary
{
    public class BinarySerializer
    {
        private static readonly Dictionary<Type, Delegate> _serializers = new();
        private static readonly Dictionary<Type, Delegate> _deserializers = new();

        private static object _lock = new object();

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
            Func<T, Stream, int> d;

            lock (_lock)
            {
                if (!_serializers.ContainsKey(type))
                    Materialize<T>();

                d = (Func<T, Stream, int>)_serializers[type];
            }

            return d(input, stream);
        }

        public static T DeSerialize<T>(Stream stream)
        {
            var type = typeof(T);
            Func<Stream, T> d;
            lock (_lock)
            {
                if (!_deserializers.ContainsKey(type))
                    Materialize<T>();

                d = (Func<Stream, T>)_deserializers[type];
            }

            return d(stream);
        }
    }
}