using System.Collections.Generic;
using System.Threading.Tasks;
using ErgoNodeSharp.Models.Responses.NodeSpyder;

namespace ErgoNodeSharp.Data
{
    public interface INodeReportingRepository
    {
        Task<IEnumerable<NodeInfo>> GetNodeInfos();
        Task<int> GetNodeInfoCount();

        Task<IEnumerable<StringValuePair>> GetVersionCount(int count = 5);
        Task<int> GetVersionRowCount();

        Task<IEnumerable<GeoSummary>> GetContinentCount();

        Task<IEnumerable<GeoSummary>> GetCountryCount(int count = 5);
        Task<int> GetCountryRowCount();

        Task<IEnumerable<GeoSummary>> GetRegionCount(string countryCode, int count = 5);
        Task<int> GetRegionRowCount(string countryCode);

        Task<IEnumerable<StringValuePair>> GetStateTypeCount();

        Task<IEnumerable<BoolValuePair>> GetVerifyingCount();

        Task<IEnumerable<IntValuePair>> GetBlocksKeptCount();

        Task<IEnumerable<StringValuePair>> GetIspCount(string countryCode = null, int count = 10);
        Task<int> GetIspRowCount(string countryCode = null);

        Task<IEnumerable<Location>> GetIspLocations(string countryCode);

        Task<IEnumerable<CountryInfo>> GetCountryInfo(string countryCode);

        Task<IEnumerable<StringValuePair>> GetDailyCount(int days = 10);
        Task<int> GetDailyRowCount();

        Task<IEnumerable<StringValuePair>> GetMonthCount(int months = 6);
        Task<int> GetMonthRowCount();

        Task<IEnumerable<StringValuePair>> GetWeekCount(int weeks = 12);
        Task<int> GetWeekRowCount();

        Task<string> GetCountyName(string countryCode);
    }
}
