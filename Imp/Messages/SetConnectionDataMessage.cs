using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class SetConnectionDataMessage : ImpMessage
    {
        public ushort UnreliablePort;
        public string[] Interfaces;

        public SetConnectionDataMessage(ushort unreliablePort, string[] interfaces)
        {
            UnreliablePort = unreliablePort;
            Interfaces = interfaces;
        }
    }
}
