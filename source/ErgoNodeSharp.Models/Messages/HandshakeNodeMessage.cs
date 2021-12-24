using System;

namespace ErgoNodeSharp.Models.Messages
{
    public class HandshakeNodeMessage : NodeMessage
    {
        public override string MessageName => "Handshake";

        public override MessageType MessageType => MessageType.Handshake;

        protected override byte[] SerializeBody()
        {
            throw new NotImplementedException();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            
        }
    }
}
