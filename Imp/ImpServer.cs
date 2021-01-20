using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DouglasDwyer.Imp;
using DouglasDwyer.Imp.Serialization;

[assembly: ShareAs(typeof(ImpServer<>), typeof(IImpServer))]
[assembly: ShareAs(typeof(ImpServer), typeof(IImpServer))]
namespace DouglasDwyer.Imp
{
    [Shared]
    public class ImpServer : IImpServer
    {
        public IEnumerable<IImpClient> ConnectedClients => ActiveClients.Values;
        public ImpClientSerializer DefaultSerializer { get; }
        public TaskScheduler DefaultRemoteTaskScheduler { get; }

        private IdentifiedCollection<IImpClient> ActiveClients = new IdentifiedCollection<IImpClient>();
        private TcpListener Listener;

        public ImpServer(IPAddress binding, int port) : this(binding, port, new ImpClientSerializer((ImpClient)null))
        {
        }

        public ImpServer(IPAddress binding, int port, ImpClientSerializer defaultSerializer)
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
            foreach(RemoteImpClient client in ConnectedClients)
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
                ImpClientSerializer serializer = (ImpClientSerializer)DefaultSerializer.Clone();
                ImpClient kClient = new ImpClient(client, this, 0, serializer);
                serializer.Client = kClient;
                return (IImpClient)Activator.CreateInstance(remoteTypeToInstantiate, new SharedObjectPath(networkID, 0), kClient); });
            writer.Write(networkID);
        }
    }

    [Shared]
    public class ImpServer<T> : ImpServer where T : IImpClient
    {
        public new List<T> ConnectedClients => base.ConnectedClients.Cast<T>().ToList();

        public ImpServer(IPAddress binding, int port) : base(binding, port) { }
    }
}
