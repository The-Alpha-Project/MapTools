// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using MapTools.Log;
using WoWFormatParser;

namespace MapTools.Helpers
{
    public static class Configuration
    {
        private static IniFile IniFile = new IniFile();
        public static bool Initialize()
        {
            try
            {
                IniFile.Load("Config.ini");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            return false;
        }

        public static WoWBuild WoWBuild
        {
            get
            {
                var build = IniFile["Configuration"]["WoWBuild"].ToString();
                if (string.IsNullOrEmpty(build))
                    Logger.Error("Unable to read World of Warcraft build from Config.ini.");

                return new WoWBuild(build);
            }
        } 

        public static Options ParserOptions
        {
            get 
            {
                return new Options() { ParseMode = ParseMode.Both, IncludeUnsupportedAndInvalidFiles = false };
            }
        }

        public static bool GenerateMesh
        {
            get
            {
                var mesh = IniFile["Configuration"]["GenerateMesh"].ToString();
                if (string.IsNullOrEmpty(mesh))
                {
                    Logger.Error("Unable to read GenerateMesh from Config.ini.");
                    return false;
                }

                return mesh.Equals("1");
            }
        }

        public static string WoWPath
        {
            get
            {
                var path = IniFile["Configuration"]["WoWPath"].ToString();

                if (string.IsNullOrEmpty(path))
                    Logger.Error("Unable to read World of Warcraft installation path from Config.ini.");

                return path;
            }
        }


        public static int ZResolution
        {
            get
            {
                var zResolution = IniFile["Configuration"]["ZResolution"].ToString();

                if (string.IsNullOrEmpty(zResolution))
                    Logger.Error("Unable to read ZResolution from Config.ini.");
                else if (int.TryParse(zResolution, out int zRes))
                    return zRes;
                else
                    Logger.Warning("Unable to parse ZResolution from Config.ini, using default value of 256.");

                return 256;
            }
        }
    }
}
