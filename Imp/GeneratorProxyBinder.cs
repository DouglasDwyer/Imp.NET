using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DouglasDwyer.Imp
{
    public abstract class GeneratorProxyBinder : IProxyBinder
    {
        private static readonly ConstructorInfo RemoteSharedObjectConstructor;
        private static readonly MethodInfo RemoteSharedObjectHostGetterMethod;
        private static readonly FieldInfo RemoteSharedObjectLocationField;
        private static readonly MethodInfo ImpClientGetRemotePropertyMethod;
        private static readonly MethodInfo ImpClientSetRemotePropertyMethod;
        private static readonly MethodInfo ImpClientGetRemoteIndexerMethod;
        private static readonly MethodInfo ImpClientSetRemoteIndexerMethod;
        private static readonly MethodInfo ImpClientCallRemoteMethod;
        private static readonly MethodInfo ImpClientCallRemoteMethodAsync;

        protected IdentifiedCollection<Type> ProxyIndex = new IdentifiedCollection<Type>();
        protected List<ProxyType> ProxyData = new List<ProxyType>();
        protected Dictionary<Type, Type> LocalClassToInterface = new Dictionary<Type, Type>();
        protected Dictionary<Type, Type> RemoteClassToInterface = new Dictionary<Type, Type>();
        protected Dictionary<Type, Type> RemoteInterfaceToClass = new Dictionary<Type, Type>();

        static GeneratorProxyBinder()
        {
            Type RSO = typeof(RemoteSharedObject);
            Type KC = typeof(ImpClient);
            RemoteSharedObjectConstructor = RSO.GetConstructor(new[] { typeof(SharedObjectPath), typeof(ImpClient) });
            RemoteSharedObjectHostGetterMethod = RSO.GetProperty("HostClient").GetMethod;
            RemoteSharedObjectLocationField = RSO.GetField("Location");
            ImpClientGetRemotePropertyMethod = KC.GetMethod("GetRemoteProperty");
            ImpClientSetRemotePropertyMethod = KC.GetMethod("SetRemoteProperty");
            ImpClientGetRemoteIndexerMethod = KC.GetMethod("GetRemoteIndexer");
            ImpClientSetRemoteIndexerMethod = KC.GetMethod("SetRemoteIndexer");
            ImpClientCallRemoteMethod = KC.GetMethod("CallRemoteMethod");
            ImpClientCallRemoteMethodAsync = KC.GetMethod("CallRemoteMethodAsync");
        }

        public virtual ProxyType GetDataForProxy(ushort id)
        {
            return ProxyData[id];
        }

        public virtual ProxyType GetDataForProxy(Type proxyInterface)
        {
            return ProxyData[ProxyIndex[proxyInterface]];
        }

        public virtual ProxyType GetDataForSharedType(Type sharedType)
        {
            if (typeof(RemoteSharedObject).IsAssignableFrom(sharedType))
            {
                return ProxyData[GetIDForRemoteType(sharedType).Value];
            }
            else
            {
                return ProxyData[GetIDForLocalType(sharedType).Value];
            }
        }

        public virtual ushort? GetIDForLocalType(Type localType)
        {
            if (LocalClassToInterface.ContainsKey(localType))
            {
                Type proxyType = LocalClassToInterface[localType];
                if (ProxyIndex.ContainsValue(proxyType))
                {
                    return ProxyIndex[proxyType];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public virtual ushort? GetIDForProxy(Type proxyInterface)
        {
            return ProxyIndex.ContainsValue(proxyInterface) ? (ushort?)ProxyIndex[proxyInterface] : null;
        }

        public virtual ushort? GetIDForRemoteType(Type remoteType)
        {
            if (RemoteClassToInterface.ContainsKey(remoteType))
            {
                Type proxyType = RemoteClassToInterface[remoteType];
                if (ProxyIndex.ContainsValue(proxyType))
                {
                    return ProxyIndex[proxyType];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public virtual ushort? GetIDForSharedType(Type sharedType)
        {
            if (typeof(RemoteSharedObject).IsAssignableFrom(sharedType))
            {
                return GetIDForRemoteType(sharedType);
            }
            else
            {
                return GetIDForLocalType(sharedType);
            }
        }

        public virtual Type GetProxyForLocalType(Type localType)
        {
            return LocalClassToInterface[localType];
        }

        public virtual Type GetProxyForRemoteType(Type remoteClass)
        {
            return RemoteClassToInterface[remoteClass];
        }

        public virtual Type GetRemoteType(ushort id)
        {
            return RemoteInterfaceToClass[ProxyIndex[id]];
        }

        public virtual Type GetRemoteType(Type proxyInterface)
        {
            return RemoteInterfaceToClass[proxyInterface];
        }

        public virtual IEnumerable<Type> GetProxyTypes()
        {
            return ProxyIndex.Values;
        }

        #region Proxy generator methods

        protected void GenerateProxies(IEnumerable<Type> proxyTypes)
        {
            AssemblyName assembly = new AssemblyName("DouglasDwyer.Imp.Proxy." + Guid.NewGuid().ToString("N"));
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
            ModuleBuilder module = assemblyBuilder.DefineDynamicModule(assembly.Name);
            foreach (Type proxy in proxyTypes)
            {
                RemoteClassToInterface[RemoteInterfaceToClass[proxy] = GenerateRemoteTypeForInterface(module, proxy).CreateTypeInfo().AsType()] = proxy;
            }
        }

        private TypeBuilder GenerateRemoteTypeForInterface(ModuleBuilder module, Type proxy)
        {
            ProxyType proxyData = ProxyData[ProxyIndex[proxy]];
            TypeBuilder builder = module.DefineType("Remote" + proxy.Name + proxy.GUID.ToString("N"), TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic, typeof(RemoteSharedObject), new[] { proxy });
            GenerateConstructor(builder);
            for (int i = 0; i < proxyData.Properties.Count; i++)
            {
                GenerateProperty(builder, proxyData.Properties[i], i, true);
            }
            for (int i = 0; i < proxyData.Methods.Count; i++)
            {
                GenerateMethod(builder, proxyData.Methods[i], i, true);
            }
            return builder;
        }

        private ConstructorBuilder GenerateConstructor(TypeBuilder type)
        {
            ParameterInfo[] info = RemoteSharedObjectConstructor.GetParameters();
            ConstructorBuilder builder = type.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, info.Select(x => x.ParameterType).ToArray());
            ILGenerator generator = builder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            for (int i = 0; i < info.Length; i++)
            {
                generator.Emit(OpCodes.Ldarg, i + 1);
            }
            generator.Emit(OpCodes.Call, RemoteSharedObjectConstructor);
            generator.Emit(OpCodes.Ret);
            return builder;
        }

        private MethodBuilder GenerateMethod(TypeBuilder type, MethodInfo info, int id, bool explicitDefinition)
        {
            ParameterInfo[] parameters = info.GetParameters();
            MethodBuilder builder = type.DefineMethod(explicitDefinition ? GetFriendlyName(info.DeclaringType) + "." + info.Name : info.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, info.ReturnType, parameters.Select(x => x.ParameterType).ToArray());
            ILGenerator generator = builder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, RemoteSharedObjectHostGetterMethod);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, RemoteSharedObjectLocationField);
            generator.Emit(OpCodes.Ldc_I4, parameters.Length);
            generator.Emit(OpCodes.Newarr, typeof(object));
            for (int i = 0; i < parameters.Length; i++)
            {
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldarg, i + 1);
                if (parameters[i].ParameterType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, parameters[i].ParameterType);
                }
                generator.Emit(OpCodes.Stelem_Ref);
            }
            generator.Emit(OpCodes.Ldc_I4, id);
            if (info.ReturnType == typeof(Task))
            {
                generator.Emit(OpCodes.Callvirt, ImpClientCallRemoteMethodAsync.MakeGenericMethod(typeof(object)));
            }
            else if (info.ReturnType.IsGenericType && info.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                generator.Emit(OpCodes.Callvirt, ImpClientCallRemoteMethodAsync.MakeGenericMethod(info.ReturnType.GenericTypeArguments[0]));
            }
            else if (info.ReturnType == typeof(void))
            {
                generator.Emit(OpCodes.Callvirt, ImpClientCallRemoteMethod.MakeGenericMethod(typeof(object)));
                generator.Emit(OpCodes.Pop);
            }
            else
            {
                generator.Emit(OpCodes.Callvirt, ImpClientCallRemoteMethod.MakeGenericMethod(info.ReturnType));
            }
            generator.Emit(OpCodes.Ret);
            if (explicitDefinition)
            {
                type.DefineMethodOverride(builder, info);
            }
            return builder;
        }

        private List<MethodInfo> GenerateProperty(TypeBuilder type, PropertyInfo info, int id, bool explicitDefinition)
        {
            PropertyBuilder builder = type.DefineProperty(explicitDefinition ? GetFriendlyName(info.DeclaringType) + "." + info.Name : info.Name, PropertyAttributes.None, info.PropertyType, info.GetIndexParameters().Select(x => x.ParameterType).ToArray());
            List<MethodInfo> toReturn = new List<MethodInfo>();
            ParameterInfo[] parameters = info.GetIndexParameters();
            if (info.CanRead)
            {
                if (parameters.Length > 0)
                {

                }
                else
                {
                    toReturn.Add(info.GetMethod);
                    builder.SetGetMethod(GenerateGetterMethod(type, info.Name, info.GetMethod, explicitDefinition));
                }
            }
            if (info.CanWrite)
            {
                if (parameters.Length > 0)
                {

                }
                else
                {
                    toReturn.Add(info.GetMethod);
                    builder.SetSetMethod(GenerateSetterMethod(type, info.Name, info.SetMethod, explicitDefinition));
                }
            }
            return toReturn;
        }

        private MethodBuilder GenerateGetIndexerMethod(TypeBuilder type, string propertyName, MethodInfo getter)
        {
            MethodBuilder method = type.DefineMethod(getter.Name, MethodAttributes.Private | MethodAttributes.Virtual, getter.ReturnType, getter.GetParameters().Select(x => x.ParameterType).ToArray());
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, RemoteSharedObjectHostGetterMethod);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, RemoteSharedObjectLocationField);
            generator.Emit(OpCodes.Ldstr, propertyName);
            generator.Emit(OpCodes.Callvirt, ImpClientGetRemoteIndexerMethod.MakeGenericMethod(getter.ReturnType));
            generator.Emit(OpCodes.Ret);
            return method;
        }

        private MethodBuilder GenerateSetIndexerMethod(TypeBuilder type, string propertyName, MethodInfo getter)
        {
            MethodBuilder method = type.DefineMethod(getter.Name, MethodAttributes.Private | MethodAttributes.Virtual, getter.ReturnType, getter.GetParameters().Select(x => x.ParameterType).ToArray());
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, RemoteSharedObjectHostGetterMethod);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, RemoteSharedObjectLocationField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldstr, propertyName);
            generator.Emit(OpCodes.Callvirt, ImpClientSetRemotePropertyMethod.MakeGenericMethod(getter.ReturnType));
            generator.Emit(OpCodes.Ret);
            return method;
        }

        private MethodBuilder GenerateGetterMethod(TypeBuilder type, string propertyName, MethodInfo getter, bool explicitDefinition)
        {
            MethodBuilder method = type.DefineMethod(explicitDefinition ? GetFriendlyName(getter.DeclaringType) + "." + getter.Name : getter.Name, MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual, getter.ReturnType, getter.GetParameters().Select(x => x.ParameterType).ToArray());
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, RemoteSharedObjectHostGetterMethod);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, RemoteSharedObjectLocationField);
            generator.Emit(OpCodes.Ldstr, propertyName);
            generator.Emit(OpCodes.Callvirt, ImpClientGetRemotePropertyMethod.MakeGenericMethod(getter.ReturnType));
            generator.Emit(OpCodes.Ret);
            if (explicitDefinition)
            {
                type.DefineMethodOverride(method, getter);
            }
            return method;
        }

        private MethodBuilder GenerateSetterMethod(TypeBuilder type, string propertyName, MethodInfo setter, bool explicitDefinition)
        {
            MethodBuilder method = type.DefineMethod(explicitDefinition ? GetFriendlyName(setter.DeclaringType) + "." + setter.Name : setter.Name, MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.Virtual, setter.ReturnType, setter.GetParameters().Select(x => x.ParameterType).ToArray());
            ILGenerator generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, RemoteSharedObjectHostGetterMethod);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, RemoteSharedObjectLocationField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldstr, propertyName);
            generator.Emit(OpCodes.Callvirt, ImpClientSetRemotePropertyMethod.MakeGenericMethod(setter.GetParameters()[0].ParameterType));
            generator.Emit(OpCodes.Ret);
            if (explicitDefinition)
            {
                type.DefineMethodOverride(method, setter);
            }
            return method;
        }

        private string GetFriendlyName(Type type)
        {
            string friendlyName = type.FullName;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }

        #endregion
    }
}
