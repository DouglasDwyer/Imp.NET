using System;
using System.Collections.Generic;
using System.Text;

namespace DouglasDwyer.Imp.Messages
{
    public class SetProxyBinderMessage : ImpMessage
    {
        public string[] Interfaces;

        public SetProxyBinderMessage(string[] interfaces)
        {
            Interfaces = interfaces;
        }
    }
}
