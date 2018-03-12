const ipc = require("electron").ipcRenderer;
const loadJson = require("load-json-file");
let config = {};

loadJson("config.json").then(json => 
{ 
    config = json;

    $.when($.ready).then(() => 
    {
        $.ajaxSetup({ contentType: "application/json; charset=utf-8"});

        $("#login").on("click", () => 
        {
            $("#login").prop("disabled", true);

            let name = $("#inputName").val();
            let pass = $("#inputPassword").val();

            $.post(config.serverUrl + "/api/account/login", 
                    JSON.stringify({ "name": name, "password": pass})
            ).done(data => 
            {
                localStorage.setItem("Token", data.token);
                localStorage.setItem("User", JSON.stringify(data.user));
                localStorage.setItem("Valid", data.valid);
                window.location = "index.html";
            }).fail(error => 
            {
                $.toast({ 
                    text: error.responseJSON,
                    heading: "Login Error",
                    icon: "error",
                    showHideTransition: "slide",
                    hideAfter: false,
                    stack: false,
                    position: "bottom-center"
                });

                $("#login").prop("disabled", false); // reenable login button
            });

        });
    });
});

