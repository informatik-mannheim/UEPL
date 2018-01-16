using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ProjectClientConfig
{
    class Program
    {
        static TcpClient client = new TcpClient();

        static void Main(string[] args)
        {
            if (args.Length != 1)
                return;

            try
            {
                client.ConnectAsync("127.0.0.1", 10050).Wait();

                if (client.Connected)
                {
                    using (StreamWriter sw = new StreamWriter(client.GetStream()))
                    {
                        sw.Write(args[0]);
                        sw.Flush();
                    }
                }
            } catch(Exception e) { Console.Error.WriteLine("Error: " + e.Message);  }
        }
    }
}