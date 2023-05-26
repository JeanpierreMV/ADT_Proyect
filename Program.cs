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
await ListRelationshipsAsync(client, "SolarPanelNetwork");


async Task UploadModelsAsync(DigitalTwinsClient client)
{
    Console.WriteLine("Upload models");
    string solarPanelNetworkModel = File.ReadAllText("SolarPanelNetwork.json");
    string panelModel = File.ReadAllText("Panel.json");


    var models = new List<string> { solarPanelNetworkModel, panelModel };


    try
    {
        await client.CreateModelsAsync(models);
        Console.WriteLine("Models uploaded to the instance:");
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"Upload models error: {e.Status}: {e.Message}");
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
    // Create SolarPanelNetwork twin
    var solarPanelNetworkTwin = new BasicDigitalTwin();
    solarPanelNetworkTwin.Metadata.ModelId = "dtmi:com:example:SolarPanelNetwork;1";


    try
    {
        await client.CreateOrReplaceDigitalTwinAsync("SolarPanelNetwork", solarPanelNetworkTwin);
        Console.WriteLine("Created SolarPanelNetwork twin");
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"Create SolarPanelNetwork twin error: {e.Status}: {e.Message}");
    }


    // Create panel twins
    var panelTwin1 = new BasicDigitalTwin();
    panelTwin1.Metadata.ModelId = "dtmi:com:example:SolarPanel;1";


    var panelTwin2 = new BasicDigitalTwin();
    panelTwin2.Metadata.ModelId = "dtmi:com:example:SolarPanel;1";


    try
    {
        await client.CreateOrReplaceDigitalTwinAsync("Panel-1", panelTwin1);
        await client.CreateOrReplaceDigitalTwinAsync("Panel-2", panelTwin2);
        Console.WriteLine("Created panel twins");
    }
    catch (RequestFailedException e)
    {
        Console.WriteLine($"Create panel twins error: {e.Status}: {e.Message}");
    }


    // Create relationships
    async Task CreateRelationshipAsync(string srcId, string targetId)
    {
        var relationship = new BasicRelationship
        {
            TargetId = targetId,
            Name = "solarPanels"
        };


        try
        {
            await client.CreateOrReplaceRelationshipAsync(srcId, $"{srcId}-solarPanels->{targetId}", relationship);
            Console.WriteLine("Created relationship successfully");
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine($"Create relationship error: {e.Status}: {e.Message}");
        }
    }


    // Connect the twins with relationships
    await CreateRelationshipAsync("SolarPanelNetwork", "Panel-1");
    await CreateRelationshipAsync("SolarPanelNetwork", "Panel-2");
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