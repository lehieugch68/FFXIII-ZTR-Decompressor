using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;
using System.Globalization;

namespace FFXIII_ZTR_Decompressor
{
    public static class ZTR
    {
        private struct Header
        {
            public int Magic;
            public int Version;
            public int TextCount;
            public int IDsDecompressedSize;
            public int TextBlocksCount;
            public int[] TextBlocksPointer;
        }
        private struct TextInfo
        {
            public byte Block;
            public byte BlockOffset;
            public ushort CompressedPointer;
        }
        private struct CompressDictionary
        {
            public int DictSize;
            public Dictionary<byte, byte[]> Dict;
        }
        private static Header ReadHeader(ref BeBinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Header header = new Header();
            header.Magic = reader.ReadInt32();
            header.Version = reader.ReadInt32();
            header.TextCount = reader.ReadInt32();
            header.IDsDecompressedSize = reader.ReadInt32();
            header.TextBlocksCount = reader.ReadInt32();
            header.TextBlocksPointer = new int[header.TextBlocksCount];
            for (int i = 0; i < header.TextBlocksCount; i++)
            {
                header.TextBlocksPointer[i] = reader.ReadInt32();
            }
            return header;
        }
        private static TextInfo[] ReadTextInfos(ref BeBinaryReader reader, Header header)
        {
            reader.BaseStream.Seek(0x14 + (header.TextBlocksCount * 4), SeekOrigin.Begin);
            TextInfo[] textInfos = new TextInfo[header.TextCount];
            for (int i = 0; i < textInfos.Length; i++)
            {
                textInfos[i].Block = reader.ReadByte();
                textInfos[i].BlockOffset = reader.ReadByte();
                textInfos[i].CompressedPointer = reader.ReadUInt16();
            }
            return textInfos;
        }
        private static CompressDictionary ReadDictionary(ref BeBinaryReader reader)
        {
            CompressDictionary dict = new CompressDictionary();
            dict.DictSize = reader.ReadInt32();
            dict.Dict = new Dictionary<byte, byte[]>();
            for (int i = 0; i < dict.DictSize / 3; i++)
            {
                byte key = reader.ReadByte();
                List<byte> value = new List<byte>();
                byte valueFirst = reader.ReadByte();
                byte valueLast = reader.ReadByte();
                byte[] valueFirstKey;
                byte[] valueLastKey;
                if (dict.Dict.TryGetValue(valueFirst, out valueFirstKey)) foreach (byte b in valueFirstKey) value.Add(b);
                else value.Add(valueFirst);
                if (dict.Dict.TryGetValue(valueLast, out valueLastKey)) foreach (byte b in valueLastKey) value.Add(b);
                else value.Add(valueLast);
                dict.Dict.Add(key, value.ToArray());
            }
            return dict;
        }
        private static string[] DecompressIDs(ref BeBinaryReader reader, Header header)
        {
            List<byte> totalBytes = new List<byte>();
            int blockCount = (int)Math.Ceiling((double)header.IDsDecompressedSize / 4096);
            for (int i = 0; i < blockCount; i++)
            {
                CompressDictionary idsDict = ReadDictionary(ref reader);
                int blockLen = 0;
                while (totalBytes.Count < header.IDsDecompressedSize && blockLen < 4096)
                {
                    byte entry = reader.ReadByte();
                    byte[] compressed;
                    if (idsDict.Dict.TryGetValue(entry, out compressed))
                    {
                        foreach (byte b in compressed) totalBytes.Add(b);
                        blockLen += compressed.Length;
                    }
                    else
                    {
                        totalBytes.Add(entry);
                        blockLen++;
                    }
                }
            }
            string[] result = Encoding.ASCII.GetString(totalBytes.ToArray()).Split((char)0);
            return result.Take(result.Length - 1).ToArray();
        }
        private static string[] DecompressText(ref BeBinaryReader reader, Header header)
        {
            List<byte> totalBytes = new List<byte>();
            long pointer = 0;
            for (int i = 0; i < header.TextBlocksPointer.Length - 1; i++)
            {
                long endBlockPointer = header.TextBlocksPointer[i + 1];
                CompressDictionary textDict = ReadDictionary(ref reader);
                pointer += textDict.DictSize + 4;
                while (pointer < endBlockPointer)
                {
                    byte entry = reader.ReadByte();
                    byte[] compressed;
                    if (textDict.Dict.TryGetValue(entry, out compressed)) foreach (byte b in compressed) totalBytes.Add(b);
                    else totalBytes.Add(entry);
                    pointer++;
                }
            }
            string[] result = Encoding.ASCII.GetString(totalBytes.ToArray()).Split(new string[] { $"{(char)0}{(char)0}" }, StringSplitOptions.None);
            Dictionary<string, byte[]> gameCode = GameEncoding.GetGameCode();
            for (int i = 0; i < result.Length; i++)
            {
                foreach (KeyValuePair<string, byte[]> entry in gameCode)
                {
                    result[i] = result[i].Replace(Encoding.ASCII.GetString(entry.Value), entry.Key);
                }
            }
            return result.Take(result.Length - 1).ToArray();
        }
        public static string[] Decompressor(string input)
        {
            using (FileStream stream = File.OpenRead(input))
            {
                BeBinaryReader reader = new BeBinaryReader(stream);
                Header header = ReadHeader(ref reader);
                TextInfo[] textInfos = ReadTextInfos(ref reader, header);
                string[] idsDecompressed = DecompressIDs(ref reader, header);
                string[] textDecompressed = DecompressText(ref reader, header);
                string[] result = new string[idsDecompressed.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = $"/*{idsDecompressed[i]}\n{textDecompressed[i]}\n*/";
                }

                reader.Close();
                return result;
            }
        }
        public static byte[] Compressor(string ztr, string txt, int compressLevel = 0)
        {
            MemoryStream result = new MemoryStream();
            Dictionary<string, string> input = new Dictionary<string, string>();
            using (FileStream txtStream = File.OpenRead(txt))
            {
                using (StreamReader sr = new StreamReader(txtStream))
                {
                    string line = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        if (line.StartsWith("/*"))
                        {
                            string key = line.Substring(2);
                            List<string> value = new List<string>();
                            try
                            {
                                while (!(line = sr.ReadLine()).EndsWith("*/"))
                                {
                                    value.Add(line);
                                }
                            } catch { }
                            input.Add(key, string.Join("\n", value.ToArray()));
                            //Console.WriteLine(string.Join("\n", value.ToArray()));
                        }
                        else line = sr.ReadLine();
                    }
                }
            }
            using (BeBinaryWriter writer = new BeBinaryWriter(result))
            {
                using (FileStream ztrStream = File.OpenRead(ztr))
                {
                    BeBinaryReader reader = new BeBinaryReader(ztrStream);
                    Header header = ReadHeader(ref reader);
                    TextInfo[] textInfos = ReadTextInfos(ref reader, header);
                    long idsCompressedPointer = reader.BaseStream.Position;
                    string[] idsDecompressed = DecompressIDs(ref reader, header);
                    long textCompressedPointer = reader.BaseStream.Position;
                    Dictionary<string, byte[]> inputText = new Dictionary<string, byte[]>();
                    Dictionary<string, byte[]> gameCode = GameEncoding.GetGameCode();
                    for (int i = 0; i < idsDecompressed.Length; i++)
                    {
                        string value;
                        if (input.TryGetValue(idsDecompressed[i], out value))
                        {
                            foreach (KeyValuePair<string, byte[]> entry in gameCode)
                            {
                                value = value.Replace(entry.Key, Encoding.ASCII.GetString(entry.Value));
                            }
                            inputText.Add(idsDecompressed[i], Encoding.ASCII.GetBytes(value));
                        } else inputText.Add(idsDecompressed[i], Array.Empty<byte>());
                    }
                    reader.BaseStream.Position = idsCompressedPointer;
                    byte[] idsCompressedData = reader.ReadBytes((int)(textCompressedPointer - idsCompressedPointer));
                    //
                }
            }
            return result.ToArray();
        }
        private static CompressDictionary DictionaryGenerator(byte[] textData, int compressLevel)
        {
            //
        }
        private static byte[] GetDictionaryValue(byte[] data)
        {
            int len = data.Length - 1;
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
            if (found == 0) return Array.Empty<byte>();
            byte firstByte = byte.Parse(found.ToString("X2").Substring(2, 2), NumberStyles.HexNumber);
            byte lastByte = byte.Parse(found.ToString("X2").Substring(0, 2), NumberStyles.HexNumber);
            return new byte[] { firstByte, lastByte };
        }
    }
}
