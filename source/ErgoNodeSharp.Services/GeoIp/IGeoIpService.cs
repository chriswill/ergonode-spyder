using ErgoNodeSharp.Models.Configuration;
using ErgoNodeSharp.Models.Responses;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSharp.Services.GeoIp
{
    public interface IGeoIpService
    {
        public Task<GeoIpResponse> GetGeoIpResponse(string address);
    }

    public class GeoIpService : IGeoIpService
    {
        private readonly string accessKey;
        private readonly ILogger<GeoIpService> logger;

        public GeoIpService(ErgoNodeSpyderConfiguration configuration, ILogger<GeoIpService> logger)
        {
            accessKey = configuration.IpStackPassword;
            this.logger = logger;
        }

        public async Task<GeoIpResponse> GetGeoIpResponse(string address)
        {
            logger.LogDebug("Performing Geo API request for {0}", address);
            try
            {
                GeoIpResponse response =
                    await $"https://api.ipstack.com/{address}?access_key={accessKey}&fields=main,connection"
                        .GetJsonAsync<GeoIpResponse>();

                return response;
            }
            catch (FlurlHttpException ex)
            {
                string errorResponse = await ex.GetResponseStringAsync();
                logger.LogError(ex, "Error making Geo API request");
                logger.LogError(errorResponse);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error making Geo API request");
                throw;
            }
        }
        
    }
}
