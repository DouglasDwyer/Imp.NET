using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    using DouglasDwyer.PowerSerializer;
    using System.Collections.Immutable;
    using System.IO;

    public class ImpPowerSerializer : PowerSerializer
    {
        public ImpClient Client { get; set; }

        public ImpPowerSerializer() : base() { }

        public ImpPowerSerializer(ImpClient client) : this() {
            Client = client;
        }

        public ImpPowerSerializer(ITypeResolver resolver) : base(resolver) { }

        public ImpPowerSerializer(ImpClient client, ITypeResolver resolver) : base(resolver) {
            Client = client;
        }

        public override byte[] Serialize(object obj)
        {
            if (obj is null)
            {
                return base.Serialize(obj);
            }
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    PowerSerializationContext context = CreateSerializationContext();
                    Type objType = context.RegisterObject(obj).Item2;
                    CheckTypeAllowance(objType);
                    if (objType == typeof(string))
                    {
                        writer.Write((string)obj);
                        long pos = writer.BaseStream.Position;
                        TypeResolver.WriteTypeID(writer, objType);
                        writer.Write((int)(writer.BaseStream.Position - pos));
                    }
                    else if (obj is Type typeRepresentation)
                    {
                        TypeResolver.WriteTypeID(writer, typeRepresentation);
                        long pos = writer.BaseStream.Position;
                        TypeResolver.WriteTypeID(writer, typeof(Type));
                        writer.Write((int)(writer.BaseStream.Position - pos));
                    }
                    else if (objType.IsPrimitive)
                    {
                        WritePrimitiveObject(writer, obj);
                        long pos = writer.BaseStream.Position;
                        TypeResolver.WriteTypeID(writer, objType);
                        writer.Write((int)(writer.BaseStream.Position - pos));
                    }
                    else
                    {
                        if (!Client.SharedTypeBinder.IsSharedType(objType))
                        {
                            if (obj is Array array)
                            {
                                byte rankSize = (byte)array.Rank;
                                writer.Write(rankSize);
                                for (int i = 0; i < array.Rank; i++)
                                {
                                    writer.Write(array.GetLength(i));
                                }
                            }
                            for (int i = 1; i < context.ObjectGraph.Count; i++)
                            {
                                SerializeObject(context, writer, context.ObjectGraph[i], context.ObjectGraph[i].GetType());
                            }
                        }
                        else
                        {
                            WriteSharedType(obj, default);
                        }
                        ImmutableList<Type> types = context.IncludedTypes;
                        long pos = writer.BaseStream.Position;
                        foreach (Type type in types)
                        {
                            Type proxyType = Client.SharedTypeBinder.GetProxyForLocalType(type);
                            if (proxyType is null)
                            {
                                proxyType = Client.SharedTypeBinder.GetProxyForRemoteType(type);
                                if(proxyType is null)
                                {
                                    TypeResolver.WriteTypeID(writer, type);
                                }
                                else
                                {
                                    if(proxyType.IsGenericType)
                                    {
                                        proxyType = proxyType.MakeGenericType(type.GetGenericArguments());
                                    }
                                    TypeResolver.WriteTypeID(writer, proxyType);
                                }
                            }
                            else
                            {
                                if (proxyType.IsGenericType)
                                {
                                    proxyType = proxyType.MakeGenericType(type.GetGenericArguments());
                                }
                                TypeResolver.WriteTypeID(writer, proxyType);
                            }
                        }
                        writer.Write((int)(writer.BaseStream.Position - pos));
                    }
                    return stream.ToArray();
                }
            }
        }

        public override object Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    PowerDeserializationContext context = CreateDeserializationContext();
                    int dataLength = data.Length - 4;
                    stream.Position = dataLength;
                    int typeSize = reader.ReadInt32();
                    stream.Position = data.Length - typeSize - 4;

                    List<Type> knownTypes = new List<Type>();
                    while (reader.BaseStream.Position < dataLength)
                    {
                        Type type = TypeResolver.ReadTypeID(reader);
                        knownTypes.Add(type);
                    }
                    stream.Position = 0;
                    context.IncludedTypes = knownTypes;

                    object obj = ReadAndCreateObject(context, reader, knownTypes[0]);
                    for (int i = 1; i < context.ObjectGraph.Count; i++)
                    {
                        DeserializeObject(context, reader, context.ObjectGraph[i], context.ObjectGraph[i].GetType());
                    }
                    return ProcessObjectGraph(context);
                }
            }
        }

        protected override void SerializeObject(PowerSerializationContext context, BinaryWriter writer, object obj, Type type)
        {
            if (!Client.SharedTypeBinder.IsSharedType(type))
            {
                base.SerializeObject(context, writer, obj, type);
            }
        }

        protected override void DeserializeObject(PowerDeserializationContext context, BinaryReader reader, object obj, Type type)
        {
            if (!Client.SharedTypeBinder.IsSharedType(type))
            {
                base.DeserializeObject(context, reader, obj, type);
            }
        }

        protected override void WriteObjectReference(PowerSerializationContext context, BinaryWriter writer, object obj)
        {
            if (obj is null)
            {
                writer.Write((ushort)0);
            }
            else if (context.HasObject(obj))
            {
                writer.Write(context.GetObjectID(obj));
            }
            else if (obj is Type typeRepresentation)
            {
                (ushort, Type) objectData = context.RegisterObject(obj);
                writer.Write(objectData.Item1);
                writer.Write(context.GetTypeID(typeof(Type)));
                TypeResolver.WriteTypeID(writer, typeRepresentation);
            }
            else
            {
                (ushort, Type) objectData = context.RegisterObject(obj);
                writer.Write(objectData.Item1);
                writer.Write(context.GetTypeID(objectData.Item2));

                if(Client.SharedTypeBinder.IsSharedType(objectData.Item2))
                {
                    if (obj is RemoteSharedObject rem && rem.HostClient == Client)
                    {
                        writer.Write(true);
                        writer.Write(rem.ObjectID);
                    }
                    else
                    {
                        writer.Write(false);
                        writer.Write(Client.GetOrRegisterLocalSharedObject(obj));
                    }
                }
                else if (obj is string oString)
                {
                    writer.Write(oString);
                }
                else if (objectData.Item2.IsPrimitive)
                {
                    WritePrimitiveObject(writer, obj);
                }
                else if (obj is Array array)
                {
                    byte rankSize = (byte)array.Rank;
                    writer.Write(rankSize);
                    for (int i = 0; i < array.Rank; i++)
                    {
                        writer.Write(array.GetLength(i));
                    }
                }
            }
        }

        protected override object ReadAndCreateObject(PowerDeserializationContext context, BinaryReader reader, Type type)
        {
            if (Client.SharedTypeBinder.IsSharedType(type))
            {
                if(reader.ReadBoolean())
                {
                    return Client.RetrieveLocalSharedObject(reader.ReadUInt16());
                }
                else
                {
                    return Client.GetOrCreateRemoteSharedObject(reader.ReadUInt16(), type);
                }
            }
            else
            {
                return base.ReadAndCreateObject(context, reader, type);
            }
        }

        protected override void CheckTypeAllowance(Type type)
        {
            if (!Client.SharedTypeBinder.IsSharedType(type))
            {
                base.CheckTypeAllowance(type);
            }
        }

        private void WriteSharedType(object obj, ushort id)
        {
            throw new NotImplementedException();
        }
    }
}
