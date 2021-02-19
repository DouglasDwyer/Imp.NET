using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class GetRemoteIndexerMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public string PropertyName;
        public ushort OperationID;
        public object[] Parameters;

        public GetRemoteIndexerMessage(ushort invocationTarget, string propertyName, object[] parameters, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyName = propertyName;
            Parameters = parameters;
            OperationID = operationID;
        }
    }
}
