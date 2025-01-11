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

string prompt = @"This is a list of music available to the user:
    {{MusicLibraryPlugin.GetMusicLibrary}} 

    This is a list of music the user has recently played:
    {{MusicLibraryPlugin.GetRecentPlays}}

    Based on their recently played music, suggest a song from
    the list to play next";

var result = await kernel.InvokePromptAsync(prompt);
Console.WriteLine(result);