# Running the application locally
This document is to outline how to run this application locally.

## Prerequisites:
Things you will need before running the application:
- .NET 7 SDK (Installing Visual Studio 2022 and selecting .NET 7 in the modules will do this, or you can go to their [website](https://dotnet.microsoft.com/en-us/download/dotnet) and download the SDK via their instructions)
- IIS Express
- Docker (In Linux mode)
- WASM tools (can be installed via `dotnet workload install wasm-tools` in the command line)

## Steps to run:
1) Load the solution in Visual Studio or Rider
2) Set the `DynamoLeagueBlazor.Server` as the start up project
3) Run the application
4) Wait for everything to setup (this can take a while on first run), once this is done it should open a new tab in your default browser. (There is an issue open to maybe make this process more visible or faster)
5) Login with one of the two accounts:
   - Username: `test@gmail.com` Password: `hunter2` Team - Space Force
   - Username: `test2@gmail.com` Password: `hunter2` Team - The Donald