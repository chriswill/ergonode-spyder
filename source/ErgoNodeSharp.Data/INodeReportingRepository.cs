using System.Collections.Generic;
using System.Threading.Tasks;
using ErgoNodeSharp.Models.Responses.NodeSpyder;

namespace ErgoNodeSharp.Data
{
    public interface INodeReportingRepository
    {
        Task<IEnumerable<NodeInfo>> GetNodeInfos();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetVersionCount();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetContinentCount();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetCountryCount();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetStateTypeCount();

        Task<IEnumerable<SimpleKeyValuePair<bool>>> GetVerifyingCount();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetIspCount();

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetDailyCount(int days = 10);

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetMonthCount(int months = 6);

        Task<IEnumerable<SimpleKeyValuePair<string>>> GetWeekCount(int weeks = 12);
    }
}
