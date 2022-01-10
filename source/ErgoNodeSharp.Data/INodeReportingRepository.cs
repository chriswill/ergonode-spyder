using System.Collections.Generic;
using System.Threading.Tasks;
using ErgoNodeSharp.Models.Responses.NodeSpyder;

namespace ErgoNodeSharp.Data
{
    public interface INodeReportingRepository
    {
        Task<IEnumerable<NodeInfo>> GetNodeInfos();

        Task<IEnumerable<StringValuePair>> GetVersionCount(int count = 5);

        Task<IEnumerable<GeoSummary>> GetContinentCount();

        Task<IEnumerable<GeoSummary>> GetCountryCount(int count = 5);

        Task<IEnumerable<GeoSummary>> GetRegionCount(string countryCode, int count = 5);

        Task<IEnumerable<StringValuePair>> GetStateTypeCount();

        Task<IEnumerable<BoolValuePair>> GetVerifyingCount();

        Task<IEnumerable<IntValuePair>> GetBlocksKeptCount();

        Task<IEnumerable<StringValuePair>> GetIspCount(int count = 10);

        Task<IEnumerable<StringValuePair>> GetDailyCount(int days = 10);

        Task<IEnumerable<StringValuePair>> GetMonthCount(int months = 6);

        Task<IEnumerable<StringValuePair>> GetWeekCount(int weeks = 12);
    }
}
