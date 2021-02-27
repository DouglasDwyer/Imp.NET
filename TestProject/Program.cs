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
using System.Runtime.InteropServices;

//[assembly: ShareAs(typeof(int[]), typeof(IList<int>))]

namespace TestProject
{
    public static class ConsoleWriter
    {
        private static bool IsReading = false;
        private static object Locker = new object();

        public static void WriteLine(string text)
        {
            lock(Locker)
            {
                if (IsReading)
                {
                    int cursorLeft = Console.CursorLeft;
                    Console.MoveBufferArea(0, Console.CursorTop, Console.BufferWidth, 1, 0, Console.CursorTop + 1);
                    Console.CursorLeft = 0;
                    Console.WriteLine(text);
                    Console.CursorLeft = cursorLeft;
                }
                else
                {
                    Console.WriteLine(text);
                }
            }
        }

        public static string ReadLine()
        {
            lock (Locker)
            {
                IsReading = true;
            }
            string toReturn = Console.ReadLine();
            lock (Locker)
            {
                Console.CursorTop--;
                ClearCurrentConsoleLine();
                IsReading = false;
            }
            return toReturn;
        }

        private static bool CheckIsReading() { lock(Locker) { return IsReading; } }

        private static void ClearCurrentConsoleLine()
        {
            lock (Locker)
            {
                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Server sewer = new Server();
            sewer.RunTests().Wait();
            return;
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
                ConsoleWriter.WriteLine("Please enter your name.");
                client.Name = Console.ReadLine();
                client.Connect("127.0.0.1", 4000);
                IChatServer server = client.Server;
                while(true)
                {
                    Task task = WaitSendM(server);
                }
            }
        }

        public static async Task WaitSendM(IChatServer server)
        {
            await server.SendMessage(ConsoleWriter.ReadLine());
        }
    }

    [Shared]
    public partial class ChatServer : ImpServer<IChatClient>
    {
        public ChatServer(IPAddress binding, int port) : base(binding, port)
        {
        }

        public async Task SendMessage(string message, [CallingClient] IChatClient sender = null)
        {
            SendMessageToEveryone("[" + sender.Name + "] " + message);            
        }

        protected override async void OnClientConnected(IChatClient client)
        {
            await client.SetServersideName(await client.GetClientsideName());
            SendMessageToEveryone("" + client.Name + " joined the chat.");
        }

        protected override void OnClientNetworkError(IChatClient client, Exception exception)
        {
            Console.WriteLine("Im having an aneurysm\n" + exception);
        }

        protected override void OnClientDisconnected(IChatClient client)
        {
            SendMessageToEveryone(client.Name + " left the chat.");
        }

        private void SendMessageToEveryone(string message)
        {
            ConsoleWriter.WriteLine(message);
            foreach (IChatClient client in ConnectedClients)
            {
                client.ReceiveMessage(message);
            }
        }
    }

    [Shared]
    public partial class ChatClient : ImpClient<IChatServer>
    {
        public string Name { get; set; }

        public async Task ReceiveMessage(string message)
        {
            ConsoleWriter.WriteLine(message);
        }

        public async Task SetServersideName(string name) => throw new NotImplementedException();

        public async Task<string> GetClientsideName()
        {
            return Name;
        }

        protected override void OnDisconnected()
        {
            ConsoleWriter.WriteLine("The client was disconnected.");
        }

        protected override void OnNetworkError(Exception e)
        {
            ConsoleWriter.WriteLine("A network error occured, resulting in disconnection:\n" + e);
        }
    }

    [ProxyFor(typeof(IChatClient))]
    public abstract class RemoteChatClient : RemoteImpClient, IChatClient
    {
        protected RemoteChatClient(ushort path, ImpClient host) : base(path, host) { }

        public string Name { get; set; }

        public abstract Task<string> GetClientsideName();
        public abstract Task ReceiveMessage(string message);
        public async Task SetServersideName(string name)
        {
            Name = name;
        }
    }
}