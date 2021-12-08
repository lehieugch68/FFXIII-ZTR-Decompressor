using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FFXIII_ZTR_Decompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Final Fantasy XIII ZTR Decompressor by LeHieu - VietHoaGame";
            if (args.Length > 0)
            {
                foreach (string file in args)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".txt")
                    {
                        string name = fileName.Split((char)46).First();
                        string lang = name.Substring(name.Length - 3);
                        int encodingCode;
                        if (!GameEncoding._EncodingCode.TryGetValue(lang, out encodingCode)) encodingCode = 65001;
                        string ztr = Path.Combine(Path.GetDirectoryName(file), $"{fileName}.ztr");
                        byte[] result = ZTR.Compressor(ztr, file, encodingCode);
                        File.WriteAllBytes($"{file}.ztr", result);
                    }
                    else if (ext == ".ztr")
                    {
                        string name = fileName.Split((char)46).First();
                        string lang = name.Substring(name.Length - 3);
                        int encodingCode;
                        if (!GameEncoding._EncodingCode.TryGetValue(lang, out encodingCode)) encodingCode = 65001;
                        string[] result = ZTR.Decompressor(file, encodingCode);
                        File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file), $"{fileName}.txt"), result);
                    }
                }
            }
            else
            {
                Console.WriteLine("Please drag and drop files/folder into this tool to unpack/repack.");
            }
            //Console.ReadKey();
        }
    }
}
