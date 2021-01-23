using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DouglasDwyer.Imp;
using DouglasDwyer.Imp.Serialization;
using DouglasDwyer.Imp.Messages;

//[assembly: ShareAs(typeof(int[]), typeof(IList<int>))]

namespace TestProject
{
    public interface IChad
    {
        void Chadley(string i, object o, int k);
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type S for server or press any other key for client.");
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                TicTacToeServer server = new TicTacToeServer(IPAddress.Any, 4000);
                server.Start();
                while (server.ConnectedClients.Count == 0) { Thread.Sleep(1); }
                Console.WriteLine("Client connected to server.");
                Console.ReadKey();
            }
            else
            {
                TicTacToeClient client = new TicTacToeClient();
                client.Connect("localhost", 4000);
                Console.WriteLine("Made a connection to the server");
                ITicTacToeServer game = client.Server;
                game.ThrowMe("kek").Result.SayHoi();
                
                Console.ReadKey();
            }

            Console.ReadLine();
        }
    }

    [Shared]
    public partial class TicTacToeServer : ImpServer<ITicTacToeClient>
    {
        public bool IsClientTurn { get; set; } = true;

        public TicTacToeServer(IPAddress ip, int port) : base(ip, port) { }

        public async Task<ITicTacToeServer> ThrowMe(string thrpwer)
        {
            return this;
        }

        public async Task SayHoi()
        {
            Console.WriteLine("HOI");
        }
    }

    [Shared]
    public partial class TicTacToeClient : ImpClient<ITicTacToeServer> {
    }
}