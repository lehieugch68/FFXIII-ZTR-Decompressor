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
            string[] test = ZTR.Decompressor(@"D:\VietHoaGame\Lightning Returns\ZTR Test\resident\system\txtres_us.ztr");

            File.WriteAllLines(@"D:\VietHoaGame\Lightning Returns\ZTR Test\resident\system\txtres_us.ztr.txt", test);
            /*string[] test = ZTR.Decompressor(@"D:\VietHoaGame\Lightning Returns\ZTR Test\txtres_us_-_Copy.txt - Copy.ztr");

            File.WriteAllLines(@"D:\VietHoaGame\Lightning Returns\ZTR Test\txtres_us_-_Copy.txt - Copy.ztr.txt", test);*/

            /*byte[] com = ZTR.Compressor(@"D:\VietHoaGame\Lightning Returns\ZTR Test\resident\system\txtres_us.ztr", @"D:\VietHoaGame\Lightning Returns\ZTR Test\resident\system\txtres_us.ztr.txt");
            File.WriteAllBytes(@"D:\VietHoaGame\Lightning Returns\ZTR Test\resident\system\txtres_us.ztr.txt.ztr", com);*/
            /*byte[] com = ZTR.Compressor(@"D:\VietHoaGame\Lightning Returns\ZTR Test\txtres_us_-_Copy.ztr", @"D:\VietHoaGame\Lightning Returns\ZTR Test\txtres_us_-_Copy.txt");
            File.WriteAllBytes(@"D:\VietHoaGame\Lightning Returns\ZTR Test\txtres_us_-_Copy.txt - Copy.ztr", com);*/
            Console.ReadKey();
        }
    }
}
