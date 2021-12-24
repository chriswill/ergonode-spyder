using System.Net;
using ErgoNodeSharp.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErgoNodeSharp.Common.Tests
{
    [TestClass]
    public class AddressRangeTests
    {
        [TestMethod]
        public void CanTestLocalHostAddress()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            bool isPrivate = address.IsPrivate();
            Assert.AreEqual(true, isPrivate);
        }

        [TestMethod]
        public void CanTestPrivateAddress()
        {
            IPAddress address = IPAddress.Parse("169.254.38.147");
            bool isPrivate = address.IsPrivate();
            Assert.AreEqual(true, isPrivate);
        }
    }
}
