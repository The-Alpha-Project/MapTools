// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Runtime.Serialization;
using WoWFormatParser.Structures.WDT;

namespace MapTools.DBC.Structures
{
    public class DBCMap
    {
        public uint ID;
        public string Directory;
        public uint PVP;
        public uint IsInMap;
        public string MapName_enUS;
        public string MapName_enGB;
        public string MapName_koKR;
        public string MapName_frFR;
        public string MapName_deDE;
        public string MapName_enCN;
        public string MapName_zhCH;
        public string MapName_enTW;
        public uint MapName_Mask;

        [IgnoreDataMember]
        public WDT WDT { get; private set; }

        [IgnoreDataMember]
        public string Name { get; set; }

        public void BindWDT(WDT wdt)
        {
            WDT = wdt;

            if (this.MapName_enUS.Equals(Path.GetFileNameWithoutExtension(wdt.FileName)))
                Name = MapName_enUS;
            else
                Name = Path.GetFileNameWithoutExtension(wdt.FileName);
        }
    }
}
