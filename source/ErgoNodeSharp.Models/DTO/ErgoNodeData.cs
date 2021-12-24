using System;
using System.Linq;
using System.Net;
using ErgoNodeSharp.Common.Extensions;
using ErgoNodeSharp.Models.Messages;

namespace ErgoNodeSharp.Models.DTO
{
    public class ErgoNodeData
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public bool PublicIp { get; set; }
        
        public string AgentName { get; set; }

        public string PeerName { get; set; }

        public string Version { get; set; }

        public int BlocksToKeep { get; set; }

        public bool NiPoPoWBootstrapped { get; set; }

        public string StateType { get; set; }

        public bool VerifyingTransactions { get; set; }

        public ErgoNodeData()
        {

        }

        public ErgoNodeData(PeerSpec peerSpec)
        {
            if (!string.IsNullOrEmpty(peerSpec.DeclaredAddress))
            {
                string[] parts = peerSpec.DeclaredAddress.Split(":");
                Address = parts[0];
                Port = int.Parse(parts[1]);
            } else if (peerSpec.FeatureCollection.Any(x => x.FeatureType == FeatureType.Address))
            {
                LocalAddressPeerFeature localAddress = (LocalAddressPeerFeature)
                    peerSpec.FeatureCollection.First(x => x.FeatureType == FeatureType.Address);

                Address = localAddress.Address;
                Port = localAddress.Port;
            }

            if (string.IsNullOrEmpty(Address)) throw new Exception("Address cannot be determined");

            IPAddress ipAddress = IPAddress.Parse(Address);
            PublicIp = !ipAddress.IsPrivate();

            ModeFeature modeFeature = peerSpec.FeatureCollection.FirstOrDefault(x => x.FeatureType == FeatureType.Mode) as ModeFeature;
            if (modeFeature != null)
            {
                BlocksToKeep = modeFeature.BlocksToKeep;
                NiPoPoWBootstrapped = modeFeature.NiPoPoWBootstrapped;
                StateType = modeFeature.StateType.ToString();
                VerifyingTransactions = modeFeature.VerifyingTransactions;
            }

            AgentName = peerSpec.AgentName;
            PeerName = peerSpec.PeerName;
            Version = peerSpec.Version.ToString(3);
        }
    }
}
