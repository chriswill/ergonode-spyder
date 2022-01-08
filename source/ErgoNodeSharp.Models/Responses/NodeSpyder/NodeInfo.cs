using System;

namespace ErgoNodeSharp.Models.Responses.NodeSpyder
{
    public class NodeInfo
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

        public int? PeerCount { get; set; }

        public DateTime DateAdded { get; set; }

        public DateTime? DateUpdated { get; set; }

        public string ContinentCode { get; set; }

        public string ContinentName { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string RegionCode { get; set; }

        public string RegionName { get; set; }

        public string City { get; set; }

        public string ZipOrPostalCode { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string Isp { get; set; }

    }
}
