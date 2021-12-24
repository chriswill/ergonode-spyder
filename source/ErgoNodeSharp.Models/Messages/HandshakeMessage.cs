using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ErgoNodeSharp.Common;
using ErgoNodeSharp.Common.Extensions;
using ErgoNodeSharp.Models.Configuration;

namespace ErgoNodeSharp.Models.Messages
{
    public class HandshakeMessage
    {
        public long UnixTimestamp { get; set; }

        public DateTimeOffset HandShakeTime
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(UnixTimestamp);
            set => UnixTimestamp = value.ToUnixTimeMilliseconds();
        }
        
        public PeerSpec PeerSpec { get; set; }

        public HandshakeMessage()
        {
            PeerSpec = new PeerSpec();
            UnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(UnixTimestamp.Write7BitEncoded());
                    byte[] peerBytes = PeerSpec.Serialize();
                    bw.Write(peerBytes);
                }

                return ms.ToArray();
            }
        }

        public static HandshakeMessage Deserialize(string message)
        {
            byte[] bytes = message.FromHexString();
            return Deserialize(bytes);
        }

        public static HandshakeMessage Deserialize(byte[] bytes)
        {
            HandshakeMessage handshakeMessage = new HandshakeMessage();
            PeerSpec peerSpec = new PeerSpec();
            int dateTimeLength = 6;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    byte[] dateBytes = reader.ReadBytes(dateTimeLength);
                    handshakeMessage.UnixTimestamp = Convert.ToInt64(dateBytes.Read7BitEncodedAsLong());

                    int agentLength = reader.ReadByte();
                    byte[] agentBytes = reader.ReadBytes(agentLength);
                    peerSpec.AgentName = Encoding.UTF8.GetString(agentBytes);

                    byte[] versionBytes = reader.ReadBytes(3);
                    peerSpec.Version = new Version(versionBytes[0], versionBytes[1], versionBytes[2]);

                    int peerNameLength = reader.ReadByte();
                    byte[] peerNameBytes = reader.ReadBytes(peerNameLength);
                    peerSpec.PeerName = Encoding.UTF8.GetString(peerNameBytes);

                    byte hasDeclaredAddress = reader.ReadByte();
                    if (hasDeclaredAddress != 0)
                    {
                        byte length = reader.ReadByte();
                        byte[] addressBytes = reader.ReadBytes(length - 2 - 2);
                        
                        IPAddress address = new IPAddress(addressBytes);
                        int port = reader.Read7BitEncodedInt();
                        peerSpec.DeclaredAddress = $"{address}:{port}";
                    }

                    int featureCount = reader.ReadByte();

                    for (int i = 0; i < featureCount; i++)
                    {
                        int featureTypeVal = reader.ReadByte();
                        int featureSize = reader.ReadByte();
                        FeatureType featureType = (FeatureType)featureTypeVal;
                        byte[] featureBytes = reader.ReadBytes(featureSize);
                        IPeerFeature feature = PeerSpec.DeserializeFeature(featureType, featureBytes);
                        peerSpec.FeatureCollection.Add(feature);
                    }
                }
            }

            handshakeMessage.PeerSpec = peerSpec;
            return handshakeMessage;
        }

        public static HandshakeMessage CreateHandshake(ErgoConfiguration ergoConfiguration, NetworkConfiguration networkConfiguration)
        {
            string[] addressParts = networkConfiguration.BindAddress.Split(new char[] { ':' });
            if (addressParts.Length != 2) throw new Exception("Invalid bind address specified");

            HandshakeMessage handshakeMessage = new HandshakeMessage();
            handshakeMessage.PeerSpec.Version = new Version(networkConfiguration.AppVersion);
            handshakeMessage.PeerSpec.AgentName = networkConfiguration.AgentName;
            handshakeMessage.PeerSpec.PeerName = networkConfiguration.NodeName;
            handshakeMessage.PeerSpec.DeclaredAddress = networkConfiguration.DeclaredAddress;

            ModeFeature mode = new ModeFeature();
            mode.StateType = ergoConfiguration.Node.StateType;
            mode.NiPoPoWBootstrapped = ergoConfiguration.Node.PoPoWBootstrap;
            mode.VerifyingTransactions = ergoConfiguration.Node.VerifyTransactions;
            mode.BlocksToKeep = ergoConfiguration.Node.BlocksToKeep;
            handshakeMessage.PeerSpec.FeatureCollection.Add(mode);

            PeerFeature peerFeature = new PeerFeature();
            peerFeature.SetNetworkType(ergoConfiguration.NetworkType);
            handshakeMessage.PeerSpec.FeatureCollection.Add(peerFeature);

            LocalAddressPeerFeature localAddressPeerFeature = new LocalAddressPeerFeature();
            handshakeMessage.PeerSpec.FeatureCollection.Add(localAddressPeerFeature);
            localAddressPeerFeature.Address = IPAddress.Parse(addressParts[0]).ToString();
            localAddressPeerFeature.Port = int.Parse(addressParts[1]);
            
            return handshakeMessage;
        }

        public override string ToString()
        {
            return "Handshake";
        }
    }

    //https://github.com/ergoplatform/ergo/blob/582fd3f6919249b0780b744e7cd16e98227272c7/src/main/scala/scorex/core/network/PeerSpec.scala
    public class PeerSpec
    {
        public string AgentName { get; set; }

        public string PeerName { get; set; }

        public Version Version { get; set; }
        
        public string DeclaredAddress { get; set; }
        
        public IList<IPeerFeature> FeatureCollection { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((byte) AgentName.Length);
                    byte[] agentNameBytes = Encoding.UTF8.GetBytes(AgentName);
                    bw.Write(agentNameBytes);
                    bw.Write((byte)Version.Major);
                    bw.Write((byte)Version.Minor);
                    bw.Write((byte)Version.Build);
                    bw.Write((byte)PeerName.Length);
                    byte[] peerNameBytes = Encoding.UTF8.GetBytes(PeerName);
                    bw.Write(peerNameBytes);
                    if (!string.IsNullOrEmpty(DeclaredAddress))
                    {
                        string[] parts = DeclaredAddress.Split(new char[] { ':' });
                        IPAddress address = IPAddress.Parse(parts[0]);
                        byte[] addressBytes = address.GetAddressBytes();
                        int port = int.Parse(parts[1]);
                        byte[] portBytes = port.Convert7BitEncoded();
                        bw.Write((byte)1);
                        bw.Write((byte)(addressBytes.Length + portBytes.Length + 2));
                        bw.Write(addressBytes);
                        bw.Write(portBytes);
                    }
                    else
                    {
                        bw.Write((byte)0); //No declared address
                    }
                    
                    bw.Write((byte) FeatureCollection.Count);

                    IPeerFeature peerFeature = FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Mode);
                    if (peerFeature != null)
                    {
                        byte[] featureBytes = peerFeature.Serialize();
                        bw.Write(featureBytes);
                    }

                    peerFeature = FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Peer);
                    if (peerFeature != null)
                    {
                        byte[] featureBytes = peerFeature.Serialize();
                        bw.Write(featureBytes);
                    }

                    peerFeature = FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Address);
                    if (peerFeature != null)
                    {
                        byte[] featureBytes = peerFeature.Serialize();
                        bw.Write(featureBytes);
                    }
                }

                return ms.ToArray();
            }
        }

        public static IPeerFeature DeserializeFeature(FeatureType featureType, byte[] bytes)
        {
            IPeerFeature feature;
            switch (featureType)
            {
                case FeatureType.Address:
                    feature = LocalAddressPeerFeature.Deserialize(bytes);
                    break;
                case FeatureType.Peer:
                    feature = PeerFeature.Deserialize(bytes);
                    break;
                case FeatureType.Mode:
                    feature = ModeFeature.Deserialize(bytes);
                    break;
                default:
                    throw new Exception("Cannot deserialize type " + featureType);
            }

            return feature;
        }

        public PeerSpec()
        {
            FeatureCollection = new List<IPeerFeature>();
        }

        public static PeerSpec Deserialize(BinaryReader reader)
        {
            PeerSpec peerSpec = new PeerSpec();

            int agentLength = reader.ReadByte();
            byte[] agentBytes = reader.ReadBytes(agentLength);
            peerSpec.AgentName = Encoding.UTF8.GetString(agentBytes);

            byte[] versionBytes = reader.ReadBytes(3);
            peerSpec.Version = new Version(versionBytes[0], versionBytes[1], versionBytes[2]);

            int peerNameLength = reader.ReadByte();
            byte[] peerNameBytes = reader.ReadBytes(peerNameLength);
            peerSpec.PeerName = Encoding.UTF8.GetString(peerNameBytes);

            byte hasDeclaredAddress = reader.ReadByte();
            if (hasDeclaredAddress != 0)
            {
                byte length = reader.ReadByte();
                byte[] addressBytes = reader.ReadBytes(length - 2 - 2);

                int port = reader.Read7BitEncodedInt();
                IPAddress address = new IPAddress(addressBytes);
                peerSpec.DeclaredAddress = $"{address}:{port}";
            }

            int featureCount = reader.ReadByte();

            for (int i = 0; i < featureCount; i++)
            {
                int featureTypeVal = reader.ReadByte();
                int featureSize = reader.ReadByte();
                FeatureType featureType = (FeatureType)featureTypeVal;
                byte[] featureBytes = reader.ReadBytes(featureSize);
                IPeerFeature feature = PeerSpec.DeserializeFeature(featureType, featureBytes);
                peerSpec.FeatureCollection.Add(feature);
            }

            return peerSpec;
        }
    }

    public interface IPeerFeature
    {
        FeatureType FeatureType { get; }

        byte[] Serialize();

    }

    public class ModeFeature : IPeerFeature
    {
        public FeatureType FeatureType => FeatureType.Mode;
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    
                    byte featureLength = 0;

                    byte stateType = (byte)StateType;
                    byte verifying = 0;
                    if (VerifyingTransactions)
                    {
                        verifying = 1;
                    }

                    byte bootstrapped = 0;
                    byte[] suffix = null;
                    
                    
                    if (NiPoPoWBootstrapped)
                    {
                        bootstrapped = 1;
                        if (NipopowSuffix.HasValue)
                        {
                            suffix = NipopowSuffix.Value.Write7BitEncoded();
                            featureLength += (byte)suffix.Length;
                        }
                    }

                    byte[] blocksKept = BlocksToKeep.EncodeZigZag().Convert7BitEncoded();


                    bw.Write((byte)FeatureType);

                    featureLength += (byte)(3 + blocksKept.Length);

                    bw.Write(featureLength);
                    bw.Write(stateType);
                    bw.Write(verifying);
                    bw.Write(bootstrapped);
                    if (NiPoPoWBootstrapped && suffix != null)
                    {
                        bw.Write(suffix);
                    }

                    
                    bw.Write(blocksKept);
                }

                return ms.ToArray();
            }
        }

        public StateType StateType { get; set; }

        public bool VerifyingTransactions { get; set; }

        public bool NiPoPoWBootstrapped { get; set; }

        public long? NipopowSuffix { get; set; }

        public int BlocksToKeep { get; set; }

        public static ModeFeature Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    ModeFeature feature = new ModeFeature();
                    int state = reader.Read();
                    feature.StateType = state == 0 ? StateType.Utxo : StateType.Digest;

                    int verify = reader.Read();
                    feature.VerifyingTransactions = verify != 0;

                    int currentPosition = (int)reader.BaseStream.Position;

                    if (currentPosition < bytes.Length)
                    {
                        int bootstrapped = reader.Read();
                        feature.NiPoPoWBootstrapped = bootstrapped != 0;
                        
                        if (feature.NiPoPoWBootstrapped)
                        {
                            byte[] nipopowBytes = reader.ReadBytes(4);
                            feature.NipopowSuffix = Convert.ToInt64(nipopowBytes.Read7BitEncodedAsLong());
                        }

                        currentPosition = (int)reader.BaseStream.Position;

                        byte[] blocksBytes = reader.ReadBytes(bytes.Length - currentPosition);
                        feature.BlocksToKeep = Convert.ToInt32(blocksBytes.Read7BitEncodedAsLong()).DecodeZigZag();
                    }

                    return feature;
                }
            }
        }

    }

    public class PeerFeature : IPeerFeature
    {
        //private int maxSize = 512;

        public FeatureType FeatureType => FeatureType.Peer;
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((byte)FeatureType);
                    byte[] sessionBytes = SessionId.Write7BitEncoded();
                    bw.Write((byte)(NetworkMagic.Length + sessionBytes.Length));
                    bw.Write(NetworkMagic);
                    bw.Write(sessionBytes);
                }

                return ms.ToArray();
            }
        }

        public byte[] NetworkMagic { get; set; }

        public ulong SessionId { get; set; }

        public void SetNetworkType(NetworkType networkType)
        {
            NetworkMagic = networkType == NetworkType.Testnet ? 
                Constants.TestnetHeader :
                Constants.MainnetHeader;
        }

        public static PeerFeature Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    PeerFeature feature = new PeerFeature();
                    feature.NetworkMagic = reader.ReadBytes(4);
                    
                    byte[] sessionBytes = reader.ReadBytes(bytes.Length - 4);
                    feature.SessionId = sessionBytes.Read7BitEncodedAsULong();
                    return feature;
                }
            }
        }

        public PeerFeature()
        {
            Random rd = new Random();
            SessionId = (ulong) rd.NextInt64(0, long.MaxValue);
        }
    }

    public class LocalAddressPeerFeature : IPeerFeature
    {
        public FeatureType FeatureType => FeatureType.Address;
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((byte)FeatureType);

                    IPAddress address = IPAddress.Parse(Address);
                    byte[] addressBytes = address.GetAddressBytes();

                    byte[] portBytes = Port.Convert7BitEncoded();
                    
                    bw.Write((byte) (addressBytes.Length + portBytes.Length)); //Feature length
                    bw.Write(addressBytes);
                    bw.Write(portBytes);
                }

                return ms.ToArray();
            }
        }

        public string Address { get; set; }

        public int Port { get; set; }

        public static LocalAddressPeerFeature Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    LocalAddressPeerFeature peerFeature = new LocalAddressPeerFeature();
                    
                    byte[] addressBytes = reader.ReadBytes(bytes.Length - 2);
                    IPAddress address = new IPAddress(addressBytes);
                    peerFeature.Address = address.ToString();
                    peerFeature.Port = reader.Read7BitEncodedInt();

                    return peerFeature;
                }
            }
        }
    }
}
