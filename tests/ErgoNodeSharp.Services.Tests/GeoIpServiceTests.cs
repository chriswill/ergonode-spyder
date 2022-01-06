using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.Responses;
using ErgoNodeSharp.Services.GeoIp;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ErgoNodeSharp.Services.Tests
{
    [TestClass]
    public class GeoIpServiceTests
    {
        [TestMethod]
        public async Task CanGetGeoIpInformation()
        {
            ErgoNodeSpyderConfiguration configuration = new ErgoNodeSpyderConfiguration();
            configuration.IpStackPassword = "123456";

            GeoIpResponse response = new GeoIpResponse();
            response.City = "MyCity";
            response.Connection.Isp = "BigIsp";
            response.ContinentCode = "NA";
            response.ContinentName = "North America";
            response.CountryCode = "US";
            response.CountryName = "United States";
            response.RegionCode = "CA";
            response.RegionName = "California";
            response.Zip = "93422";
            response.IpAddress = "162.251.188.10";

            IHttpClientFactory httpClientFactoryMock = A.Fake<IHttpClientFactory>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json")
            });

            HttpClient fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            A.CallTo(httpClientFactoryMock.CreateClient()).WithReturnType<HttpClient>().Returns(fakeHttpClient);

            ILoggerFactory factory = new NullLoggerFactory();
            ILogger<GeoIpService> logger = factory.CreateLogger<GeoIpService>();
            
            IGeoIpService geoIpService = new GeoIpService(configuration, httpClientFactoryMock, logger);
            GeoIpResponse? geoIspResponse = await geoIpService.GetGeoIpResponse("162.251.188.10");
            Assert.IsNotNull(geoIspResponse);
            Assert.IsInstanceOfType(geoIspResponse, typeof(GeoIpResponse));
            Assert.AreEqual("MyCity", geoIspResponse.City);
            Assert.AreEqual("BigIsp", geoIspResponse.Connection.Isp);
            Assert.AreEqual("NA", geoIspResponse.ContinentCode);
            Assert.AreEqual("North America", geoIspResponse.ContinentName);
            Assert.AreEqual("US", geoIspResponse.CountryCode);
            Assert.AreEqual("United States", geoIspResponse.CountryName);
            Assert.AreEqual("CA", geoIspResponse.RegionCode);
            Assert.AreEqual("California", geoIspResponse.RegionName);
            Assert.AreEqual("93422", geoIspResponse.Zip);
            Assert.AreEqual("162.251.188.10", geoIspResponse.IpAddress);
        }
    }
}