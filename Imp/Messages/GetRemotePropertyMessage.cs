﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    internal class GetRemotePropertyMessage : ImpMessage
    {
        public ushort InvocationTarget;
        public ushort PropertyID;
        public ushort OperationID;

        public GetRemotePropertyMessage(ushort invocationTarget, ushort propertyID, ushort operationID)
        {
            InvocationTarget = invocationTarget;
            PropertyID = propertyID;
            OperationID = operationID;
        }
    }
}
