using RestSharp;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectClient
{
    public static class Extensions
    {
        const string MESSAGE_TERMINATOR = "|||";

        public static string ReadMessage(this TcpClient client)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[10000];
            int read = 0;
            var stream = client.GetStream();

            while (client.Available > read)
            {
                read += stream.Read(buffer, 0, buffer.Length);
                string part = new string(Encoding.UTF8.GetChars(buffer, 0, read));
                builder.Append(part);
            }

            return builder.ToString();
        }

        public static bool SendMessage(this TcpClient client, string message)
        {
            if (client.Connected)
            {
                var transmit = Encoding.UTF8.GetBytes(message + MESSAGE_TERMINATOR);
                var stream = client.GetStream();
                stream.Write(transmit, 0, transmit.Length);
                stream.Flush();
                return true;
            }
            else
                return false;
        }

        public static bool SendStatusMessage(this TcpClient client, string channel, string status)
        {
            return client.SendMessage($"{channel}|{status}");
        }

        public static bool IsConnected(this Socket sock)
        {
            try
            {
                return !(sock.Poll(1, SelectMode.SelectRead) && sock.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public static async Task<RestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }

        public static void Log(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Console.WriteLine($"[{Assembly.GetEntryAssembly().GetName().Name}:{Path.GetFileName(sourceFilePath)}@{sourceLineNumber}]: {message}");
        }
    }
}
