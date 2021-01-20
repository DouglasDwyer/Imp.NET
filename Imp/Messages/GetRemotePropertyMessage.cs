using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class GetRemotePropertyMessage : ImpMessage
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
