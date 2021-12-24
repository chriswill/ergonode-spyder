using System.Net;
using System.Net.Sockets;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class TcpClientExtensions
    {
        public static string GetAddress(this TcpClient client)
        {
            string address = "unknown IP";

            try
            {
                if (client.Client.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    address = ipEndPoint.Address.IsIPv4MappedToIPv6 ? 
                        $"{ipEndPoint.Address.MapToIPv4()}:{ipEndPoint.Port}" : 
                        $"{ipEndPoint.Address}:{ipEndPoint.Port}";
                }
            }
            catch { }
            
            return address;
        }
    }
}
