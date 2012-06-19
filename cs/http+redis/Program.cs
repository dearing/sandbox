using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using BookSleeve;

namespace ConsoleApplication1
{
    class Program
    {

        public static RedisConnection redis = new RedisConnection("localhost");
        public static DirectoryInfo di = new DirectoryInfo(@"./");

        static void Main(string[] args)
        {
            Console.WriteLine(di);
            if (!di.Exists)
            {
                Console.WriteLine("Invalid root: {0}");
                return;
            }



            List<String> prefix = new List<string>();
            prefix.Add("http://localhost:8080/");
            prefix.Add("http://localhost:8081/");

            using (redis.Open())
            {
                redis.SetKeepAlive(10);
                redis.Error += redis_Error;
                //Console.WriteLine("REDIS connection {0} opened.\t{1}", redis.Host, redis);

                Webserver(prefix.ToArray());


                Console.WriteLine("stalling...");
                Console.ReadLine();
            }

        }

        static void redis_Error(object sender, BookSleeve.ErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("REDIS >> {0}", e.Exception.Message);
            Console.ResetColor();
        }

        static async void Webserver(string[] prefixes)
        {
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            var listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
                Console.WriteLine("Prefix :{0}",s);
            }

            listener.Start();
            Console.WriteLine("{0}=>Listening...", listener);

            while (listener.IsListening)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                var context = await listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                //Console.WriteLine("{0} >> {1} >> {2}", request.RemoteEndPoint, request.HttpMethod, request.RawUrl);
   

                String req = di.FullName + (request.RawUrl).Split(new char[] {'?'},StringSplitOptions.RemoveEmptyEntries).First();


                switch (req)
                {
                    case "/":
                        await ServeRedisAsync(new FileInfo(req + "index.html"), response);
                        break;
                    case "/exit":
                        response.Close();
                        listener.Abort();
                        break;
                    default:
                        await ServeRedisAsync(new FileInfo(req), response);
                        break;
                }
            }
            listener.Stop();
            //Console.WriteLine("REDIS connection {0} opened.", redis.Host);
            redis.Close(true);
            redis.Error -= redis_Error;
            redis.Dispose();
        }

        static async Task ServeRedisAsync(FileInfo file, HttpListenerResponse response)
        {
            var content = await redis.Strings.Get(0, file.FullName);

            if (content == null)
            {
                if (!file.Exists)
                {
                    //Console.WriteLine(@"404 >> {0}", file.FullName);
                    response.StatusCode = 404;
                    var data = System.Text.Encoding.UTF8.GetBytes("<html><body><p>404: file not found</p></body></html>");
                    await response.OutputStream.WriteAsync(data, 0, data.Length);
                }
                else
                {
                    //Console.WriteLine(@"200 >> {0}", file.FullName);
                    response.StatusCode = 200;
                    var data = File.ReadAllBytes(file.FullName);
                    await response.OutputStream.WriteAsync(data, 0, data.Length);
                    await redis.Strings.Set(0, file.FullName, data);
                    //Console.WriteLine("REDIS:0 >> Updated with contents of {0}", file.FullName);
                }
            }
            else
            {
                await response.OutputStream.WriteAsync(content, 0, content.Length);
            }
            response.Close();
        }

        static async Task ServeStaticAsync(FileInfo file, HttpListenerResponse response)
        {
            if (!file.Exists)
            {
                Console.WriteLine(@"404 >> {0}",file.FullName);
                response.StatusCode = 404;
                var data = System.Text.Encoding.UTF8.GetBytes("<html><body><p>404: file not found</p></body></html>");
                await response.OutputStream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                Console.WriteLine(@"200 >> {0}", file.FullName);
                response.StatusCode = 200;
                var data = File.ReadAllBytes(file.FullName);
                await response.OutputStream.WriteAsync(data, 0, data.Length);
            }
            response.Close();
        }

    }
}
