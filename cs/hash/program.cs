using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("hash")]
[assembly: AssemblyDescription("hash, or: How I learned to love the digest.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jacob Dearing")]
[assembly: AssemblyProduct("hash")]
[assembly: AssemblyCopyright("Copyright © Jacob Dearing 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("ce5d6a76-2aff-440e-bc4f-af0549958ed2")]
[assembly: AssemblyVersion("0.22.*")]
[assembly: AssemblyFileVersion("0.22.0.0")]

namespace hash
{
class Program
    {
        static void Main(string[] args)
        {
            Engine.ParseArgs(args);
            Engine.GetFileList();

            if (Engine.verbose == true) Engine.ReportArgs(args);

            #region Working Mode - Switch
            switch (Engine.workingmode)
            {
                case Engine.Mode.HashFile:
                    Engine.ModeHashFiles();
                    break;
                case Engine.Mode.DirectInput:
                    Engine.ModeDirect();
                    break;
                case Engine.Mode.Strings:
                    Engine.ModeString();
                    break;
                case Engine.Mode.ValidateChecksums:
                    Console.WriteLine();
                    Engine.ModeValidate();
                    break;
                case Engine.Mode.GenerateChecksumFile:
                    break;

                case Engine.Mode.Help:
                    Engine.ModeHelp();
                    break;
            }
            #endregion Working Mode - Switch

            //Console.ReadLine();   
        }
    }
    /// <summary>
    /// The brains of this project,
    /// Handles the command lines switches, 
    /// computatation and comparison of hashes.
    /// </summary>
    static class Engine
    {
        #region Enumuration
        /// <summary>
        /// Logical modes for the program to operate in.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// The logic expects the all non switch arguments
            /// to be files.
            /// </summary>
            HashFile,
            /// <summary>
            /// Every passed argument is dumped and the standard
            /// input is opened for hashing.
            /// </summary>
            DirectInput,
            /// <summary>
            /// Open the StdIn stream and passes it to the hashing compute method.
            /// This will ignore all other switches.
            /// </summary>
            Strings,
            /// <summary>
            /// Each argument is understood to be an n-sized list
            /// of files containing filenames and checksums for validation.
            /// </summary>
            ValidateChecksums,
            /// <summary>
            /// This mode is not activly used.
            /// </summary>
            GenerateChecksumFile,
            /// <summary>
            /// The program just spits out the availiable help and exits.
            /// </summary>
            Help
        }
        /// <summary>
        /// The 5 hashing algorithims supported in this program.
        /// </summary>
        public enum HashingFunction
        {
            /// <summary>
            /// Use the Message Digest 5 algorithim.
            /// </summary>
            MD5,
            /// <summary>
            ///  Use the SHA1 algotirithm.
            /// </summary>
            SHA1,
            /// <summary>
            /// Use the SHA-256 algorithim
            /// </summary>
            SHA256,
            /// <summary>
            /// Use the SHA-384 algorithim
            /// </summary>
            SHA384,
            /// <summary>
            /// Use the SHA-512 algorithim
            /// </summary>
            SHA512
        }

        #endregion Enumuration

        #region Fields/Properties
        /// <summary>
        /// Internal list of files to be checked according to the current operating mode.
        /// </summary>
        private static List<String> parameters, files;
        /// <summary>
        /// Verbosity; do we mention details like warnings or how many files just got hashed?
        /// </summary>
        public static Boolean verbose;
        /// <summary>
        /// When globbing do we allow for recusing through subdirectories?
        /// </summary>
        public static Boolean recursive;
        /// <summary>
        /// When reporting Hex Formatted strings do we use uppercase
        /// or lowercase letters?
        /// </summary>
        private static Boolean uppercasehex;
        /// <summary>
        /// The current working mode the program functions in.
        /// </summary>
        public static Mode workingmode;
        /// <summary>
        /// There are 5 hashing algorithms availiable to us.
        /// Internally we can use the ENUM HashingFunction.
        /// The user will have to specifiy with command line arguments.
        /// </summary>
        private static HashingFunction hashingfunction;
        #endregion Fields/Properties

        #region Public Methods
        /// <summary>
        /// This program supports alot of command line functionality and the
        /// strings passed are interpreteded in differant ways because of this.
        /// </summary>
        /// <param name="args">The args passed the the controlling main loop.</param>
        public static void ReportArgs(params string[] args)
        {
            Console.WriteLine();
            if (args.Length == 0)
                return;

            int i = 0;
            Console.WriteLine("- Mode         : {0}", workingmode);
            if (workingmode == Mode.ValidateChecksums)
                Console.WriteLine("- HashFunction : Auto Detect");
            else
                Console.WriteLine("- HashFunction : {0}", hashingfunction);

            if (files.Count > 0)
            {
                Console.WriteLine("- FileNames    : ");
                i = 0;
                foreach (string file in files)
                    Console.WriteLine("\t[{0:d2}] '{1}' ", i++, file);
            }
            Console.WriteLine("\n- Arguments Passed:");
            i = 0;
            foreach (string arg in args)
                Console.WriteLine("\t[{0:d2}] {1},", i++, arg);
            Console.WriteLine();
        }
        /// <summary>
        /// At this time is just a simple switch for the passed args.
        /// Note: this must be called for the information provided to make any sence.
        /// </summary>
        /// <param name="args">Arguments to work with.</param>
        public static void ParseArgs(string[] args)
        {
            if (parameters == null) parameters = new List<string>();

            foreach (string arg in args)
            {
                switch (Substitute(arg.ToLower()))
                {

                    case @"--hashfunction:md5":
                        hashingfunction = HashingFunction.MD5;
                        break;
                    case @"--hashfunction:sha1":
                        hashingfunction = HashingFunction.SHA1;
                        break;
                    case @"--hashfunction:sha256":
                        hashingfunction = HashingFunction.SHA256;
                        break;
                    case @"--hashfunction:sha384":
                        hashingfunction = HashingFunction.SHA384;
                        break;
                    case @"--hashfunction:sha512":
                        hashingfunction = HashingFunction.SHA512;
                        break;


                    case @"--help":
                        workingmode = Mode.Help;
                        break;
                    case @"--direct":
                        workingmode = Mode.DirectInput;
                        break;
                    case @"--strings":
                        workingmode = Mode.Strings;
                        break;
                    case @"--file":
                        workingmode = Mode.HashFile;
                        break;
                    case @"--checksum":
                        workingmode = Mode.ValidateChecksums;
                        break;

                    case @"--verbose":
                        verbose = true;
                        break;
                    case @"--recursive":
                        recursive = true;
                        break;
                    case @"--uppercasehex":
                        uppercasehex = true;
                        break;

                    default:
                        parameters.Add(arg);
                        break;
                }
            }
        }
        /// <summary>
        /// Builds the the list with extant files.  
        /// This eliminates the need for constant checks.
        /// It will also unroll a glob including the subdirectories if --recursive is passed.
        /// </summary>
        public static void GetFileList()
        {
            files = new List<String>();
            if (parameters.Count < 1)
                return;
            foreach (String filename in parameters)
            {
                if (filename.Contains(@"*"))
                {
                    String[] temp = SeperateSearchString(filename);
                    try
                    {
                        if (String.IsNullOrEmpty(temp[0]))
                            temp[0] = "." + Path.DirectorySeparatorChar;
                        if (!recursive)
                            files.AddRange(Directory.GetFiles(temp[0], temp[1], SearchOption.TopDirectoryOnly));
                        else
                            files.AddRange(Directory.GetFiles(temp[0], temp[1], SearchOption.AllDirectories));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: {0}", e.Message);
                        continue;
                    }
                }

                else
                {
                    if (File.Exists(filename))
                        files.Add(filename);
                }
            }

            if (verbose) Console.WriteLine("\n{0} files found.", files.Count);
        }

        #endregion Public Methods

        #region Operating Modes
        /// <summary>
        /// Spits out some help and exits.
        /// </summary>
        public static void ModeHelp()
        {
            System.Reflection.Assembly program = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName me = program.GetName();
            Console.WriteLine("H * A * S * H");
            Console.WriteLine("{0} :\t{1}", me.Name, me.CodeBase);
            Console.WriteLine("\t{0}.{1} build {2} revision {3}\n", me.Version.Major, me.Version.Minor, me.Version.Build, me.Version.Revision);
            Console.WriteLine("Hash some files; verify some checksums; convert some strings etc..\n");
            Console.WriteLine(" -f /f\t--files    \tfile1 file2 * *.mp3 // the default behavior");
            Console.WriteLine(" -c /c\t--checksum \tchecksum_file1 checksum_file2 checksum_file3");
            Console.WriteLine(" -s /s\t--strings  \t\"some password\" another_password");
            Console.WriteLine(" -d /d\t--direct   \topen the STDIN for hashing - ignores other switches\n");
            Console.WriteLine(" -hf /hf\t--hashfunction:{MD5,SHA1,SHA256,SHA384,SHA512}\n");
            Console.WriteLine(" -v /v\t--verbose\textra talk - don't use if you are making digest files");
            Console.WriteLine(" -r /r\t--recursive\tsearch down into directories with provided patterns");
            Console.WriteLine(" -u /u\t--uppercasehex\tuppercase the result strings");
        }
        /// <summary>
        /// I'm going to have to decide on a philosophy in dealing with
        /// binary and text mode.  For now I'l leave this out.
        /// </summary>
        public static void ModeDirect()
        {
            Console.WriteLine("ModeDirect is disabled in this build.");
            return;
            //using (Stream s = Console.OpenStandardOutput())
            //    Console.WriteLine(Compute(s));
        }
        /// <summary>
        /// Instead of hashing the files provided we interpret them as file(s)
        /// of lists of hashes and filenames to parse and check.
        /// </summary>
        public static void ModeValidate()
        {
            Char[] splits = { ' ', '*', '=' };
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.DirectoryName != String.Empty) 
                    Environment.CurrentDirectory = fi.DirectoryName;
                
                Int32 line_count = 0;
                String[] lines = File.ReadAllLines(fi.FullName);
                foreach (string line in lines)
                {
                    line_count++;
                    String[] temp = line.Split(splits, 2);

                    if (temp.Length != 2)
                    {
                        if (verbose)
                            Console.WriteLine("\nBad Data.  File {0} at line number -{1}-\n", fi.FullName, line_count);
                        continue;
                    }

    

                    FileInfo entry = new FileInfo(temp[1]);

                    Console.Write("        {0} {1}", temp[0], entry.Name);
                    String result = Compute(GetHashAlgorithm(temp[0]), entry.FullName);

                    if (String.IsNullOrEmpty(result))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("\r[NOFILE]\n");
                        Console.ResetColor();
                    }
                    else
                    {
                        if (String.Compare(temp[0], result, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("\r [PASS] \n");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("\r [FAIL] \n");
                            if (verbose)
                                Console.Write(" [REAL] {0} {1}\n", result, temp[1]);
                            Console.ResetColor();

                        }
                    }
                }
            }
        }
        /// <summary>
        /// To quickly hash a string or series of string we use the --strings mode.
        /// </summary>
        public static void ModeString()
        {
            foreach (string str in parameters)
                Console.WriteLine("{0} {1}", Compute(str), str); ;
        }
        /// <summary>
        /// The default mode is to accept all args that are not switches and
        /// hash them as files with MD5.
        /// </summary>
        public static void ModeHashFiles()
        {
            foreach (string file in files)
            {
                string result = Compute(hashingfunction, file);
                if (result != string.Empty) Console.WriteLine("{0}{1}{2}", result, " ", file);
            }
        }
        #endregion

        #region Computation
        /// <summary>
        /// This will open a stream and digest it's contents.
        /// </summary>
        /// <param name="s">S for stream :)</param>
        /// <returns>Returns the hash result of the computation or a String.Empty value if else.</returns>
        public static string Compute(Stream s)
        {
            using (HashAlgorithm hash = GetHashAlgorithm(hashingfunction))
            {
                try
                {
                    hash.ComputeHash(s);
                    return Byte2hex(hash.Hash);
                }
                catch //(Exception e)
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// The work horse of the program.
        /// Executes the computation of the file with IO and returns the result.
        /// </summary>
        /// <param name="hf">Specify what Hashing Algorithm to
        /// use instead of working on the present set.</param>
        /// <param name="filename">The file to open and compute for given hash algorithim.</param>
        /// <returns>A hex formatted string of the hashing function.</returns>
        public static string Compute(HashingFunction hf, String filename)
        {
            using (HashAlgorithm hash = GetHashAlgorithm(hf))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(filename))
                        hash.ComputeHash(new BinaryReader(fs).BaseStream);
                    return Byte2hex(hash.Hash);
                }
                catch //(Exception e)
                {
                    return String.Empty;
                }

            }
        }
        /// <summary>
        /// The baby hashing for strings to utilize the present mode.
        /// </summary>
        /// <param name="str">String to compute</param>
        /// <returns>HEX formatted checksum.</returns>
        public static string Compute(String str)
        {
            using (HashAlgorithm hash = GetHashAlgorithm())
            {
                hash.ComputeHash(Encoding.Default.GetBytes(str));
                return Byte2hex(hash.Hash);
            }
        }
        #endregion Computation

        #region Specific Functions
        /// <summary>
        /// When a path is presented we need to 
        /// seperate the glob pattern from the directory.
        /// </summary>
        /// <param name="filename">The file name needed work.</param>
        /// <returns>Two strings are returned: [0]The directory and [1]The search pattern ie(*|*.cs) etc.</returns>
        private static string[] SeperateSearchString(string filename)
        {
            StringBuilder path = new StringBuilder();
            String[] temp = Regex.Split(filename, @"[\" + Path.DirectorySeparatorChar.ToString() + @"]", RegexOptions.CultureInvariant);
            int i = 0;
            for (i = 0; i < temp.Length - 1; i++)
            {
                path.Append(temp[i]);
                path.Append(Path.DirectorySeparatorChar);
            }
            return (new string[] { path.ToString(), temp[i] });
        }
        /// <summary>
        /// Returns the appropriate HashAlgorithm class 
        /// according to our globally set Enum HashingFunction.
        /// </summary>
        /// <returns>HashAlgorthim from ENUM-HashingFunction</returns>
        private static HashAlgorithm GetHashAlgorithm()
        {
            switch (hashingfunction)
            {
                case HashingFunction.MD5: return new MD5CryptoServiceProvider();
                case HashingFunction.SHA1: return new SHA1Managed();
                case HashingFunction.SHA256: return new SHA256Managed();
                case HashingFunction.SHA384: return new SHA384Managed();
                case HashingFunction.SHA512: return new SHA512Managed();
                default:
                    if (verbose)
                        Console.WriteLine("\nUnsupported Hash Function {0}.  Returning MD5.\n",hashingfunction.ToString());
                    return new MD5CryptoServiceProvider();
            }
        }
        /// <summary>
        /// Returns the appropriate HashAlgorithm class 
        /// according to our specified Enum HashingFunction.
        /// </summary>
        /// <param name="algo">A specified hashing function rather than what we have globally stored.</param>
        /// <returns>HashAlgorthim from ENUM-HashingFunction</returns>
        private static HashAlgorithm GetHashAlgorithm(HashingFunction algo)
        {
            switch (algo)
            {
                case HashingFunction.MD5: return new MD5CryptoServiceProvider();
                case HashingFunction.SHA1: return new SHA1Managed();
                case HashingFunction.SHA256: return new SHA256Managed();
                case HashingFunction.SHA384: return new SHA384Managed();
                case HashingFunction.SHA512: return new SHA512Managed();
                default:
                    if (verbose)
                        Console.WriteLine("\nUnknown Algorithm: '{0}'.  Returning MD5...\n", algo);
                    return new MD5CryptoServiceProvider();
            }
        }
        /// <summary>
        /// Every HEX form checksum is a fixed length string.  
        /// This returns what hashing function was used to generate the provided checksum string.
        /// </summary>
        /// <param name="checksum">The checksum to compare for a hash.</param>
        /// <returns>The appropriate HashingFunction enum of the checksum length.</returns>
        private static HashingFunction GetHashAlgorithm(String checksum)
        {
            switch (checksum.Length)
            {
                case 32: return HashingFunction.MD5;
                case 40: return HashingFunction.SHA1;
                case 64: return HashingFunction.SHA256;
                case 92: return HashingFunction.SHA384;
                case 128: return HashingFunction.SHA512;
                default:
                    if(verbose)
                        Console.WriteLine("\nUnknown checksum: '{0}'.  Attempting MD5...\n",checksum);
                    return HashingFunction.MD5;
            }
        }
        /// <summary>
        /// Simply converts the binary Hash to a HEX formated string.
        /// </summary>
        /// <param name="data">The byte array provided by a completed hashing algorithm.</param>
        /// <returns>HEX formated checksum.</returns>
        private static string Byte2hex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                if (uppercasehex)
                    sb.Append(b.ToString("X2"));
                else
                    sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// A simple string lookup and exchanging table to simplify the argument switching.
        /// </summary>
        /// <param name="value">A string to compare.</param>
        /// <returns>The result if a string is matched otherwise returns what was provided.</returns>
        private static string Substitute(string value)
        {
            switch (value)
            {
                case @"-hf:md5":
                case @"/hf:md5":
                    return @"--hashfunction:md5";
                case @"-hf:sha1":
                case @"/hf:sha1":
                    return @"--hashfunction:sha1";
                case @"-hf:sha256":
                case @"/hf:sha256":
                    return @"--hashfunction:sha256";
                case @"-hf:sha384":
                case @"/hf:sha384":
                    return @"--hashfunction:sha384";
                case @"-hf:sha512":
                case @"/hf:sha512":
                    return @"--hashfunction:sha512";

                case @"-v":
                case @"/v":
                    return @"--verbose";
                case @"-r":
                case @"/r":
                    return @"--recursive";
                case @"-c":
                case @"/c":
                    return @"--checksum";
                case @"-f":
                case @"/f":
                    return @"--files";
                case @"-d":
                case @"/d":
                    return @"--direct";

                case @"-s":
                case @"/s":
                case @"--string":
                    return @"--strings";

                case @"-u":
                case @"/u":
                    return @"--uppercasehex";

                case @"-h":
                case @"/h":
                case @"-?":
                case @"/?":
                    return @"--help";

                default:
                    return value;
            }
        }

        #endregion Specific Functions
    }
}