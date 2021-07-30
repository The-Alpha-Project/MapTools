﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using AlphaCoreExtractor.DBC;
using AlphaCoreExtractor.Log;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.Helpers;
using AlphaCoreExtractor.Helpers.Enums;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.Generator
{
    public class MapFilesGenerator
    {
        public static bool GenerateMapFiles(CMapObj map)
        {
            try
            {
                Logger.Notice($"Generating .map files for Map {map.DBCMap.MapName_enUS}");
                var fileCount = 0;

                // HeightMap
                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        var tileBlock = map.TileBlocks[tileBlockX, tileBlockY];
                        if (tileBlock != null)
                        {
                            var mapID = map.DBCMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            if (File.Exists(outputFileName))
                                throw new Exception("Found existent invalid file!");

                            var cell = TransformHeightData(tileBlock);

                            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
                            {
                                fs.Write(Encoding.ASCII.GetBytes(Globals.MapVersion), 0, 10);
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                    for (int cy = 0; cy < 256; cy++)
                                        for (int cx = 0; cx < 256; cx++)
                                            bw.Write(CalculateZ(cell, (double)cy, (double)cx));
                            }

                            fileCount++;
                        }
                    }
                }

                // AreaInformation (AreaNumber 0.5.3, Flags 1.12, AreaLevel 1.12, ExploreBit Custom, FactionGroup mask 1.12)
                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        var tileBlock = map.TileBlocks[tileBlockX, tileBlockY];
                        if (tileBlock != null)
                        {
                            var mapID = map.DBCMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            using (FileStream fs = new FileStream(outputFileName, FileMode.Append))
                            {
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                {
                                    for (int cy = 0; cy < Constants.TileSize; cy++)
                                    {
                                        for (int cx = 0; cx < Constants.TileSize; cx++)
                                        {
                                            var cell = tileBlock.Tiles[cy, cx];
                                            var areaNumber = cell.areaNumber;

                                            if (CachedNonExistent.ContainsKey(map.DBCMap.ID) && CachedNonExistent[map.DBCMap.ID].Contains(areaNumber))
                                            {
                                                WriteNullArea(bw, map.DBCMap.ID, areaNumber);
                                            }
                                            else
                                            {
                                                if (map.DBCMap.ID < 2 && areaNumber < 4000000000 && DBCStorage.TryGetAreaByMapIdAndAreaNumber(map.DBCMap.ID, areaNumber, out AreaTable areaTable))
                                                {
                                                    bw.Write((int)areaTable.ID);
                                                    bw.Write((uint)areaTable.AreaNumber);
                                                    bw.Write((byte)areaTable.Area_Flags);
                                                    bw.Write((byte)areaTable.Area_Level);
                                                    bw.Write((ushort)areaTable.Exploration_Bit);
                                                    bw.Write((byte)areaTable.FactionGroupMask);
                                                }
                                                else
                                                    WriteNullArea(bw, map.DBCMap.ID, areaNumber);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                // Liquids
                for (int tileBlockX = 0; tileBlockX < Constants.TileBlockSize; tileBlockX++)
                {
                    for (int tileBlockY = 0; tileBlockY < Constants.TileBlockSize; tileBlockY++)
                    {
                        var tileBlock = map.TileBlocks[tileBlockX, tileBlockY];
                        if (tileBlock != null)
                        {
                            bool[,] liquid_show = new bool[Constants.GridSize, Constants.GridSize];
                            float[,] liquid_height = new float[Constants.GridSize + 1, Constants.GridSize + 1];
                            sbyte[,] liquid_flag = new sbyte[Constants.GridSize + 1, Constants.GridSize + 1];

                            var mapID = map.DBCMap.ID.ToString("000");
                            var blockX = tileBlockX.ToString("00");
                            var blockY = tileBlockY.ToString("00");
                            var outputFileName = $@"{Paths.OutputMapsPath}{mapID}{blockX}{blockY}.map";

                            using (FileStream fs = new FileStream(outputFileName, FileMode.Append))
                            {
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                {
                                    for (int i = 0; i < Constants.TileSize; i++)
                                    {
                                        for (int j = 0; j < Constants.TileSize; j++)
                                        {
                                            var cell = tileBlock.Tiles[i, j];

                                            if (cell == null || cell.MCLQSubChunks.Count == 0)
                                                continue;

                                            MCLQSubChunk liquid;
                                            if (cell.MCLQSubChunks.Count > 1)
                                                liquid = cell.MCLQSubChunks.First(mc => mc.Flag != SMChunkFlags.FLAG_LQ_OCEAN);
                                            else
                                                liquid = cell.MCLQSubChunks.First();


                                            for (int y = 0; y < Constants.Cell_Size; y++)
                                            {
                                                int cy = i * Constants.Cell_Size + y;
                                                for (int x = 0; x < Constants.Cell_Size; x++)
                                                {
                                                    int cx = j * Constants.Cell_Size + x;
                                                    // Check if this liquid is rendered by the client.
                                                    if (liquid.Flags[y, x] != 0x0F)
                                                    {
                                                        liquid_show[cy, cx] = true;

                                                        // Overwrite DEEP water flag.
                                                        if ((liquid.Flags[y, x] & (1 << 7)) != 0)
                                                            liquid.Flag = SMChunkFlags.FLAG_LQ_DEEP;
                                                    }
                                                }
                                            }

                                            for (int y = 0; y <= Constants.Cell_Size; y++)
                                            {
                                                int cy = i * Constants.Cell_Size + y;
                                                for (int x = 0; x <= Constants.Cell_Size; x++)
                                                {
                                                    int cx = j * Constants.Cell_Size + x;
                                                    liquid_height[cy, cx] = liquid.GetHeight(y, x);
                                                    liquid_flag[cy, cx] = (sbyte)liquid.Flag;
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
                                                bw.Write(liquid_flag[y, x]);
                                                bw.Write(liquid_height[y, x]);
                                            }
                                            else
                                                bw.Write((sbyte)-1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (fileCount == 0)
                    Logger.Warning($"No tile data for Map {map.DBCMap.MapName_enUS}");
                else
                    Logger.Success($"Generated {fileCount} .map files for Map {map.DBCMap.MapName_enUS}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        private static Dictionary<uint, HashSet<uint>> CachedNonExistent = new Dictionary<uint, HashSet<uint>>();
        private static void WriteNullArea(BinaryWriter bw, uint mapid, uint areaNumber)
        {
            if (!CachedNonExistent.ContainsKey(mapid))
                CachedNonExistent.Add(mapid, new HashSet<uint>());

            if (!CachedNonExistent[mapid].Contains(areaNumber))
            {
                Logger.Warning($"Unable to locate AreaNumber {areaNumber} information for Map {mapid}");
                CachedNonExistent[mapid].Add(areaNumber);
            }

            bw.Write((int)-1); // ZoneId
        }

        private static Cell TransformHeightData(CMapArea tileBlock)
        {
            Cell cell = new Cell();

            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    cell.V8[x, y] = (float)tileBlock.Tiles[x / 8, y / 8].MCVTSubChunk.V8[x % 8, y % 8];
                    cell.V9[x, y] = (float)tileBlock.Tiles[x / 8, y / 8].MCVTSubChunk.V9[x % 8, y % 8];
                }

                cell.V9[x, 128] = (float)tileBlock.Tiles[x / 8, 15].MCVTSubChunk.V9[x % 8, 8];
                cell.V9[128, x] = (float)tileBlock.Tiles[15, x / 8].MCVTSubChunk.V9[8, x % 8];
            }

            cell.V9[128, 128] = (float)tileBlock.Tiles[15, 15].MCVTSubChunk.V9[8, 8];

            return cell;
        }

        private static float CalculateZ(Cell cell, double cy, double cx)
        {
            var x = (cy * Constants.TileSizeYrds) / ((double)256 - 1);
            var y = (cx * Constants.TileSizeYrds) / ((double)256 - 1);
            return Convert.ToSingle(GetZ(cell, x, y));
        }

        private static double GetZ(Cell cell, double x, double z)
        {
            Vector<double>[] v = new Vector<double>[3] { new Vector<double>(), new Vector<double>(), new Vector<double>() };
            Vector<double> p = new Vector<double>();

            // Find out quadrant
            int xc = (int)(x / Constants.UnitSize);
            int zc = (int)(z / Constants.UnitSize);

            if (xc > 127)
                xc = 127;

            if (zc > 127)
                zc = 127;

            double lx = x - xc * Constants.UnitSize;
            double lz = z - zc * Constants.UnitSize;
            p.X = lx;
            p.Z = lz;

            v[0].X = Constants.UnitSize / 2;
            v[0].Y = cell.V8[xc, zc];
            v[0].Z = Constants.UnitSize / 2;

            if (lx > lz)
            {
                v[1].X = Constants.UnitSize;
                v[1].Y = cell.V9[xc + 1, zc];
                v[1].Z = 0;
            }
            else
            {
                v[1].X = 0.0;
                v[1].Y = cell.V9[xc, zc + 1];
                v[1].Z = Constants.UnitSize;
            }

            if (lz > Constants.UnitSize - lx)
            {
                v[2].X = Constants.UnitSize;
                v[2].Y = cell.V9[xc + 1, zc + 1];
                v[2].Z = Constants.UnitSize;
            }
            else
            {
                v[2].X = 0;
                v[2].Y = cell.V9[xc, zc];
                v[2].Z = 0;
            }

            return -Solve(v, p);
        }

        /// <summary>ñ
        /// Plane equation ax+by+cz+d=0
        /// </summary>
        private static double Solve(Vector<double>[] v, Vector<double> p)
        {
            double a = v[0].Y * (v[1].Z - v[2].Z) + v[1].Y * (v[2].Z - v[0].Z) + v[2].Y * (v[0].Z - v[1].Z);
            double b = v[0].Z * (v[1].X - v[2].X) + v[1].Z * (v[2].X - v[0].X) + v[2].Z * (v[0].X - v[1].X);
            double c = v[0].X * (v[1].Y - v[2].Y) + v[1].X * (v[2].Y - v[0].Y) + v[2].X * (v[0].Y - v[1].Y);
            double d = v[0].X * (v[1].Y * v[2].Z - v[2].Y * v[1].Z) + v[1].X * (v[2].Y * v[0].Z - v[0].Y * v[2].Z) + v[2].X * (v[0].Y * v[1].Z - v[1].Y * v[0].Z);

            return ((a * p.X + c * p.Z - d) / b);
        }
    }
}
