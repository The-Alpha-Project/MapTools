// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;

namespace AlphaCoreExtractor.Core
{
    public class C3Vector
    {
        public float x;
        public float y;
        public float z;

        /// <summary>
        /// Vector3
        /// </summary>
        /// <param name="toWowCoords">To get WoW coords, read it as: {Y, Z, X}</param>
        public C3Vector(BinaryReader reader, bool toWowCoords = false)
        {
            if (!toWowCoords)
            {
                z = reader.ReadSingle();
                y = reader.ReadSingle();
                z = reader.ReadSingle();
            }
            else
            {
                y = reader.ReadSingle();
                z = reader.ReadSingle();
                x = reader.ReadSingle();
            }
        }
    }
}
