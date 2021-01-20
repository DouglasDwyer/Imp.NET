using System;
using System.Collections.Generic;
using System.IO;
using DouglasDwyer.Imp.Messages;
using DouglasDwyer.Imp.Serialization;

namespace DouglasDwyer.Imp
{
    public class ImpClientSerializer : ImpSerializer
    {
        public ImpClient Client { get; set; }

        private ImpClientSerializer(IdentifiedCollection<Type> types, IReadOnlyDictionary<ushort, ImpSerializationRule> rules, ImpClient client) : base(types, rules)
        {
            Client = client;
        }

        public ImpClientSerializer(ImpClient client) : base(new[] {
            typeof(SharedObjectPath),
            typeof(CallRemoteMethodMessage),
            typeof(GetRemoteServerObjectMessage),
            typeof(ReturnRemoteMethodMessage),
            typeof(ReturnRemoteServerObjectMessage),
            typeof(GetRemotePropertyMessage),
            typeof(ReturnRemotePropertyMessage),
            typeof(GetRemoteIndexerMessage),
            typeof(ReturnRemoteIndexerMessage),
            typeof(SetRemotePropertyMessage) })
        {
            Client = client;
        }

        public ImpClientSerializer(IEnumerable<Type> compositeTypes) : base(compositeTypes)
        {
        }

        public ImpClientSerializer(IEnumerable<Type> compositeTypes, IEnumerable<ImpSerializationRule> ruleset) : base(compositeTypes, ruleset)
        {
        }

        protected override Action<ImpSerializer, BinaryWriter, object> GetSerializerForType(ushort typeID)
        {
            Action<ImpSerializer, BinaryWriter, object> baseSerializer = base.GetSerializerForType(typeID);
            if (typeID == ushort.MaxValue)
            {
                return (s, x, y) =>
                {
                    if(!Client.SerializeSharedObject(s, x, y)) {
                        baseSerializer(s, x, y);
                    }
                };
            }
            else
            {
                return baseSerializer;
            }
        }

        protected override Func<ImpSerializer, BinaryReader, object> GetDeserializerForType(ushort typeID)
        {

            Func<ImpSerializer, BinaryReader, object> baseSerializer = base.GetDeserializerForType(typeID);
            if (typeID == ushort.MaxValue)
            {
                return (s, x) =>
                {
                    if (x.ReadBoolean())
                    {
                        return Client.DeserializeSharedObject(s, x);
                    }
                    else
                    {
                        return baseSerializer(s, x);
                    }
                };
            }
            else
            {
                return baseSerializer;
            }
        }

        public override object Clone()
        {
            return new ImpClientSerializer(SerializationTypes, SerializationRuleset, Client);
        }
    }
}
