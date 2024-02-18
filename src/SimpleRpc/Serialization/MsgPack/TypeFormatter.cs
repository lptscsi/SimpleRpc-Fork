using MessagePack;
using MessagePack.Formatters;
using SimpleRpc.Serialization.Wire.Library.Extensions;
using SimpleRpc.Serialization.Wire.Library.Internal;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SimpleRpc.Serialization.MsgPack
{
    public class TypeFormatter<T> : IMessagePackFormatter<T> 
        where T : Type
    {
        private static readonly ConcurrentDictionary<ByteArrayKey, Type> _byteTypeNameLookup = new ConcurrentDictionary<ByteArrayKey, Type>(ByteArrayKeyComparer.Instance);
        private static readonly ConcurrentDictionary<Type, byte[]> _typeByteNameLookup = new ConcurrentDictionary<Type, byte[]>();

        public static readonly List<KeyValuePair<string, string>> _macroses = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("System", "$s"),
            new KeyValuePair<string, string>("Collection", "$c"),
        };

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
            }

            var stringAsBytes = _typeByteNameLookup.GetOrAdd(value, type =>
            {
                var shortName = type.GetShortAssemblyQualifiedName();
                _macroses.ForEach(x=>
                {
                    shortName = shortName.Replace(x.Key, x.Value);
                });

                var byteArr =new ByteArrayKey(Encoding.UTF8.GetBytes(shortName));

                _byteTypeNameLookup.TryAdd(byteArr, type); //add to reverse cache

                return byteArr.Bytes;
            });

            writer.Write(stringAsBytes);
        }

        T IMessagePackFormatter<T>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                return null;
            }

            var byteArr = new ByteArrayKey(reader.ReadBytes().Value.ToArray());

            return (T)_byteTypeNameLookup.GetOrAdd(byteArr, b =>
            {
                var typename = TypeEx.ToQualifiedAssemblyName(Encoding.UTF8.GetString(byteArr.Bytes));
                _macroses.ForEach(x =>
                {
                    typename = typename.Replace(x.Value, x.Key);
                });

                var type = Type.GetType(typename, true);

                _typeByteNameLookup.TryAdd(type, b.Bytes); //add to reverse cache

                return type;
            });
        }
    }
}
