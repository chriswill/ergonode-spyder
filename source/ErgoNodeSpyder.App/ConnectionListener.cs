using System.Net;
using System.Net.Sockets;
using ErgoNodeSharp.Common;
using ErgoNodeSharp.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSpyder.App
{
    public delegate Task ClientConnected(TcpClient client);

    public class ConnectionListener
    {
        public event ClientConnected? OnClientConnected;

        private readonly ILogger<ConnectionListener>? logger;
        private TcpListener? listener;
        private bool started;
        
        public ConnectionListener()
        {
            if (ApplicationLogging.LoggerFactory != null)
            {
                logger = ApplicationLogging.LoggerFactory.CreateLogger<ConnectionListener>();
            }
        }
        
        /// <summary>
        /// Begins listening on the port provided to the constructor
        /// </summary>
        public void Start(IPAddress address, int port)
        {
            listener = new TcpListener(address, port);
            logger?.LogInformation("Started server on {address}:{port}", address, port);

            Task.Run(ListenForClients);
            
            started = true;
        }

        /// <summary>
        /// Runs in its own thread. Responsible for accepting new clients and kicking them off into their own thread
        /// </summary>
        private void ListenForClients()
        {
            if (listener == null) return;
            listener.Start();

            while (started)
            {
                TcpClient client = listener.AcceptTcpClient();
                
                logger?.LogInformation("New client connected from {address}", client.GetAddress());

                OnClientConnected?.Invoke(client).GetAwaiter().GetResult();

                Thread.Sleep(15);
            }
        }

        /// <summary>
        /// Stops the server from accepting new clients
        /// </summary>
        public void Stop()
        {
            if (listener == null || listener.Pending()) return;
            listener.Stop();
            started = false;
        }
    }
}
