using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ADT_Proyect.Data;
using System;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Azure;
using System.Text.Json;
using ADT_Proyect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:7283")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Azure Digital Twins integration
var adtInstanceUrl = "https://AzureDT-01.api.eus.digitaltwins.azure.net";
var credential = new DefaultAzureCredential();
var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

await UploadModelsAsync(client);
await CreateTwinsAndRelationshipsAsync(client);
await ListRelationshipsAsync(client, "sampleTwin-0");

async Task UploadModelsAsync(DigitalTwinsClient client)
{
    Console.WriteLine("Upload a model");
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

    // Read a list of models back from the service
    AsyncPageable<DigitalTwinsModelData> modelDataList = client.GetModelsAsync();
    await foreach (DigitalTwinsModelData md in modelDataList)
    {
        Console.WriteLine($"Model: {md.Id}");
    }
}

async Task CreateTwinsAndRelationshipsAsync(DigitalTwinsClient client)
{
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

    async Task CreateRelationshipAsync(string srcId, string targetId)
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

    // Connect the twins with relationships
    await CreateRelationshipAsync("sampleTwin-0", "sampleTwin-1");
    await CreateRelationshipAsync("sampleTwin-0", "sampleTwin-2");
}

async Task ListRelationshipsAsync(DigitalTwinsClient client, string srcId)
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

// Run the application
app.Run();
