// AlphaLegends
// https://github.com/The-Alpha-Project/alpha-legends

namespace MapTools.DBC.Reader
{
    class DBCHeader
    {
        public string Signature;
        public uint RecordCount;
        public uint FieldCount;
        public uint RecordSize;
        public uint StringBlockSize;
        public bool IsValidDbcFile => Signature == "WDBC";
        public bool IsValidDb2File => Signature == "WDB2";
    }
}
