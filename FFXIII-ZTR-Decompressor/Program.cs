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
                    string name = fileName.Split((char)46).First();
                    string lang = name.Substring(name.Length - 3);
                    if (!GameEncoding.EncodingCode.TryGetValue(lang, out int encodingCode)) encodingCode = 65001;

                    switch (ext)
                    {
                        case ".txt":
                        {
                            string ztr = Path.Combine(Path.GetDirectoryName(file), $"{fileName}.ztr");
                            byte[] result = ZTR.Compressor(ztr, file, encodingCode);
                            File.WriteAllBytes($"{file}.ztr", result);
                            break;
                        }
                        case ".ztr":
                        {
                            string[] result = ZTR.Decompressor(file, encodingCode);
                            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file), $"{fileName}.txt"), result);
                            break;
                        }
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
