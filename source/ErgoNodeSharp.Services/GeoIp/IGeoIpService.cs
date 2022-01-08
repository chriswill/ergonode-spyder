using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ErgoNodeSharp.Services.GeoIp
{
    public interface IGeoIpService
    {
        public Task<GeoIpResponse?> GetGeoIpResponse(string address);
    }

    public class GeoIpService : IGeoIpService
    {
        private readonly string accessKey;
        private readonly ILogger<GeoIpService> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public GeoIpService(ErgoNodeSpyderConfiguration configuration, IHttpClientFactory httpClientFactory,
            ILogger<GeoIpService> logger)
        {
            accessKey = configuration.IpStackPassword;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<GeoIpResponse?> GetGeoIpResponse(string address)
        {
            logger.LogDebug("Performing Geo API request for {Address}", address);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.ipstack.com/{address}?access_key={accessKey}&fields=main,connection")
            {
                Headers =
                {
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                    { "Connection", "keep-alive" },
                    { "User-Agent", "NodeSpyder" }
                }
            };

            HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GeoIpResponse>(json);
            }

            string content = await httpResponseMessage.Content.ReadAsStringAsync();
            logger.LogError("Error making Geo API request: {StatusCode}", httpResponseMessage.StatusCode);
            logger.LogError(content);

            return null;
        }
    }
}
