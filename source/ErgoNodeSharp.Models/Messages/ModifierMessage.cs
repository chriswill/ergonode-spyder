using System;

namespace ErgoNodeSharp.Models.Messages
{
    public class ModifierMessage : NodeMessage
    {
        public override string MessageName => "Modifier";
        public override MessageType MessageType => MessageType.Modifier;
        protected override byte[] SerializeBody()
        {
            throw new NotImplementedException();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            
        }
    }
}
