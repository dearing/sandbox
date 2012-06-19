using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

using SevenZip;

namespace MultiZip
{
    /// <summary>
    /// Static class to handle the programs basic needs.
    /// </summary>
    public static class Engine
    {
        #region Fields
        /// <summary>
        /// The SevenZip workhorse; wraps the 7z.dll calls
        /// </summary>
        public static SevenZipCompressor Compressor;
        /// <summary>
        /// After argument dequeued values; we populate this with
        /// search patterns to be worked with.
        /// </summary>
        public static List<String> Patterns = new List<String>();
        #endregion Fields

        #region Properties

        /// <summary>
        /// Assembly Name gleaned from reflection.
        /// </summary>
        static public String ProgramName { get; set; }
        /// <summary>
        /// A String in the format: [name] v#.# build # revision #
        /// </summary>
        static public String VersionString { get; set; }
        /// <summary>
        /// Extra talk to the console.
        /// </summary>
        static public Boolean Verbose { get; set; }
        /// <summary>
        /// When a file is matched from a searchpattern this will be supplied
        /// as a suffix to the original files name.  example.txt => example.txt.zip
        /// </summary>
        static public String Ext { get; set; }
        /// <summary>
        /// Toggle color in the verbose console output.
        /// </summary>
        static public Boolean Color { get; set; }
        /// <summary>
        /// If true then instead of matching files in a workingdirectory,
        /// we match folders and archive them recursivly as individual archives.
        /// </summary>
        static public Boolean DirectoryMode { get; set; }

        #endregion Properties

        #region Reflection
        /// <summary>
        /// Generate some useful reporting info.
        /// </summary>
        static void ReflectAssembly()
        {
            AssemblyName assembly = Assembly.GetExecutingAssembly().GetName();
            ProgramName = assembly.Name;
            VersionString = String.Format(
                "{0} v{1}.{2} build {3} revision {4}",
                assembly.Name,
                assembly.Version.Major,
                assembly.Version.Minor,
                assembly.Version.Build,
                assembly.Version.Revision);
        }
        #endregion Properties

        #region Constructor
        /// <summary>
        /// Initialize Version and Name and Load the settings 
        /// from the app config.
        /// </summary>
        static Engine()
        {
            ReflectAssembly();
            ReadSettings();

            Ext = ".7z";
        }
        #endregion Constructor

        #region Configuration and Settings
        /// <summary>
        /// Read relevant settings from the application config.
        /// </summary>
        static void ReadSettings()
        {
            Ext = ReadSetting("Suffix");
            SevenZipCompressor.SetLibraryPath(ReadSetting("LibraryPath"));
            Compressor = new SevenZip.SevenZipCompressor();
            Compressor.ArchiveFormat = Parse_ArchiveFormat(ReadSetting("ArchiveFormat"));
            Compressor.CompressionLevel =Parse_CompressionLevel(ReadSetting("CompressionLevel"));
            Compressor.CompressionMethod = Parse_CompressionMethod(ReadSetting("CompressionMethod"));
            Compressor.CompressionMode = Parse_CompressionMode(ReadSetting("CompressionMode"));
            Compressor.DirectoryStructure = Parse_DirectoryStructure(ReadSetting("DirectoryStructure"));
        }

        /// <summary>
        /// Parse a string (read from config files for example) and
        /// return the equivalent Enum.
        /// </summary>
        /// <param name="setting">value to match</param>
        /// <returns>best match (defaulted or better)</returns>
        public static OutArchiveFormat Parse_ArchiveFormat(String setting)
        {
            switch (setting.ToLower())
            {
                case "bzip2": return SevenZip.OutArchiveFormat.BZip2;
                case "gzip": return SevenZip.OutArchiveFormat.GZip;
                case "sevenzip": return SevenZip.OutArchiveFormat.SevenZip;
                case "tar": return SevenZip.OutArchiveFormat.Tar;
                case "xz": return SevenZip.OutArchiveFormat.XZ;
                case "zip": return SevenZip.OutArchiveFormat.Zip;
            }

            Console.WriteLine("Unrecognized ArchiveFormat {0}.  Check config files; defaulting to `SevenZip`", setting);
            return SevenZip.OutArchiveFormat.SevenZip;
        }
        /// <summary>
        /// Parse a string (read from config files for example) and
        /// return the equivalent Enum.
        /// </summary>
        /// <param name="setting">value to match</param>
        /// <returns>best match (defaulted or better)</returns>
        public static CompressionLevel Parse_CompressionLevel(String setting)
        {
            switch (setting.ToLower())
            {
                case "fast": return SevenZip.CompressionLevel.Fast;
                case "high": return SevenZip.CompressionLevel.High;
                case "low": return SevenZip.CompressionLevel.Low;
                case "none": return SevenZip.CompressionLevel.None;
                case "normal": return SevenZip.CompressionLevel.Normal;
                case "ultra": return SevenZip.CompressionLevel.Ultra;
            }
            Console.WriteLine("Unrecognized CompressionLevel Setting `{0}`.  Check config files; defaulting to `Normal`", setting);
            return SevenZip.CompressionLevel.Normal;
        }
        /// <summary>
        /// Parse a string (read from config files for example) and
        /// return the equivalent Enum.
        /// </summary>
        /// <param name="setting">value to match</param>
        /// <returns>best match (defaulted or better)</returns>
        public static CompressionMethod Parse_CompressionMethod(String setting)
        {
            switch (setting.ToLower())
            {
                case "bzip2": return SevenZip.CompressionMethod.BZip2;
                case "copy": return SevenZip.CompressionMethod.Copy;
                case "default": return SevenZip.CompressionMethod.Default;
                case "deflate": return SevenZip.CompressionMethod.Deflate;
                case "deflate64": return SevenZip.CompressionMethod.Deflate64;
                case "lzma": return SevenZip.CompressionMethod.Lzma;
                case "lzma2": return SevenZip.CompressionMethod.Lzma2;
                case "ppmd": return SevenZip.CompressionMethod.Ppmd;
            }
            Console.WriteLine("Unrecognized CompressionMethod Setting `{0}`.  Check config files; defaulting to `Default`", setting);
            return SevenZip.CompressionMethod.Default;

        }
        /// <summary>
        /// Parse a string (read from config files for example) and
        /// return the equivalent Enum.
        /// </summary>
        /// <param name="setting">value to match</param>
        /// <returns>best match (defaulted or better)</returns>
        public static CompressionMode Parse_CompressionMode(String setting)
        {
            switch (setting.ToLower())
            {
                case "append": return SevenZip.CompressionMode.Append;
                case "create": return SevenZip.CompressionMode.Create;
            }
            Console.WriteLine("Unrecognized CompressionMode Setting `{0}`.  Check config files; defaulting to `Create`", setting);
            return SevenZip.CompressionMode.Create;
        }
        /// <summary>
        /// Parse a string (read from config files for example) and
        /// return the equivalent Enum.
        /// </summary>
        /// <param name="setting">value to match</param>
        /// <returns>best match (defaulted or better)</returns>
        public static Boolean Parse_DirectoryStructure(String setting)
        {
            switch (setting.ToLower())
            {
                case "true":
                case "1":
                    return true;
                case "false":
                case "0":
                    return false;
            }
            Console.WriteLine("Unrecognized DirectoryStructure Setting `{0}`.  Check config files; defaulting to `False`", setting);
            return false;
        }

        /// <summary>
        /// Read a string from the config file.
        /// </summary>
        /// <param name="key">the key to lookup</param>
        /// <returns>Value or String.Empty</returns>
        public static string ReadSetting(string key)
        {
            return ConfigurationSettings.AppSettings[key];
        }
        #endregion Configuration and Settings

        #region Compression

        public static void Work(String SearchPattern)
        {
            try
            {
                if (DirectoryMode)
                    ModeDirectory(SearchPattern);
                else
                    ModeFiles(SearchPattern);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}\n{1}\n",e.Message,e.StackTrace);
                //throw;
            }
        }

        static void ModeFiles(String SearchPattern)
        {
            FileInfo fi, fi2;
            var files = Directory.GetFiles(@".\", SearchPattern, SearchOption.TopDirectoryOnly);
            foreach (var item in files)
            {
                if (File.Exists(item))
                {
                    fi = new FileInfo(item);

                    if(Engine.Verbose) Console.Write("\t{0}", fi.Name + Ext);

                    Compressor.CompressFiles(fi.Name + Ext, fi.Name);
                    fi2 = new FileInfo(item + Ext);

                    double value = ((double)fi2.Length / (double)fi.Length);

                    if (Engine.Verbose)
                    {
                        if (Engine.Color)
                        {
                            if (value >= 1)
                                Console.ForegroundColor = ConsoleColor.Red;
                            else
                                Console.ForegroundColor = ConsoleColor.Cyan;
                        }
                        Console.WriteLine("\r{0:000.0%}", value);
                        if(Engine.Color) Console.ResetColor();
                    }
                }
            }
        }

        static void ModeDirectory(String SearchPattern)
        {
            //Console.WriteLine("Folder MODE!");
            DirectoryInfo fi;
            var folders = Directory.GetDirectories(@".\", SearchPattern, SearchOption.TopDirectoryOnly);
            foreach (var item in folders)
            {
                if (Directory.Exists(item))
                {
                    fi = new DirectoryInfo(item);
                    if (Engine.Verbose) Console.WriteLine("\t{0}", fi.Name + Ext);

                    Compressor.CompressDirectory(fi.Name, fi.Name + Ext, true);
                }
            }
        }

        #endregion Compression

        /// <summary>
        /// Help switch was called
        /// </summary>
        internal static void Help()
        {
            Console.WriteLine();
            Console.WriteLine("Multizip is a simple `one archive per match` utility.");
            Console.WriteLine();
            Console.WriteLine("Syntax: [hvcads] [optional args] [search patterns]");
            Console.WriteLine("");
            Console.WriteLine("- h        : this output");
            Console.WriteLine("- v        : extra talk in console output");
            Console.WriteLine("- c        : use color with verbose output");
            Console.WriteLine("- d        : target directories with search-patterns");
            Console.WriteLine("- a [arg]  : optional args contains supplied ArchiveFormat value.");
            Console.WriteLine("- s [arg]  : optional args contains supplied Suffix string.");
            Console.WriteLine();
            Console.WriteLine("Arguments are handled as a queue; this means that if the first");
            Console.WriteLine("switch with optional args is 's' then the next value after the");
            Console.WriteLine("switch argument string is expected to be for the 's' switch and");
            Console.WriteLine("so on and such all the way down (switch order is not important).");
            Console.WriteLine();
            Console.WriteLine("For Example: multizip as SevenZip .7z *.gba");
            Console.WriteLine("           : multizip sa .zip ZIP *.gba");
            Console.WriteLine("           : multizip vc *.gba *.snes *.nes");
            Console.WriteLine("           : multizip vcd roms-*");
            Console.WriteLine("           : multizip vdas TAR .tar roms-*");
            Console.WriteLine("           : multizip vas BZIP2 .bz2 roms-*");
            Console.WriteLine();
            Console.WriteLine("The defaults for the program are loaded from {0}.exe.config",ProgramName);
        }
    }
}
