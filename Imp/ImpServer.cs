using System;
using System.Collections.Generic;
using System.ComponentModel;
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

[assembly: ShareAs(typeof(ImpServer<>), typeof(IImpServer<>))]
[assembly: ShareAs(typeof(ImpServer), typeof(IImpServer))]
namespace DouglasDwyer.Imp
{
    [Shared]
    public class ImpServer : IImpServer
    {
        [Local]
        public IEnumerable<IImpClient> ConnectedClients => ActiveClients.Values;
        [Local]
        public IProxyBinder DefaultProxyBinder { get; set; }
        [Local]
        public ImpPowerSerializer DefaultSerializer { get; set; }
        [Local]
        public TaskScheduler DefaultRemoteTaskScheduler { get; set; }

        private IdentifiedCollection<IImpClient> ActiveClients = new IdentifiedCollection<IImpClient>();
        private TcpListener Listener;
        private UdpClient UnreliableListener;

        public ImpServer(IPAddress binding, int port) : this(binding, port, new ImpPowerSerializer(), RuntimeProxyBinder.CreateAndBind())
        {
        }

        public ImpServer(IPAddress binding, int port, ImpPowerSerializer defaultSerializer, IProxyBinder binder)
        {
            Listener = new TcpListener(binding, port);
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            UnreliableListener = new UdpClient(AddressFamily.InterNetworkV6);
            UnreliableListener.Client = socket;
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

        [Local]
        public virtual void Start()
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            UnreliableListener.BeginReceive(AcceptUnreliableMessage, null);
        }

        [Local]
        public virtual void Stop()
        {
            Listener.Stop();
            foreach(RemoteImpClient client in ConnectedClients)
            {
                client.HostClient.Disconnect();
            }
        }

        [Local]
        public virtual object GetCallingClientData(IImpClient client)
        {
            return client;
        }

        protected virtual void OnClientConnected(IImpClient client) { }

        protected virtual void OnClientNetworkError(IImpClient client, Exception exception) { }

        protected virtual void OnClientDisconnected(IImpClient client) { }

        internal void ReportClientNetworkError(IImpClient client, Exception exception)
        {
            OnClientNetworkError(client, exception);
        }

        internal void ReportClientDisconnected(IImpClient client)
        {
            OnClientDisconnected(client);
        }

        private void AcceptTcpClient(IAsyncResult result)
        {
            TcpClient client = Listener.EndAcceptTcpClient(result);
            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            try
            {
                BinaryWriter writer = new BinaryWriter(client.GetStream());
                BinaryReader reader = new BinaryReader(client.GetStream());//BadProxyBinder.Instance.GetRemoteClass(ShareAsAttribute.ProxyIndex[reader.ReadUInt16()]);
                ImpClient kClient = null;
                ActiveClients.Add(x =>
                {
                    ushort networkID = (ushort)(x + 1);
                    ImpPowerSerializer serializer = (ImpPowerSerializer)DefaultSerializer.Clone();
                    kClient = new ImpClient(client, this, networkID, UnreliableListener, DefaultProxyBinder, serializer, DefaultRemoteTaskScheduler);
                    return kClient.RemoteClient;
                });
                OnClientConnected(kClient.RemoteClient);
            }
            catch(Exception e)
            {
                OnClientNetworkError(null, e);
            }
        }

        private void AcceptUnreliableMessage(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = null;
                byte[] data = UnreliableListener.EndReceive(result, ref clientEndPoint);
                UnreliableListener.BeginReceive(AcceptUnreliableMessage, null);
                ushort id;
                if (BitConverter.IsLittleEndian)
                {
                    id = BitConverter.ToUInt16(new byte[] { data[0], data[1] }, 0);
                }
                else
                {
                    id = BitConverter.ToUInt16(new byte[] { data[1], data[0] }, 0);
                }
                ImpClient client = ((RemoteSharedObject)ActiveClients[(ushort)(id - 1)]).HostClient;
                if (client.DoesRemoteEndPointMatch(clientEndPoint))
                {
                    client.ProcessUnreliableMessage(data.Skip(2).ToArray());
                }
            }
            catch (Exception e)
            {
                OnClientNetworkError(null, e);
            }
        }
    }

    [Shared]
    public class ImpServer<T> : ImpServer, IImpServer<T> where T : IImpClient
    {
        [Local]
        public new List<T> ConnectedClients => base.ConnectedClients.Cast<T>().ToList();

        public ImpServer(IPAddress binding, int port) : base(binding, port) { }

        protected virtual void OnClientConnected(T client) { }

        protected virtual void OnClientNetworkError(T client, Exception exception) { }

        protected virtual void OnClientDisconnected(T client) { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnClientConnected(IImpClient client) {
            OnClientConnected((T)client);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnClientNetworkError(IImpClient client, Exception exception) {
            OnClientNetworkError((T)client, exception);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnClientDisconnected(IImpClient client) {
            OnClientDisconnected((T)client);
        }
    }
}
