using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

[assembly: AssemblyTitle("MultiZip")]
[assembly: AssemblyDescription("A simple program to compress a pattern of files individually within a working directory.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jacob Dearing")]
[assembly: AssemblyProduct("multizip")]
[assembly: AssemblyCopyright("Copyright © Jacob Dearing 2009")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("0.5.*")]
[assembly: AssemblyFileVersion("0.5.0.0")]

namespace multizip
{
    class Program
    {
        #region Fields

        private static Boolean verbose, quiet, overwrite;
        private static FileInfo fi;
        private static List<String> patterns = new List<String>();

        #endregion Fields

        static void Main(String[] args)
        {
            verbose = quiet = overwrite = false;
            HandleArgs(args);

            if (patterns.Count == 0)
                ModeHelp();

            foreach (String pattern in patterns)
            {
                try
                {
                    foreach (String file in Directory.GetFiles(Environment.CurrentDirectory, pattern))
                    {
                        fi = new FileInfo(file);
                        Print(String.Format(" >>   Compressing {0}", fi.Name));
                        long c = CompressFile(fi);
                        if (c != -1)
                            Print(String.Format("\r {0:0%} \n", (decimal)c / fi.Length), ConsoleColor.Cyan);
                        else
                            Print(String.Format("\r     \n", ConsoleColor.DarkRed));
                    }
                }
                catch (Exception e)
                {
                    if (verbose)
                        Print(String.Format("\n{0}\n{1}\n", e.Message, e.StackTrace), ConsoleColor.DarkRed);
                    else
                        Print(String.Format("\n{0}\n", e.Message));
                }
            }
        }

        private static long CompressFile(FileInfo fi)
        {
            long w = 0;
            if (!overwrite && File.Exists(fi.Name.Replace(fi.Extension, ".zip")))
            {
                if (!quiet)
                    Print(String.Format("\r !!   Skipping extant file {0}", fi.Name), ConsoleColor.DarkCyan);
                return -1;
            }
            using (Stream z = File.Open(fi.Name.Replace(fi.Extension, ".zip"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (ZipFile zip = new ZipFile(z))
                {
                    zip.BeginUpdate();
                    zip.Add(fi.Name);
                    zip.CommitUpdate();
                    w = z.Length;
                    zip.Close();
                }
                return w;
            }
        }

        private static void HandleArgs(String[] args)
        {
            foreach (String arg in args)
            {
                switch (arg)
                {
                    case "-v":
                    case "/v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "-q":
                    case "/q":
                    case "--quiet":
                        quiet = true;
                        break;
                    case "-o":
                    case "/o":
                    case "--overwrite":
                        overwrite = true;
                        break;
                    case "-h":
                    case "/h":
                    case "-?":
                    case "/?":
                    case "--help":
                        ModeHelp();
                        break;
                    default:
                        patterns.Add(arg);
                        break;
                }
            }
        }
        private static void ModeHelp()
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.Reflection.AssemblyName n = a.GetName();
            Console.WriteLine("{0} version {1}.{2} build {3} revision {4}", n.Name.ToUpper(), n.Version.Major, n.Version.Minor, n.Version.Build, n.Version.Revision);
            Console.WriteLine(
                 "A simple program to compress a pattern of files individually, swapping their extensions for `.zip`.\n\n"
                 + "   {0} [@switch(s)] [pattern(s)]\n\n"
                 + "switch:\n\n"
                 + "   -q /q --quiet        - no information while working\n"
                 + "   -v /v --verbose      - more information while working\n"
                 + "   -o /o --overwrite    - overwrite extant archives\n"
                 + "   -h /h -? /? --help   - display program information and usage\n"
                 , n.Name.ToUpper()
                );
            Environment.Exit(0);
        }

        private static void Print(String Message)
        {
            if (quiet)
                return;
            Console.Write(Message);
        }
        private static void Print(String Message, ConsoleColor Color)
        {
            if (quiet)
                return;

            Console.ForegroundColor = Color;
            Console.Write(Message);
            Console.ResetColor();
        }
    }
}
