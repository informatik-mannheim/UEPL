const ipc = require("electron").ipcRenderer;
let interval;
let lectures = [];
let dirty = false;
let activeContext = "";
let contexts = [];
let panelButtonContainers = [];

ipc.on("message-client", (event, arg) => 
{
    const message = `Message from main: ${arg}`;
    //document.getElementById("message").innerHTML = message;
    console.debug(message);
});

document.getElementById("testbtn").addEventListener("click", () => ipc.send("message-host", "getcontext"));
document.getElementById("cleanbtn").addEventListener("click", () => ipc.send("message-host", "clean"));
document.getElementById("resetbtn").addEventListener("click", () => ipc.send("message-host", "resetcontext"));
document.getElementById("verbosebtn").addEventListener("click", () => ipc.send("message-host", "verbose"));

function SetActiveContext(context)
{
    if(context !== activeContext)
    {
        activeContext = context;
        
        panelButtonContainers.forEach(pbc => 
        {
            if(pbc.DATA.id == activeContext)
                pbc.Active = true;
            else
                pbc.Active = false;
        });
    }
}

ipc.on("active-context", (event, arg) => 
{
    SetActiveContext(arg);
    console.debug("active context: " + arg);
});

ipc.on("progress-event-begin", (event, arg) => 
{
    window.clearInterval(interval);
    document.getElementById("loader-container").classList.remove("hide");
    document.getElementById("content").classList.add("hide");
    console.debug("begin progress-event with arg: " + arg);
});

ipc.on("progress-event-report", (event, arg) => 
{
    document.getElementById("loader-text").textContent = arg;
    console.debug("report in progress-event: " + arg);
});

ipc.on("progress-event-end", (event, arg) => 
{
    document.getElementById("loader-container").classList.add("hide");
    document.getElementById("content").classList.remove("hide");
    console.debug("end progress-event with arg: " + arg);
    interval = window.setInterval(updateData, 30000);
});

ipc.on("service-status", (event, ...args) => 
{
    if(args.length !== undefined && args.length == 2)
    {
        console.log(args[0]);
        console.log(args[1]);
    }
    else
        console.log(args);
});

ipc.on("set-context-state", (event, arg) => 
{
    let index = arg.indexOf(":");

    if(index != -1)
    {
        let splitted = arg.split(":");

        if(splitted.length != 2)
            return;

        let pcb = panelButtonContainers.find(val => val.DATA.id === splitted[1]);

        if(pcb === undefined)
            return;

        switch(splitted[0])
        {
            case "active":
                activeContext = pcb.DATA.id;
                pcb.Active = true;
            break;

            case "deactive":
                activeContext = "NONE";
                pcb.Active = false;
            break;

            case "installed":
                pcb.Installed = true;
            break;

            case "removed":
                pcb.Installed = false;
            break;

            case "downloaded":
                pcb.Downloaded = true;
            break;

            case "cleaned":
                pcb.Clear();
            break;
        }

    }

});

ipc.on("all-context-data", (event, arg) =>
{
    let data = JSON.parse(arg);

    for(var i = 0; i < data.length; i++)
    {
        let ctx = data[i];
        let pcb = panelButtonContainers.find(val => val.DATA.id === ctx.ID);

        if(pcb === undefined)
            return;
        
        pcb.Active = ctx.Active;
        pcb.Installed = ctx.Installed;
        pcb.Downloaded = ctx.Downloaded;
    }

});

let update = () => 
{
    panelButtonContainers = [];

    let lectureContainer = document.getElementById("elements");
    lectureContainer.innerHTML = "";

    lectures.forEach(lec => 
    {
        let obj = new PanelButtonContainer(lec, (event) => {
            let txt = event.target.textContent;
            let cmd = "";

            if(txt === "Change Context")
                cmd = "acontext";
            else if(txt === "Download & Install")
                cmd = "icontext";
            else if(txt === "Remove")
                cmd = "rcontext";
            else if(txt === "Deactivate")
                cmd ="ucontext";
            else
                return;

            ipc.send("message-host", `${cmd}|${lec.id}`);
        });
        panelButtonContainers.push(obj);
        lectureContainer.appendChild(obj.DOM);    
    });

    ipc.send("message-host", "getcontextdata");
    dirty = false;
};

let updateData = () => 
{
    console.debug("Checking for new lectures...");
    fetch("http://elke.sr.hs-mannheim.de:10000/api/lecture").then(response => 
    {
        if(response.ok)
            return response.json();
        else
            return null;
    }).then(data => 
    {
        if(data === null)
            return;
        
        for(var i = 0; i < data.length; i++)
        {
            if(!lectures.some(elem => elem.id == data[i].id))
            {
                lectures.push(data[i]);
                dirty = true;
            } 
        }

        if(dirty)
            update();

    }).catch(err => { console.error(err); });
};

// get updated lectures
interval = window.setInterval(updateData, 30000);
updateData();
ipc.send("message-host", "getcontext");
ipc.send("check-services");

class PanelButtonContainer
{
    UpdateStyle()
    {
        this.btn.classList.remove("btn-primary", "btn-success", "btn-default", "btn-info", "btn-danger");
        this.secBtn.classList.remove("btn-primary", "btn-success", "btn-default", "btn-info", "btn-danger");
        this.dom.classList.remove("panel-primary", "panel-success", "panel-default", "panel-info");
        this.icon.classList.remove("fa-square-o", "fa-check-square-o");

        if(this.active)
        {
            this.icon.classList.add("fa-check-square-o");            
            this.dom.classList.add("panel-primary");
            this.btn.classList.add("btn-danger");
            this.btn.textContent = "Deactivate";
            this.secBtn.classList.add("hide");
        }
        else if(this.installed)
        {
            this.icon.classList.add("fa-check-square-o");            
            this.dom.classList.add("panel-success");
            this.btn.textContent = "Change Context";
            this.btn.classList.add("btn-primary");
            this.secBtn.textContent = "Remove";
            this.secBtn.classList.add("btn-danger");
            this.secBtn.classList.remove("hide");
        }
        else
        {
            this.icon.classList.add("fa-square-o");
            this.dom.classList.add("panel-default");
            this.btn.textContent = "Download & Install";
            this.btn.classList.add("btn-info");
            this.secBtn.classList.add("hide");
        }
    }

    get Active() { return this.active; }
    set Active(value)
    {
        if(this.active === value)
            return;

        this.active = value;
        this.UpdateStyle();
    }

    get Installed() { return this.installed; }
    set Installed(value) 
    { 
        if(this.installed === value) 
            return;

        this.installed = value;
        this.UpdateStyle();
    } 

    Clear() { this.active = this.downloaded = this.installed = false; this.UpdateStyle(); }

    get DATA() { return this.data; }
    get DOM() { return this.domContainer; }

    constructor(element, clickEvent)
    {
        // Data
        this.data = element;
        this.installed = this.active = this.data.id === activeContext;
        this.clickEvent = clickEvent;

        // Base Container & Sizing
        this.domContainer = document.createElement("div");
        this.domContainer.classList.add("col-lg-4", "col-md-4", "col-sm-6", "col-xs-12");

        // Panel Container
        this.dom = document.createElement("div");
        this.dom.classList.add("panel", this.active ? "panel-primary" : "panel-default", "text-center");
        
        // HEADING
        this.header = document.createElement("div");
        this.header.classList.add("panel-heading");
            this.headerTitle = document.createElement("h3");
            this.headerTitle.classList.add("panel-title");
            this.headerTitle.textContent = `${element.id}`;
            this.iconSpan = document.createElement("span");
            this.iconSpan.classList.add("pull-right");
            this.iconSpan.style.fontSize = "20px";
            this.iconSpan.style.fontWeight = "bold";
                this.icon = document.createElement("i");
                this.icon.classList.add("fa", this.installed ? "fa-check-square-o" : "fa-square-o");
            this.iconSpan.appendChild(this.icon);
        this.header.appendChild(this.headerTitle);
        this.header.appendChild(this.iconSpan);
        this.dom.appendChild(this.header);

        // BODY
        this.content = document.createElement("div");
        this.content.classList.add("panel-body");
            this.text = document.createElement("p");
            this.text.textContent = `Lecture: ${element.name}`;
        this.content.appendChild(this.text);
        this.dom.appendChild(this.content);    
        
        // FOOTER
        this.footer = document.createElement("div");
        this.footer.classList.add("panel-footer", "text-center");

            // BUTTONS
            this.btn = document.createElement("button");
            this.btn.classList.add("btn");
            this.secBtn = document.createElement("button");
            this.secBtn.classList.add("btn");

            //this.btn.style.marginTop = "10px";
            this.secBtn.style.marginLeft = "10px";
            //this.secBtn.style.marginTop = "10px";

            this.btn.addEventListener("click", clickEvent);
            this.secBtn.addEventListener("click", clickEvent);

            this.footer.appendChild(this.btn);
            this.footer.appendChild(this.secBtn);

        this.dom.appendChild(this.footer);
        this.domContainer.appendChild(this.dom);

        this.UpdateStyle();
    }
}