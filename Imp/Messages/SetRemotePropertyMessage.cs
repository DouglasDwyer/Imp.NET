using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    internal class SetRemotePropertyMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public ushort PropertyID;
        public ushort OperationID;
        public object Value;

        public SetRemotePropertyMessage(ushort invocationTarget, ushort propertyID, object value, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyID = propertyID;
            Value = value;
            OperationID = operationID;
        }
    }
}
