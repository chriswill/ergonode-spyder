namespace ErgoNodeSharp.Models.Configuration
{
    public class ErgoNodeSpyderConfiguration
    {
        public string DatabaseType { get; set; }
        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }
        public bool PerformGeoIpLookup { get; set; }

        public string IpStackPassword { get; set; }
    }
}
