using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DouglasDwyer.Knetworking;
using DouglasDwyer.Knetworking.Serialization;

[assembly: ShareAs(typeof(KnetworkingServer<>), typeof(IKnetworkingServer))]
[assembly: ShareAs(typeof(KnetworkingServer), typeof(IKnetworkingServer))]
namespace DouglasDwyer.Knetworking
{
    [Shared]
    public class KnetworkingServer : IKnetworkingServer
    {
        public IEnumerable<IKnetworkingClient> ConnectedClients => ActiveClients.Values;
        public KnetworkingClientSerializer DefaultSerializer { get; }
        public TaskScheduler DefaultRemoteTaskScheduler { get; }

        private IdentifiedCollection<IKnetworkingClient> ActiveClients = new IdentifiedCollection<IKnetworkingClient>();
        private TcpListener Listener;

        public KnetworkingServer(IPAddress binding, int port) : this(binding, port, new KnetworkingClientSerializer((KnetworkingClient)null))
        {
        }

        public KnetworkingServer(IPAddress binding, int port, KnetworkingClientSerializer defaultSerializer)
        {
            Listener = new TcpListener(binding, port);
            DefaultSerializer = defaultSerializer;
            DefaultRemoteTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public void Start()
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);
        }

        public void Stop()
        {
            Listener.Stop();
            foreach(RemoteKnetworkingClient client in ConnectedClients)
            {
                client.HostClient.Disconnect();
            }
        }

        private void AcceptTcpClient(IAsyncResult result)
        {
            TcpClient client = Listener.EndAcceptTcpClient(result);
            BinaryWriter writer = new BinaryWriter(client.GetStream());
            BinaryReader reader = new BinaryReader(client.GetStream());
            Type remoteTypeToInstantiate = ProxyBinder.Instance.GetRemoteClass(ShareAsAttribute.ProxyIndex[reader.ReadUInt16()]);
            ushort networkID = 0;
            ActiveClients.Add(x => {
                networkID = (ushort)(x + 1);
                KnetworkingClientSerializer serializer = (KnetworkingClientSerializer)DefaultSerializer.Clone();
                KnetworkingClient kClient = new KnetworkingClient(client, this, 0, serializer);
                serializer.Client = kClient;
                return (IKnetworkingClient)Activator.CreateInstance(remoteTypeToInstantiate, new SharedObjectPath(networkID, 0), kClient); });
            writer.Write(networkID);
        }
    }

    [Shared]
    public class KnetworkingServer<T> : KnetworkingServer where T : IKnetworkingClient
    {
        public new List<T> ConnectedClients => base.ConnectedClients.Cast<T>().ToList();

        public KnetworkingServer(IPAddress binding, int port) : base(binding, port) { }
    }
}
