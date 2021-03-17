using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DouglasDwyer.Imp;
using DouglasDwyer.Imp.Messages;

[assembly: ShareAs(typeof(ImpServer<>), typeof(IImpServer<>))]
[assembly: ShareAs(typeof(ImpServer), typeof(IImpServer))]
namespace DouglasDwyer.Imp
{
    /// <summary>
    /// Represents a TCP server that can send shared interfaces across the network as references.
    /// </summary>
    [Shared]
    public class ImpServer : IImpServer
    {
        /// <summary>
        /// Returns a list of all the clients currently connected to this server.
        /// </summary>
        [Local]
        public IEnumerable<IImpClient> ConnectedClients => ActiveClients.Values;
        /// <summary>
        /// The default shared type binder utilized when creating new clients.
        /// </summary>
        [Local]
        public IProxyBinder DefaultProxyBinder { get; set; }
        /// <summary>
        /// The default network serializer utilized when creating new clients.
        /// </summary>
        [Local]
        public INetworkSerializer DefaultSerializer { get; set; }
        /// <summary>
        /// The default remote task scheduler utilized when creating new clients.
        /// </summary>
        [Local]
        public TaskScheduler DefaultRemoteTaskScheduler { get; set; }

        private IdentifiedCollection<IImpClient> ActiveClients = new IdentifiedCollection<IImpClient>();
        private TcpListener Listener;
        private UdpClient UnreliableListener;

        /// <summary>
        /// Creates a new server bound to the specified port. The server will listen for connections on all IP addresses.
        /// </summary>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        public ImpServer(int port) : this(IPAddress.Any, port) { }

        /// <summary>
        /// Creates a new server bound to the specified IP address and port.
        /// </summary>
        /// <param name="binding">The IP address on which to listen for incoming connections.</param>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        public ImpServer(IPAddress binding, int port) : this(binding, port, new ImpPowerSerializer(), RuntimeProxyBinder.CreateAndBind()) { }

        /// <summary>
        /// Creates a new server bound to the specified IP address and port. The server will employ the given serializer and shared type binder when creating new clients and sending interfaces across the network.
        /// </summary>
        /// <param name="binding">The IP address on which to listen for incoming connections.</param>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        /// <param name="defaultSerializer">The default serializer that clients should use when sending interfaces across the network.</param>
        /// <param name="binder">The default shared type binder that clients should use when sending interfaces across the network.</param>
        public ImpServer(IPAddress binding, int port, INetworkSerializer defaultSerializer, IProxyBinder binder)
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

        /// <summary>
        /// Starts the server and begins listening for incoming connections/unreliable packets.
        /// </summary>
        [Local]
        public virtual void Start()
        {
            Listener.Start();
            Listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            UnreliableListener.BeginReceive(AcceptUnreliableMessage, null);
        }

        /// <summary>
        /// Stops the server, disconnects all connected clients, and ceases to listen for new connections or unreliable packets.
        /// </summary>
        [Local]
        public virtual void Stop()
        {
            Listener.Stop();
            try { UnreliableListener.Close(); } catch { }
            foreach(RemoteImpClient client in ConnectedClients)
            {
                client.HostClient.Disconnect();
            }
        }

        /// <summary>
        /// Called whenever a new client connects to the server.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        protected virtual void OnClientConnected(IImpClient client) { }

        /// <summary>
        /// Called whenever a client is disconnected from the server due to an exception in networking code.
        /// </summary>
        /// <param name="client">The client that has been disconnected.</param>
        /// <param name="exception">The exception that was thrown.</param>
        protected virtual void OnClientNetworkError(IImpClient client, Exception exception) { }

        /// <summary>
        /// Called whenever a client disconnects from the server.
        /// </summary>
        /// <param name="client">The client that has been disconnected.</param>
        protected virtual void OnClientDisconnected(IImpClient client) { }

        internal void ReportClientNetworkError(IImpClient client, Exception exception)
        {
            OnClientNetworkError(client, exception);
        }

        internal void ReportClientDisconnected(IImpClient client)
        {
            ActiveClients.Remove(client);
            OnClientDisconnected(client);
        }

        internal virtual void CheckClientType(Type type) { }

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
                    CheckClientType(kClient.RemoteClient.GetType());
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

    /// <summary>
    /// Represents a TCP server that can send shared interfaces across the network as references.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IImpClient"/> with which this server works. Clients whose shared interface does not derive from this type will not be able to connect.</typeparam>
    [Shared]
    public class ImpServer<T> : ImpServer, IImpServer<T> where T : IImpClient
    {
        /// <summary>
        /// Returns a list of all the clients currently connected to this server.
        /// </summary>
        [Local]
        public new IEnumerable<T> ConnectedClients => base.ConnectedClients.Cast<T>();

        /// <summary>
        /// Creates a new server bound to the specified port. The server will listen for connections on all IP addresses.
        /// </summary>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        public ImpServer(int port) : base(port) { }

        /// <summary>
        /// Creates a new server bound to the specified IP address and port.
        /// </summary>
        /// <param name="binding">The IP address on which to listen for incoming connections.</param>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        public ImpServer(IPAddress binding, int port) : base(binding, port) { }

        /// <summary>
        /// Creates a new server bound to the specified IP address and port. The server will employ the given serializer and shared type binder when creating new clients and sending interfaces across the network.
        /// </summary>
        /// <param name="binding">The IP address on which to listen for incoming connections.</param>
        /// <param name="port">The port on which to listen for incoming connections.</param>
        /// <param name="defaultSerializer">The default serializer that clients should use when sending interfaces across the network.</param>
        /// <param name="binder">The default shared type binder that clients should use when sending interfaces across the network.</param>
        public ImpServer(IPAddress binding, int port, INetworkSerializer defaultSerializer, IProxyBinder binder) : base(binding, port, defaultSerializer, binder) { }

        /// <summary>
        /// Called whenever a new client connects to the server.
        /// </summary>
        /// <param name="client">The client that has connected.</param>
        protected virtual void OnClientConnected(T client) { }

        /// <summary>
        /// Called whenever a client is disconnected from the server due to an exception in networking code.
        /// </summary>
        /// <param name="client">The client that has been disconnected.</param>
        /// <param name="exception">The exception that was thrown.</param>
        protected virtual void OnClientNetworkError(T client, Exception exception) { }

        /// <summary>
        /// Called whenever a client disconnects from the server.
        /// </summary>
        /// <param name="client">The client that has been disconnected.</param>
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

        internal override void CheckClientType(Type type)
        {
            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new SecurityException("A client of remote type " + type + " attempted to connect to this server, but " + type + " does not inherit from the required shared interface.");
            }
        }
    }
}
