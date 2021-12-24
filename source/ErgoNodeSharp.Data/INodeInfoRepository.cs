using System.Collections.Generic;
using System.Threading.Tasks;
using ErgoNodeSharp.Models.DTO;
using ErgoNodeSharp.Models.Messages;
using ErgoNodeSharp.Models.Responses;

namespace ErgoNodeSharp.Data
{
    public interface INodeInfoRepository
    {
        public Task<IEnumerable<NodeIdentifier>> GetAddressesForConnection(int topN = 10);

        public Task<IEnumerable<string>> GetAddressesForGeoLookup(int topN = 10);

        public Task AddUpdateNode(PeerSpec node);

        public Task AddUpdateNodes(IEnumerable<PeerSpec> nodes, string address);

        public Task RecordFailedConnection(string address);

        public Task UpdateNodeGeo(GeoIpResponse response);

        public Task UpdateDateTables();
    }
}