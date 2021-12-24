using System.IO;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class LongExtensions
    {
        public static byte[] Write7BitEncoded(this long value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write7BitEncodedInt64(value);
                }
                return ms.ToArray();
            }
        }

        public static byte[] Write7BitEncoded(this ulong value)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Write out an int 7 bits at a time. The high bit of the byte,
                // when on, tells reader to continue reading more bytes.
                //
                // Using the constants 0x7F and ~0x7F below offers smaller
                // codegen than using the constant 0x80.

                while (value > 0x7Fu)
                {
                    ms.WriteByte((byte)((uint)value | ~0x7Fu));
                    value >>= 7;
                }

                ms.WriteByte((byte)value);
                return ms.ToArray();
            }

                
        }
    }
}
