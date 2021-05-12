# CosmosDB Utility

A simple command-line utility that can be used to test certain Cosmos DB SQL API features.

In order to compile and run this application you will need [.NET 5+ SDK](https://dotnet.microsoft.com/download/dotnet/5.0) on whatever platform you are running (Windows, Mac or Linux).

If you want you can also open and edit this solution using Visual Studio Code which will allow you to debug it when running if you'd like to see how it functions. The solutions relies on the [Cosmos DB .NET v3 SDK](https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-sdk-dotnet-standard).

## Compile and run

In order to compile and run this application you will require:

- The .NET 5 SDK (as mentioned above).
- An active Cosmos DB account, configured with the SQL API with a Database named `MusicService` and a Container named `TrendingArtists`.
- An updated configuration file (appsettings.json).

## Configuration

Note that you don't need to change DatabaseName, CollectionName or SQLQuery.

If you wish to test out geo-reads and writes you can list a preferred location which will override the primary location for your Cosmos DB account. Note that your Cosmos DB database must be replicated to this Region before you attempt to to use it. You can determine the value to put here by viewing your Cosmos DB database in the Azure Portal and noting the Region name you wish to use. 

You can see available Regions also by using the 'status' command in the tool when it is running.

```json
{
  "AppSettings": {
    "GlobalDatabaseUri": "https://YOUR_COSMOS_ACCOUNT.documents.azure.com:443/",
    "GlobalDatabaseKey": "YOUR_ACCOUNT_KEY",
    "PreferredRegion": "East US",
    "DatabaseName": "MusicService",
    "CollectionName": "TrendingArtists",
    "SQLQuery": "SELECT TOP 1 c.id, c.activeUsersListening,c.artistListenCountTimestamp, c.artistName,c.currentDirection FROM c WHERE c.internalName = @internalName ORDER BY c.artistListenCountTimestamp DESC"
  }
}
```
## Contribution
The original code for this utility was created by Warner Chaves (warner@createdatapros.com)