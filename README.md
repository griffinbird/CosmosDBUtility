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
## Demo

### RU's effect on workloads 
This is a simulation of a music service, and we are keeping track of how many people are listening to a particular artist.

You can enter `Status` to show the current config.

When you start the application. Fill out the country name (only has bearing internally for data creation purposes), number of artists, how many calls per second and how many interations. Good default is `1,5,5`. Also fire up a 2nd instance of this application in another terminal window which is connecting to a container named TrendingArtist2. ie 1 app will connect to TrendingArtist and the other TrendingArtist2.

To test this out, let simulate a write. `start write` in both terminals.

This was a pretty small workload. Let's run something more beefy, type `reset` and use the following config `20, 30, 10`. `start write`. You shouldn't see any exeception. Now use the same configuration `20,30,10` in for TrendingArtist2. Whilst that is running, do `start write` on TrendingArtist. 

This will simulate both containers being spiked at the sametime. We shouldn't see any execptions, but we will notice that the number of successful calls will be lower when comparted to TrendingArtist2 and the average throughput will be different. Basically our througput will be lower when running them at the sametime. This is because they are both using the same resources ie. 400 RU shared at the database level.

Now lets bump up TrendingArtist to 30,30,10 and the same for TrendingArtist2 using the `reset` command, followed by `start write` on both containers. You should notice that both containers are processing data visible slower. Once this is complete, you will notice that we have execeptions as the cosmos db has run out of resources to process these request. (Note: In the code we have turned off automatic retries)

Now go into the Azure Portal and set the database throughput to Autoscale, and `start write` on both containers. You will noticed that both containers calls and throughput are similar and the execeptions have dissappeared.

### Consistency effect on workloads
`reset` and config with `1,1,5` on TrendingArtist2 `start read`
Then enter `status`. The consistency model is set to `bounded staleness`
I can change the consistency to session for example, `consistency session`
Rerun the workload `start read`
Now examine both runs. You will see that bounded staleness cost more RU (2x in fact)











## Contribution
The original code for this utility was created by Warner Chaves (warner@createdatapros.com)