using System;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class StringExtensions
    {
        public static byte[] FromHexString(this string source)
        {
            int numberChars = source.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(source.Substring(i, 2), 16);
            return bytes;
        }
    }
}
