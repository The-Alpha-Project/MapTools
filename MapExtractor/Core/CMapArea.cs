// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Collections.Generic;

using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.DBC.Structures;
using AlphaCoreExtractor.Generators.Mesh;

namespace AlphaCoreExtractor.Core
{
    public class CMapArea : IDisposable
    {
        /// <summary>
        /// This ADT bounds.
        /// </summary>
        public Rect Bounds = Rect.Empty;

        /// <summary>
        /// General TileBlock information,
        /// </summary>
        public SMAreaHeader AreaHeader;

        /// <summary>
        /// List of textures used for texturing the terrain in this TileBlock.
        /// </summary>
        public MTEXChunk MTEXChunk;

        /// <summary>
        /// MDX refs for this TileBlock.
        /// </summary>
        public Dictionary<uint, SMDoodadDef> MDXs = new Dictionary<uint, SMDoodadDef>();

        /// <summary>
        /// WMO refs for this TileBlock.
        /// </summary>
        public Dictionary<uint, SMMapObjDef> WMOs = new Dictionary<uint, SMMapObjDef>();

        /// <summary>
        /// Offsets/Sizes for each Tile.
        /// </summary>
        public SMChunkInfo[,] TilesInformation = new SMChunkInfo[(int)Constants.TileSize, (int)Constants.TileSize];

        /// <summary>
        /// The actual Tiles.
        /// </summary>
        public SMChunk[,] Tiles = new SMChunk[(int)Constants.TileSize, (int)Constants.TileSize];

        /// <summary>
        /// Failed to parse something?
        /// </summary>
        public bool Errors = true;

        /// <summary>
        /// Used to read file tokens and validate chunks.
        /// </summary>
        private DataChunkHeader DataChunkHeader;

        public CMapArea(uint offset, CMapObj adtReader, DataChunkHeader dataChunkHeader, bool buildWMOs = false, bool buildMDXs = false)
        {
            DataChunkHeader = dataChunkHeader;

            // MHDR offset
            adtReader.SetPosition(offset);

            // AreaHeader
            if (!BuildAreaHeader(adtReader))
                return;

            // MCIN, 256 Entries, so a 16*16 Chunkmap.
            if (!BuildMCIN(adtReader))
                return;

            // MTEX, List of textures used for texturing the terrain in this map tile.
            if (!BuildMTEX(adtReader))
                return;

            // MDDF, Placement information for doodads (MDX models)
            // Additional to this, the models to render are referenced in each MCRF chunk.
            if (!BuildMDDF(adtReader))
                return;

            // MODF, Placement information for WMOs.
            // Additional to this, the WMOs to render are referenced in each MCRF chunk.
            if (!BuildMODF(adtReader))
                return;

            // The MCNK chunks have a large block of data that starts with a header, and then has sub-chunks of its own.
            // Each map chunk has 9x9 vertices, and in between them 8x8 additional vertices, several texture layers, normal vectors,
            // a shadow map, etc.
            // MCNK, The header is 128 bytes like later versions, but information inside is placed slightly differently.
            // Offsets are relative to the end of MCNK header.
            if (!BuildMCNK(adtReader))
                return;

            if (buildWMOs)
            {
                // TODO: Build this ADT WMOs objects in order to append them to the terrain mesh.
            }

            if (buildMDXs)
            {
                // TODO: Build this ADT MDXs models objects in order to append them to the terrain mesh.
            }

            Errors = false;
        }

        private bool BuildMCNK(CMapObj adtReader)
        {
            try
            {
                for (int x = 0; x < Constants.TileSize; x++)
                {
                    for (int y = 0; y < Constants.TileSize; y++)
                    {
                        adtReader.SetPosition(TilesInformation[x, y].offset);

                        DataChunkHeader.Fill(adtReader);
                        if (DataChunkHeader.Token != Tokens.MCNK)
                            throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MCNK]"}");

                        Tiles[x, y] = new SMChunk(adtReader);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMODF(CMapObj adtReader)
        {
            try
            {
                DataChunkHeader.Fill(adtReader);
                if (DataChunkHeader.Token != Tokens.MODF)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MODF]"}");

                //MODF (Placement information for WMOs. Additional to this, the WMOs to render are referenced in each MCRF chunk)
                var dataChunk = adtReader.ReadBytes(DataChunkHeader.Size);
                WMOs = SMMapObjDef.BuildFromChunk(dataChunk, adtReader.WMOs);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMDDF(CMapObj adtReader)
        {
            try
            {
                DataChunkHeader.Fill(adtReader);
                if (DataChunkHeader.Token != Tokens.MDDF)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MDDF]"}");

                var dataChunk = adtReader.ReadBytes(DataChunkHeader.Size);
                MDXs = SMDoodadDef.BuildFromChunck(dataChunk, (adtReader as CMapObj).MDXs);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMTEX(CMapObj adtReader)
        {
            try
            {
                DataChunkHeader.Fill(adtReader);
                if (DataChunkHeader.Token != Tokens.MTEX)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MTEX]"}");

                var dataChunk = adtReader.ReadBytes(DataChunkHeader.Size);
                MTEXChunk = MTEXChunk.BuildFromChunk(dataChunk);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildMCIN(CMapObj adtReader)
        {
            try
            {
                DataChunkHeader.Fill(adtReader);
                if (DataChunkHeader.Token != Tokens.MCIN)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MCIN]"}");

                // All tiles should be used, meaming we should have valid offset and size for each tile.
                for (int x = 0; x < Constants.TileSize; x++)
                    for (int y = 0; y < Constants.TileSize; y++)
                        TilesInformation[x, y] = new SMChunkInfo(adtReader);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private bool BuildAreaHeader(CMapObj adtReader)
        {
            try
            {
                DataChunkHeader.Fill(adtReader);
                if (DataChunkHeader.Token != Tokens.MHDRChunk)
                    throw new Exception($"Invalid token, got [{DataChunkHeader.Token}] expected {"[MHDRChunk]"}");

                AreaHeader = new SMAreaHeader(adtReader);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Set ADT bounds.
        /// </summary>
        public void SetBounds(uint tileX, uint tileY)
        {
            var topLeftX = Constants.CenterPoint - ((tileX) * Constants.TileSizeYrds);
            var topLeftY = Constants.CenterPoint - ((tileY) * Constants.TileSizeYrds);
            var botRightX = topLeftX - Constants.TileSizeYrds;
            var botRightY = topLeftY - Constants.TileSizeYrds;
            Bounds = new Rect(new Point(topLeftX, topLeftY), new Point(botRightX, botRightY));
        }

        #region #Helpers
        public IEnumerable<string> GetAreaNames(uint mapID)
        {
            HashSet<uint> areas = new HashSet<uint>();

            for (int x = 0; x < Constants.TileSize; x++)
            {
                for (int y = 0; y < Constants.TileSize; y++)
                {
                    var areaID = Tiles[x, y].areaNumber;
                    if (!areas.Contains(areaID))
                    {
                        if (DBCStorage.TryGetAreaByMapIdAndAreaNumber(mapID, areaID, out AreaTable areaTable))
                            yield return areaTable.Name;
                        else
                            yield return $"Unknown area {areaID}";
                        areas.Add(areaID);
                    }
                }
            }
        }
        #endregion

        public void Dispose()
        {
            AreaHeader = null;
            MTEXChunk = null;
            MDXs = null;
            WMOs = null;
            TilesInformation = null;

            foreach (var tile in Tiles)
                tile.Dispose();

            Tiles = null;
            DataChunkHeader = null;
        }
    }
}
