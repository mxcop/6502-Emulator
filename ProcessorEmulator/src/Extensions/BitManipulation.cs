using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessorEmulator.Extensions
{
    public static class BM
    {
        /// <summary>
        /// Set single bit in byte.
        /// </summary>
        public static void SetBit(ref byte aByte, byte pos, bool value)
        {
            if (value)
            {
                // left-shift 1, then bitwise OR
                aByte = (byte)(aByte | (1 << pos));
            }
            else
            {
                // left-shift 1, then take complement, then bitwise AND
                aByte = (byte)(aByte & ~(1 << pos));
            }
        }

        /// <summary>
        /// Read single bit from byte.
        /// </summary>
        public static bool GetBit(byte aByte, int pos)
        {
            // left-shift 1, then bitwise AND, then check for non-zero
            return ((aByte & (1 << pos)) != 0);
        }

        /// <summary>
        /// Convert byte into readable hex string.
        /// </summary>
        public static string ByteToHex(byte aByte)
        {
            StringBuilder hex = new StringBuilder(2);
            hex.AppendFormat("{0:x2}", aByte);
            return hex.ToString().ToUpper();
        }

        /// <summary>
        /// Convert ushort into readable hex string.
        /// </summary>
        public static string UShortToHex(ushort aShort)
        {
            StringBuilder hex = new StringBuilder(4);
            hex.AppendFormat("{0:x4}", aShort);
            return hex.ToString().ToUpper();
        }

        /// <summary>
        /// Convert byte array to a readable hex string.
        /// </summary>
        public static string BytesToHex(byte[] aBytes, string seperator = " ")
        {
            StringBuilder hex = new StringBuilder(aBytes.Length * 2);
            for (int b = 0; b < aBytes.Length; b++)
            {
                hex.AppendFormat("{0:x2}", aBytes[b]);

                if (b < aBytes.Length - 1)
                    hex.Append(seperator);
            }
            return hex.ToString().ToUpper();
        }

        /// <summary>
        /// Combine a high and low byte (litte endian)
        /// </summary>
        /// <returns></returns>
        public static ushort CombineBytes(byte high, byte low)
        {
            return (ushort)(((high) & 0xFF) << 8 | (low) & 0xFF);
        }
    }
}
