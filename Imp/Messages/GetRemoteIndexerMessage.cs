using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class GetRemoteIndexerMessage : ImpMessage
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
