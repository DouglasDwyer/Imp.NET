using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class ReturnRemoteServerObjectMessage : ImpMessage
    {
        public object Server;

        public ReturnRemoteServerObjectMessage(IImpServer server)
        {
            Server = server;
        }
    }
}
