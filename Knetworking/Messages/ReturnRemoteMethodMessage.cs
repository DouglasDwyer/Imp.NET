using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class ReturnRemoteMethodMessage : KnetworkingMessage
    {
        public ushort OperatonID;
        public object Result;
        public RemoteException ExceptionResult;

        public ReturnRemoteMethodMessage(ushort operationID, object result, RemoteException exceptionResult)
        {
            OperatonID = operationID;
            Result = result;
            ExceptionResult = exceptionResult;
        }
    }
}
