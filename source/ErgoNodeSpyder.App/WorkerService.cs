using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ErgoNodeSpyder.App.Node;
using ErgoNodeSharp.Common;
using ErgoNodeSharp.Common.Extensions;
using ErgoNodeSharp.Data;
using ErgoNodeSharp.Models;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.DTO;
using ErgoNodeSharp.Models.Messages;
using ErgoNodeSharp.Models.Responses;
using ErgoNodeSharp.Services.GeoIp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace ErgoNodeSpyder.App
{
    public sealed class WorkerService : BackgroundService
    {
        private readonly ErgoConfiguration ergoConfiguration;
        private readonly NetworkConfiguration networkConfiguration;
        private readonly INodeInfoRepository nodeRepository;
        private readonly ErgoNodeSpyderConfiguration spyderConfiguration;
        private readonly IGeoIpService geoIpService;
        private readonly ILogger<WorkerService> logger;
        private ConnectionListener? listener;
        private Timer? moreNodesTimer;
        private Timer? geoInfoTimer;
        private Timer? analyticsTimer;

        //Our list of current connections
        private readonly ConcurrentDictionary<string, NodeConnection> nodeConnections;

        //Which nodes that have been sent GetPeers requests
        private readonly ConcurrentDictionary<string, byte> getPeerRequests;

        public WorkerService(ErgoConfiguration ergoConfiguration, NetworkConfiguration networkConfiguration, 
            INodeInfoRepository nodeRepository, IGeoIpService geoIpService, ErgoNodeSpyderConfiguration spyderConfiguration, ILoggerFactory factory)
        {
            this.ergoConfiguration = ergoConfiguration;
            this.networkConfiguration = networkConfiguration;
            this.nodeRepository = nodeRepository;
            this.geoIpService = geoIpService;
            this.spyderConfiguration = spyderConfiguration;
            ApplicationLogging.LoggerFactory = factory;
            logger = ApplicationLogging.LoggerFactory.CreateLogger<WorkerService>();
            nodeConnections = new ConcurrentDictionary<string, NodeConnection>();
            getPeerRequests = new ConcurrentDictionary<string, byte>();
        }

        private void MoreNodesTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            logger.LogDebug("Node query event executed");
            if (nodeConnections.Count > 0)
            {
                logger.LogInformation("{0} node connections stored in dictionary", nodeConnections.Count);
            }
            
            foreach (KeyValuePair<string, NodeConnection> nodeConnection in nodeConnections)
            {
                logger.LogInformation(nodeConnection.Key);
            }
            
            Task<IEnumerable<NodeIdentifier>> task = nodeRepository.GetAddressesForConnection();
            List<NodeIdentifier> identifiers = task.Result.ToList();

            logger.LogDebug("{0} waiting nodes found for connection", identifiers.Count);

            foreach (NodeIdentifier ident in identifiers)
            {
                string address = $"{ident.Address}:{ident.Port}";
                bool existingConnection = nodeConnections.TryGetValue(address, out _);
                if (existingConnection)
                {
                    Task.Run(() => NodeConnection_OnConnectionFailed(address));
                }
                else
                {
                    Task.Run(() => ConnectToNode(address));
                }
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            moreNodesTimer = new Timer(TimeSpan.FromMinutes(2).TotalMilliseconds);
            moreNodesTimer.Elapsed += MoreNodesTimer_Elapsed;
            moreNodesTimer.AutoReset = true;
            moreNodesTimer.Enabled = true;

            if (spyderConfiguration.PerformGeoIpLookup)
            {
                geoInfoTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
                geoInfoTimer.Elapsed += GeoInfoTimer_Elapsed;
                geoInfoTimer.AutoReset = true;
                geoInfoTimer.Enabled = true;
            }
            
            analyticsTimer = new Timer(TimeSpan.FromHours(4).TotalMilliseconds);
            analyticsTimer.Elapsed += AnalyticsTimer_Elapsed;
            analyticsTimer.AutoReset = true;
            analyticsTimer.Enabled = true;

            //listener = new ConnectionListener();
            //listener.OnClientConnected += Listener_OnClientConnected;

            string address = networkConfiguration.BindAddress;
            string[] parts = address.Split(":");
            
            IPAddress ipAddress = "0.0.0.0".Equals(parts[0]) ? 
                IPAddress.Any : 
                IPAddress.Parse(parts[0]);
            int port = int.Parse(parts[1]);

            //listener.Start(ipAddress, port);

            foreach (string peer in networkConfiguration.KnownPeers)
            {
                Task.Run(() => ConnectToNode(peer));
            }

            return Task.CompletedTask;
        }

        private void GeoInfoTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Task<IEnumerable<string>> task = nodeRepository.GetAddressesForGeoLookup(25);

            List<string> nodeAddresses = task.Result.ToList();
            if (nodeAddresses.Count > 0)
            {
                logger.LogInformation("{0} waiting nodes found for geo lookup", nodeAddresses.Count);
            }
            
            foreach (string address in nodeAddresses)
            {
                //This is all single-threaded... don't think there is any reason to multi-thread
                logger.LogDebug("Updating geo information for {0}", address);
                GeoIpResponse? response = null;
                try
                {
                    Task<GeoIpResponse?> geoTask = geoIpService.GetGeoIpResponse(address);
                    response = geoTask.Result;
                }
                catch
                {
                    logger.LogError("Error making Geo request");
                }

                if (response != null)
                {
                    Task databaseTask = nodeRepository.UpdateNodeGeo(response);
                    databaseTask.Wait();
                }
            }
        }

        private void AnalyticsTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender is Timer timer)
            {
                DateTime nextExecution = e.SignalTime.AddMilliseconds(timer.Interval);
                logger.LogInformation("Analytics timer executed. Next execution time at {0}", nextExecution);
            }
            else
            {
                logger.LogInformation("Analytics timer executed.");
            }
            
            IEnumerable<NodeIdentifier> waitingNodes = nodeRepository.GetAddressesForConnection(1).Result;
            if (waitingNodes.Any())
            {
                logger.LogWarning("Node query is currently running. Analytics cannot be run at this time.");
                return;
            }

            Task t = nodeRepository.PerformMaintenanceAndAnalytics();
            t.Wait();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

            }
            return Task.CompletedTask;
        }

        private async Task Listener_OnClientConnected(TcpClient client)
        {
            CancellationToken token = new CancellationToken();
            NodeConnection nodeConnection = new NodeConnection(token);
            nodeConnections.TryAdd(client.GetAddress(), nodeConnection);

            HandshakeMessage message = HandshakeMessage.CreateHandshake(ergoConfiguration, networkConfiguration);
            await nodeConnection.RegisterConnection(client, message);
        }
        
        private async Task ConnectToNode(string address)
        {
            CancellationToken token = new CancellationToken();
            NodeConnection nodeConnection = new NodeConnection(token);
            nodeConnection.OnDataReceived += NodeConnectionOnDataReceived;
            nodeConnection.OnConnectionFailed += NodeConnection_OnConnectionFailed;

            logger.LogInformation("Connecting to {0}", address);
            string[] parts = address.Split(new char[] { ':' });
            IPAddress ipAddress = IPAddress.Parse(parts[0]);
            int port = int.Parse(parts[1]);
            await nodeConnection.Connect(ipAddress, port);
            
            nodeConnections.TryAdd(address, nodeConnection);

            while (!token.IsCancellationRequested)
            {
                await nodeConnection.ReadData();
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private async Task NodeConnection_OnConnectionFailed(string address)
        {
            logger.LogError("Connection failed to {0}", address);
            await nodeRepository.RecordFailedConnection(address);
            bool result = getPeerRequests.TryRemove(address, out _);
            if (!result)
            {
                logger.LogDebug("Unable to remove {0} from Peer requests list", address);
            }
            result = nodeConnections.TryRemove(address, out NodeConnection? nodeConnection);
            if (!result)
            {
                logger.LogDebug("Unable to remove {0} from Node connections list", address);
            }
            nodeConnection?.Dispose();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("The service is stopping");
            //listener?.Stop();
            
            moreNodesTimer?.Stop();
            moreNodesTimer?.Dispose();

            geoInfoTimer?.Stop();
            geoInfoTimer?.Dispose();

            analyticsTimer?.Stop();
            analyticsTimer?.Dispose();

            return Task.CompletedTask;
        }

        private async Task NodeConnectionOnDataReceived(byte[] data, NodeConnection nodeConnection)
        {
            if (data.Length == 0) return;
            logger.LogInformation("Data received from {0}", nodeConnection.Address);
            string message = data.ToHexString();
            string ipAddress = nodeConnection.Address;

            byte[] header = data.SubArray(0, 4);
            if (header.SequenceEqual(Constants.MainnetHeader) || header.SequenceEqual(Constants.TestnetHeader))
            {
                MessageType messageType = (MessageType)data[4];
                logger.LogDebug($"Message ({messageType})\r\n" + message);
                try
                {
                    INodeMessage nodeMessage = NodeMessage.Deserialize(data);

                    if (nodeMessage.MessageType == MessageType.Peers)
                    {
                        PeersMessage peersMessage = (PeersMessage)nodeMessage;
                        logger.LogInformation("Received {0} from {1}", nodeMessage, ipAddress);
                        await nodeRepository.AddUpdateNodes(peersMessage.Peers, ipAddress);
                        
                        logger.LogInformation("Disconnecting from {0}", ipAddress);
                        bool result = getPeerRequests.TryRemove(ipAddress, out _);
                        if (!result)
                        {
                            logger.LogDebug("Unable to remove {0} from Peer requests list", ipAddress);
                        }
                        result = nodeConnections.TryRemove(ipAddress, out _);
                        if (!result)
                        {
                            logger.LogDebug("Unable to remove {0} from Node connections list", ipAddress);
                        }
                        nodeConnection.Disconnect();
                        nodeConnection.Dispose();
                    }
                    else
                    {
                        logger.LogWarning("Received unsupported message type {0} from {1}", nodeMessage, ipAddress);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
            }
            else
            {
                HandshakeMessage receivedHandshake = HandshakeMessage.Deserialize(data);
                logger.LogInformation("Received {0} message from {1}", receivedHandshake, ipAddress);

                //Have we been sending GetPeers messages to this node already?
                bool peerRequestsExist = getPeerRequests.TryGetValue(ipAddress, out byte requestCount);
                if (peerRequestsExist && requestCount >= 5)
                {
                    nodeConnection.Disconnect();
                    await NodeConnection_OnConnectionFailed(ipAddress);
                    logger.LogWarning("Node {0} was disconnected because it would not respond with a Peers message", ipAddress);
                    return;
                }

                await nodeRepository.AddUpdateNode(receivedHandshake.PeerSpec);

                //this is a handshake
                HandshakeMessage handshakeMessage = HandshakeMessage.CreateHandshake(ergoConfiguration, networkConfiguration);
                byte[] messageBytes = handshakeMessage.Serialize();

                Thread.Sleep(15);
                logger.LogInformation("Sending {0} message to {1}", handshakeMessage, ipAddress);
                await nodeConnection.SendData(messageBytes);

                Thread.Sleep(15);
                INodeMessage getPeersMessage = new GetPeersMessage();
                getPeersMessage.SetNetworkType(ergoConfiguration.NetworkType);
                byte[] bytes = getPeersMessage.Serialize();
                logger.LogInformation("Sending {0} message to {1}", getPeersMessage, ipAddress);
                await nodeConnection.SendData(bytes);

                if (peerRequestsExist)
                {
                    byte currentRequests = requestCount++;
                    getPeerRequests.TryUpdate(ipAddress, requestCount, currentRequests);
                }
                else
                {
                    getPeerRequests.TryAdd(ipAddress, 1);
                }

            }
        }
    }
}
