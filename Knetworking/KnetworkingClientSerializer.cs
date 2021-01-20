using System;
using System.Collections.Generic;
using System.IO;
using DouglasDwyer.Knetworking.Messages;
using DouglasDwyer.Knetworking.Serialization;

namespace DouglasDwyer.Knetworking
{
    public class KnetworkingClientSerializer : KnetworkingSerializer
    {
        public KnetworkingClient Client { get; set; }

        private KnetworkingClientSerializer(IdentifiedCollection<Type> types, IReadOnlyDictionary<ushort, KnetworkingSerializationRule> rules, KnetworkingClient client) : base(types, rules)
        {
            Client = client;
        }

        public KnetworkingClientSerializer(KnetworkingClient client) : base(new[] {
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

        public KnetworkingClientSerializer(IEnumerable<Type> compositeTypes) : base(compositeTypes)
        {
        }

        public KnetworkingClientSerializer(IEnumerable<Type> compositeTypes, IEnumerable<KnetworkingSerializationRule> ruleset) : base(compositeTypes, ruleset)
        {
        }

        protected override Action<KnetworkingSerializer, BinaryWriter, object> GetSerializerForType(ushort typeID)
        {
            Action<KnetworkingSerializer, BinaryWriter, object> baseSerializer = base.GetSerializerForType(typeID);
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

        protected override Func<KnetworkingSerializer, BinaryReader, object> GetDeserializerForType(ushort typeID)
        {

            Func<KnetworkingSerializer, BinaryReader, object> baseSerializer = base.GetDeserializerForType(typeID);
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
            return new KnetworkingClientSerializer(SerializationTypes, SerializationRuleset, Client);
        }
    }
}
