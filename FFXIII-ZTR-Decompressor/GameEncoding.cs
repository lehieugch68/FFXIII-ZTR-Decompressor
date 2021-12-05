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
            gameCode.Add("®", new byte[] { 0x85, 0x6E });
            gameCode.Add("é", new byte[] { 0x85, 0xC8 });

            gameCode.Add("「", new byte[] { 0x81, 0x75 });
            gameCode.Add("」", new byte[] { 0x81, 0x76 });
            gameCode.Add("･", new byte[] { 0x81, 0x45 });
            gameCode.Add("★", new byte[] { 0x81, 0x9A });

            gameCode.Add("{Color Gold}", new byte[] { 0xF9, 0x42 });
            gameCode.Add("{Color SkyBlue}", new byte[] { 0xF9, 0x40 });
            gameCode.Add("{Color Red}", new byte[] { 0xF9, 0x43 });
            gameCode.Add("{Color BlizzardBlue}", new byte[] { 0xF9, 0x41 });
            gameCode.Add("{Color Yellow}", new byte[] { 0xF9, 0x44 });
            gameCode.Add("{Color Green}", new byte[] { 0xF9, 0x45 });
            gameCode.Add("{Color WhiteGreen}", new byte[] { 0xF9, 0x4D });
            gameCode.Add("{Color WhitePurple}", new byte[] { 0xF9, 0x4C });
            gameCode.Add("{Color Rose}", new byte[] { 0xF9, 0x48 });
            gameCode.Add("{Color Purple}", new byte[] { 0xF9, 0x49 });
            gameCode.Add("{Color Sand}", new byte[] { 0xF9, 0x47 });
            gameCode.Add("{Color SandOrange}", new byte[] { 0xF9, 0x4A });
            gameCode.Add("{Color GoldDark}", new byte[] { 0xF9, 0x55 });
            gameCode.Add("{Color RedDark}", new byte[] { 0xF9, 0x56 });
            gameCode.Add("{Color Transparent}", new byte[] { 0xF9, 0x4E });
            gameCode.Add("{Color OliveDark}", new byte[] { 0xF9, 0x52 });
            gameCode.Add("{Color WhiteGray}", new byte[] { 0xF9, 0x4B });

            gameCode.Add("{VarF4 64}", new byte[] { 0xF4, 0x40 });
            gameCode.Add("{VarF4 65}", new byte[] { 0xF4, 0x41 });
            gameCode.Add("{VarF4 66}", new byte[] { 0xF4, 0x42 });
            gameCode.Add("{VarF2 90}", new byte[] { 0xF2, 0x5A });
            gameCode.Add("{VarF2 114}", new byte[] { 0xF2, 0x72 });
            gameCode.Add("{VarF2 116}", new byte[] { 0xF2, 0x74 });
            gameCode.Add("{VarF2 91}", new byte[] { 0xF2, 0x5B });
            gameCode.Add("{VarF2 92}", new byte[] { 0xF2, 0x5C });
            gameCode.Add("{VarF2 95}", new byte[] { 0xF2, 0x5F });
            gameCode.Add("{VarFF 208}", new byte[] { 0xFF, 0xD0 });
            gameCode.Add("{VarFF 255}", new byte[] { 0xFF, 0xFF });
            gameCode.Add("{VarFF 241}", new byte[] { 0xFF, 0xF1 });
            gameCode.Add("{VarF7 64}", new byte[] { 0xF7, 0x40 });
            gameCode.Add("{VarF7 65}", new byte[] { 0xF7, 0x41 });
            gameCode.Add("{VarF6 64}", new byte[] { 0xF6, 0x40 });

            gameCode.Add("{Var83 182}", new byte[] { 0x83, 0xB6 });

            gameCode.Add("{Icon Eye01}", new byte[] { 0xF0, 0x61 });
            gameCode.Add("{Icon Gunblade}", new byte[] { 0xF0, 0x49 });
            gameCode.Add("{Icon Boomerang}", new byte[] { 0xF0, 0x4C });
            gameCode.Add("{Icon Wrench}", new byte[] { 0xF0, 0x53 });
            gameCode.Add("{Icon Ring}", new byte[] { 0xF0, 0x58 });
            gameCode.Add("{Icon Bracert}", new byte[] { 0xF0, 0x57 });
            gameCode.Add("{Icon Material01}", new byte[] { 0xF0, 0x56 });
            gameCode.Add("{Icon Screw}", new byte[] { 0xF0, 0x55 });
            gameCode.Add("{Icon Note}", new byte[] { 0xF0, 0x54 });
            gameCode.Add("{Icon Attention}", new byte[] { 0xF0, 0x41 });
            gameCode.Add("{Icon Doc}", new byte[] { 0xF0, 0x46 });
            gameCode.Add("{Icon Earring}", new byte[] { 0xF0, 0x59 });
            gameCode.Add("{Icon Brooch}", new byte[] { 0xF0, 0x5A });
            gameCode.Add("{Icon Shotgun}", new byte[] { 0xF0, 0x4A });
            gameCode.Add("{Icon Exclamation}", new byte[] { 0xF0, 0x42 });

            gameCode.Add("{Key Start}", new byte[] { 0xF1, 0x44 });
            gameCode.Add("{Key Square}", new byte[] { 0xF1, 0x42 });
            gameCode.Add("{Key L1}", new byte[] { 0xF1, 0x46 });
            gameCode.Add("{Key L2}", new byte[] { 0xF1, 0x48 });
            gameCode.Add("{Key R1}", new byte[] { 0xF1, 0x47 });
            gameCode.Add("{Key R2}", new byte[] { 0xF1, 0x49 });
            gameCode.Add("{Key Cross}", new byte[] { 0xF1, 0x40 });
            gameCode.Add("{Key Triangle}", new byte[] { 0xF1, 0x43 });
            gameCode.Add("{Key Circle}", new byte[] { 0xF1, 0x41 });
            gameCode.Add("{Key LeftRightPad}", new byte[] { 0xF1, 0x5F });
            gameCode.Add("{Key LSPress}", new byte[] { 0xF1, 0x54 });
            gameCode.Add("{Key Analog}", new byte[] { 0xF1, 0x5B });
            gameCode.Add("{Key Right}", new byte[] { 0xF1, 0x4C });
            gameCode.Add("{Key Left}", new byte[] { 0xF1, 0x4A });
            gameCode.Add("{Key Select}", new byte[] { 0xF1, 0x45 });
            gameCode.Add("{Key RSUp}", new byte[] { 0xF1, 0x59 });
            gameCode.Add("{Key RSDown}", new byte[] { 0xF1, 0x57 });
            gameCode.Add("{Key RSPress}", new byte[] { 0xF1, 0x55 });
            gameCode.Add("{Key LStick}", new byte[] { 0xF1, 0x5C });
            gameCode.Add("{Key 97}", new byte[] { 0xF1, 0x61 });
            gameCode.Add("{Key NPad}", new byte[] { 0xF1, 0x5D });

            gameCode.Add("{End}", new byte[] { 0x0 });
            gameCode.Add("{Escape}", new byte[] { 0x1 });
            gameCode.Add("{Italic}", new byte[] { 0x2 });
            gameCode.Add("{Many}", new byte[] { 0x3 });
            gameCode.Add("{Article}", new byte[] { 0x4 });
            gameCode.Add("{ArticleMany}", new byte[] { 0x5 });

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
