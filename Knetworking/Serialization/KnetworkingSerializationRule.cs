using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DouglasDwyer.Knetworking.Serialization
{
    public sealed class KnetworkingSerializationRule
    {
        public Type TargetType { get; }
        public Action<KnetworkingSerializer, BinaryWriter, object> Serializer { get; }
        public Func<KnetworkingSerializer, BinaryReader, object> Deserializer { get; }

        public KnetworkingSerializationRule(Type type, Action<KnetworkingSerializer, BinaryWriter, object> serializer, Func<KnetworkingSerializer, BinaryReader, object> deserializer)
        {
            TargetType = type;
            Serializer = serializer;
            Deserializer = deserializer;
        }
    }
}
