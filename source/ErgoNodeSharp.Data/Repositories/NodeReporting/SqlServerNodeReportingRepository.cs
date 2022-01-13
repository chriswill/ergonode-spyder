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
      ,[AgentName]
      ,[PeerName]
      ,[Version]
      ,[BlocksToKeep]
      ,[NiPoPoWBootstrapped]
      ,[StateType]
      ,[VerifyingTransactions]
      ,[PeerCount]
      ,[DateAdded]
      ,[DateUpdated]
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

        public async Task<int> GetNodeInfoCount()
        {
            string sql = @"
  SELECT COUNT(*)
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetVersionCount(int count = 5)
        {
            string sql = @$"
  SELECT TOP ({count}) [Version] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  GROUP BY [Version]
  ORDER BY Count(*) DESC, [Version] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<int> GetVersionRowCount()
        {
            string sql = @"
  SELECT COUNT(DISTINCT([Version]))
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<GeoSummary>> GetContinentCount()
        {
            string sql = @"
  SELECT ISNULL([ContinentCode],'Unknown') as [Code], ISNULL([ContinentName],'Unknown') as [Name], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  GROUP BY [ContinentCode], [ContinentName]
  ORDER BY Count(*) DESC, [ContinentName] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<GeoSummary>(sql);
            }
        }

        public async Task<IEnumerable<GeoSummary>> GetCountryCount(int count = 5)
        {
            string sql = @$"
  SELECT TOP ({count}) ISNULL([CountryCode],'Unknown') as [Code], ISNULL([CountryName],'Unknown') as [Name], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK) 
  GROUP BY [CountryCode], [CountryName]
  ORDER BY Count(*) DESC, [CountryName] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<GeoSummary>(sql);
            }
        }

        public async Task<int> GetCountryRowCount()
        {
            string sql = @"
  SELECT COUNT(DISTINCT(ISNULL([CountryCode],'Unknown')))
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)   
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<GeoSummary>> GetRegionCount(string countryCode, int count = 5)
        {
            string sql = @$"
  SELECT TOP ({count}) ISNULL([RegionCode],'Unknown') as [Code], ISNULL([RegionName],'Unknown') as [Name], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE CountryCode = @countryCode
  GROUP BY [RegionCode], [RegionName]
  ORDER BY Count(*) DESC, [RegionName] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<GeoSummary>(sql, new {countryCode});
            }
        }

        public async Task<int> GetRegionRowCount(string countryCode)
        {
            string sql = @"
  SELECT COUNT(DISTINCT(ISNULL([RegionCode],'Unknown')))
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)   
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetStateTypeCount()
        {
            string sql = @"
  SELECT [StateType] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  GROUP BY [StateType]
  ORDER BY Count(*) DESC, [StateType] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<IEnumerable<BoolValuePair>> GetVerifyingCount()
        {
            string sql = @"
  SELECT [VerifyingTransactions] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  GROUP BY [VerifyingTransactions]
  ORDER BY Count(*) DESC, [VerifyingTransactions] ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<BoolValuePair>(sql);
            }
        }

        public async Task<IEnumerable<IntValuePair>> GetBlocksKeptCount()
        {
            string sql = @"
  SELECT [BlocksToKeep] as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  GROUP BY [BlocksToKeep]
  ORDER BY Count(*) DESC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<IntValuePair>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetIspCount(string countryCode = null, int count = 10)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                string sql = @$"
  SELECT TOP ({count}) ISNULL([ISP],'Unknown') as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK) 
  GROUP BY [ISP]
  ORDER BY Count(*) DESC, ISNULL([ISP],'Unknown') ASC
";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
                }
            }
            else
            {
                string sql = @$"
  SELECT TOP ({count}) ISNULL([ISP],'Unknown') as [Key], Count(*) as [Value]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE CountryCode = @countryCode
  GROUP BY [ISP]
  ORDER BY Count(*) DESC, ISNULL([ISP],'Unknown') ASC
";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    return await connection.QueryAsyncWithRetry<StringValuePair>(sql, new {countryCode});
                }
            }
        }

        public async Task<int> GetIspRowCount(string countryCode = null)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                string sql = @"
  SELECT COUNT(DISTINCT(ISNULL([ISP],'Unknown')))
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)   
";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
                }
            }
            else
            {
                string sql = @"
  SELECT COUNT(DISTINCT(ISNULL([ISP],'Unknown')))
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE CountryCode = @countryCode
";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    return await connection.ExecuteScalarAsyncWithRetry<int>(sql, new { countryCode });
                }
            }

        }

        public async Task<IEnumerable<Location>> GetIspLocations(string countryCode)
        {
            string sql = @"
  SELECT DISTINCT Latitude, Longitude
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE CountryCode = @countryCode  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<Location>(sql, new { countryCode });
            }
        }

        public async Task<IEnumerable<CountryInfo>> GetCountryInfo(string countryCode)
        {
            string sql = @"
  SELECT IsNull(RegionName, 'Unknown') as [Region], IsNull(City, 'Unknown') as [City], Count(*) as [Count]
  FROM [dbo].[ActiveNodes] WITH (NOLOCK)
  WHERE CountryCode = @countryCode 
  GROUP BY RegionName, City
  ORDER BY Count(*) DESC, RegionName ASC
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<CountryInfo>(sql, new { countryCode });
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetDailyCount(int days = 10)
        {
            string sql = @$"
  SELECT TOP ({days}) CONVERT(varchar, [Day], 23) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByDay] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<int> GetDailyRowCount()
        {
            string sql = @"
  SELECT COUNT(*)
  FROM [dbo].[NodesByDay] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetMonthCount(int months = 6)
        {
            string sql = @$"
  SELECT TOP ({months}) (Cast([Year] as [varchar](4)) + '-' + FORMAT([Month], 'd2')) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByMonth] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<int> GetMonthRowCount()
        {
            string sql = @"
  SELECT COUNT(*)
  FROM [dbo].[NodesByMonth] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<IEnumerable<StringValuePair>> GetWeekCount(int weeks = 12)
        {
            string sql = @$"
  SELECT TOP ({weeks}) CONVERT([varchar], DATEADD(wk, DATEDIFF(wk, 6, DATEFROMPARTS([Year], 1, 1)) + ([Week]-1), 6),23) as [Key], NodeCount as [Value]
  FROM [dbo].[NodesByWeek] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<StringValuePair>(sql);
            }
        }

        public async Task<int> GetWeekRowCount()
        {
            string sql = @"
  SELECT COUNT(*)
  FROM [dbo].[NodesByWeek] WITH (NOLOCK)  
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql);
            }
        }

        public async Task<string> GetCountyName(string countryCode)
        {

            string sql = @"
SELECT TOP(1) CountryName
FROM dbo.Nodes WITH (NOLOCK)  
WHERE CountryCode = @countryCode
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<string>(sql, new {countryCode});
            }

        }
    }
}
