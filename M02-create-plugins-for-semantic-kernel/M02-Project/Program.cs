#pragma warning disable SKEXP0050 
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


// // Builtin Plugins
// builder.Plugins.AddFromType<ConversationSummaryPlugin>();

// var kernel = builder.Build();

// string input = @"I'm a vegan in search of new recipes. I love spicy food! 
// Can you give me a list of breakfast recipes that are vegan friendly?";

// var result = await kernel.InvokeAsync(
//     "ConversationSummaryPlugin", 
//     "GetConversationActionItems", 
//     new() {{ "input", input }});

// Console.WriteLine(result);


// // Own prompt, use personas in prompts
// var kernel = builder.Build();

// string language = "French";
// string history = @"I'm traveling with my kids and one of them 
//     has a peanut allergy.";
// string prompt = @$"
//     You are a travel assistant. You are helpful, creative, and very friendly. 
//     Consider the traveler's background:
//     ${history}

//     Create a list of helpful phrases and words in ${language} a traveler would find useful.

//     Group phrases by category. Include common direction words. 
//     Display the phrases in the following format: 
//     Hello - Ciao [chow]

//     Begin with: 'Here are some phrases in ${language} you may find helpful:' 
//     and end with: 'I hope this helps you on your trip!'";

// var result = await kernel.InvokePromptAsync(prompt);
// Console.WriteLine(result);


// // Define message roles
// var kernel = builder.Build();

// string input = @"I'm planning an anniversary trip with my spouse. We like hiking, mountains, 
//     and beaches. Our travel budget is $15000";
// string prompt = @$"
//     The following is a conversation with an AI travel assistant. 
//     The assistant is helpful, creative, and very friendly.

//     <message role=""user"">Can you give me some travel destination suggestions?</message>

//     <message role=""assistant"">Of course! Do you have a budget or any specific 
//     activities in mind?</message>

//     <message role=""user"">${input}</message>";

// var result = await kernel.InvokePromptAsync(prompt);
// Console.WriteLine(result);


// // Saving prompts to files
// var kernel = builder.Build();

// kernel.ImportPluginFromType<ConversationSummaryPlugin>();

// var prompts = kernel.ImportPluginFromPromptDirectory("Prompts/TravelPlugins");

// string input = @"I'm planning an anniversary trip with my spouse. We like hiking, 
//     mountains, and beaches. Our travel budget is $15000";

// var result = await kernel.InvokeAsync<string>(prompts["SuggestDestinations"],
//     new() {{ "input", input }});

// Console.WriteLine(result);


// Include history
var kernel = builder.Build();

kernel.ImportPluginFromType<ConversationSummaryPlugin>();

var prompts = kernel.ImportPluginFromPromptDirectory("Prompts/TravelPlugins");

ChatHistory history = [];

string input = @"I'm planning an anniversary trip with my spouse. We like hiking, 
    mountains, and beaches. Our travel budget is $15000";

var result = await kernel.InvokeAsync<string>(prompts["SuggestDestinations"],
    new() {{ "input", input }});

Console.WriteLine(result);
history.AddUserMessage(input);
history.AddAssistantMessage(result);

Console.WriteLine("Where would you like to go?");
input = Console.ReadLine();

result = await kernel.InvokeAsync<string>(prompts["SuggestActivities"],
    new() {
        { "history", history },
        { "destination", input },
    }
);
Console.WriteLine(result);