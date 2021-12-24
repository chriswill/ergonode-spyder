namespace ErgoNodeSharp.Models.Messages
{
    public class SyncMessage : NodeMessage
    {
        public override MessageType MessageType => MessageType.Sync;

        public override string MessageName => "Sync";

        protected override byte[] SerializeBody()
        {
            throw new System.NotImplementedException();
        }

        public override void DeserializeBody(byte[] bytes)
        {
            
        }
    }
}
