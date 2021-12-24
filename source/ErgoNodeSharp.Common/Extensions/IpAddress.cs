using System.Net;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class IPAddressExtensions
    {
        public static bool IsPrivate(this IPAddress ipAddress)
        {
            if (ipAddress.Equals(IPAddress.Loopback)) return true;
            IpAddressRange range1 = new IpAddressRange(IPAddress.Parse("10.0.0.0"), IPAddress.Parse("10.255.255.255"));
            IpAddressRange range2 = new IpAddressRange(IPAddress.Parse("172.16.0.0"), IPAddress.Parse("172.31.255.255"));
            IpAddressRange range3 = new IpAddressRange(IPAddress.Parse("169.254.0.0"), IPAddress.Parse("169.254.255.255"));
            IpAddressRange range4 = new IpAddressRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"));

            return range1.IsInRange(ipAddress) || range2.IsInRange(ipAddress) || range3.IsInRange(ipAddress) || range4.IsInRange(ipAddress);
        }
    }
}
