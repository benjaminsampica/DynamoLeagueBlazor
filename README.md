[![.NET](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml/badge.svg)](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml)

# Introduction

### April 8th, 2023 This repository is archived and no further fixes or features will be worked on. After five years, the leagues effort has outgrown how much fun it is, weighed against our continued evolving lives and other hobbies :).

Dynamo League is a fantasy football league based off the Yahoo fantasy football league with heavily customized rule sets.
It is comprised of two separate stand-alone applications - a client UI written using Blazor WebAssembly and an API server written using ASP.NET Core.

The League operates around teams and players. Teams, managed by users, are added and removed every season depending on participation. Players are actual NFL players and are imported by site managers.

A new season begins every May and continues until the end of December. At the end, points are tallied and rewards are distributed based on how that team's players performed in the real NFL.

# Getting Started

Ensure you've installed all necessary prerequisites before running the project.

## Prerequisites
- .NET 7 SDK (Installing Visual Studio 2022 and selecting .NET 7 in the modules will do this, or you can go to their [website](https://dotnet.microsoft.com/en-us/download/dotnet) and download the SDK via their instructions)
- IIS Express
- Docker (in Linux mode)
- WASM tools (can be installed via `dotnet workload install wasm-tools` in the command line)

## Running locally
1) Load the solution in Visual Studio or Rider.
2) Set the `DynamoLeagueBlazor.Server` as the start up project.
3) Run the application.
4) Wait for everything to setup (this can take a while on first run), once this is done it should open a new tab in your default browser.
5) Login with one of the two accounts:
   - Team - Space Force 
     - Username: `test@gmail.com` 
     - Password: `hunter2`
   - Team - The Donald
     - Username: `test2@gmail.com` 
     - Password: `hunter2`

# Contributing

Please make sure all tests pass before submitting a new pull request.

## Adding a Migration

New migrations can be added to the database by:

1. Installing the dotnet ef tools via `dotnet tool install --global dotnet-ef`
2. Running the following command with a command line while inside the `/src/Server` folder

 `dotnet ef migrations add {YourMigrationName} -o ./Infrastructure/Migrations --context ApplicationDbContext --project DynamoLeagueBlazor.Server.csproj`
