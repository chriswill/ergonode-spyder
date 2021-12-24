using System.IO;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class IntegerExtensions
    {
        public static int DecodeZigZag(this int val)
        {
            return (val >> 1) ^ -(val & 1);
        }

        public static int EncodeZigZag(this int val)
        {
            return (val >> 31) ^ (val << 1);
        }

        public static byte[] Convert7BitEncoded(this int val)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write7BitEncodedInt(val);
                }
                return ms.ToArray();
            }
        }

        public static byte[] ToByteArray(this int value)
        {
            return new byte[] {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            };
        }
    }
}
