using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class GetRemoteIndexerMessage : KnetworkingMessage
    {
        public SharedObjectPath InvocationTarget;
        public string PropertyName;
        public ushort OperationID;
        public object[] Parameters;

        public GetRemoteIndexerMessage(SharedObjectPath invocationTarget, string propertyName, object[] parameters, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyName = propertyName;
            Parameters = parameters;
            OperationID = operationID;
        }
    }
}
