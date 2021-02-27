using DouglasDwyer.Imp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{

    [Shared]
    public partial class Server : ImpServer<IClient>
    {
        private int UnreliableCount = 10000;

        public Server() : base(IPAddress.Any, 40572) { }

        [Local]
        public async Task RunTests()
        {
            Start();
            Client clientee = new Client();
            await clientee.ConnectAsync(IPAddress.Loopback, 40572);
            while(ConnectedClients.Count == 0) { }
            IClient client = ConnectedClients[0];
            int i = new Random().Next();
            Debug.Assert(client.ReturnNumber(i) == i);
            Debug.Assert(client.Return(i) == i);
            Debug.Assert(await client.ReturnString("bob") == "bob");
            BORT bee = new BORT(i);
            Debug.Assert(await client.ReturnAsync(bee) == bee);
            Debug.Assert(await client.IsClientCalled());
            
            try
            {
                client.JOB(ref i);
                Debug.Assert(false);
            }
            catch { }
            try
            {
                client.YEET(ref i);
                Debug.Assert(false);
            }
            catch { }
            try
            {
                client.tester();
                Debug.Assert(false);
            }
            catch { }

            client.CallServerUnreliably();
            while(UnreliableCount > 0) { }
            Console.WriteLine("all tests pass.");
            Console.ReadKey();
        }

        [Unreliable]
        public void ReceiveUnreliableMessage([CallingClient] IClient client = null)
        {
            if(UnreliableCount > 0)
            {
                UnreliableCount--;
                client.CallServerUnreliably();
            }
        }

        public async Task<bool> IsClientCalled([CallingClient] IClient client = null)
        {
            return client != null && client is RemoteSharedObject;
        }
    }

    [Shared]
    public partial class Client : ImpClient<IServer>
    {
        public int ReturnNumber(int i)
        {
            return i;
        }

        public T Return<T>(T toReturn)
        {
            return toReturn;
        }

        public async Task<string> ReturnString(string str)
        {
            return str;
        }

        public async Task<T> ReturnAsync<T>(T toReturn)
        {
            return toReturn;
        }

        public async Task<bool> IsClientCalled([CallingClient] IClient client = null)
        {
            return await Server.IsClientCalled();
        }

        public ref int JOB(ref int input)
        {
            return ref input;
        }

        private int yert = 0;

        public ref int tester()
        {
            return ref yert;
        }

        public int YEET(ref int i)
        {
            return i;
        }

        [Unreliable]
        public void CallServerUnreliably()
        {
            Server.ReceiveUnreliableMessage();
        }
    }

    public struct BORT
    {
        public int I;

        public BORT(int i)
        {
            I = i;
        }

        public static bool operator ==(BORT a, BORT b)
        {
            return a.I == b.I;
        }

        public static bool operator !=(BORT a, BORT b)
        {
            return !(a == b);
        }
    }
}
