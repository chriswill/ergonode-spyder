using ErgoNodeSharp.Models.Messages;

namespace ErgoNodeSharp.Models.Configuration
{
    public class NodeConfiguration
    {
        public StateType StateType { get; set; }

        public bool VerifyTransactions { get; set; }

        public int BlocksToKeep { get; set; }

        public bool PoPoWBootstrap { get; set; }
    }
}
