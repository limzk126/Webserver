using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Webserver
{
    public static class Server
    {
        private static HttpListener listener;
        public static int maxSimultConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultConnections, maxSimultConnections);

        /// <summary>
        /// Returns list of Ip addresses assigned to localhost network devices, such as hardwired ethernet, wireless, etc.
        /// </summary>
        /// <returns></returns>
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

            return ret;
        }

        private static HttpListener InitializeListener(List<IPAddress> localhostIps)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");


            localhostIps.ForEach(ip =>
            {
                Console.WriteLine($"Listening on IP http://{ip}/");
                listener.Prefixes.Add($"http://{ip}/");
            });

            return listener;
        }
        
        public static void Start()
        {
            List<IPAddress> localHostIps = GetLocalHostIPs();
            listener = InitializeListener(localHostIps);

            Start(listener);
        }

        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        private static void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private static async void StartConnectionListener(HttpListener listener)
        {
            HttpListenerContext context = await listener.GetContextAsync();

            sem.Release();

            string response = "Hello Browser!";
            byte[] encoded = Encoding.UTF8.GetBytes(response);

            context.Response.ContentLength64 = encoded.Length;
            context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            context.Response.OutputStream.Close();
        }
    }
}
