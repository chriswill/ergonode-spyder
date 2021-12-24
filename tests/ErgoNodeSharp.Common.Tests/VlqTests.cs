using ErgoNodeSharp.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ErgoNodeSharp.Common.Tests
{
    [TestClass]
    public class VlqTests
    {

        [TestMethod]
        public void CanDecodeBytes()
        {
            string s = "bcd2919cee2e";
            byte[] bytes = s.FromHexString();
            
            Assert.AreEqual(1610134874428L,bytes.Read7BitEncodedAsLong());
        }

        [TestMethod]
        public void CanEncodeBytes()
        {
            string s = "bcd2919cee2e";
            byte[] originalBytes = s.FromHexString();
            long val = 1610134874428;
            byte[] decodedBytes = val.Write7BitEncoded();
            CollectionAssert.AreEqual(originalBytes, decodedBytes);
        }
    }
}