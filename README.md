# LocalDiscogsApi

## Setup

[![Build status](https://github.com/mark-rafter/LocalDiscogsApi/workflows/Build%20and%20Test/badge.svg)](https://github.com/mark-rafter/LocalDiscogsApi/actions?query=workflow%3A%22Build+and+Test%22)

1. Clone repo
2. [Setup MongoDB](https://docs.mongodb.com/guides/cloud/connectionstring/)
3. Add MongoDB connection string to the project's secrets via the CLI
```
dotnet user-secrets set "DatabaseOptions:ConnectionString" "<your-connection-string-here>"
```
4. Run the API
```
dotnet run
```
