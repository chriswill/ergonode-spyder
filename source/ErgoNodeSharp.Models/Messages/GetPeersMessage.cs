using System;

namespace ErgoNodeSharp.Models.Messages
{
    public class GetPeersMessage : NodeMessage
    {
        public override string MessageName => "GetPeers";

        public override MessageType MessageType => MessageType.GetPeers;

        protected override byte[] SerializeBody()
        {
            return Array.Empty<byte>();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            
        }
    }
}
