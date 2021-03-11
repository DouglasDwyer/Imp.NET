using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    internal class GetRemoteIndexerMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public ushort PropertyID;
        public ushort OperationID;
        public object[] Parameters;

        public GetRemoteIndexerMessage(ushort invocationTarget, ushort propertyID, object[] parameters, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyID = propertyID;
            Parameters = parameters;
            OperationID = operationID;
        }
    }
}
