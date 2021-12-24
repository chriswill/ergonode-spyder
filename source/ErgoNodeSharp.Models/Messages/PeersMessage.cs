using System;
using System.Collections.Generic;
using System.IO;

namespace ErgoNodeSharp.Models.Messages
{
    public class PeersMessage : NodeMessage
    {
        public override string MessageName => "Peers";
        public override MessageType MessageType => MessageType.Peers;

        public List<PeerSpec> Peers { get; set; }

        public PeersMessage()
        {
            Peers = new List<PeerSpec>();
        }

        protected override byte[] SerializeBody()
        {
            throw new NotImplementedException();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            List<PeerSpec> peers = new List<PeerSpec>();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    int count = reader.Read7BitEncodedInt();
                    for (int i = 0; i < count; i++)
                    {
                        PeerSpec peerSpec = PeerSpec.Deserialize(reader);
                        peers.Add(peerSpec);
                    }
                }
            }

            Peers = peers;
        }
    }
    
}
