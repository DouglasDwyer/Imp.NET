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
using DouglasDwyer.Imp.Messages;
using DouglasDwyer.Imp.Serialization;

[assembly: ShareAs(typeof(ImpServer<>), typeof(IImpServer))]
[assembly: ShareAs(typeof(ImpServer), typeof(IImpServer))]
namespace DouglasDwyer.Imp
{
    [Shared]
    public class ImpServer : IImpServer
    {
        public IEnumerable<IImpClient> ConnectedClients => ActiveClients.Values;
        public IProxyBinder DefaultProxyBinder { get; set; }
        public ImpClientSerializer DefaultSerializer { get; set; }
        public TaskScheduler DefaultRemoteTaskScheduler { get; set; }

        private IdentifiedCollection<IImpClient> ActiveClients = new IdentifiedCollection<IImpClient>();
        private TcpListener Listener;

        public ImpServer(IPAddress binding, int port) : this(binding, port, new ImpClientSerializer((ImpClient)null), RuntimeProxyBinder.CreateAndBind())
        {
        }

        public ImpServer(IPAddress binding, int port, ImpClientSerializer defaultSerializer, IProxyBinder binder)
        {
            Listener = new TcpListener(binding, port);
            DefaultSerializer = defaultSerializer;
            DefaultProxyBinder = binder;
            if (SynchronizationContext.Current is null)
            {
                DefaultRemoteTaskScheduler = TaskScheduler.Current;
            }
            else
            {
                DefaultRemoteTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
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
            Type remoteTypeToInstantiate = DefaultProxyBinder.GetRemoteType(Type.GetType(reader.ReadString())); //BadProxyBinder.Instance.GetRemoteClass(ShareAsAttribute.ProxyIndex[reader.ReadUInt16()]);
            ushort networkID = 0;
            ImpClient kClient = null;
            ActiveClients.Add(x => {
                networkID = (ushort)(x + 1);
                ImpClientSerializer serializer = (ImpClientSerializer)DefaultSerializer.Clone();
                kClient = new ImpClient(client, this, 0, DefaultProxyBinder, serializer, DefaultRemoteTaskScheduler);
                serializer.Client = kClient;
                return (IImpClient)Activator.CreateInstance(remoteTypeToInstantiate, new SharedObjectPath(networkID, 0), kClient); });
            writer.Write(networkID);
            kClient.SendImpMessage(new SetProxyBinderMessage(DefaultProxyBinder.GetProxyTypes().Select(x => x.AssemblyQualifiedName).ToArray()));
        }
    }

    [Shared]
    public class ImpServer<T> : ImpServer where T : IImpClient
    {
        public new List<T> ConnectedClients => base.ConnectedClients.Cast<T>().ToList();

        public ImpServer(IPAddress binding, int port) : base(binding, port) { }
    }
}
