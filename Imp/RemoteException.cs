using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Represents an exception that has been thrown by a method/property call executing on a remote host.
    /// </summary>
    public class RemoteException : Exception
    {
        public override string StackTrace => RemoteErrorStackTrace + (string.IsNullOrEmpty(base.StackTrace) ? "" : "\n   --- End of Imp remote stack trace ---\n" + base.StackTrace);
        public override string Message => RemoteErrorMessage;
        private string RemoteErrorMessage;
        private string RemoteErrorStackTrace;

        /// <summary>
        /// Creates a new remote exception with the given message and stack trace from the remote host.
        /// </summary>
        /// <param name="remoteMessage">The original message, as thrown on the remote host.</param>
        /// <param name="remoteStackTrace">The original message, as thrown on the remote host.</param>
        public RemoteException(string remoteMessage, string remoteStackTrace)
        {
            RemoteErrorMessage = remoteMessage;
            RemoteErrorStackTrace = remoteStackTrace;
        }

        /// <summary>
        /// Creates a new remote exception with the given message and stack trace from the remote host.
        /// </summary>
        /// <param name="remoteMessage">The original message, as thrown on the remote host.</param>
        /// <param name="remoteStackTrace">The original message, as thrown on the remote host.</param>
        /// <param name="source">The name of the application or object that caused this error, as thrown on the remote host.</param>
        public RemoteException(string remoteMessage, string remoteStackTrace, string source) : this(remoteMessage, remoteStackTrace)
        {
            Source = source;
        }
    }
}