using System;
using System.IO;

namespace ErgoNodeSharp.Common.Extensions
{
    public static class ArrayExtensions
    {
        public static string ToHexString(this byte[] source)
        {
            return BitConverter
                .ToString(source)
                .Replace("-", "")
                .ToLower();
        }
        public static byte[] ToByteArray(this sbyte[] source)
        {
            byte[] unsigned = new byte[source.Length];
            Buffer.BlockCopy(source, 0, unsigned, 0, source.Length);
            return unsigned;
        }

        public static sbyte[] ToSignedByteArray(this byte[] source)
        {
            sbyte[] signed = new sbyte[source.Length];
            Buffer.BlockCopy(source, 0, signed, 0, source.Length);
            return signed;
        }

        /// <summary>
        /// Concatenates two given byte arrays and returns a new byte array containing all the elements. 
        /// </summary>
        /// <remarks>
        /// This is a lot faster than Linq (~30 times)
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="firstArray">First set of bytes in the final array.</param>
        /// <param name="secondArray">Second set of bytes in the final array.</param>
        /// <returns>The concatenated array of bytes.</returns>
        public static byte[] ConcatFast(this byte[] firstArray, byte[] secondArray)
        {
            if (firstArray == null)
                throw new ArgumentNullException(nameof(firstArray), "First array can not be null!");
            if (secondArray == null)
                throw new ArgumentNullException(nameof(secondArray), "Second array can not be null!");


            byte[] result = new byte[firstArray.Length + secondArray.Length];
            Buffer.BlockCopy(firstArray, 0, result, 0, firstArray.Length);
            Buffer.BlockCopy(secondArray, 0, result, firstArray.Length, secondArray.Length);
            return result;
        }
        
        /// <summary>
        /// Creates a new array from the given array by taking a specified number of items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), "Input can not be null!");
            if (index < 0 || count < 0)
                throw new IndexOutOfRangeException("Index or count can not be negative.");
            if (sourceArray.Length != 0 && index > sourceArray.Length - 1 || sourceArray.Length == 0 && index != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");
            if (count > sourceArray.Length - index)
                throw new IndexOutOfRangeException("Array is not long enough.");


            byte[] result = new byte[count];
            Buffer.BlockCopy(sourceArray, index, result, 0, count);
            return result;
        }

        public static List<int> IndexOfSequence(this byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }

        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            if (end < 0)
                end = source.Length;

            var len = end - start;

            // Return new array.
            var res = new T[len];
            for (var i = 0; i < len; i++) res[i] = source[i + start];
            return res;
        }

        public static T[] Slice<T>(this T[] source, int start)
        {
            return Slice<T>(source, start, -1);
        }

        public static int Read7BitEncodedAsInt(this byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader reader = new BinaryReader(ms))
                {
                    return reader.Read7BitEncodedInt();
                }
            }
        }

        public static long Read7BitEncodedAsLong(this byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (BinaryReader reader = new BinaryReader(ms))
                {
                    return reader.Read7BitEncodedInt64();
                }
            }
        }

        public static ulong Read7BitEncodedAsULong(this byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Seek(0, SeekOrigin.Begin);

                ulong result = 0;
                byte byteReadJustNow;

                // Read the integer 7 bits at a time. The high bit
                // of the byte when on means to continue reading more bytes.
                //
                // There are two failure cases: we've read more than 10 bytes,
                // or the tenth byte is about to cause integer overflow.
                // This means that we can read the first 9 bytes without
                // worrying about integer overflow.

                const int MaxBytesWithoutOverflow = 9;
                for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
                {
                    // ReadByte handles end of stream cases for us.
                    byteReadJustNow = (byte)ms.ReadByte();
                    result |= (byteReadJustNow & 0x7Ful) << shift;

                    if (byteReadJustNow <= 0x7Fu)
                    {
                        return result; // early exit
                    }
                }

                // Read the 10th byte. Since we already read 63 bits,
                // the value of this byte must fit within 1 bit (64 - 63),
                // and it must not have the high bit set.

                byteReadJustNow = (byte)ms.ReadByte();
                if (byteReadJustNow > 0b_1u)
                {
                    throw new FormatException("Invalid format");
                }

                result |= (ulong)byteReadJustNow << (MaxBytesWithoutOverflow * 7);
                return result;
            }
        }

        // packing an array of 4 bytes to an int, big endian, clean code
        public static int FromByteArray(this byte[] bytes)
        {
            return ((bytes[0] & 0xFF) << 24) |
                   ((bytes[1] & 0xFF) << 16) |
                   ((bytes[2] & 0xFF) << 8) |
                   ((bytes[3] & 0xFF) << 0);
        }
    }
}
