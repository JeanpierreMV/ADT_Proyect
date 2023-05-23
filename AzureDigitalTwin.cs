using System;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Azure;
using System.Text.Json;

namespace ADT_Proyect
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string adtInstanceUrl = "https://AzureDT-01.api.eus.digitaltwins.azure.net";

            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

            Console.WriteLine($"Service client created - ready to go");
            Console.WriteLine();
            Console.WriteLine($"Upload a model");
            string dtdl = File.ReadAllText("SampleModel.json");
            var models = new List<string> { dtdl };

            try
            {
                await client.CreateModelsAsync(models);
                Console.WriteLine("Models uploaded to the instance:");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Upload model error: {e.Status}: {e.Message}");
            }

            AsyncPageable<DigitalTwinsModelData> modelDataList = client.GetModelsAsync();
            await foreach (DigitalTwinsModelData md in modelDataList)
            {
                Console.WriteLine($"Model: {md.Id}");
            }

            var twinData = new BasicDigitalTwin();
            twinData.Metadata.ModelId = "dtmi:example:SampleModel;1";
            twinData.Contents.Add("data", $"Hello World!");

            string prefix = "sampleTwin-";
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    twinData.Id = $"{prefix}{i}";
                    await client.CreateOrReplaceDigitalTwinAsync<BasicDigitalTwin>(twinData.Id, twinData);
                    Console.WriteLine($"Created twin: {twinData.Id}");
                }
                catch (RequestFailedException e)
                {
                    Console.WriteLine($"Create twin error: {e.Status}: {e.Message}");
                }
            }

            await CreateRelationshipAsync(client, "sampleTwin-0", "sampleTwin-1");
            await CreateRelationshipAsync(client, "sampleTwin-0", "sampleTwin-2");

            await ListRelationshipsAsync(client, "sampleTwin-0");
        }

        async static Task CreateRelationshipAsync(DigitalTwinsClient client, string srcId, string targetId)
        {
            var relationship = new BasicRelationship
            {
                TargetId = targetId,
                Name = "contains"
            };

            try
            {
                string relId = $"{srcId}-contains->{targetId}";
                await client.CreateOrReplaceRelationshipAsync(srcId, relId, relationship);
                Console.WriteLine("Created relationship successfully");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Create relationship error: {e.Status}: {e.Message}");
            }
        }

        async static Task ListRelationshipsAsync(DigitalTwinsClient client, string srcId)
        {
            try
            {
                AsyncPageable<BasicRelationship> results = client.GetRelationshipsAsync<BasicRelationship>(srcId);
                Console.WriteLine($"Twin {srcId} is connected to:");
                await foreach (BasicRelationship rel in results)
                {
                    Console.WriteLine($" -{rel.Name}->{rel.TargetId}");
                }
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"Relationship retrieval error: {e.Status}: {e.Message}");
            }
        }
    }
}
