using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;


[assembly: AssemblyTitle("hash")]
[assembly: AssemblyDescription("message digest command line tool")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jacob Dearing")]
[assembly: AssemblyProduct("hash")]
[assembly: AssemblyCopyright("Copyright © Jacob Dearing 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("fa39c574-8025-49c1-b90d-560136d9d0c9")]
[assembly: AssemblyVersion("0.5.*")]
[assembly: AssemblyFileVersion("0.5.0.0")]

namespace hash
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = String.Empty;
            List<string> p = new List<string>();

            if (args.Length > 0)
                s = args[0];
            else
            {
                Help();
                return;
            }
            if (args.Length > 1)
            {
                p.AddRange(args);
                p.RemoveAt(0);
            }

            foreach (var item in s.ToLowerInvariant())
            {
                switch (item)
                {
                    case 'h':
                        Help();
                        return;
                    case 's':
                        Engine.StringsMode = true;
                        break;
                    case 'r':
                        Engine.Recursive = true;
                        break;
                    case 'u':
                        Engine.UppperCase = true;
                        break;
                    case 'd':
                        if (p.Count > 0)
                        {
                            try
                            {
                                Engine.Mode = (DigestMode)Enum.Parse(typeof(DigestMode), p[0], true);
                                p.RemoveAt(0);
                            }
                            catch (ArgumentException e)
                            {
                                Console.WriteLine("`{0}` is not a recognized digest method",p[0]);
                                return;
                            }
                            
                        }
                        else
                        {
                            Console.WriteLine("digest method switch present without supplemental argument");
                            return;
                        }
                        break;
                }
            }

            List<String> q = new List<String>();
            
            foreach (var item in p)
                if (item.Contains("*") && !item.StartsWith(".."))
                    q.AddRange(Directory.GetFiles(@".\", item, Engine.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                else
                    q.Add(item);

            p = q;

            if (Engine.StringsMode)
                foreach (var item in p)
                    Console.WriteLine("{0}={1}", Engine.DigestString(item, Engine.Mode), item);
            else
                foreach (var item in p)
                    Console.WriteLine("{0}={1}", Engine.DigestFile(item, Engine.Mode), item);

        }

        public static void Help()
        {
            var me = Assembly.GetExecutingAssembly().GetName().Name.ToUpperInvariant();
            Console.WriteLine(
                "\n{0} v{1}  digest a set of strings or files",
                me,
                Assembly.GetExecutingAssembly().GetName().Version
                );

            Console.WriteLine("syntax: {0} [dhrsu] [contextual parameters]\n", me);
            Console.WriteLine("  h:    show this help");
            Console.WriteLine("  r:    recursivly iterate through folders");
            Console.WriteLine("  u:    return the hash as uppercase");
            Console.WriteLine("  d:    set the digest method: MD5, *SHA1, SHA256, SHA384, SHA512");
            Console.WriteLine("  s:    parameters are whole strings to be hashed\n");

            Console.WriteLine("{0} defaults to accepting a list of files to be hashed with SHA1",me);
            Console.WriteLine("example usage:");
            Console.WriteLine("\t{0} s \"dig dug\" dig dug",me);
            Console.WriteLine("\t{0} sd md5 \"dig dug\" dig dug", me);
            Console.WriteLine("\t{0} file.dll file.exe *.txt",me);
            Console.WriteLine("\t{0} dr sha256 *",me);
                       
        }
    }

    public enum DigestMode
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    static class Engine
    {
        public static Boolean Recursive     = false;
        public static Boolean StringsMode   = false;
        public static Boolean UppperCase    = false;
        public static DigestMode Mode       = DigestMode.SHA1;

        public static String Byte2hex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                if (UppperCase)
                    sb.Append(b.ToString("X2"));
                else
                    sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static String DigestFile(String uri, DigestMode mode = DigestMode.SHA1)
        {
            byte[] data = new byte[0];

            if (File.Exists(uri))
            {
                FileInfo fi = new FileInfo(uri);
                switch (mode)
                {
                    case DigestMode.MD5:
                        data = MD5.Create().ComputeHash(fi.OpenRead());
                        break;
                    case DigestMode.SHA1:
                        data = SHA1.Create().ComputeHash(fi.OpenRead());
                        break;
                    case DigestMode.SHA256:
                        data = SHA256.Create().ComputeHash(fi.OpenRead());
                        break;
                    case DigestMode.SHA384:
                        data = SHA384.Create().ComputeHash(fi.OpenRead());
                        break;
                    case DigestMode.SHA512:
                        data = SHA512.Create().ComputeHash(fi.OpenRead());
                        break;
                }
            }

            return Engine.Byte2hex(data);
        }

        public static String DigestString(String g, DigestMode mode = DigestMode.SHA1)
        {
            byte[] data = new byte[0];
            switch (mode)
            {

                case DigestMode.MD5:
                    data = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(g));
                    break;
                case DigestMode.SHA1:
                    data = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(g));
                    break;
                case DigestMode.SHA256:
                    data = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(g));
                    break;
                case DigestMode.SHA384:
                    data = SHA384.Create().ComputeHash(Encoding.ASCII.GetBytes(g));
                    break;
                case DigestMode.SHA512:
                    data = SHA512.Create().ComputeHash(Encoding.ASCII.GetBytes(g));
                    break;
            }

            return Engine.Byte2hex(data);
        }

    }
}