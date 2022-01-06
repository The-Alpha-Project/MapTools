﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using AlphaCoreExtractor.Log;
using System.Collections.Generic;
using AlphaCoreExtractor.Helpers;

namespace AlphaCoreExtractor.Core
{
    public class SMDoodadDef
    {
        public string filePath;
        public uint nameId;
        public uint uniqueId;
        public C3Vector pos;
        public C3Vector rot;
        public ushort scale;
        public ushort flags;

        public SMDoodadDef(BinaryReader reader, List<string> mdxPaths)
        {
            nameId = reader.ReadUInt32();
            filePath = mdxPaths[(int)nameId];
            uniqueId = reader.ReadUInt32();
            pos = new C3Vector(reader, true);
            rot = new C3Vector(reader, true);
            scale = reader.ReadUInt16();
            flags = reader.ReadUInt16();
        }

        public static Dictionary<uint, SMDoodadDef> BuildFromChunck(byte[] chunk, List<string> mdxPaths)
        {
            Dictionary<uint, SMDoodadDef> mdxDefs = new Dictionary<uint, SMDoodadDef>();
            using (MemoryStream ms = new MemoryStream(chunk))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                while (reader.BaseStream.Position != chunk.Length)
                {
                    var doodadDef = new SMDoodadDef(reader, mdxPaths);
                    mdxDefs.Add(doodadDef.uniqueId, doodadDef);
                }
            }

            if (Globals.Verbose)
                Logger.Info($"Loaded {mdxDefs.Count} MDX definitions.");

            return mdxDefs;
        }
    }
}
