using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DouglasDwyer.Imp
{
    internal class CallingClientMethodInvoker : RemoteMethodInvoker
    {
        public int ClientArgumentLocation { get; }

        public CallingClientMethodInvoker(MethodInfo method, int argumentLocation) : base(method) {
            ClientArgumentLocation = argumentLocation;
        }

        public override Task<object> Invoke(IImpClient client, ImpClient caller, object target, object[] args, Type[] genericArguments)
        {
            if (client is RemoteSharedObject rem)
            {
                args[ClientArgumentLocation] = client;
            }
            return base.Invoke(client, caller, target, args, genericArguments);
        }
    }
}
