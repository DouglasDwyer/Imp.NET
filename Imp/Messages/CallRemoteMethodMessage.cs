using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class CallRemoteMethodMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public ushort MethodID;
        public ushort OperationID;
        public object[] Arguments;
        public Type[] GenericArguments;

        public CallRemoteMethodMessage(ushort invocationTarget, ushort methodID, object[] arguments, Type[] genericArguments, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            MethodID = methodID;
            Arguments = arguments;
            GenericArguments = genericArguments;
            OperationID = operationID;
        }
    }
}
