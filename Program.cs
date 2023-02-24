using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace CosmosDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

            Console.WriteLine("Cosmos Demo");
            bool reset = true;
            bool stop = false;
            var cosmosDb = new CosmosClientWrapper(configuration);
            string sqlQueryText = configuration["AppSettings:SQLQuery"];
            Console.WriteLine("Enter country name:");
            string countryName = Console.ReadLine();

             while (reset && !stop)
            {

                Console.WriteLine("Enter amount of artists:");
                string serverLine = Console.ReadLine();
                int artistCount = Int32.Parse(serverLine);

                Dictionary<string, ArtistTrend> artistRegistry = new Dictionary<string, ArtistTrend>(artistCount);

                for (int i = 1; i <= artistCount; i++)
                {
                    string internalArtistName = "a" + i.ToString();
                    ArtistTrend currentArtist = new ArtistTrend(internalArtistName, countryName);
                    currentArtist.currentDirection = TrendDirection.Bouncing;
                    artistRegistry[internalArtistName] = currentArtist;
                }

                Console.WriteLine("Enter calls per second:");
                serverLine = Console.ReadLine();
                int delayMs = (int)(1000 / int.Parse(serverLine));
                Console.WriteLine("We will call every " + delayMs.ToString() + " milliseconds");
                Console.WriteLine("Enter how many iterations:");
                serverLine = Console.ReadLine();
                int iterations = int.Parse(serverLine);

                reset = false;
                while (!stop && !reset)
                {
                    try
                    {
                        Console.WriteLine(" ");
                        Console.WriteLine("Enter command:");
                        serverLine = Console.ReadLine();
                        if (serverLine == "stop")
                        {
                            stop = true;
                        }
                        else if (serverLine == "reset")
                        {
                            reset = true;
                        }
                        else if(serverLine == "status")
                        {                            
                            Console.WriteLine("Cosmos Service Endpoint: " + cosmosDb.client.Endpoint);                            
                            Console.WriteLine("Cosmos Application Region: " + (string.IsNullOrWhiteSpace(cosmosDb.client.ClientOptions.ApplicationRegion) ? "Not Set" : cosmosDb.client.ClientOptions.ApplicationRegion));
                            Console.WriteLine("Connection Mode: " + cosmosDb.client.ClientOptions.ConnectionMode.ToString());
                            Console.WriteLine("Read Regions:");
                            foreach(var region in cosmosDb.readRegions)
                            {
                                Console.WriteLine($" >> {region.Name}");
                            }
                            Console.WriteLine("Write Regions:");
                            foreach(var region in cosmosDb.writeRegions)
                            {
                                Console.WriteLine($" >> {region.Name}");
                            }
                            Console.WriteLine("Database Name: " + cosmosDb.database);
                            Console.WriteLine("Container Name: " + cosmosDb.collection);
                            Console.WriteLine("Request Units: " + cosmosDb.GetRequestUnits().ToString());
                            Console.WriteLine("Indexing Policy: " + cosmosDb.GetIndexingPolicy());
                            Console.WriteLine("Consistency Level: " + (cosmosDb.client.ClientOptions.ConsistencyLevel == null ? "Not Set" : cosmosDb.client.ClientOptions.ConsistencyLevel.ToString()));
                        }
                        else if (serverLine == "query")
                        {
                            Console.WriteLine("Loaded Query: ");
                            Console.WriteLine(sqlQueryText);
                        }
                        else if (serverLine == "georead")
                        {
                            if(string.IsNullOrEmpty(cosmosDb.preferredRegion))
                            {
                                Console.WriteLine("To use Geo-Read you must define a value for 'PreferredRegion' in the appsettings.json file. Cosmos must be replicated to this Region.");
                                Console.WriteLine("Value should be the string value of Microsoft.Azure.Cosmos.Regions - see: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.regions?view=azure-dotnet");
                            }
                            else
                            {
                                cosmosDb.ChangeGeoReads(true);
                                Console.WriteLine("Enabled Geo-Replicated Read with Preferred Region: {0}",cosmosDb.preferredRegion);
                            }                              
                        }
                        else if (serverLine == "geowrite")
                        {
                            if(string.IsNullOrEmpty(cosmosDb.preferredRegion))
                            {
                                Console.WriteLine("To use Geo-Wite you must define a value for 'PreferredRegion' in the appsettings.json file. Cosmos must be replicated to this Region.");
                                Console.WriteLine("Value should be the string value of Microsoft.Azure.Cosmos.Regions - see: https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.regions?view=azure-dotnet");
                            }
                            else
                            {
                                cosmosDb.ChangeGeoWrite(true);
                                Console.WriteLine("Enabled Geo-Replicated Writes with Preferred Region: {0}",cosmosDb.preferredRegion);
                            }
                        }
                        else if (serverLine == "nogeoread")
                        {
                            cosmosDb.ChangeGeoReads(false);
                            Console.WriteLine("Disabled Geo-Replicated Reads");
                        }
                        else if (serverLine == "directmode")
                        {
                            cosmosDb.ChangeConnectionMode(true);
                            Console.WriteLine("Enabled direct mode connection");
                        }
                        else if (serverLine == "gatewaymode")
                        {
                            cosmosDb.ChangeConnectionMode(false);
                            Console.WriteLine("Enabled gateway mode connection");
                        }
                        else if (serverLine == "flipindexing")
                        {
                            cosmosDb.SwitchIndexingPolicy();
                            long smallWaitTimeMilliseconds = 1000;
                            long progress = 0;

                            while (progress < 100)
                            {
                                progress = cosmosDb.GetIndexingProgressPercentage();
                                Console.WriteLine("Indexing policy flip complete percentage: " + progress.ToString() + "%");
                                Task.Delay(TimeSpan.FromMilliseconds(smallWaitTimeMilliseconds)).Wait();
                            }
                            Console.WriteLine("Indexing policy flipped.");
                        }
                        else if (serverLine == "partitions")
                        {
                            QueryDefinition sqlquery = new QueryDefinition(sqlQueryText)
                            .WithParameter("@internalName", artistRegistry.Keys.ElementAt(0));
                            cosmosDb.AnalyzePartitionInfo(sqlquery).Wait();
                        }
                        else if (serverLine == "metrics" || serverLine == "execquery")
                        {
                            DateTime dtstart = DateTime.Now;

                            if (serverLine == "metrics")
                            {
                                if (cosmosDb.lastQueryMetrics != null)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Displaying metrics for last executed query.");
                                    Console.WriteLine("===========================================");
                                    Console.Write(cosmosDb.lastQueryMetrics);
                                }
                                else
                                {
                                    Console.WriteLine("You need to run 'execquery' first to populate metrics.");
                                }
                            }
                            else if (serverLine == "execquery")
                            {
                                QueryDefinition sqlquery = new QueryDefinition(sqlQueryText)
                                .WithParameter("@internalName", artistRegistry.Keys.ElementAt(0));
                                Console.WriteLine(cosmosDb.ExecuteQuery(sqlquery));
                                Console.WriteLine("");
                            }
                            DateTime dtstop = DateTime.Now;
                            double seconds = dtstop.Subtract(dtstart).TotalMilliseconds;
                            Console.WriteLine("Total Client Time: " + seconds + " milliseconds");
                        }
                        else if (serverLine == "trend increase")
                        {
                            foreach (ArtistTrend currentArtist in artistRegistry.Values)
                            {
                                currentArtist.currentDirection = TrendDirection.Increasing;
                            }
                        }
                        else if (serverLine == "trend decrease")
                        {
                            foreach (ArtistTrend currentArtist in artistRegistry.Values)
                            {
                                currentArtist.currentDirection = TrendDirection.Decreasing;
                            }
                        }
                        else if (serverLine == "trend bouncing")
                        {
                            foreach (ArtistTrend currentArtist in artistRegistry.Values)
                            {
                                currentArtist.currentDirection = TrendDirection.Bouncing;
                            }
                        }
                        else if (serverLine == "trend stable")
                        {
                            foreach (ArtistTrend currentArtist in artistRegistry.Values)
                            {
                                currentArtist.currentDirection = TrendDirection.Stable;
                            }
                        }
                        else if ((serverLine == "start write") || (serverLine == "start write conflict") || (serverLine == "start read"))
                        {
                            DateTime dtstart = DateTime.Now;
                            cosmosDb.ReinitializeCounters();
                            Parallel.ForEach(artistRegistry.Values, new ParallelOptions(){

                                MaxDegreeOfParallelism = 4
                            }, (currentArtist) =>
                            {
                                for (int i = 0; i < iterations; i++)
                                {
                                    double rusIteration;
                                    DateTime dt1 = DateTime.Now;
                                    if (serverLine == "start write")
                                    {
                                        rusIteration = cosmosDb.ObjectOneSecondWriteWithDelay(currentArtist, delayMs).Result;
                                    }
                                    else if (serverLine == "start write conflict")
                                    {
                                        rusIteration = cosmosDb.ObjectOneSecondWriteWithConflict(currentArtist).Result;
                                    }
                                    else if (serverLine == "start read")
                                    {
                                        QueryDefinition sqlquery = new QueryDefinition(sqlQueryText)
                                        .WithParameter("@artistName", currentArtist.artistName);
                                        rusIteration = cosmosDb.ObjectOneSecondReadWithDelay(sqlquery, delayMs).Result;
                                    }
                                    else
                                    {
                                        throw new Exception("Command does not parse");
                                    }

                                    DateTime dt2 = DateTime.Now;
                                    Console.WriteLine("Iteration " + i.ToString() + " for artist " + currentArtist.artistName + " completed in " + dt2.Subtract(dt1).TotalSeconds.ToString() + " and consumed " + rusIteration.ToString() + " request units.");
                                }
                            });
                            DateTime dtstop = DateTime.Now;
                            double seconds = dtstop.Subtract(dtstart).TotalSeconds;
                            Console.WriteLine("Successful calls: " + cosmosDb.SuccesfulCallsCounter.ToString());
                            Console.WriteLine("Average Throughput: " + (cosmosDb.SuccesfulCallsCounter / seconds).ToString() + " calls/sec");
                            Console.WriteLine("Calls with Exceptions: " + cosmosDb.ExceptionCounter.ToString());
                        }
                        else
                        {
                            string[] split = serverLine.Split(new Char[] { ' ' });


                            if (split[0] == "resize")
                            {
                                int RUs = int.Parse(split[1]);
                                cosmosDb.ResizeRequestUnits(RUs).Wait();
                                Console.WriteLine("Request units resized to: " + RUs.ToString() + " request units");
                            }
                            else if (split[0] == "consistency")
                            {
                                cosmosDb.ChangeConsistencyModel(split[1]);
                                Console.WriteLine("Consistency model changed to: " + split[1]);
                            }
                            else
                            {
                                throw new Exception("Command does not parse");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Issue with command: " + e.Message);
                    }
                }
            }
        }

        private static string BuildPreferredRegionList(IReadOnlyList<string> regionList)
        {
            if (regionList == null || regionList.Count == 0)
            {
                return "Not Set";
            }

            System.Text.StringBuilder result = new();
            var tracker = 0;
            foreach (var entry in regionList)
            {
                tracker++;
                result.Append(entry);
                if (tracker < regionList.Count)
                {
                    result.Append("; ");
                }
            }
            return result.ToString();
        }

    }

    class CosmosClientWrapper
    {
        private string endpointUrl;
        private string accountKey;

        public IEnumerable<AccountRegion> readRegions;
        public IEnumerable<AccountRegion> writeRegions;
        public string database;
        public string collection;
        public string preferredRegion;
        private Container cosmosContainer;
        public string lastQueryMetrics;
        private bool geoRead = false;
        private bool geoWrite = false;
        private bool directMode = false;
        public CosmosClient client;

        string consistencyModel = "bounded";
        public bool stop = false;
        public int ExceptionCounter = 0;
        public int SuccesfulCallsCounter = 0;

        public CosmosClientWrapper(IConfigurationRoot configuration)
        {
            endpointUrl = configuration["AppSettings:GlobalDatabaseUri"];
            accountKey = configuration["AppSettings:GlobalDatabaseKey"];
            database = configuration["AppSettings:DatabaseName"];
            collection = configuration["AppSettings:CollectionName"];
            preferredRegion = configuration["AppSettings:PreferredRegion"];

            this.initializeClient();
            /*uncomment if you need to create the document collection with the conflict feed enabled (portal doesn't have the option)
            client.CreateDocumentCollectionIfNotExistsAsync(
              UriFactory.CreateDatabaseUri(database), new DocumentCollection
              {
                  Id = collection,
                  ConflictResolutionPolicy = new ConflictResolutionPolicy
                  {
                      Mode = ConflictResolutionMode.Custom,
                  },
              }).Wait();*/
        }

        public async Task AnalyzePartitionInfo(QueryDefinition sqlquery)
        {
            ContainerResponse response = await cosmosContainer.ReadContainerAsync(new ContainerRequestOptions { PopulateQuotaInfo = true });
            string partitionIndex = string.Empty;

            if (response.Headers["x-ms-documentdb-partitionkeyrangeid"] != null)
            {
                // lookup range;
                partitionIndex = response.Headers["collection-partition-index"];
            }
            else
            {
                Console.WriteLine("No Partition Key Range found");
            }

            var partitionKeyRanges = await GetPartitionKeyRanges();

            PrintSummaryStats(response, partitionKeyRanges);

            if (partitionIndex != string.Empty)
            {
                await PrintPerPartitionStats(response, partitionIndex, sqlquery);
            }
        }

        private async Task<IReadOnlyList<FeedRange>> GetPartitionKeyRanges()
        {
            return await cosmosContainer.GetFeedRangesAsync();
        }

        private static void PrintSummaryStats(ContainerResponse collection, IReadOnlyList<FeedRange> partitionKeyRanges)
        {
            Console.WriteLine("Summary: ");
            Console.WriteLine("\tpartitions: {0}", partitionKeyRanges.Count);
            Console.WriteLine();
        }

        private async Task PrintPerPartitionStats(ContainerResponse response, string partitionKey, QueryDefinition sqlquery)
        {
            Console.WriteLine("Per partition stats: ");
            await PrintPartitionStatsByPartitionKeyRange(response, partitionKey, sqlquery);
        }

        private async Task PrintPartitionStatsByPartitionKeyRange(ContainerResponse response, string partitionKey, QueryDefinition sqlquery)
        {
            FeedResponse<ArtistTrend> perPartitionResponse = await GetPartitionUsageStats(partitionKey, sqlquery);
            if (perPartitionResponse == null)
            {
                Console.WriteLine("\tPartition.{0} documentsSize: 0 GB", partitionKey);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Displaying metrics for last executed query on Partition {0}.", partitionKey);
            Console.WriteLine("=================================================================");
            Console.Write(lastQueryMetrics);
        }

        private async Task<FeedResponse<ArtistTrend>> GetPartitionUsageStats(string partitionKey, QueryDefinition sqlquery)
        {
            string continuationToken = null;
            FeedIterator<ArtistTrend> feedIterator = cosmosContainer.GetItemQueryIterator<ArtistTrend>(
                           sqlquery,
                           continuationToken: continuationToken,
                           new QueryRequestOptions() { MaxItemCount = 1, PartitionKey = new PartitionKey(partitionKey) });

            FeedResponse<ArtistTrend> feedResponse = await feedIterator.ReadNextAsync();
            PopulateQueryMetrics(feedResponse.Diagnostics);

            return feedResponse;
        }

        private string GetPartitionKeyPropertyName(Container collection)
        {
            ContainerResponse response = cosmosContainer.ReadContainerAsync(new ContainerRequestOptions { PopulateQuotaInfo = true }).Result;

            return response.Resource.PartitionKeyPath.Replace("/", "");
        }

        public void ReinitializeCounters()
        {
            SuccesfulCallsCounter = 0;
            ExceptionCounter = 0;
        }

        private void initializeClient()
        {
            var connPolicy = new CosmosClientOptions();
            /*****
             * Read the docs to understand:
             * - Preferred and Primary Regions: https://docs.microsoft.com/en-us/azure/cosmos-db/troubleshoot-sdk-availability
             * - Multi-master how-to: https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-multi-master?tabs=api-async#netv3
             *****/
            if (geoRead || geoWrite)
            {
                connPolicy.ApplicationRegion = preferredRegion;
            }
            // SDK v3 default is direct mode.
            if (!directMode)
            {
                connPolicy.ConnectionMode = ConnectionMode.Gateway;
            }

            connPolicy.MaxRetryAttemptsOnRateLimitedRequests = 0;

            switch (consistencyModel)
            {
                case "strong":
                    connPolicy.ConsistencyLevel = ConsistencyLevel.Strong;
                    break;
                case "bounded":
                    connPolicy.ConsistencyLevel = ConsistencyLevel.BoundedStaleness;
                    break;
                case "session":
                    connPolicy.ConsistencyLevel = ConsistencyLevel.Session;
                    break;
                case "consistentprefix":
                    connPolicy.ConsistencyLevel = ConsistencyLevel.ConsistentPrefix;
                    break;
                case "eventual":
                    connPolicy.ConsistencyLevel = ConsistencyLevel.Eventual;
                    break;
            }
            client = new CosmosClient(endpointUrl, accountKey, connPolicy);
            this.consistencyModel = client.ClientOptions.ConsistencyLevel.ToString();
            cosmosContainer = client.GetContainer(database, collection);
            PopulateRegionInformation();
        }

        private async Task PopulateRegionInformation()
        {
            var accountPropeties = await client.ReadAccountAsync();
            this.readRegions = accountPropeties.ReadableRegions;
            this.writeRegions = accountPropeties.WritableRegions;
        }

        public void ChangeConsistencyModel(string consistencyModel)
        {
            this.consistencyModel = consistencyModel;
            initializeClient();
        }

        public void ChangeGeoReads(bool geoRead)
        {
            // Not sure this works as expected
            this.geoRead = geoRead;
            initializeClient();
        }

        public void ChangeGeoWrite(bool geoWrite)
        {
            // Not sure this works as expected
            this.geoWrite = geoWrite;
            initializeClient();
        }

        public void ChangeConnectionMode(bool directMode)
        {
            this.directMode = directMode;
            initializeClient();
        }

        public string GetRequestUnits()
        {
            int? rus;
            string requestUnits;
            rus = cosmosContainer.ReadThroughputAsync().Result.GetValueOrDefault();
            if (rus != 0)
                requestUnits = rus.ToString() + " at the container level";
            else
            {
                rus = client.GetDatabase(database).ReadThroughputAsync().Result.GetValueOrDefault();
                requestUnits = rus.ToString() + " shared at the database level";
            }
            return requestUnits;

        }

        public string GetIndexingPolicy()
        {
            ContainerResponse response = cosmosContainer.ReadContainerAsync().Result;
            return (response.Resource.IndexingPolicy.Automatic == true) ? "Automatic" : "Manual";
        }

        public async void SwitchIndexingPolicy()
        {
            ContainerResponse response = await cosmosContainer.ReadContainerAsync();

            if (response.Resource.IndexingPolicy.IndexingMode == IndexingMode.Consistent)
            {
                response.Resource.IndexingPolicy.IndexingMode = IndexingMode.None;
                response.Resource.IndexingPolicy.IncludedPaths.Clear();
                response.Resource.IndexingPolicy.ExcludedPaths.Clear();
            }
            else if (response.Resource.IndexingPolicy.IndexingMode == IndexingMode.None)
            {
                response.Resource.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
            }
            // need to disable automatic indexing to change mode.
            response.Resource.IndexingPolicy.Automatic = false;
            await cosmosContainer.ReplaceContainerAsync(response.Resource);
        }

        public long GetIndexingProgressPercentage()
        {
            ContainerResponse response = cosmosContainer.ReadContainerAsync().Result;
            if (response.Headers["x-ms-documentdb-collection-index-transformation-progress"] != null)
            {
                return long.Parse(response.Headers["x-ms-documentdb-collection-index-transformation-progress"]);
            }
            return 100;
        }

        public async Task ResizeRequestUnits(int Rus)
        {
            await cosmosContainer.ReplaceThroughputAsync(Rus);
        }

        private void PopulateQueryMetrics(CosmosDiagnostics diagnostics)
        {
            lastQueryMetrics = string.Empty;

            var parsedDiags = JObject.Parse(diagnostics.ToString());

            var childNodes = parsedDiags.Children();

            JToken metricToken = parsedDiags.Descendants()
                            .Where(t => t.Type == JTokenType.Property && ((JProperty)t).Name == "Query Metrics")
                            .Select(p => ((JProperty)p).Value)
                            .FirstOrDefault();

            if (metricToken != null)
            {
                lastQueryMetrics = metricToken.Value<string>();
            }
        }


        public async Task<double> WriteConflictDocumentFromObject(Object obj)
        {
            var m = (ArtistTrend)obj;
            ItemResponse<JObject> itemResponse = await cosmosContainer.CreateItemAsync(JObject.Parse(m.ReadAsJSONConflict()));
            PopulateQueryMetrics(itemResponse.Diagnostics);
            return itemResponse.RequestCharge;
        }

        public async Task ObjectCollectionInfiniteWriteWithDelay(IEnumerable<Object> collection, int delayMs)
        {
            while (!stop)
            {
                await WriteDocumentFromObjectCollection(collection);
                await Task.Delay(delayMs);
            }
        }

        public async Task<double> ObjectOneSecondReadWithDelay(QueryDefinition sqlquery, int delayMs)
        {
            int looptime = delayMs;
            double rusConsumed = 0;
            for (int i = 0; i < 1000; i = i + looptime)
            {
                try
                {
                    DateTime dt1 = DateTime.Now;
                    rusConsumed = rusConsumed + await ReadDocumentFromQuery(sqlquery);
                    DateTime dt2 = DateTime.Now;
                    int callTime = dt2.Subtract(dt1).Milliseconds;
                    //Console.WriteLine("Call time: " + callTime.ToString());
                    if (callTime < delayMs)
                    {
                        await Task.Delay(delayMs - callTime);
                        looptime = delayMs;
                    }
                    else
                    {
                        looptime = callTime;
                    }

                    this.SuccesfulCallsCounter++;
                }
                catch (Exception)
                {
                    this.ExceptionCounter++;
                }
            }
            return rusConsumed;
        }

        public async Task<double> ObjectOneSecondWriteWithConflict(Object o)
        {
            double rusConsumed = 0;
            try
            {
                DateTime dt1 = DateTime.Now;
                rusConsumed = rusConsumed + await WriteConflictDocumentFromObject(o);
                DateTime dt2 = DateTime.Now;
                int callTime = dt2.Subtract(dt1).Milliseconds;
                this.SuccesfulCallsCounter++;
            }
            catch (Exception)
            {
                this.ExceptionCounter++;
            }
            return rusConsumed;
        }

        public async Task<double> ObjectOneSecondWriteWithDelay(Object o, int delayMs)
        {
            int looptime = delayMs;
            double rusConsumed = 0;
            for (int i = 0; i < 1000; i = i + looptime)
            {
                try
                {
                    DateTime dt1 = DateTime.Now;
                    rusConsumed = rusConsumed + await WriteDocumentFromObject(o);
                    DateTime dt2 = DateTime.Now;
                    int callTime = dt2.Subtract(dt1).Milliseconds;
                    //Console.WriteLine("Call time: " + callTime.ToString());
                    if (callTime < delayMs)
                    {
                        await Task.Delay(delayMs - callTime);
                        looptime = delayMs;
                    }
                    else
                    {
                        looptime = callTime;
                    }

                    this.SuccesfulCallsCounter++;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    this.ExceptionCounter++;
                }
            }
            return rusConsumed;
        }

        public async Task<double> WriteDocumentFromObject(Object obj)
        {
            ItemResponse<JObject> itemResponse = await cosmosContainer.CreateItemAsync(JObject.Parse(obj.ToString()));
            return itemResponse.RequestCharge;
        }

        public async Task<double> ReadDocumentFromQuery(QueryDefinition sqlquery)
        {
            double ruCounter = 0;

            string continuationToken = null;
            do
            {
                FeedIterator<ArtistTrend> feedIterator = cosmosContainer.GetItemQueryIterator<ArtistTrend>(
                            sqlquery,
                            continuationToken: continuationToken);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<ArtistTrend> feedResponse = await feedIterator.ReadNextAsync();
                    PopulateQueryMetrics(feedResponse.Diagnostics);
                    continuationToken = feedResponse.ContinuationToken;
                    ruCounter += feedResponse.RequestCharge;
                }
            } while (continuationToken != null);

            return ruCounter;
        }

        public string ExecuteQuery(QueryDefinition sqlquery)
        {
            // Code assumes there is data in the Cosmos DB.

            FeedIterator<ArtistTrend> feedIterator = cosmosContainer.GetItemQueryIterator<ArtistTrend>(
                        sqlquery,
                        continuationToken: null);

            FeedResponse<ArtistTrend> feedResponse = feedIterator.ReadNextAsync().Result;
            PopulateQueryMetrics(feedResponse.Diagnostics);

            return feedResponse.First<dynamic>().ToString();
        }

        public async Task WriteDocumentFromObjectCollection(IEnumerable<Object> collection)
        {
            var writeDocumentTasks = new List<Task>();

            foreach (var item in collection)
            {
                writeDocumentTasks.Add(WriteDocumentFromObject(item));
            }

            await Task.WhenAll(writeDocumentTasks);
        }
    }
}
