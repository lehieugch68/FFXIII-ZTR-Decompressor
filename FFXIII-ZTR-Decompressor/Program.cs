using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FFXIII_ZTR_Decompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] test = ZTR.Decompressor(@"C:\Users\LeHieu\Downloads\auto_yus_us_compressed\auto_yus_us-bak.ztr");

            File.WriteAllLines(@"C:\Users\LeHieu\Downloads\auto_yus_us_compressed\auto_yus_us.txt", test);

            //ZTR.Compressor(@"C:\Users\LeHieu\Downloads\auto_yus_us_compressed\auto_yus_us-bak.ztr", @"C:\Users\LeHieu\Downloads\auto_yus_us_compressed\auto_yus_us.txt");
            Console.ReadKey();
        }
    }
}
