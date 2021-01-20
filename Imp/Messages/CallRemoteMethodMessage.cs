using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class CallRemoteMethodMessage : ImpMessage
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
