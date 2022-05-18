[![.NET](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml/badge.svg)](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml)

# Introduction

Dynamo League is a fantasy football league based off the Yahoo fantasy football league with heavily customized rule sets.
It is comprised of two separate stand-alone applications - a client UI written using Blazor WebAssembly and an API server written using ASP.NET Core.

# Getting Started

Running & contributing to Dynamo League Blazor requires the following:

- .NET 6 SDK
- IIS Express
- SQL Server LocalDB

The latter two are installed automatically with either Visual Studio or Rider.

By default, a test account is created with administrator permissions with the following login information:

Username `test@gmail.com`

Password `hunter2`

# Contributing

Please make sure all tests pass before submitting a new pull request.

## Adding a Migration

New migrations can be added to the database by:

1. Installing the dotnet ef tools via `dotnet tool install --global dotnet-ef`
2. Running the following command with a command line while inside the `/src/Server` folder
 `dotnet ef migrations add {YourMigrationName} -o ./Infrastructure/Migrations --context ApplicationDbContext --project DynamoLeagueBlazor.Server.csproj