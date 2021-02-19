using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DouglasDwyer.Imp;
using DouglasDwyer.Imp.Serialization;
using DouglasDwyer.Imp.Messages;
using System.Collections;

//[assembly: ShareAs(typeof(int[]), typeof(IList<int>))]

namespace TestProject
{
    public interface IChad
    {
        void Chadley(string i, object o, int k);
    }

    class Program
    {
        public static void Bob<T>() where T : new() { }

        static void Main(string[] args)
        {
            Console.WriteLine("Type S for server or press any other key for client.");
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                ChatServer server = new ChatServer(IPAddress.Any, 4000);
                server.Start();
                Console.ReadKey();
            }
            else
            {
                ChatClient client = new ChatClient();
                client.Connect("127.0.0.1", 4000);
                IChatServer server = client.Server;
                while(true)
                {
                    Task task = WaitSendM(server);
                    try
                    {
                        task.Wait();
                    }catch { }
                    Console.ReadKey();
                }
            }
        }

        public static async Task WaitSendM(IChatServer server)
        {
            await server.SendMessage(Console.ReadLine());
        }
    }

    [Shared]
    public partial class ImDed<T> { }

    [Shared]
    public partial class ChatServer : ImpServer<IChatClient>
    {
        public ChatServer(IPAddress binding, int port) : base(binding, port)
        {
        }

        public async Task SendMessage(string message, [CallingClient] IChatClient sender = null)
        {
            throw new Exception("your mother homo");
            SendMessageToEveryone("[" + sender.Name + "] " + message);            
        }

        protected override void OnClientConnected(IChatClient client)
        {
            SendMessageToEveryone("" + client.Name + " joined the chat.");
        }

        protected override void OnClientNetworkError(IChatClient client, Exception exception)
        {
            Console.WriteLine("Im having an aneurysm\n" + exception);
        }

        protected override void OnClientDisconnected(IChatClient client)
        {
            SendMessageToEveryone("Sombaby left the chat.");
        }

        private void SendMessageToEveryone(string message)
        {
            Console.WriteLine(message);
            foreach (IChatClient client in ConnectedClients)
            {
                client.ReceiveMessage(message);
            }
        }
    }

    [Shared]
    public partial class ChatClient : ImpClient<IChatServer>
    {
        public string Name { get; } = Guid.NewGuid().ToString();

        public async Task ReceiveMessage(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine("The client was disconnected.");
        }

        protected override void OnNetworkError(Exception e)
        {
            Console.WriteLine("A network error occured, resulting in disconnection:\n" + e);
        }

        [Unreliable]
        public void bob() { }
    }
}