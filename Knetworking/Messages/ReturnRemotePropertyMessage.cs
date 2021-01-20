using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class ReturnRemotePropertyMessage : KnetworkingMessage
    {
        public ushort OperatonID;
        public object Result;
        public RemoteException ExceptionResult;

        public ReturnRemotePropertyMessage(ushort operationID, object result, RemoteException exceptionResult)
        {
            OperatonID = operationID;
            Result = result;
            ExceptionResult = exceptionResult;
        }
    }
}
