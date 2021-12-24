using System.Net;
using System.Net.Sockets;

namespace ErgoNodeSharp.Common
{
    public class IpAddressRange
    {
        readonly AddressFamily addressFamily;
        readonly byte[] lowerBytes;
        readonly byte[] upperBytes;

        public IpAddressRange(IPAddress lowerInclusive, IPAddress upperInclusive)
        {
            // Assert that lower.AddressFamily == upper.AddressFamily

            addressFamily = lowerInclusive.AddressFamily;
            lowerBytes = lowerInclusive.GetAddressBytes();
            upperBytes = upperInclusive.GetAddressBytes();
        }

        public bool IsInRange(IPAddress address)
        {
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            byte[] addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (int i = 0; i < lowerBytes.Length &&
                            (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }
    }
}
