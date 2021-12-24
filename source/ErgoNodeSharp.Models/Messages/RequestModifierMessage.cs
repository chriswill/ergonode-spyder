using System;

namespace ErgoNodeSharp.Models.Messages
{
    public class RequestModifierMessage: NodeMessage
    {
        public override string MessageName => "RequestModifier";
        public override MessageType MessageType => MessageType.RequestModifier;
        protected override byte[] SerializeBody()
        {
            throw new NotImplementedException();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            
        }
    }
}
