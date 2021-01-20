using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class GetRemotePropertyMessage : KnetworkingMessage
    {
        public SharedObjectPath InvocationTarget;
        public string PropertyName;
        public ushort OperationID;

        public GetRemotePropertyMessage(SharedObjectPath invocationTarget, string propertyName, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyName = propertyName;
            OperationID = operationID;
        }
    }
}
