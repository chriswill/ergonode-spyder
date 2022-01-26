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

        public async Task<int> GetAddressCountForConnection()
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method GetAddressCountForConnection");
            string sql = @"dbo.GetNodeCountForUpdate";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteScalarAsyncWithRetry<int>(sql, null, null, null, CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<NodeIdentifier>> GetAddressesForConnection(int topN = 10)
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method GetAddressesForConnection");
            string sql = "[dbo].[GetNodesForUpdate]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<NodeIdentifier>(sql, new {topN}, null, new int(), CommandType.StoredProcedure);
            }
        }

        //Geo location will update every month
        public async Task<IEnumerable<string>> GetAddressesForGeoLookup(int topN = 10)
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method GetAddressesForGeoLookup");
            string sql = "[dbo].[GetAddressesForGeoLookup]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                return await connection.QueryAsyncWithRetry<string>(sql, new {topN}, null, null, CommandType.StoredProcedure);
            }

        }

        public async Task RecordHandshake(PeerSpec peerSpec, string address)
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method RecordHandshake method for {address}", address);
            //sets the value of the declared address to whatever address we thought we were connected with
            peerSpec.DeclaredAddress = address;

            DataTable dataTable = CreateNodeDataTable(new List<PeerSpec>{peerSpec});
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@tvp", dataTable.AsTableValuedParameter("dbo.NodeTableType"));
                
                await connection.ExecuteAsyncWithRetry("[dbo].[RecordNodeHandshake]", dynamicParameters, null, null,
                    CommandType.StoredProcedure);
            }
        }
        
        public async Task AddUpdatePeers(IEnumerable<PeerSpec> peerSpecs, string address)
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method AddUpdatePeers");
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

                await connection.ExecuteAsyncWithRetry("[dbo].[AddUpdateDiscoveredNodes]", dynamicParameters, null, null,
                    CommandType.StoredProcedure);
            }
        }

        public async Task RecordFailedConnection(string address)
        {
            logger.LogDebug("Executing SqlServerNodeInfoRepository method RecordFailedConnection");
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
            logger.LogDebug("Executing SqlServerNodeInfoRepository method UpdateNodeGeo");
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
            logger.LogInformation("Executing SqlServerNodeInfoRepository method PerformMaintenanceAndAnalytics");
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
