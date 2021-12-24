using Newtonsoft.Json;

namespace ErgoNodeSharp.Models.Responses
{
    public class GeoIpResponse
    {
        [JsonProperty("ip")]
        public string IpAddress { get; set; }

        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }

        [JsonProperty("continent_name")]
        public string ContinentName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("region_code")]
        public string RegionCode { get; set; }

        [JsonProperty("region_name")]
        public string RegionName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("connection")]
        public Connection Connection { get; set; }

        public GeoIpResponse()
        {
            Connection = new Connection();
        }
    }

    public class Connection
    {
        [JsonProperty("asn")]
        public int Asn { get; set; }

        [JsonProperty("isp")]
        public string Isp { get; set; }
    }
}
