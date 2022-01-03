using System.Collections.Generic;
using System.Threading.Tasks;
using ErgoNodeSharp.Data.MsSql;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.Responses.NodeSpyder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSharp.Data.Repositories.NodeReporting
{
    public class SqlServerNodeReportingRepository : INodeReportingRepository
    {
        private readonly string connectionString;
        private readonly ILogger<SqlServerNodeReportingRepository> logger;

        public SqlServerNodeReportingRepository(SpyderAppConnection appConnection,
            ILogger<SqlServerNodeReportingRepository> logger)
        {
            connectionString = appConnection.ConnectionString;
            this.logger = logger;
        }

        public async Task<IEnumerable<Models.Responses.NodeSpyder.NodeInfo>> GetNodeInfos()
        {
            string sql = @"
SELECT [Address]
      ,[Port]
      ,[PublicIP]
      ,[AgentName]
      ,[PeerName]
      ,[Version]
      ,[BlocksToKeep]
      ,[NiPoPoWBootstrapped]
      ,[StateType]
      ,[VerifyingTransactions]
      ,[PeerCount]
      ,[ContinentCode]
      ,[CountryCode]
      ,[ContinentName]
      ,[CountryName]
      ,[RegionCode]
      ,[RegionName]
      ,[City]
      ,[ZipOrPostalCode]
      ,[Latitude]
      ,[Longitude]
      ,[ISP]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<Models.Responses.NodeSpyder.NodeInfo>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetVersionCount()
        {
            string sql = @"
  Select [Version] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  Group by [Version]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetContinentCount()
        {
            string sql = @"
  Select ISNULL([ContinentName],'Unknown') as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE PublicIp = 1
  Group by [ContinentName]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetCountryCount()
        {
            string sql = @"
  Select ISNULL([CountryName],'Unknown') as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE PublicIp = 1
  Group by [CountryName]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetStateTypeCount()
        {
            string sql = @"
  Select [StateType] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  Group by [StateType]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<BoolValuePair>> GetVerifyingCount()
        {
            string sql = @"
  Select [VerifyingTransactions] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  Group by [VerifyingTransactions]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<BoolValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetIspCount(int count = 10)
        {
            string sql = @$"
  Select TOP({count}) ISNULL([ISP],'Unknown') as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE PublicIp = 1
  Group by [ISP]
  Order by Count(*) desc
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetDailyCount(int days = 10)
        {
            string sql = @$"
  Select TOP({days}) CONVERT(varchar, [Day], 23) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByDay] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetMonthCount(int months = 6)
        {
            string sql = @$"
  Select TOP({months}) (Cast([Month] as varchar) + '-' + Cast([Year] as varchar)) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByMonth] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetWeekCount(int weeks = 12)
        {
            string sql = @$"
  Select TOP({weeks}) (Cast([Week] as varchar) + '-' + Cast([Year] as varchar)) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByWeek] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }
    }
}
