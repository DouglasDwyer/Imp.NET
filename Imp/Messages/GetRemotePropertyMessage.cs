using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class GetRemotePropertyMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public string PropertyName;
        public ushort OperationID;

        public GetRemotePropertyMessage(ushort invocationTarget, string propertyName, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyName = propertyName;
            OperationID = operationID;
        }
    }
}
