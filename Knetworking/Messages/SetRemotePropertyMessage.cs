using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class SetRemotePropertyMessage : KnetworkingMessage
    {
        public SharedObjectPath InvocationTarget;
        public string PropertyName;
        public ushort OperationID;
        public object Value;

        public SetRemotePropertyMessage(SharedObjectPath invocationTarget, string methodName, object value, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyName = methodName;
            Value = value;
            OperationID = operationID;
        }
    }
}
