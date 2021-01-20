using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class ReturnRemoteIndexerMessage : ImpMessage
    {
        public ushort OperatonID;
        public object Result;
        public RemoteException ExceptionResult;

        public ReturnRemoteIndexerMessage(ushort operationID, object result, RemoteException exceptionResult)
        {
            OperatonID = operationID;
            Result = result;
            ExceptionResult = exceptionResult;
        }
    }
}
