using System.Net;
using System.Net.Sockets;
using ErgoNodeSharp.Common;
using ErgoNodeSharp.Common.Extensions;
using ErgoNodeSharp.Models.Messages;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSpyder.App.Node
{
    public delegate Task DataReceived(byte[] data, NodeConnection nodeConnection);
    public delegate Task ConnectionFailed(string address);

    public class NodeConnection : IDisposable
    {
        public event DataReceived? OnDataReceived;
        public event ConnectionFailed? OnConnectionFailed;

        public string Address { get; private set; }
        
        private readonly ILogger<NodeConnection>? logger;
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private readonly CancellationToken token;
        private readonly NetworkBuffer networkBuffer;
        private const int ReadBufferSize = 8192;
        private const int WriteBufferSize = 8192;
        
        public NodeConnection(CancellationToken token = default(CancellationToken))
        {
            if (ApplicationLogging.LoggerFactory != null)
                logger = ApplicationLogging.LoggerFactory.CreateLogger<NodeConnection>();
            this.token = token;

            networkBuffer = new NetworkBuffer();
            networkBuffer.ReadBuffer = new byte[ReadBufferSize];
            networkBuffer.WriteBuffer = new byte[WriteBufferSize];
            Address = string.Empty;
        }

        public async Task Connect(IPAddress address, int port)
        {
            tcpClient = new TcpClient();
            Address = $"{address}:{port}";
            try
            {
                await tcpClient.ConnectAsync(address, port, token);
                stream = tcpClient.GetStream();
            }
            catch
            {
                OnConnectionFailed?.Invoke(Address);
            }
        }

        public async Task RegisterConnection(TcpClient client, HandshakeMessage handshakeMessage)
        {
            tcpClient = client;
            Address = client.GetAddress();
            stream = tcpClient.GetStream();
            byte[] bytes = handshakeMessage.Serialize();
            logger?.LogInformation("Sending {message} message to {address}", "handshake", Address);
            await SendData(bytes);
        }

        public async Task ReadData()
        {
            if (stream is { CanRead: true })
            {
                byte[] allData = Array.Empty<byte>();
                do
                {
                    Array.Clear(networkBuffer.ReadBuffer);
                    int numberOfBytesRead = await stream.ReadAsync(networkBuffer.ReadBuffer, 0, ReadBufferSize, token);
                    byte[] dataBytes = networkBuffer.ReadBuffer.SubArray(0, numberOfBytesRead);
                    if (allData.Length == 0)
                    {
                        allData = dataBytes;
                    }
                    else
                    {
                        allData.ConcatFast(dataBytes);
                    }

                } while (stream.DataAvailable);

                if (OnDataReceived != null && tcpClient != null)
                {
                    await OnDataReceived.Invoke(allData, this);
                }
            }
        }

        public async Task SendData(byte[] data)
        {
            if (stream is { CanWrite: true })
            {
                if (data.Length <= WriteBufferSize)
                {
                    await stream.WriteAsync(data, 0, data.Length, token);
                }
                else
                {
                    //TODO:finish implementation
                    Array.ConstrainedCopy(data, 0, networkBuffer.WriteBuffer, networkBuffer.CurrentWriteByteCount, data.Length);

                    networkBuffer.CurrentWriteByteCount += data.Length;
                }
            }
        }

        public void Disconnect()
        {
            tcpClient?.Close();
        }

        public void Dispose()
        {
            stream?.Dispose();
            tcpClient?.Dispose();
            stream = null;
            tcpClient = null;
        }
    }
}
