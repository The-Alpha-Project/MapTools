// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;

using MapTools.Log;
using MapTools.DBC;
using MapTools.Helpers;
using MapTools.DBC.Structures;

using WoWFormatParser.Extensions;
using WoWFormatParser.Structures.ADT;

namespace MapTools.Generators
{
    public class MapFilesGenerator
    {
        public static void GenerateMapFiles(DBCMap dbcMap, out int generatedMaps)
        {
            generatedMaps = 0;
            try
            {
                uint total_tiles = Convert.ToUInt32(Constants.TileBlockSize * Constants.TileBlockSize);
                int processed_tiles = 0;
                Logger.Notice($"Generating .map files for Map {dbcMap.Name}");

                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        if (dbcMap.WDT.Tiles != null && dbcMap.WDT.Tiles[tileBlockX, tileBlockY] is ADT adt)
                        {
                            var mapID = dbcMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            if (File.Exists(outputFileName))
                            {
                                try { File.Delete(outputFileName); }
                                catch (Exception) { return; }
                            }

                            using (FileStream fileStream = new FileStream(outputFileName, FileMode.Create))
                            {
                                //Map version.
                                fileStream.WriteMapVersion();

                                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                                {
                                    WriteHeightMap(binaryWriter, adt);
                                    WriteAreaInformation(binaryWriter, adt, dbcMap);
                                    WriteLiquids(binaryWriter, adt);
                                }
                            }

                            generatedMaps++;
                        }
                        Logger.Progress("Generating map files", ++processed_tiles, total_tiles, 200);
                    }
                }

                if (generatedMaps == 0)
                    Logger.Warning($"No tile data for {dbcMap.Name}, map is WMO based.");
                else
                    Logger.Success($"Generated {generatedMaps} .map files for Map {dbcMap.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private static void WriteLiquids(BinaryWriter binaryWriter, ADT adt)
        {
            bool[,] liquid_show = new bool[(int)Constants.GridSize, (int)Constants.GridSize];
            float[,] liquid_height = new float[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];
            MCNK_Flags[,] liquid_flag = new MCNK_Flags[(int)Constants.GridSize + 1, (int)Constants.GridSize + 1];

            for (int i = 0; i < Constants.TileSize; i++)
            {
                for (int j = 0; j < Constants.TileSize; j++)
                {
                    var cell = adt.MapChunks[i, j];

                    if (cell == null || cell.Liquids == null || cell.Liquids.Length == 0)
                        continue;

                    MCLQ liquid = null;
                    if (cell.Liquids.Length > 1)
                        liquid = cell.Liquids.First(f => f.Flag != MCNK_Flags.IsOcean);
                    else
                        liquid = cell.Liquids[0];

                    for (int y = 0; y < Constants.CellSize; y++)
                    {
                        int cy = i * Constants.CellSize + y;
                        for (int x = 0; x < Constants.CellSize; x++)
                        {
                            int cx = j * Constants.CellSize + x;
                            // Check if this liquid is rendered by the client.
                            if (liquid.Tiles[y, x] != 0x0F)
                            {
                                liquid_show[cy, cx] = true;

                                // Overwrite DEEP water flag.
                                if ((liquid.Tiles[y, x] & (1 << 7)) != 0)
                                    liquid.Flag = MCNK_Flags.IsDeepWater;

                                liquid_height[cy, cx] = liquid.GetHeight(y, x);
                                liquid_flag[cy, cx] = (MCNK_Flags)liquid.Flag;
                            }
                            else
                            {
                                liquid_flag[cy, cx] = MCNK_Flags.None;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < Constants.GridSize; y++)
            {
                for (int x = 0; x < Constants.GridSize; x++)
                {
                    if (liquid_show[y, x])
                    {
                        binaryWriter.Write((uint)liquid_flag[y, x]);
                        binaryWriter.Write(liquid_height[y, x]);
                    }
                    else
                        binaryWriter.Write((uint)MCNK_Flags.None);
                }
            }
        }

        private static void WriteAreaInformation(BinaryWriter binaryWriter, ADT adt, DBCMap map)
        {
            for (int cy = 0; cy < Constants.TileSize; cy++)
            {
                for (int cx = 0; cx < Constants.TileSize; cx++)
                {
                    var cell = adt.MapChunks[cy, cx];
                    var areaNumber = cell.Areaid;

                    if (map.ID < 2 && areaNumber < 4000000000 && DBCStorage.TryGetAreaByMapIdAndAreaNumber(map.ID, areaNumber, out AreaTable areaTable))
                    {
                        binaryWriter.Write((int)areaTable.ID);
                        binaryWriter.Write((uint)areaTable.AreaNumber);
                        binaryWriter.Write((byte)areaTable.Area_Flags);
                        binaryWriter.Write((byte)areaTable.Area_Level);
                        binaryWriter.Write((ushort)areaTable.Exploration_Bit);
                        binaryWriter.Write((byte)areaTable.FactionGroupMask);
                    }
                    else
                        binaryWriter.Write((int)-1);
                }
            }
        }

        private static void WriteHeightMap(BinaryWriter binaryWriter, ADT adt)
        {
            var transformed = adt.TransformHeightData();
            for (int cy = 0; cy < 256; cy++)
                for (int cx = 0; cx < 256; cx++)
                    binaryWriter.Write(transformed.CalculateZ(cy, cx));
        }
    }
}
