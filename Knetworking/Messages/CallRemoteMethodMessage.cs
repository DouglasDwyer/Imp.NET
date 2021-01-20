using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class CallRemoteMethodMessage : KnetworkingMessage
    {
        public SharedObjectPath InvocationTarget;
        public ushort MethodID;
        public ushort OperationID;
        public object[] Parameters;

        public CallRemoteMethodMessage(SharedObjectPath invocationTarget, ushort methodID, object[] parameters, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            MethodID = methodID;
            Parameters = parameters;
            OperationID = operationID;
        }
    }
}
