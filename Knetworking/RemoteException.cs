using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking
{
    public class RemoteException : Exception
    {
        public override string StackTrace => RemoteErrorStackTrace + (string.IsNullOrEmpty(base.StackTrace) ? "" : "\n   --- End of Knetworking remote stack trace ---\n" + base.StackTrace);
        public override string Message => RemoteErrorMessage;
        private string RemoteErrorMessage;
        private string RemoteErrorStackTrace;

        public RemoteException(string remoteMessage, string remoteStackTrace)
        {
            RemoteErrorMessage = remoteMessage;
            RemoteErrorStackTrace = remoteStackTrace;
        }

        public RemoteException(string remoteMessage, string remoteStackTrace, string source) : this(remoteMessage, remoteStackTrace)
        {
            Source = source;
        }
    }
}