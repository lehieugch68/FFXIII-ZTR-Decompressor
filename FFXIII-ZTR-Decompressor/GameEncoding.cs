using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIII_ZTR_Decompressor
{
    public class GameEncoding
    {
        private static Dictionary<string, byte[]> _Instance;
        private static Dictionary<string, byte[]> Instance()
        {
            Dictionary<string, byte[]> gameCode = new Dictionary<string, byte[]>();

            gameCode.Add("—", new byte[] { 0x85, 0x57 });
            gameCode.Add("{End}", new byte[] { 0x0 });
            gameCode.Add("{Escape}", new byte[] { 0x1 });
            gameCode.Add("{Italic}", new byte[] { 0x2 });
            gameCode.Add("\n", new byte[] { 0x40, 0x72 });

            return gameCode;

        }
        public static Dictionary<string, byte[]> GetGameCode()
        {
            if (_Instance == null)
            {
                _Instance = Instance();
            }
            return _Instance;
        }
    }
}
