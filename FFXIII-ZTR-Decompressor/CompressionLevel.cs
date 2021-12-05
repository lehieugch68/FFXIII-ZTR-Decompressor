using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Be.IO;
using System.Globalization;

namespace FFXIII_ZTR_Decompressor
{
    public static class CompressionLevel
    {
        /*private static List<byte[]> _Default = new List<byte[]>()
        { 
            new byte[] { 0x0, 0x0 },
            new byte[] { 0x0, 0x1 },
            new byte[] { 0x2E, 0x20 },
            new byte[] { 0x2C, 0x20 },
            new byte[] { 0x21, 0x20 },
            new byte[] { 0x3F, 0x20 },
            new byte[] { 0x40, 0x72 },
            new byte[] { 0x21, 0x3F },
        };*/
        public static bool Increase(ref Dictionary<byte, byte[]> dict, byte[] input, ref byte[] unusedBytes, int unusedBytesIndex)
        {
            byte[] value = GetDictionaryValue(input);
            if (unusedBytes.Length < 1 || value == null || dict.Values.Any(v => v.SequenceEqual(value))) return false;
            dict.Add(unusedBytes[unusedBytesIndex], value);
            return true;
        }
        private static byte[] GetDictionaryValue(byte[] data)
        {
            int len = data.Length - 1;
            if (len < 1) return null;
            int max = 0;
            int found = 0;
            int[] dict = new int[ushort.MaxValue + 1];
            for (int i = 0; i < len; i++)
            {
                int num = data[i] | (data[i + 1] << 8);
                int cur = ++dict[num];
                if (cur > max)
                {
                    max = cur;
                    found = num;
                }
            }
            string hex = found == 0 ? "0000" : found.ToString("X2");
            byte firstByte = byte.Parse(hex.Length == 2 ? hex.Substring(1, hex.Length - 1) : hex.Substring(2, hex.Length - 2), NumberStyles.HexNumber);
            byte lastByte = byte.Parse(hex.Length == 2 ? hex.Substring(0, hex.Length - 1) : hex.Substring(0, hex.Length - 2), NumberStyles.HexNumber);
            return new byte[] { firstByte, lastByte };
        }
    }
}
