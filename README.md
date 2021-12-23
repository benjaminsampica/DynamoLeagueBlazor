[![.NET](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml/badge.svg)](https://github.com/benjaminsampica/DynamoLeagueBlazor/actions/workflows/dotnet.yml)

# Introduction

Dynamo League is a fantasy football league based off the Yahoo fantasy football league with heavily customized rulesets. 
It is comprised of two separate stand-alone applications - a client UI written with Blazor WebAssembly and an API server written with ASP.NET Core.

# Getting Started

Running & contributing to Dynamo League Blazor requires the following:

- .NET 6 SDK
- IIS Express
- SQL Server LocalDb to run tests

The latter two are installed automatically with either Visual Studio or Rider IDE's. 
If you're a brand new contributor an account will need to be created by modifying `SeedDataCommand` and adding a new user with a role using the identity manager.
