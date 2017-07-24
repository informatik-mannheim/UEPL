using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

using RestSharp;
using static ProjectClient.Extensions;
using System.Threading.Tasks;

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

        static bool     started = true, quit, verbose = true;
        static int      tries = 0, ipcPort = 10050;
        static string   context = string.Empty, activeContext = context,
                        protocol = "http", host = "elke.sr.hs-mannheim.de:10000",
                        resourcePath = "/api/recipe/{lec}/{cmd}", resourceInfoPath = "/api/lecture/{lec}",
                        directory = string.Empty, recipeRepository = string.Empty;

        static RestClient restClient;
        static RestRequest restRequest;
        //static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        static ManualResetEventSlim commandReceivedEvent = new ManualResetEventSlim(false);
        static ManualResetEventSlim continueRequestedEvent = new ManualResetEventSlim(false);
        static Config config = new Config();
        static List<Context> contexts = new List<Context>();
        static TcpListener tcpListener;
        static TcpClient tcpClient;
        volatile static ContextCommand actualCommand = ContextCommand.NONE;

        static Program()
        {
            Console.CancelKeyPress += (sender, e) => 
            {
                e.Cancel = true;
                quit = true;

                if (tcpClient != null && tcpClient.Client.IsConnected())
                    tcpClient.Client.Shutdown(SocketShutdown.Both);

                commandReceivedEvent.Set();
                continueRequestedEvent.Set();
            };

            Init();
            config.OnConfigChanged += (index, value) => { config.WriteConfig(); };
        }

        static void Main(string[] args)
        {
            // Use SO_REUSEADDR to reuse an idle socket (TIME_IDLE) state
            tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            tcpListener.Start(1);
            new Thread(new ThreadStart(AcceptAndHandleClients)).Start(); // Handle Incoming Connections & Commands
            ServiceMain(); // Actual Main Loop

            // CLEANUP
            tcpListener.Stop();
            config?.WriteConfig();

            foreach (var ctx in contexts)
                ctx.Save(Path.Combine(recipeRepository, ctx.ID, "meta"));
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
                    contexts.Add(Context.Parse(File.ReadAllText(metadataFile)));
                else
                    contexts.Add(new Context(false, false, false, string.Empty, Path.GetFileName(repo)));
            }

            foreach(var ctx in contexts)
            {
                Log($"Context {ctx.ID} with values (installed:{ctx.Installed}) (active:{ctx.Active}) (downloaded:{ctx.Downloaded})");
            }

            // read config.ini in exec folder and parse all values
            config.ReadConfig(); 

            activeContext = context = config[nameof(activeContext)];
            bool.TryParse(config[nameof(verbose)], out verbose);
            protocol = config[nameof(protocol)];
            host = config[nameof(host)];
            resourcePath = config[nameof(resourcePath)];
            resourceInfoPath = config[nameof(resourceInfoPath)];
            int.TryParse(config[nameof(ipcPort)], out ipcPort);

            restClient = new RestClient(protocol + "://" + host);
            tcpListener = new TcpListener(IPAddress.Any, ipcPort);
        }

        static void ServiceMain()
        {
            Log("Service is starting...");

            while (!quit)
            {
                if (!started || actualCommand == ContextCommand.NONE)
                {
                    if(verbose)
                        Log("Wait...");

                    commandReceivedEvent.Wait();
                    commandReceivedEvent.Reset();

                    if(verbose)
                        Log("Unwait...");

                    continue;
                }
                else if(actualCommand != ContextCommand.NONE)
                {
                    if (verbose)
                        Log("main loop");

                    if (tries > 0 && tries < 5)
                    {
                        Thread.Sleep(1000);
                    }
                    else if (tries >= 5)
                    {
                        // TODO: go back to previous working context
                        if (verbose)
                            Log("Semi wait...");

                        commandReceivedEvent.Wait();
                        commandReceivedEvent.Reset();

                        if (verbose)
                            Log("Semi unwait...");

                        continue;
                    }
                    else
                        Log("New context: " + context);

                    SendProgressEvent(ProgressEvent.Begin, context);
                    ExecuteRestRequest(actualCommand).Wait();
                }
            }

            Log("Service stopped!");
        }

        private static async Task ExecuteRestRequest(ContextCommand command, bool automatic = false)
        {
            if(command == ContextCommand.Switch && activeContext != "NONE" && !string.IsNullOrWhiteSpace(activeContext) && activeContext != context)
                await ExecuteRestRequest(ContextCommand.Unswitch, true);

            var cmdName = Context.Commands[(int)command];
            var usedContext = automatic ? activeContext : context;

            string commandFilePath = Path.Combine(recipeRepository, usedContext, $"{cmdName}.sh");
            var contextRepo = Path.Combine(recipeRepository, usedContext);

            if (!Directory.Exists(contextRepo))
                Directory.CreateDirectory(contextRepo);

            restRequest = new RestRequest(resourcePath, Method.GET);
            restRequest.AddUrlSegment("lec", usedContext);
            restRequest.AddUrlSegment("cmd", cmdName);

            var targetURI = $"{protocol}://{host}{resourcePath.Replace("{lec}", usedContext).Replace("{cmd}", cmdName)}";

            if (verbose)
                Log($"GET Request to {targetURI}");

            SendProgressEvent(ProgressEvent.Report, $"Requesting {targetURI}...");

            // Possible change: Callback to async code due to an extension
            // https://stackoverflow.com/questions/41390647/how-to-use-restsharp-netcore-in-asp-net-core
            // var res = restClient.ExecuteAsync(restRequest).Result;
            // then it isn't necessary to trigger a ResetEvent in order to continue with the execution

            try
            {
                var res = await restClient.ExecuteAsync(restRequest);

                var script = res.Content; 
                //var script = res.Content.Replace(" && ", " || true && ");

                if (verbose)
                    Log($"GET Response => {res.StatusCode} ({(int)res.StatusCode})");

                if ((int)res.StatusCode >= 400)
                {
                    tries++;
                    return;
                }

                if (!File.Exists(commandFilePath) || !File.ReadAllText(commandFilePath).Contains(script))
                {
                    File.WriteAllText(commandFilePath, "#!/bin/bash" + Environment.NewLine + script + Environment.NewLine + "exit $?;");
                    Log($"File written to: {commandFilePath}");
                }

                if (verbose)
                    Log("Executing script file: " + commandFilePath);

                SendProgressEvent(ProgressEvent.Report, $"Executing Script '{command}'...");

                var psi = new ProcessStartInfo("/bin/bash", commandFilePath);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = false;
                psi.RedirectStandardError = false;
                psi.WorkingDirectory = Path.GetDirectoryName(commandFilePath);
                var proc = Process.Start(psi);
                proc.WaitForExit();

                int exitcode = proc.ExitCode;
                Log($"Job finished with exit code: {exitcode}");

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

        static void SendProgressEvent(ProgressEvent evt, string arg)
        {
            switch (evt)
            {
                case ProgressEvent.Begin:
                    tcpClient.SendStatusMessage("progress-event-begin", arg);
                    break;
                case ProgressEvent.Report:
                    tcpClient.SendStatusMessage("progress-event-report", arg);
                    break;
                case ProgressEvent.End:
                    tcpClient.SendStatusMessage("progress-event-end", arg);
                    break;
            }
        }

        static void SetContextState(ContextCommand command, string context)
        {
            var contextInCollection = contexts.Find(ctx => ctx.ID == context);

            if (contextInCollection == null)
                return;

            switch (command)
            {
                case ContextCommand.NONE:
                    contextInCollection.Downloaded = contextInCollection.Active = contextInCollection.Installed = false;
                    tcpClient.SendStatusMessage("set-context-state", $"cleaned:{context}");
                    config[nameof(activeContext)] = activeContext = context = "NONE";
                    break;
                case ContextCommand.Download:
                    contextInCollection.Downloaded = true;
                    tcpClient.SendStatusMessage("set-context-state", $"downloaded:{context}");
                    break;
                case ContextCommand.Install:
                    contextInCollection.Installed = true;
                    tcpClient.SendStatusMessage("set-context-state", $"installed:{context}");
                    break;
                case ContextCommand.Switch:
                    config[nameof(activeContext)] = activeContext = context;
                    contexts.ForEach(ctx => ctx.Active = false);
                    contextInCollection.Active = true;
                    tcpClient.SendStatusMessage("set-context-state", $"active:{context}");
                    break;
                case ContextCommand.Unswitch:
                    activeContext = "NONE";
                    contextInCollection.Active = false;
                    tcpClient.SendStatusMessage("set-context-state", $"deactive:{context}");
                    break;
                case ContextCommand.Remove:
                    contextInCollection.Installed = false;
                    tcpClient.SendStatusMessage("set-context-state", $"removed:{context}");
                    break;
                default:
                    break;
            }
        }

        static void UpdateClient()
        {
            var combination = $"{protocol}://{host}";
            restClient = new RestClient(combination);
            Log($"Changed RestClient: {combination}");
        }

        static void IPCMessageReceived(string message)
        {
            if (verbose)
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
                    Log("Verbose Mode: " + verbose);
                    return;

                case "resetcontext":
                    config[nameof(activeContext)] = activeContext = context = "NONE";
                    return;

                case "clean":
                    Log("Cleaning all repos...");

                    foreach (var file in Directory.EnumerateFiles(recipeRepository, "*.sh", SearchOption.AllDirectories))
                    {
                        File.Delete(file);
                        Log($"File deleted: {file}");
                    }

                    contexts.ForEach(ctx => SetContextState(ContextCommand.NONE, ctx.ID));
                    Log("Cleanup successful!");

                    return;

                case "getcontext":
                    tcpClient.SendStatusMessage("active-context", activeContext);
                    return;

                case "getcontextdata":
                    tcpClient.SendStatusMessage("all-context-data", Newtonsoft.Json.JsonConvert.SerializeObject(contexts.ToArray()));
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

        static void AcceptAndHandleClients()
        {
            while(!quit)
            {
                try
                {
                    tcpClient = tcpListener.AcceptTcpClientAsync().Result;

                    if (verbose)
                        Log("Client connected: " + tcpClient.Client.RemoteEndPoint.ToString());

                    while (tcpClient.Client.Connected)
                    {
                        SpinWait.SpinUntil(() => tcpClient.Available > 0 || (tcpClient.Client.Poll(1, SelectMode.SelectRead) && tcpClient.Client.Available == 0) || quit);

                        if (tcpClient.Available == 0) // no data received, so a shutdown is requested
                            tcpClient.Client.Shutdown(SocketShutdown.Both);

                        if (!tcpClient.Client.Connected)
                        {
                            if (verbose)
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