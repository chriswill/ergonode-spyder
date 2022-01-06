using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using ErgoNodeSharp.Data.MsSql;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.DTO;
using ErgoNodeSharp.Models.Messages;
using ErgoNodeSharp.Models.Responses;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSharp.Data.Repositories.NodeInfo
{
    public class SqlServerNodeInfoRepository : INodeInfoRepository
    {
        private readonly ILogger<SqlServerNodeInfoRepository> logger;
        private readonly string connectionString;

        public SqlServerNodeInfoRepository(ErgoNodeSpyderConfiguration spyderConfiguration,
            ILogger<SqlServerNodeInfoRepository> logger)
        {
            connectionString = spyderConfiguration.ConnectionString;
            this.logger = logger;
        }

        public async Task<IEnumerable<NodeIdentifier>> GetAddressesForConnection(int topN = 10)
        {
            logger.LogDebug("Executing GetAddressesForConnection");
            string sql = @$"
  SELECT TOP ({topN}) [Address], [Port]
  FROM [dbo].[Nodes]
  WHERE PublicIP = 1
     AND (DatePeersQueried IS NULL OR DatePeersQueried < DATEADD(DAY, -1, GETUTCDATE()))
     AND (DateContactAttempted IS NULL OR DateContactAttempted < DATEADD(DAY, -1, GETUTCDATE()))
  ORDER BY NEWID()
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<NodeIdentifier>(sql);
            }
        }

        //Geo location will update every month
        public async Task<IEnumerable<string>> GetAddressesForGeoLookup(int topN = 10)
        {
            logger.LogDebug("Executing GetAddressesForGeoLookup");
            string sql = $@"
  SELECT TOP ({topN}) [Address]
  FROM [dbo].[Nodes]
  WHERE PublicIP = 1 AND (GeoDateUpdated IS NULL OR GeoDateUpdated < DATEADD(MONTH, -1, GETUTCDATE()))
  ORDER BY NEWID()
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<string>(sql);
            }

        }

        public async Task AddUpdateNode(PeerSpec peerSpec)
        {
            await AddUpdateNodes(new List<PeerSpec> { peerSpec }, null);
        }
        
        public async Task AddUpdateNodes(IEnumerable<PeerSpec> peerSpecs, string address)
        {
            DataTable dataTable = CreateNodeDataTable(peerSpecs);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@tvp", dataTable.AsTableValuedParameter("dbo.NodeTableType"));
                if (!string.IsNullOrEmpty(address))
                {
                    string[] parts = address.Split(":");
                    dynamicParameters.Add("@address", parts[0]);
                    dynamicParameters.Add("@port", int.Parse(parts[1]));
                }
                else
                {
                    dynamicParameters.Add("@address", null);
                    dynamicParameters.Add("@port", null);
                }

                await connection.ExecuteAsync("[dbo].[AddUpdateDiscoveredNodes]", dynamicParameters, null, null,
                    CommandType.StoredProcedure);
            }
        }

        public async Task RecordFailedConnection(string address)
        {
            string[] parts = address.Split(":");
            int port = int.Parse(parts[1]);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsyncWithRetry("dbo.RecordFailedConnection", new { address = parts[0], port },
                    null, null, CommandType.StoredProcedure);
            }
        }

        public async Task UpdateNodeGeo(GeoIpResponse response)
        {
            string command = @"
Update dbo.Nodes
SET
    ContinentCode = @continentCode,
    ContinentName = @continentName,
    CountryCode = @countryCode,
    CountryName = @countryName,
    RegionCode = @regionCode,
    RegionName = @regionName,
    City = @city,
    ZipOrPostalCode = @zip,
    Latitude = @latitude,
    Longitude = @longitude,
    ISP = @isp,
    GeoDateUpdated = GETUTCDATE()
WHERE
    Address = @address
";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsyncWithRetry(command, new
                {
                    address = response.IpAddress,
                    continentCode = response.ContinentCode,
                    continentName = response.ContinentName,
                    countryCode = response.CountryCode,
                    countryName = response.CountryName,
                    regionCode = response.RegionCode,
                    regionName = response.RegionName,
                    city = response.City,
                    zip = response.Zip,
                    latitude = response.Latitude,
                    longitude = response.Longitude,
                    isp = response.Connection?.Isp
                });
            }
        }

        public async Task PerformMaintenanceAndAnalytics()
        {
            logger.LogInformation("Executing PerformMaintenanceAndAnalytics");
            string sql = "dbo.DailyMaintenanceAndAnalytics";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.ExecuteAsyncWithRetry(sql, null, null, null, CommandType.StoredProcedure);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
        }

        private DataTable CreateNodeDataTable(IEnumerable<PeerSpec> nodes)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Address", typeof(string)) { MaxLength = 50 });
            table.Columns.Add(new DataColumn("Port", typeof(int)));
            table.Columns.Add(new DataColumn("PublicIp", typeof(bool)));
            table.Columns.Add(new DataColumn("AgentName", typeof(string)) { MaxLength = 100 });
            table.Columns.Add(new DataColumn("PeerName", typeof(string)) { MaxLength = 100 });
            table.Columns.Add(new DataColumn("Version", typeof(string)) { MaxLength = 20 });
            table.Columns.Add(new DataColumn("BlocksToKeep", typeof(int)));
            table.Columns.Add(new DataColumn("NiPoPoWBootstrapped", typeof(bool)));
            table.Columns.Add(new DataColumn("StateType", typeof(string)) { MaxLength = 10 });
            table.Columns.Add(new DataColumn("VerifyingTransactions", typeof(bool)));

            foreach (PeerSpec peerSpec in nodes)
            {
                ErgoNodeData nodeData = new ErgoNodeData(peerSpec);
                if ("ergo-spyder".Equals(peerSpec.AgentName)) continue;
                if (nodeData.Address == "0.0.0.0") continue;

                DataRow row = table.NewRow();
                row["Address"] = nodeData.Address;
                row["Port"] = nodeData.Port;
                row["PublicIp"] = nodeData.PublicIp;
                row["AgentName"] = nodeData.AgentName;
                row["PeerName"] = nodeData.PeerName;
                row["Version"] = nodeData.Version;
                row["BlocksToKeep"] = nodeData.BlocksToKeep;
                row["NiPoPoWBootstrapped"] = nodeData.NiPoPoWBootstrapped;
                row["StateType"] = nodeData.StateType;
                row["VerifyingTransactions"] = nodeData.VerifyingTransactions;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
