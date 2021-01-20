using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DouglasDwyer.Imp.Serialization
{
    public sealed class ImpSerializationRule
    {
        public Type TargetType { get; }
        public Action<ImpSerializer, BinaryWriter, object> Serializer { get; }
        public Func<ImpSerializer, BinaryReader, object> Deserializer { get; }

        public ImpSerializationRule(Type type, Action<ImpSerializer, BinaryWriter, object> serializer, Func<ImpSerializer, BinaryReader, object> deserializer)
        {
            TargetType = type;
            Serializer = serializer;
            Deserializer = deserializer;
        }
    }
}
