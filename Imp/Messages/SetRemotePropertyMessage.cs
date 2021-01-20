using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class SetRemotePropertyMessage : ImpMessage
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
