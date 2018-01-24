# UbiLearnCompanion (Project UEPL)

## ULC Desktop Client
Electron based application designed to control the configuration states on a machine (requires ULC Service).

## ULC Service

Service component which uses system features to manage local resources and applications, aswell as settings and additional services.

## ULC Service Command

Tool to send service requests and test some use cases.

## ULC WebAPI

Hosting application, written in C#. Could be hosted on any platform .Net Core is running on. There is a dockerfile to create a test container and run the actual API and Frontend. You may need to edit wwwroot/app/app.js and setup your own dns-entry or change the settings to connect to another machine.

## Website
For further informations, see our website: [UbiLearnCompanion](http://informatik-mannheim.github.io/UEPL/)