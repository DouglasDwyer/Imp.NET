using System;

namespace DouglasDwyer.Imp.Messages
{
    internal class CallRemoteUnreliableMethodMessage : ImpMessage
    {
        public ushort ObjectID;
        public object[] Parameters;
        public Type[] GenericArguments;
        public ushort MethodID;

        public CallRemoteUnreliableMethodMessage(ushort obj, object[] arguments, Type[] genericArguments, ushort methodID)
        {
            ObjectID = obj;
            Parameters = arguments;
            GenericArguments = genericArguments;
            MethodID = methodID;
        }
    }
}