using System;
using System.IO;
using System.Linq;
using ErgoNodeSharp.Common;
using ErgoNodeSharp.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSharp.Models.Messages
{
    public interface INodeMessage
    {
        byte[] MagicBytes { get; set; }

        string MessageName { get; }

        MessageType MessageType { get; }

        int MessageLength { get; set; }

        byte[] HandshakeChecksum { get; set; }

        byte[] Serialize();

        void DeserializeBody(byte[] bytes);

        void SetNetworkType(NetworkType networkType);

    }

    public abstract class NodeMessage : INodeMessage
    {
        public byte[] MagicBytes { get; set; }

        public abstract string MessageName { get; }

        public abstract MessageType MessageType { get; }

        public int MessageLength { get; set; }

        public byte[] HandshakeChecksum { get; set; }

        public byte[] MessageBody { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(MagicBytes);
                    writer.Write((byte)MessageType);
                    
                    byte[] body = SerializeBody();
                    writer.Write(body.Length);
                    
                    if (body.Length > 0)
                    {
                        byte[] blakeHash = Blake2Fast.Blake2b.ComputeHash(body);
                        writer.Write(blakeHash.SubArray(0, 4));

                        writer.Write(body);
                    }
                }

                return ms.ToArray();
            }
        }

        protected abstract byte[] SerializeBody();

        public void SetNetworkType(NetworkType networkType)
        {
            MagicBytes = networkType == NetworkType.Testnet ?
                Constants.TestnetHeader :
                Constants.MainnetHeader;
        }

        protected NodeMessage()
        {
            SetNetworkType(NetworkType.Mainnet);
        }

        public abstract void DeserializeBody(byte[] bytes);

        public override string ToString()
        {
            return $"Message({(byte)MessageType}: {MessageName})";
        }

        public static INodeMessage Deserialize(byte[] bytes)
        {
            ILogger<INodeMessage> logger = ApplicationLogging.LoggerFactory?.CreateLogger<INodeMessage>();
            INodeMessage message;
            
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    byte[] magicBytes = reader.ReadBytes(4);
                    byte messageCode = reader.ReadByte();
                    
                    byte[] lengthBytes = reader.ReadBytes(4);
                    int messageLength = lengthBytes.FromByteArray();

                    int length = bytes.Length;
                    
                    MessageType messageType = (MessageType)messageCode;

                    switch (messageType)
                    {
                        case MessageType.GetPeers:
                            message = new GetPeersMessage();
                            break;
                        case MessageType.Inv:
                            message = new InvMessage();
                            break;
                        case MessageType.Modifier:
                            message = new ModifierMessage();
                            break;
                        case MessageType.Peers:
                            message = new PeersMessage();
                            break;
                        case MessageType.RequestModifier:
                            message = new RequestModifierMessage();
                            break;
                        case MessageType.Sync:
                            message = new SyncMessage();
                            break;
                        case MessageType.Handshake:
                            message = new HandshakeNodeMessage();
                            break;
                        default:
                            throw new Exception("Unable to deserialize message type " + messageCode);
                    }
                    if (messageLength > 0)
                    {
                        byte[] checksumBytes = reader.ReadBytes(4);
                        message.HandshakeChecksum = checksumBytes;
                        long pos = reader.BaseStream.Position;
                        byte[] messageBody = reader.ReadBytes((int)(bytes.Length - pos));
                        byte[] hash = Blake2Fast.Blake2b.ComputeHash(32, messageBody).SubArray(0, 4);
                        if (!checksumBytes.SequenceEqual(hash))
                        {
                            logger?.LogWarning("Checksum does not equal body");
                        }
                        message.DeserializeBody(messageBody);
                    }
                    message.MagicBytes = magicBytes;
                    message.MessageLength = messageLength;
                    
                }
            }

            return message;
        }

        public bool Validate()
        {
            byte[] bodyBytes = SerializeBody();
            if (bodyBytes.Length == 0) return true;
            byte[] hash = Blake2Fast.Blake2b.ComputeHash(32, bodyBytes).SubArray(0, 4);
            return HandshakeChecksum.SequenceEqual(hash);
        }

    }
}
