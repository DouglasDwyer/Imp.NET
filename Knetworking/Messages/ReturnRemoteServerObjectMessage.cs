using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Knetworking.Messages
{
    public class ReturnRemoteServerObjectMessage : KnetworkingMessage
    {
        public object Server;

        public ReturnRemoteServerObjectMessage(IKnetworkingServer server)
        {
            Server = server;
        }
    }
}
