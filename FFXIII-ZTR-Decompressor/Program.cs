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
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".txt")
                    {
                        string ztr = Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.ztr");
                        byte[] result = ZTR.Compressor(ztr, file);
                        File.WriteAllBytes($"{file}.ztr", result);
                    }
                    else if (ext == ".ztr")
                    {
                        string[] result = ZTR.Decompressor(file);
                        File.WriteAllLines(Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.txt"), result);
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
