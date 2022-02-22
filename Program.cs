// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using MapTools.MPQ;
using MapTools.DBC;
using MapTools.Log;
using MapTools.Helpers;
using MapTools.Generators;
using MapTools.DBC.Structures;

using WoWFormatParser.Structures.WDT;

namespace MapTools
{
    public static class Program
    {
        public static List<DBCMap> LoadedMaps = new List<DBCMap>();
        private static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        private static Thread MapsThread;
        private static volatile bool IsRunning = false;

        public static void Main(string[] args)
        {
            IsRunning = true;
            MapsThread = new Thread(new ThreadStart(StartProcess));
            MapsThread.Name = "MapsThread";
            MapsThread.Start();

            while (IsRunning)
                Thread.Sleep(2000);

            MapsThread.Join(2000);
            MapsThread.Interrupt();
            Console.ReadLine();
            SetDefaultTitle();
        }

        private static void StartProcess()
        {
            SetDefaultTitle();
            PrintHeader();

            try
            {
                if (!Configuration.Initialize())
                {
                    Logger.Error("Unable to read Config.ini, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                Logger.Info($"Using Z Resolution: {Configuration.ZResolution}");

                // Extract Map.dbc and AreaTable.dbc
                if (!DBCExtractor.ExtractDBC())
                {
                    Logger.Error("Unable to extract DBC files, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Load both files in memory.
                if (!DBCStorage.Initialize())
                {
                    Logger.Error("Unable to initialize DBC Storage, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                Dictionary<DBCMap, string> WDTFiles;
                if (!WDTExtractor.ExtractWDTFiles(out WDTFiles))
                {
                    Logger.Error("Unable to extract WDT files, exiting...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // Flush .map files output dir.
                Directory.Delete(Paths.OutputMapsPath, true);

                int GeneratedMapFiles = 0;
                int GeneratedMeshes = 0;
                //Begin parsing adt files and generate .map files.
                foreach (var entry in WDTFiles)
                {
                    DBCMap dbcMap = entry.Key;
                    string filePath = entry.Value;

                    using (WoWFormatParser.WoWFormatParser parser = new WoWFormatParser.WoWFormatParser(Configuration.WoWPath, Configuration.WoWBuild, Configuration.ParserOptions))
                    {
                        Logger.Notice($"Processing map: {dbcMap.MapName_enUS} ContinentID: {dbcMap.ID} IsInstance: {dbcMap.IsInMap != 1}");
                        Logger.Notice($"Please wait, this can take a while...");
                        var wdt = parser.ParseFile<WDT>(filePath, Configuration.WoWBuild);
                        Logger.Success("Map information loaded successfully.");
                        dbcMap.BindWDT(wdt);
                        
                        // Generate HeightMap. (.map files)
                        MapFilesGenerator.GenerateMapFiles(dbcMap, out int generatedMaps);
                        GeneratedMapFiles += generatedMaps;

                        // Generate 3d mesh.
                        if (Configuration.GenerateMesh)
                        {
                            MeshGenerator.GenerateMesh(dbcMap, out int generatedMeshes);
                            GeneratedMeshes += generatedMeshes;
                        }
               
                        LoadedMaps.Add(dbcMap);
                    }

                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                }

                WDTFiles?.Clear();
                Console.WriteLine();
                Logger.Success($"Generated a total of {GeneratedMapFiles} .map files.");
                Logger.Success($"Generated a total of {GeneratedMeshes} .obj files.");
                Logger.Success("Process Complete, press any key to exit...");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public static void SetDefaultTitle()
        {
            Console.Title = $"AlphaCore Map Extractor {Version}";
        }

        private static void PrintHeader()
        {
            Console.WriteLine("TheAlphaProject");
            Console.WriteLine("Discord: https://discord.gg/RzBMAKU");
            Console.WriteLine("Github: https://github.com/The-Alpha-Project");
            Console.WriteLine();
        }
    }
}
