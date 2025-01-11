using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

// Build a config object and retrieve user settings.
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
string? yourEndpoint = config["AzureOAIEndpoint"];
string? yourApiKey = config["AzureOAIKey"];
string? yourDeploymentName = config["AzureOAIDeploymentName"];
string? yourModelName = config["AzureOAIModelName"];

// Check for missing or incorrect values.
if(string.IsNullOrEmpty(yourEndpoint) || string.IsNullOrEmpty(yourApiKey) || string.IsNullOrEmpty(yourDeploymentName) || string.IsNullOrEmpty(yourModelName))
{
    Console.WriteLine("Please check your appsettings.json file for missing or incorrect values.");
    return;
}

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    yourDeploymentName,
    yourEndpoint,
    yourApiKey,
    yourModelName);

var kernel = builder.Build();
kernel.ImportPluginFromType<MusicLibraryPlugin>();

var result = await kernel.InvokeAsync(
    "MusicLibraryPlugin", 
    "AddToRecentlyPlayed", 
    new() {
        ["artist"] = "Tiara", 
        ["song"] = "Danse", 
        ["genre"] = "French pop, electropop, pop"
    }
);

Console.WriteLine(result);