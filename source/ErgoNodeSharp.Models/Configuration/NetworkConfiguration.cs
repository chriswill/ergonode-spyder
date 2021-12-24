using System.Collections.Generic;

namespace ErgoNodeSharp.Models.Configuration
{
    public class NetworkConfiguration
    {
        public string BindAddress { get; set; }

        public string DeclaredAddress { get; set; }

        public string NodeName { get; set; }

        public List<string> KnownPeers { get; set; }

        public string AppVersion { get; set; }

        public string AgentName { get; set; }

        public NetworkConfiguration()
        {
            KnownPeers = new List<string>();
        }
    }
}
