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

        public ImpPowerSerializer(ImpClient client) : base() {
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
                    else if (objType.IsPrimitive)
                    {
                        WritePrimitiveObject(writer, obj);
                        long pos = writer.BaseStream.Position;
                        TypeResolver.WriteTypeID(writer, objType);
                        writer.Write((int)(writer.BaseStream.Position - pos));
                    }
                    else
                    {
                        ushort? sharedID = Client.SharedTypeBinder.GetIDForSharedType(objType);
                        if (sharedID is null)
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
                            WriteSharedType(obj, sharedID.Value);
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
                                    TypeResolver.WriteTypeID(writer, proxyType);
                                    writer.Write(true);
                                }
                            }
                            else
                            {
                                TypeResolver.WriteTypeID(writer, proxyType);
                                writer.Write(false);
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
                        if(type.IsInterface)
                        {
                            bool isLocal = reader.ReadBoolean();
                            if(isLocal)
                            {
                                knownTypes.Add(type);
                            }
                            else
                            {
                                knownTypes.Add(Client.SharedTypeBinder.GetRemoteType(type));
                            }
                        }
                        else
                        {
                            knownTypes.Add(type);
                        }
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
            ushort? id = Client.SharedTypeBinder.GetIDForSharedType(type);
            if (id is null)
            {
                base.SerializeObject(context, writer, obj, type);
            }
        }

        protected override void DeserializeObject(PowerDeserializationContext context, BinaryReader reader, object obj, Type type)
        {
            ushort? id = Client.SharedTypeBinder.GetIDForSharedType(type);
            if (id is null)
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
            else
            {
                (ushort, Type) objectData = context.RegisterObject(obj);
                writer.Write(objectData.Item1);
                writer.Write(context.GetTypeID(objectData.Item2));

                ushort? id = Client.SharedTypeBinder.GetIDForSharedType(objectData.Item2);
                if(id != null)
                {
                    if (obj is RemoteSharedObject rem)
                    {
                        writer.Write(rem.ObjectID);
                    }
                    else
                    {
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
            ushort? id = Client.SharedTypeBinder.GetIDForSharedType(type);
            if (id != null)
            {
                if(type.IsInterface)
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

        private void WriteSharedType(object obj, ushort id)
        {
            throw new NotImplementedException();
        }
    }
}
