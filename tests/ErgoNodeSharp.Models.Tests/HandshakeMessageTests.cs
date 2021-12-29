using System;
using System.Linq;
using ErgoNodeSharp.Common.Extensions;
using ErgoNodeSharp.Models.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErgoNodeSharp.Models.Tests
{
    [TestClass]
    public class HandshakeMessageTests
    {
        [TestMethod]
        public void CanDeserializeHandshake()
        {
            //HandshakeSpecification.scala
            string message = "bcd2919cee2e076572676f726566030306126572676f2d6d61696e6e65742d332e332e36000210040001000102067f000001ae46";
            string agentName = "ergoref";
            string peerName = "ergo-mainnet-3.3.6";
            Version version = new Version(3, 3, 6);

            HandshakeMessage handshakeMessage = HandshakeMessage.Deserialize(message);
            Assert.IsNotNull(handshakeMessage);
            Assert.IsNotNull(handshakeMessage.PeerSpec);
            Assert.AreEqual(agentName,handshakeMessage.PeerSpec.AgentName);
            Assert.AreEqual(peerName, handshakeMessage.PeerSpec.PeerName);
            Assert.AreEqual(version, handshakeMessage.PeerSpec.Version);
            Assert.AreEqual(1610134874428, handshakeMessage.UnixTimestamp);
            Assert.AreEqual(2, handshakeMessage.PeerSpec.FeatureCollection.Count);
            
            IPeerFeature? peerFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Mode);
            Assert.IsNotNull(peerFeature);

            peerFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Address);

            Assert.IsNotNull(peerFeature);

            peerFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Peer);

            Assert.IsNull(peerFeature);
        }

        [TestMethod]
        public void CanDeserializeCurrentHandshake()
        {
            string message =
                "8b8a86ded82f076572676f72656604000f136572676f2d746573746e65742d342e302e31350003100400010001030e020000028cd8a0bce2e8f28ed8010206c0a8f581bc46";
            
            string agentName = "ergoref";
            string peerName = "ergo-testnet-4.0.15";
            Version version = new Version(4, 0, 15);

            HandshakeMessage handshakeMessage = HandshakeMessage.Deserialize(message);
            Assert.IsNotNull(handshakeMessage);

            Assert.IsNotNull(handshakeMessage.PeerSpec);
            Assert.AreEqual(agentName, handshakeMessage.PeerSpec.AgentName);
            Assert.AreEqual(peerName, handshakeMessage.PeerSpec.PeerName);
            Assert.AreEqual(version, handshakeMessage.PeerSpec.Version);
            Assert.AreEqual(1638727255307, handshakeMessage.UnixTimestamp);
            Assert.AreEqual(3, handshakeMessage.PeerSpec.FeatureCollection.Count);

            IPeerFeature? genericFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Mode);
            Assert.IsNotNull(genericFeature);
            ModeFeature modeFeature = (ModeFeature)genericFeature;
            Assert.AreEqual(StateType.Utxo, modeFeature.StateType);
            Assert.AreEqual(true, modeFeature.VerifyingTransactions);
            Assert.AreEqual(-1, modeFeature.BlocksToKeep);

            genericFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Address);

            Assert.IsNotNull(genericFeature);
            LocalAddressPeerFeature localAddressPeerFeature = (LocalAddressPeerFeature) genericFeature;
            Assert.AreEqual("192.168.245.129", localAddressPeerFeature.Address);
            Assert.AreEqual(9020, localAddressPeerFeature.Port);

            genericFeature =
                handshakeMessage.PeerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Peer);

            Assert.IsNotNull(genericFeature);
            PeerFeature peerFeature = (PeerFeature)genericFeature;
            Assert.AreEqual(15572826588688428044UL, peerFeature.SessionId);
        }

        [TestMethod]
        public void CanDeserializeCurrentHandshake2()
        {
            string message =
                "8ce9929cde2f076572676f72656604000a136572676f2d746573746e65742d342e302e31330108904cdd64bc4602100400010001030e02000002ebd9ade79ca1e8d4fb01";
            HandshakeMessage handshakeMessage = HandshakeMessage.Deserialize(message);
            Assert.IsNotNull(handshakeMessage);
        }

        [TestMethod]
        public void CanSerializeAndDeserialize()
        {
            string message =
                "8b8a86ded82f076572676f72656604000f136572676f2d746573746e65742d342e302e31350003100400010001030e020000028cd8a0bce2e8f28ed8010206c0a8f581bc46";

            HandshakeMessage handshakeMessage = HandshakeMessage.Deserialize(message);

            byte[] bytes = handshakeMessage.Serialize();
            string deserializedMessage = bytes.ToHexString();

            HandshakeMessage handshakeMessage2 = HandshakeMessage.Deserialize(deserializedMessage);
            Assert.IsNotNull(handshakeMessage2);
            Assert.AreEqual(message.Length, deserializedMessage.Length);
            Assert.AreEqual(message, deserializedMessage);
        }
    }
}