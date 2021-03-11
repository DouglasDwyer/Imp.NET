using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    internal class SetRemoteIndexerMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public ushort PropertyID;
        public ushort OperationID;
        public object Value;
        public object[] Arguments;

        public SetRemoteIndexerMessage(ushort invocationTarget, ushort propertyID, object value, object[] arguments, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyID = propertyID;
            Arguments = arguments;
            Value = value;
            OperationID = operationID;
        }
    }
}
