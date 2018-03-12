console.time("init");

const { app, BrowserWindow, dialog } = require("electron");
const ipc = require("electron").ipcMain;
const url = require("url");
const path = require("path");
const net = require("net");
const ps = require("./lib/ps-node");

const loadJson = require("load-json-file");
let config = {};
loadJson("config.json").then(json => { config = json; });

let win;
let sockData = "";
let winIPCSender = null;
let client = new net.Socket();
client.connected = false;

// Disable Hardware Acceleration (MacOS virtual environment doesn't support 'GPU Acceleration')
app.disableHardwareAcceleration();

function ab2str(buf) { return String.fromCharCode.apply(null, new Uint8Array(buf)); }

function handleData(data) 
{  
    if(data.indexOf("|||") != -1)
    {
        var splitted = data.split("|||");

        for(var i = 0; i < splitted.length; i++)
        {
            if(splitted[i] !== "")   
                handleData(splitted[i]);
        }

        return;
    }

    if(data.indexOf("|") == -1)
        return;

    var localData = data.split("|");

    if(winIPCSender !== null)
        winIPCSender.send(localData[0], localData[1]);
}



client.on("data", data => { sockData = ab2str(data); handleData(sockData); });
client.on("error", err => console.error(err));
client.on("close", () => { client.destroy(); client.connected = false; });

ipc.on("message-host", (event, arg) => 
{
    if(winIPCSender === null)
        winIPCSender = event.sender;

    if(client.connected)
        client.write(arg);
    else
    {
        client.removeAllListeners("connect");
        client.connect(config.servicePort || 10050, config.serviceIP || "127.0.0.1", () => { client.connected = true; client.write(arg); });
    }
});

ipc.on("check-services", (event, arg) => 
{
    let ocService = "owncloud";
    let ulcService = "dotnet";

    if(process.platform == "win32")
    {
        ocService += ".exe";
        ulcService += ".exe";
    }

    checkServiceStatus(ocService).then(result => {event.sender.send("service-status", "owncloud", result); });
    checkServiceStatus(ulcService).then(result => { event.sender.send("service-status", "pclient", result); });
});

function checkServiceStatus(name)
{
    let p = new Promise((resolve, reject) => 
    {
        ps.lookup({command: name}).then(resultList => 
        {
            for(let i = 0; i < resultList.length; i++)
            {
                let process = resultList[i];

                if (process)
                {
                    resolve(true);
                    break;
                }
            }

            resolve(false);
        });
    });

    return p;
}



function createMainWindow()
{
    win = new BrowserWindow({width: 800, height: 600, show: true});
    win.webContents.openDevTools({ mode: "undocked" });

    let loadUri = url.format({ 
        pathname: path.join(__dirname, "login.html"),
        protocol: 'file:',
        slashes: true
    });

    win.setMenuBarVisibility(false);
    win.loadURL(loadUri);
    win.on("closed", () => win = null);
}

app.on("ready", () => 
{
    console.timeEnd("init");
    console.time("window");
    createMainWindow();
    console.timeEnd("window");
    
    client.connect(config.servicePort || 10050, config.serviceIP || "127.0.0.1", () => client.connected = true);
});

app.on("window-all-closed", () => 
{
    if(process.platform !== "darwin")
        app.quit();
});

app.on("activate", () => 
{
    if(win === null)
        createMainWindow();
});