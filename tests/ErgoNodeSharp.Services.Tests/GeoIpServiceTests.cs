using System.Threading.Tasks;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.Responses;
using ErgoNodeSharp.Services.GeoIp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErgoNodeSharp.Services.Tests
{
    [TestClass, Ignore]
    public class GeoIpServiceTests
    {
        [TestMethod]
        public async Task CanGetGeoIpInformation()
        {
            ErgoNodeSpyderConfiguration configuration = new ErgoNodeSpyderConfiguration();
            configuration.IpStackPassword = "";

            ILoggerFactory factory = new NullLoggerFactory();
            ILogger<GeoIpService> logger = factory.CreateLogger<GeoIpService>();

            IGeoIpService geoIpService = new GeoIpService(configuration, logger);
            GeoIpResponse response = await geoIpService.GetGeoIpResponse("162.251.188.10");
            Assert.IsNotNull(response);
        }
    }
}