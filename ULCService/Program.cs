using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using RestSharp;
using static ProjectClient.Extensions;
using System.Threading.Tasks;

using static System.Environment;

namespace ProjectClient
{
    internal enum ProgressEvent
    {
        Begin,
        Report,
        End
    };

    class Program
    {
        public const string PROJECT_NAME = "ProjectClient";
        public const char CMD_SEPARATOR = '|';

        static bool     started = true, quit, verbose = true, enableSignatureService = false;
        static int      tries = 0, ipcPort = 10050;
        static string   context = string.Empty, activeContext = context,
                        protocol = "http", host = "localhost:10000",
                        resourcePath = "/api/recipe/{lec}/{cmd}", resourceInfoPath = "/api/lecture/{lec}",
                        directory = string.Empty, recipeRepository = string.Empty,
                        certFile = string.Empty, certPassword = string.Empty;

        static RestClient restClient;
        static RestRequest restRequest;

        static ManualResetEventSlim commandReceivedEvent = new ManualResetEventSlim(false);
        static ManualResetEventSlim continueRequestedEvent = new ManualResetEventSlim(false);
        static Config config = new Config();
        static List<Context> contexts = new List<Context>();

        static TcpListener tcpListener;
        static TcpClient tcpClient;

        static ProcessStartInfo processInfo;
        static RSA rsa;

        volatile static ContextCommand actualCommand = ContextCommand.NONE;

        static Program()
        {
            Console.CancelKeyPress += (sender, e) => 
            {
                e.Cancel = true; quit = true;

                if (tcpClient != null && tcpClient.Client.IsConnected())
                    tcpClient.Client.Shutdown(SocketShutdown.Both);

                commandReceivedEvent.Set();
                continueRequestedEvent.Set();
            };

            Init();
            PlatformDetect();
            config.OnConfigChanged += (index, value) => { config.WriteConfig(); };
        }

        static void PlatformDetect()
        {
            if (OSVersion.Platform == PlatformID.Win32NT)
                processInfo.FileName = "cmd.exe";
            else
                processInfo.FileName = "/bin/bash";
        }

        static void Main(string[] args)
        {
            // Use SO_REUSEADDR to reuse an idle socket (TIME_IDLE) state
            StartTCPServer();
            ServiceMain();

            // CLEANUP
            tcpListener.Stop();
            config?.WriteConfig();

            foreach (var ctx in contexts)
                ctx.Save(Path.Combine(recipeRepository, ctx.ID, "meta"));
        }

        private static Task StartTCPServer()
        {
            tcpListener = new TcpListener(IPAddress.Any, ipcPort);
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.Start(1);
            return Task.Run((Action)AcceptAndHandleClients);
        }

        internal static void InitSignatureService()
        {
            var pubcert = new X509Certificate2(certFile, certPassword);
            rsa = pubcert.GetRSAPublicKey();
        }

        static void Init()
        {
            // Create a repo directory to save scripts and files
            directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            recipeRepository = Path.Combine(directory, "repo");

            if (!Directory.Exists(recipeRepository))
                Directory.CreateDirectory(recipeRepository);

            // All chached contexts in repo directory
            foreach(var repo in Directory.EnumerateDirectories(recipeRepository))
            {
                string metadataFile = Path.Combine(repo, "meta");
                if (File.Exists(metadataFile))
                    contexts.Add(Context.Load(metadataFile));
                else
                    contexts.Add(new Context(id: Path.GetFileName(repo)));
            }

            contexts.ForEach(ctx => Log($"Context {ctx.ID} with values (installed:{ctx.Installed}) (active:{ctx.Active}) (downloaded:{ctx.Downloaded})"));

            // read config.ini in exec folder and parse all values
            config.ReadConfig(); 

            activeContext = context = config[nameof(activeContext)];

	    if(activeContext == string.Empty)
		activeContext = context = "NONE";

            bool.TryParse(config[nameof(verbose)], out verbose);
            protocol = config[nameof(protocol)];
            host = config[nameof(host)];
            resourcePath = config[nameof(resourcePath)];
            resourceInfoPath = config[nameof(resourceInfoPath)];
            int.TryParse(config[nameof(ipcPort)], out ipcPort);
            bool.TryParse(config[nameof(enableSignatureService)], out enableSignatureService);
            certFile = config[nameof(certFile)];
            certPassword = config[nameof(certPassword)];

            restClient = new RestClient(protocol + "://" + host);

            processInfo = new ProcessStartInfo();
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            processInfo.CreateNoWindow = true;

            if(enableSignatureService)
                InitSignatureService();
        }

        static void ServiceMain()
        {
            Log("Service is starting...");

            while (!quit)
            {
                if (!started || actualCommand == ContextCommand.NONE)
                {
                    Log("Wait...");
                    commandReceivedEvent.Wait();
                    commandReceivedEvent.Reset();
                    Log("Unwait...");
                    continue;
                }
                else if(actualCommand != ContextCommand.NONE)
                {
                    Log("Main loop");

                    if (tries > 0 && tries < 5)
                    {
                        Thread.Sleep(1000);
                    }
                    else if (tries >= 5)
                    {
                        actualCommand = ContextCommand.NONE; // failed
                    }
                    else
                        Log("New context: " + context);

                    SendProgressEvent(ProgressEvent.Begin, context);

                    // Download files before install command is executed
                    var ctx = contexts.Find(c => c.ID == context);

                    if (actualCommand == ContextCommand.Install && (ctx == null || !ctx.Downloaded))
                    {
                        ExecuteScriptFileForCommand(ContextCommand.Download).Wait();
                        actualCommand = ContextCommand.Install;
                        SendProgressEvent(ProgressEvent.Begin, context);
                    }

                    ExecuteScriptFileForCommand(actualCommand).Wait();
                }
            }

            Log("Service stopped!");
        }

        internal static async Task ExecuteScriptFileForCommand(ContextCommand command, bool automatic = false)
        {
            if(command == ContextCommand.Switch && activeContext != "NONE" && !string.IsNullOrWhiteSpace(activeContext) && activeContext != context)
                await ExecuteScriptFileForCommand(ContextCommand.Unswitch, true);

            var cmdName = Context.Commands[(int)command];
            var usedContext = automatic ? activeContext : context;

            string commandFilePath = Path.Combine(recipeRepository, usedContext, $"{cmdName}." + (OSVersion.Platform == PlatformID.Win32NT ? "bat" : "sh"));
            var contextRepo = Path.Combine(recipeRepository, usedContext);

            if (!Directory.Exists(contextRepo))
                Directory.CreateDirectory(contextRepo);

            restRequest = new RestRequest(resourcePath, Method.GET);
            restRequest.AddUrlSegment("lec", usedContext);
            restRequest.AddUrlSegment("cmd", cmdName);

            var targetURI = $"{protocol}://{host}{resourcePath.Replace("{lec}", usedContext).Replace("{cmd}", cmdName)}";

            Log($"GET Request to {targetURI}");

            SendProgressEvent(ProgressEvent.Report, $"Requesting {targetURI}...");

            try
            {
                var res = await restClient.ExecuteAsync(restRequest);
                var script = res.Content;

                Log($"GET Response => {res.StatusCode} ({(int)res.StatusCode})");

                if ((int)res.StatusCode >= 400)
                {
                    tries++;
                    return;
                }

                // Signature verification
                if(enableSignatureService && !VerifySignature(res))
                {
                    tries = 5;
                    Log($"Script file are not properly signed by the server!");
                    return;
                }

                // Check if the content matches the updated script file
                // Content != FileContent => WriteScriptFile
                if (!File.Exists(commandFilePath) || !File.ReadAllText(commandFilePath).Contains(script))
                {
                    WriteScriptFile(commandFilePath, script);
                    Log($"File written to: {commandFilePath}");
                }

                int exitcode = ExecuteScript(command, commandFilePath);

                if (exitcode == 0)
                    SetContextState(command, usedContext);

                actualCommand = ContextCommand.NONE;
                tries = 0;
            }
            catch (Exception e)
            {
                Log("Error during script: " + e.Message);
                tries++;
            }
            finally
            {
                if (!automatic && !(tries > 0 && tries < 5))
                    SendProgressEvent(ProgressEvent.End, activeContext);
            }
        }

        internal static int ExecuteScript(ContextCommand command, string commandFilePath)
        {
            Log("Executing script file: " + commandFilePath);
            SendProgressEvent(ProgressEvent.Report, $"Executing Script '{command}'...");

            if (OSVersion.Platform == PlatformID.Unix)
                processInfo.Arguments = commandFilePath;
            else
                processInfo.Arguments = @"/C " + commandFilePath;

            processInfo.WorkingDirectory = Path.GetDirectoryName(commandFilePath);

            List<string> output = new List<string>(), error = new List<string>();

            var proc = new Process();
            proc.StartInfo = processInfo;
            DateTime lastUpdate = DateTime.Now;

            proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                lastUpdate = DateTime.Now;
                output.Add(e.Data);
            });

            proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                lastUpdate = DateTime.Now;
                error.Add(e.Data);
            });

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            while (!proc.HasExited)
            {
                if (lastUpdate.AddMinutes(3) < DateTime.Now)
                    proc.Kill();

                Thread.Sleep(1000);
            }

            Log($"Job finished with exit code: {proc.ExitCode}");
            return proc.ExitCode;
        }

        internal static bool VerifySignature(RestResponse res)
        {
            byte[] signature = null;

            foreach (var header in res.Headers)
            {
                if (header.Name == "Signature")
                {
                    // BASE64 Encoded SHA256 signature
                    string sig = header.Value as string;

                    if (sig == null)
                        throw new System.Security.SecurityException("No signature data found!");

                    signature = Convert.FromBase64String(sig);

                    break;
                }
            }

            if (signature == null)
                return false;

            var data = res.RawBytes;
            var verified = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return verified;
        }

        internal static void WriteScriptFile(string commandFilePath, string script)
        {
            if (OSVersion.Platform == PlatformID.Unix)
                File.WriteAllText(commandFilePath, @"#!/bin/bash" + NewLine + script + NewLine + "exit $?;");
            else
                File.WriteAllText(commandFilePath, script + NewLine + @"exit /b %errorlevel%");
        }

        internal static bool SendProgressEvent(ProgressEvent evt, string arg)
        {
            bool sent = false;

            switch (evt)
            {
                case ProgressEvent.Begin:
                    sent = tcpClient.SendChannelMessage("progress-event-begin", arg);
                    break;
                case ProgressEvent.Report:
                    sent = tcpClient.SendChannelMessage("progress-event-report", arg);
                    break;
                case ProgressEvent.End:
                    sent = tcpClient.SendChannelMessage("progress-event-end", arg);
                    break;

                default:
                    break;
            }

            if (!sent)
                Log($"Status {evt} was NOT sent over tcp socket...");
	    else
		Log($"Status {evt} sent to client...");

            return sent;
        }

        internal static void SetContextState(ContextCommand command, string context)
        {
            Log($"Context-State-Set: {context} => {command}");

            var contextInCollection = contexts.Find(ctx => ctx.ID == context);

            if (contextInCollection == null && !(command == ContextCommand.Download || command == ContextCommand.Install))
                return;
            else if(contextInCollection == null && context != "NONE")
            {
                contextInCollection = new Context();
                contextInCollection.ID = context;
                contexts.Add(contextInCollection);
            }

            switch (command)
            {
                case ContextCommand.NONE:
                    contextInCollection.Downloaded = contextInCollection.Active = contextInCollection.Installed = false;
                    tcpClient.SendChannelMessage("set-context-state", $"cleaned:{context}");
                    config[nameof(activeContext)] = activeContext = context = "NONE";
                    break;
                case ContextCommand.Download:
                    contextInCollection.Downloaded = true;
                    tcpClient.SendChannelMessage("set-context-state", $"downloaded:{context}");
                    break;
                case ContextCommand.Install:
                    contextInCollection.Installed = true;
                    tcpClient.SendChannelMessage("set-context-state", $"installed:{context}");
                    break;
                case ContextCommand.Switch:
                    config[nameof(activeContext)] = activeContext = context;
                    contexts.ForEach(ctx => ctx.Active = false);
                    contextInCollection.Active = true;
                    tcpClient.SendChannelMessage("set-context-state", $"active:{context}");
                    break;
                case ContextCommand.Unswitch:
                    activeContext = "NONE";
                    contextInCollection.Active = false;
                    tcpClient.SendChannelMessage("set-context-state", $"deactive:{context}");
                    break;
                case ContextCommand.Remove:
                    contextInCollection.Installed = false;
		    contextInCollection.Downloaded = false;
                    tcpClient.SendChannelMessage("set-context-state", $"removed:{context}");
                    break;
                default:
                    break;
            }

            contextInCollection.Save(Path.Combine(recipeRepository, contextInCollection.ID, "meta"));
        }

        internal static void UpdateClient()
        {
            var uri = $"{protocol}://{host}";
            restClient = new RestClient(uri);
            Log($"Changed RestClient: {uri}");
        }

        internal static void IPCMessageReceived(string message)
        {
            Log("Message Received: " + message);

            var containsAdditionalInfos = message.IndexOf(CMD_SEPARATOR);
            string command = string.Empty;
            string addInfo = string.Empty;

            if (containsAdditionalInfos != -1)
            {
                string[] messageParts = message.Split(CMD_SEPARATOR);

                if (messageParts.Length != 2)
                    return;

                command = messageParts[0];
                addInfo = messageParts[1];
            }
            else
                command = message;

            switch(command)
            {
                case "start":
                    started = true;
                    break;

                case "stop":
                    started = false;
                    break;

                case "quit":
                    quit = true;
                    break;

                case "verbose":
                    verbose = !verbose;
                    Extensions.VERBOSE = verbose;
                    Log("Verbose Mode: " + verbose, ignoreVerbose: true);
                    return;

                case "resetcontext":
                    config[nameof(activeContext)] = activeContext = context = "NONE";
                    return;

                case "clean":
                    Log("Cleaning all repos...");

                    foreach (var file in Directory.EnumerateFiles(recipeRepository, "*.*", SearchOption.AllDirectories))
                    {
                        File.Delete(file);
                        Log($"File deleted: {file}");
                    }

                    contexts.ForEach(ctx => SetContextState(ContextCommand.NONE, ctx.ID));
                    Log("Cleanup successful!");

                    return;

                case "getcontext":
                    tcpClient.SendChannelMessage("active-context", activeContext);
                    return;

                case "getcontextdata":
                    tcpClient.SendChannelMessage("all-context-data", Newtonsoft.Json.JsonConvert.SerializeObject(contexts.ToArray()));
                    return;

                case "acontext":
                    actualCommand = ContextCommand.Switch;
                    context = addInfo;
                    break;

                case "ucontext":
                    actualCommand = ContextCommand.Unswitch;
                    context = addInfo;
                    break;

                case "dlcontext":
                    actualCommand = ContextCommand.Download;
                    context = addInfo;
                    break;

                case "icontext":
                    actualCommand = ContextCommand.Install;
                    context = addInfo;
                    break;

                case "rcontext":
                    actualCommand = ContextCommand.Remove;
                    context = addInfo;
                    break;

                case "chhost":
                    config[nameof(host)] = host = addInfo;
                    UpdateClient();
                    return;

                case "chproto":
                    config[nameof(protocol)] = protocol = addInfo;
                    UpdateClient();
                    return;

                default:
                    Log($"IPC => Unknow Message: {message}");
                    return;
            }

            if (command.IndexOf("context") != -1 && command.IndexOf("context") < 2)
                tries = 0;

            commandReceivedEvent.Set();
        }

        internal static void AcceptAndHandleClients()
        {
            while(!quit)
            {
                try
                {
                    tcpClient = tcpListener.AcceptTcpClientAsync().Result;

                    Log("Client connected: " + tcpClient.Client.RemoteEndPoint.ToString());

                    while (tcpClient.Client.Connected)
                    {
                        SpinWait.SpinUntil(() => tcpClient.Available > 0 || (tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0) || quit);

                        if (tcpClient.Available == 0) // no data received, so a shutdown is requested
                            tcpClient.Client.Shutdown(SocketShutdown.Both);

                        if (!tcpClient.Client.Connected)
                        {
                            Log("Client disconnected...");
                            break;
                        }

                        var message = tcpClient.ReadMessage();

                        if (!string.IsNullOrWhiteSpace(message))
                            IPCMessageReceived(message);
                    }
                }
                catch(Exception)
                {
                    return;
                }
            }
        }
    }
}
