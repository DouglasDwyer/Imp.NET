using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DouglasDwyer.Imp.Serialization
{
    /*public class BadImpSerializer
    {
        public Action<BinaryWriter, object> SerializeLocalSharedObject { get; set; }
        public Func<BinaryReader, object> DeserializeSharedObject { get; set; }

        private Dictionary<Type, Action<BinaryWriter, object>> PredefinedSerializers = new Dictionary<Type, Action<BinaryWriter, object>>();
        private Dictionary<Type, Func<BinaryReader, object>> PredefinedDeserializers = new Dictionary<Type, Func<BinaryReader, object>>();

        private List<Type> SerializerTypes = new List<Type>();

        public BadImpSerializer(IEnumerable<Type> predefinedList)
        {
            foreach(Type type in predefinedList)
            {
                SerializerTypes.Add(type);
                PredefinedSerializers[type] = CreateSerializerForType(type);
                PredefinedDeserializers[type] = CreateDeserializerForType(type);
            }
        }

        public byte[] Serialize(object obj)
        {
            using(MemoryStream stream = new MemoryStream())
            using(BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(SerializerTypes.IndexOf(obj.GetType()));
                PredefinedSerializers[obj.GetType()](writer, obj);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return (T)PredefinedDeserializers[SerializerTypes[reader.ReadInt32()]](reader);
            }
        }

        public Func<BinaryReader, object> CreateDeserializerForType(Type type)
        {
            if (type == typeof(byte))
            {
                return x => x.ReadByte();
            }
            else if (type == typeof(short))
            {
                return x => x.ReadInt16();
            }
            else if (type == typeof(int))
            {
                return x => x.ReadInt32();
            }
            else if (type == typeof(long))
            {
                return x => x.ReadInt64();
            }
            else if (type == typeof(sbyte))
            {
                return x => x.ReadSByte();
            }
            else if (type == typeof(ushort))
            {
                return x => x.ReadUInt16();
            }
            else if (type == typeof(uint))
            {
                return x => x.ReadUInt32();
            }
            else if (type == typeof(ulong))
            {
                return x => x.ReadUInt64();
            }
            else if (type == typeof(string))
            {
                return x => x.ReadString();
            }
            else if(type == typeof(RemoteException))
            {
                return x => new RemoteException(x.ReadString(), x.ReadString(), x.ReadString());
            }
            else if (type == typeof(SharedObjectPath))
            {
                return x => new SharedObjectPath(x.ReadUInt16(), x.ReadUInt16());
            }
            else if (BadProxyBinder.Instance.GetRemoteClass(type) != null)
            {
                return DeserializeSharedObject;
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                return x => {
                    Array array = Array.CreateInstance(elementType, x.ReadInt32());
                    for(int i = 0; i < array.Length; i++)
                    {
                        array.SetValue(CreateDeserializerForType(Type.GetType(x.ReadString()))(x), i);
                    }
                    return array;
                };
            }
            else
            {
                return x => {
                    if(x.ReadBoolean())
                    {
                        return null;
                    }
                    else
                    {
                        Type newType = Type.GetType(x.ReadString());
                        object obj = FormatterServices.GetUninitializedObject(newType);
                        foreach(FieldInfo info in newType.GetFields())
                        {
                            if(!x.ReadBoolean())
                            {
                                Type nT = info.FieldType;
                                if(x.ReadBoolean())
                                {
                                    nT = Type.GetType(x.ReadString());
                                }
                                info.SetValue(obj, CreateDeserializerForType(nT)(x));
                            }
                        }
                        return obj;
                    }
                };
            }
        }

        public Action<BinaryWriter, object> CreateSerializerForType(Type type)
        {
            if (type == typeof(byte))
            {
                return (x, y) => x.Write((byte)y);
            }
            else if (type == typeof(short))
            {
                return (x, y) => x.Write((short)y);
            }
            else if (type == typeof(int))
            {
                return (x, y) => x.Write((int)y);
            }
            else if (type == typeof(long))
            {
                return (x, y) => x.Write((long)y);
            }
            else if (type == typeof(sbyte))
            {
                return (x, y) => x.Write((sbyte)y);
            }
            else if (type == typeof(ushort))
            {
                return (x, y) => x.Write((ushort)y);
            }
            else if (type == typeof(uint))
            {
                return (x, y) => x.Write((uint)y);
            }
            else if (type == typeof(ulong))
            {
                return (x, y) => x.Write((ulong)y);
            }
            else if (type == typeof(string))
            {
                return (x, y) => x.Write((string)y);
            }
            else if (type == typeof(SharedObjectPath))
            {
                return (x, y) => { x.Write(((SharedObjectPath)y).OwnerID); x.Write(((SharedObjectPath)y).ObjectID); };
            }
            else if(type == typeof(RemoteException))
            {
                return (x, y) => {
                    RemoteException e = (RemoteException)y;
                    x.Write(e.Message);
                    x.Write(e.StackTrace);
                    x.Write(e.Source);
                };
            }
            else if(typeof(RemoteSharedObject).IsAssignableFrom(type))
            {
                Action<BinaryWriter, object> pathSerializer = CreateSerializerForType(typeof(SharedObjectPath));
                return (x, y) => { pathSerializer(x, ((RemoteSharedObject)y).ObjectID); x.Write(type.FullName); };
            }
            else if (ShareAsAttribute.SharedTypes.ContainsKey(type) || BadProxyBinder.Instance.GetRemoteClass(type) != null)
            {
                return SerializeLocalSharedObject;
            }
            else if(type.IsConstructedGenericType && ShareAsAttribute.SharedTypes.Any(x => x.Key == type.GetGenericTypeDefinition()))
            {
                return SerializeLocalSharedObject;
            }
            else if(type.IsArray)
            {
                return (x, y) => {
                    Array array = (Array)y;
                    x.Write(array.Length);
                    foreach(object obj in array)
                    {
                        Type oType = obj.GetType();
                        if (typeof(RemoteSharedObject).IsAssignableFrom(oType))
                        {
                            x.Write(BadProxyBinder.Instance.GetProxyInterface(oType).AssemblyQualifiedName);
                        }
                        else if(ShareAsAttribute.SharedTypes.ContainsKey(oType))
                        {
                            x.Write(ShareAsAttribute.SharedTypes[oType].AssemblyQualifiedName);
                        }
                        else
                        {
                            x.Write(obj.GetType().AssemblyQualifiedName);
                        }
                        CreateSerializerForType(obj.GetType())(x, obj);
                    }
                };
            }
            else
            {
                return (x, y) => {
                    if (y is null)
                    {
                        x.Write(true);
                    }
                    else {
                        x.Write(false);
                        Type yType = y.GetType();
                        x.Write(yType.AssemblyQualifiedName);
                        foreach (FieldInfo info in y.GetType().GetFields())
                        {
                            object value = info.GetValue(y);
                            if (value is null)
                            {
                                x.Write(true);
                            }
                            else
                            {
                                x.Write(false);
                                if (value.GetType().IsValueType)
                                {
                                    x.Write(true);
                                    x.Write(value.GetType().AssemblyQualifiedName);
                                }
                                else
                                {
                                    x.Write(false);
                                }
                                CreateSerializerForType(value.GetType())(x, value);
                            }
                        }
                    }
                };
            }
        }
    }*/
}
