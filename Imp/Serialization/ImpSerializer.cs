using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace DouglasDwyer.Imp.Serialization
{
    public class ImpSerializer : ICloneable
    {
        public static IEnumerable<ImpSerializationRule> DefaultSerializationRuleset;

        public IdentifiedCollection<Type> SerializationTypes { get; }
        public IReadOnlyDictionary<ushort, ImpSerializationRule> SerializationRuleset { get; }

        static ImpSerializer()
        {
            DefaultSerializationRuleset = new List<ImpSerializationRule>()
            {
                new ImpSerializationRule(typeof(NullType), (s, x, y) => { }, (s, x) => null),
                new ImpSerializationRule(typeof(byte), (s, x, y) => x.Write((byte)y), (s, x) => x.ReadByte()),
                new ImpSerializationRule(typeof(short), (s, x, y) => x.Write((short)y), (s, x) => x.ReadInt16()),
                new ImpSerializationRule(typeof(int), (s, x, y) => x.Write((int)y), (s, x) => x.ReadInt32()),
                new ImpSerializationRule(typeof(long), (s, x, y) => x.Write((long)y), (s, x) => x.ReadInt64()),
                new ImpSerializationRule(typeof(sbyte), (s, x, y) => x.Write((sbyte)y), (s, x) => x.ReadSByte()),
                new ImpSerializationRule(typeof(ushort), (s, x, y) => x.Write((ushort)y), (s, x) => x.ReadUInt16()),
                new ImpSerializationRule(typeof(uint), (s, x, y) => x.Write((uint)y), (s, x) => x.ReadUInt32()),
                new ImpSerializationRule(typeof(ulong), (s, x, y) => x.Write((ulong)y), (s, x) => x.ReadUInt64()),
                new ImpSerializationRule(typeof(float), (s, x, y) => x.Write((float)y), (s, x) => x.ReadSingle()),
                new ImpSerializationRule(typeof(double), (s, x, y) => x.Write((double)y), (s, x) => x.ReadDouble()),
                new ImpSerializationRule(typeof(string), (s, x, y) => x.Write((string)y), (s, x) => x.ReadString()),
                new ImpSerializationRule(typeof(object), (s, x, y) => {
                    if(y is null) {
                        x.Write(s.SerializationTypes[typeof(NullType)]);
                    }
                    else {
                        Type type = y.GetType();
                        ushort id = s.SerializationTypes.ContainsValue(type) ? s.SerializationTypes[type] : ushort.MaxValue;
                        x.Write(id);
                        s.GetSerializerForType(id)(s, x, y);
                    }
                }, (s, x) => s.GetDeserializerForType(x.ReadUInt16())(s, x)),
                new ImpSerializationRule(typeof(byte[]), (s, x, y) => s.SerializeArray(x, (byte[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadByte())),
                new ImpSerializationRule(typeof(short[]), (s, x, y) => s.SerializeArray(x, (short[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadInt16())),
                new ImpSerializationRule(typeof(int[]), (s, x, y) => s.SerializeArray(x, (int[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadInt32())),
                new ImpSerializationRule(typeof(long[]), (s, x, y) => s.SerializeArray(x, (long[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadInt64())),
                new ImpSerializationRule(typeof(sbyte[]), (s, x, y) => s.SerializeArray(x, (sbyte[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadSByte())),
                new ImpSerializationRule(typeof(ushort[]), (s, x, y) => s.SerializeArray(x, (ushort[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadUInt16())),
                new ImpSerializationRule(typeof(uint[]), (s, x, y) => s.SerializeArray(x, (uint[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadUInt32())),
                new ImpSerializationRule(typeof(ulong[]), (s, x, y) => s.SerializeArray(x, (ulong[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadUInt64())),
                new ImpSerializationRule(typeof(float[]), (s, x, y) => s.SerializeArray(x, (float[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadSingle())),
                new ImpSerializationRule(typeof(double[]), (s, x, y) => s.SerializeArray(x, (double[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadDouble())),
                new ImpSerializationRule(typeof(string[]), (s, x, y) => s.SerializeArray(x, (string[])y, (a, b, c) => x.Write(c)), (s, x) => s.DeserializeArray(x, (a, b) => x.ReadString())),
                new ImpSerializationRule(typeof(object[]), (s, x, y) => s.SerializeArray(x, (object[])y, (a, b, c) => {
                    if(c is null) {
                        x.Write(s.SerializationTypes[typeof(NullType)]);
                    }
                    else {
                        Type type = c.GetType();
                        ushort id = a.SerializationTypes.ContainsValue(type) ? a.SerializationTypes[type] : ushort.MaxValue;
                        x.Write(id);
                        a.GetSerializerForType(id)(a, b, c);
                    }
                }), (s, x) => s.DeserializeArray(x, (a, b) => a.GetDeserializerForType(b.ReadUInt16())(a, b))),
                new ImpSerializationRule(typeof(RemoteException), (s, x, y) => {
                    if(y is null)
                    {
                        x.Write(true);
                    }
                    else {
                        x.Write(false);
                        RemoteException e = (RemoteException)y;
                        x.Write(e.Message);
                        x.Write(e.StackTrace);
                        x.Write(e.Source);
                    }
                }, (s, x) => x.ReadBoolean() ? null : new RemoteException(x.ReadString(), x.ReadString(), x.ReadString())),
            };
        }

        protected ImpSerializer(IdentifiedCollection<Type> types, IReadOnlyDictionary<ushort, ImpSerializationRule> rules)
        {
            SerializationTypes = types;
            SerializationRuleset = rules;
        }

        public ImpSerializer(IEnumerable<Type> compositeTypes) : this(compositeTypes, DefaultSerializationRuleset) { }

        public ImpSerializer(IEnumerable<Type> compositeTypes, IEnumerable<ImpSerializationRule> ruleset)
        {
            SerializationTypes = new IdentifiedCollection<Type>();
            Dictionary<ushort, ImpSerializationRule> rules = new Dictionary<ushort, ImpSerializationRule>();
            foreach(ImpSerializationRule rule in ruleset)
            {
                rules[SerializationTypes.Add(rule.TargetType)] = rule;
            }
            foreach (Type compositeType in compositeTypes)
            {
                Action<ImpSerializer, BinaryWriter, object> serializer = (s, x, y) => { };
                Func<ImpSerializer, BinaryReader, object> deserializer = (s, x) => FormatterServices.GetUninitializedObject(compositeType);
                foreach (FieldInfo field in compositeType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    Action<ImpSerializer, BinaryWriter, object> oldSerializer = serializer;
                    Action<ImpSerializer, BinaryWriter, object> fieldSerializer = rules[SerializationTypes[field.FieldType]].Serializer;
                    serializer = (s, x, y) => { oldSerializer(s, x, y); fieldSerializer(s, x, field.GetValue(y)); };

                    Func<ImpSerializer, BinaryReader, object> oldDeserializer = deserializer;
                    Func<ImpSerializer, BinaryReader, object> fieldDeserializer = rules[SerializationTypes[field.FieldType]].Deserializer;
                    deserializer = (s, x) => { object toReturn = oldDeserializer(s, x); field.SetValue(toReturn, fieldDeserializer(s, x)); return toReturn; };
                }

                rules[SerializationTypes.Add(compositeType)] = new ImpSerializationRule(compositeType, serializer, deserializer);
            }
            SerializationRuleset = new ReadOnlyDictionary<ushort, ImpSerializationRule>(rules);
        }

        public virtual byte[] Serialize(object toSerialize)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                ushort typeID = SerializationTypes[toSerialize.GetType()];
                writer.Write(typeID);
                SerializationRuleset[typeID].Serializer(this, writer, toSerialize);
                return stream.ToArray();
            }
        }

        public virtual (object, Type) Deserialize(byte[] toDeserialize)
        {
            using (MemoryStream stream = new MemoryStream(toDeserialize))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ushort typeID = reader.ReadUInt16();
                ImpSerializationRule rule = SerializationRuleset[typeID];
                return (rule.Deserializer(this, reader), rule.TargetType);
            }
        }

        public virtual T Deserialize<T>(byte[] toDeserialize)
        {
            return (T)Deserialize(toDeserialize).Item1;
        }

        protected virtual Action<ImpSerializer, BinaryWriter, object> GetSerializerForType(ushort typeID)
        {
            if (typeID == ushort.MaxValue)
            {
                return null;
            }
            else
            {
                return SerializationRuleset[typeID].Serializer;
            }
        }

        protected virtual Func<ImpSerializer, BinaryReader, object> GetDeserializerForType(ushort typeID)
        {
            if (typeID == ushort.MaxValue)
            {
                return null;
            }
            else
            {
                return SerializationRuleset[typeID].Deserializer;
            }
        }

        private void SerializeArray<T>(BinaryWriter writer, T[] array, Action<ImpSerializer, BinaryWriter, T> individualAction)
        {
            writer.Write(array.Length);
            foreach (T obj in array)
            {
                individualAction(this, writer, obj);
            }
        }

        private object DeserializeArray<T>(BinaryReader reader, Func<ImpSerializer, BinaryReader, T> individualAction)
        {
            T[] toReturn = new T[reader.ReadInt32()];
            for(int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = individualAction(this, reader);
            }
            return toReturn;
        }

        public virtual object Clone()
        {
            return new ImpSerializer(SerializationTypes, SerializationRuleset);
        }

        private sealed class NullType { }
    }
}
