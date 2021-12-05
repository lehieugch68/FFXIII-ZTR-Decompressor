using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Be.IO;

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
                if (dict.Dict.TryGetValue(valueFirst, out valueFirstKey)) value.AddRange(valueFirstKey);
                else value.Add(valueFirst);
                if (dict.Dict.TryGetValue(valueLast, out valueLastKey)) value.AddRange(valueLastKey);
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
                    if (textDict.Dict.TryGetValue(entry, out compressed)) totalBytes.AddRange(compressed);
                    else totalBytes.Add(entry);
                    pointer++;
                }
            }
            string[] result = new string[header.TextCount];
            Dictionary<string, byte[]> gameCode = GameEncoding.GetGameCode();
            int index = 0;
            for (int i = 0; i < result.Length; i++)
            {
                List<byte> textBytes = new List<byte>();
                while (index < totalBytes.Count - 1)
                {
                    if (totalBytes[index] == 0 && totalBytes[index + 1] == 0)
                    {
                        index += 2;
                        break;
                    }
                    else textBytes.Add(totalBytes[index++]);
                } 
                foreach (KeyValuePair<string, byte[]> entry in gameCode)
                {
                    byte[] temp = ByteArrayHandler.ReplaceBytes(textBytes.ToArray(), entry.Value, Encoding.UTF8.GetBytes(entry.Key));
                    if (temp != null) textBytes = temp.ToList();
                }
                result[i] = Encoding.UTF8.GetString(textBytes.ToArray());
            }
            return result.ToArray();
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
        public static byte[] Compressor(string ztr, string txt)
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
                    int uncompressedSize = 0;
                    for (int i = 0; i < idsDecompressed.Length; i++)
                    {
                        string value;
                        if (input.TryGetValue(idsDecompressed[i], out value))
                        {
                            foreach (KeyValuePair<string, byte[]> entry in gameCode)
                            {
                                value = value.Replace(entry.Key, Encoding.ASCII.GetString(entry.Value));
                            }
                            byte[] bs = Encoding.ASCII.GetBytes(value);
                            byte[] textBytes = new byte[bs.Length + 2];
                            bs.CopyTo(textBytes, 0);
                            inputText.Add(idsDecompressed[i], textBytes);
                            uncompressedSize += textBytes.Length;
                        } else inputText.Add(idsDecompressed[i], new byte[2]);
                    }
                    reader.BaseStream.Position = idsCompressedPointer;
                    byte[] idsCompressedData = reader.ReadBytes((int)(textCompressedPointer - idsCompressedPointer));

                    //Testing
                    int blockCount = (int)Math.Ceiling((double)uncompressedSize / 4096);

                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    writer.Write(header.Magic);
                    writer.Write(header.Version);
                    writer.Write(header.TextCount);
                    writer.Write(header.IDsDecompressedSize);
                    writer.Write(blockCount + 1);
                    long blockTextOffset = writer.BaseStream.Position;
                    writer.Write(new byte[(blockCount + 1) * 4]);
                    long textInfoOffset = writer.BaseStream.Position;
                    writer.Write(new byte[header.TextCount * 4]);
                    writer.Write(idsCompressedData);

                    //int index = 0;
                    int textIndex = 0;
                    int blockPointer = 0;
                    List<byte[]> texts = new List<byte[]>();
                    for (int i = 0; i < inputText.Count; i++)
                    {
                        texts.Add(inputText.ElementAt(i).Value);
                    }
                    for (int i = 0; i < blockCount; i++)
                    {
                        long blockOffset = writer.BaseStream.Position;
                        List<byte> uncompressedBlock = new List<byte>();
                        int start = textIndex;
                        while (uncompressedBlock.Count + inputText.ElementAt(textIndex).Value.Length < 4096 && textIndex < inputText.Count - 1)
                        {
                            uncompressedBlock.AddRange(inputText.ElementAt(textIndex++).Value);
                        }
                        int end = textIndex;
                        int textPointer = 0;
                        byte[] unusedBytes = GetUnusedBytes(uncompressedBlock.ToArray());
                        int unusedBytesIndex = 0;
                        List<byte> compressedBlock = new List<byte>();
                        Dictionary<byte, byte[]> dict = new Dictionary<byte, byte[]>();
                        for (int x = start; x <= end; x++)
                        {
                            bool optimized = false;
                            byte[] compressedText = texts[x];
                            foreach (KeyValuePair<byte, byte[]> entry in dict)
                            {
                                byte[] temp = ByteArrayHandler.ReplaceBytes(compressedText, entry.Value, new byte[] { entry.Key });
                                if (temp != null) compressedText = temp;
                            }
                            while (compressedText.Length >= 2 && !optimized && unusedBytesIndex < unusedBytes.Length)
                            {
                                optimized = !CompressionLevel.Increase(ref dict, compressedText, ref unusedBytes, unusedBytesIndex++);
                                if (!optimized)
                                {
                                    byte[] temp = ByteArrayHandler.ReplaceBytes(compressedText, dict.Last().Value, new byte[] { dict.Last().Key });
                                    if (temp != null) compressedText = temp;
                                }
                            }
                            textInfos[x].CompressedPointer = (ushort)textPointer;
                            textPointer += compressedText.Length;
                            compressedBlock.AddRange(compressedText);
                        }
                        writer.Write((int)(dict.Count * 3));
                        foreach (KeyValuePair<byte, byte[]> entry in dict)
                        {
                            writer.Write(entry.Key);
                            writer.Write(entry.Value);
                        }
                        writer.Write(compressedBlock.ToArray());
                        long pos = writer.BaseStream.Position;
                        writer.BaseStream.Position = blockTextOffset + ((i + 1) * 4);
                        blockPointer += (dict.Count * 3) + 4 + compressedBlock.Count;
                        writer.Write(blockPointer);
                        for (int x = start; x <= end; x++)
                        {
                            writer.BaseStream.Position = textInfoOffset + (x * 4);
                            writer.Write((byte)i);
                            writer.BaseStream.Position += 1;
                            writer.Write(textInfos[x].CompressedPointer);
                        }
                        writer.BaseStream.Position = pos;

                        /*writer.Write(dict.DictSize);
                        foreach (KeyValuePair<byte, byte[]> entry in dict.Dict)
                        {
                            writer.Write(entry.Key);
                            writer.Write(entry.Value);
                        }
                        long textOffset = writer.BaseStream.Position;
                        List<byte> compressedBlock = new List<byte>();
                        bool max = false;
                        while (index < inputText.Count && !max)
                        {
                            byte[] compressedText = inputText.ElementAt(index).Value;
                            foreach (KeyValuePair<byte, byte[]> entry in dict.Dict)
                            {

                                byte[] temp = ByteArrayHandler.ReplaceBytes(compressedText, entry.Value, new byte[] { entry.Key });
                                if (temp != null) compressedText = temp;
                            }
                            if (dict.DictSize + 4 + compressedBlock.Count + compressedText.Length <= 4096)
                            {
                                compressedBlock.AddRange(compressedText);
                                writer.BaseStream.Position = textInfoOffset + (index * 4);
                                writer.Write((byte)i);
                                if (index < inputText.Count - 1)
                                {
                                    writer.BaseStream.Position = textInfoOffset + ((index + 1) * 4) + 2;
                                    textPointer += compressedText.Length;
                                    writer.Write((ushort)(textPointer));
                                }
                                index++;
                            }
                            else
                            {
                                max = true;

                            }
                        }
                        writer.BaseStream.Position = textOffset;
                        writer.Write(compressedBlock.ToArray());
                        long pos = writer.BaseStream.Position;
                        writer.BaseStream.Position = blockTextOffset + ((i + 1) * 4);
                        blockPointer += dict.DictSize + 4 + compressedBlock.Count;
                        writer.Write(blockPointer);
                        writer.BaseStream.Position = pos;*/
                    }
                }
            }
            return result.ToArray();
        }
        private static byte[] GetUnusedBytes(byte[] input)
        {
            List<byte> result = new List<byte>();
            int find = 3;
            while (find < 0xFF)
            {
                if (!input.Contains(Convert.ToByte(find)))
                {
                    result.Add(Convert.ToByte(find));
                }
                find++;
            }
            return result.ToArray();
        }
    }
}
