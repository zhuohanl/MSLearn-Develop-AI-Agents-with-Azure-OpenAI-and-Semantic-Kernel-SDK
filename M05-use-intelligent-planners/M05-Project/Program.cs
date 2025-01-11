using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace M05Project
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Telemetry setup code goes here
            // Replace the connection string with your Application Insights connection string
            var connectionString = "<your-connection-string>";

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService("TelemetryApplicationInsightsQuickstart");

            // Enable model diagnostics with sensitive data.
            AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

            using var traceProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource("Microsoft.SemanticKernel*")
                .AddAzureMonitorTraceExporter(options => options.ConnectionString = connectionString)
                .Build();

            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddMeter("Microsoft.SemanticKernel*")
                .AddAzureMonitorMetricExporter(options => options.ConnectionString = connectionString)
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                // Add OpenTelemetry as a logging provider
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddAzureMonitorLogExporter(options => options.ConnectionString = connectionString);
                    // Format log messages. This is default to false.
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                });
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            // Logic code goes here
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
            builder.Services.AddSingleton(loggerFactory);
            builder.AddAzureOpenAIChatCompletion(
                yourDeploymentName,
                yourEndpoint,
                yourApiKey,
                yourModelName);

            var kernel = builder.Build();

            kernel.ImportPluginFromType<MusicLibraryPlugin>();
            kernel.ImportPluginFromType<MusicConcertPlugin>();
            kernel.ImportPluginFromPromptDirectory("Prompts");

            OpenAIPromptExecutionSettings settings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // // Example 1: Automatically invoke function (final step is to call a promt)
            // string prompt = @"I live in Portland OR USA. Based on my recently 
            //     played songs and a list of upcoming concerts, which concert 
            //     do you recommend?";

            // var result = await kernel.InvokePromptAsync(prompt, new(settings));

            // Console.WriteLine(result);


            // // Example 2: Create a function from a prompt
            // // Create a function from a prompt
            // var songSuggesterFunction = kernel.CreateFunctionFromPrompt(
            //     promptTemplate: @"Based on the user's recently played music:
            //         {{$recentlyPlayedSongs}}
            //         recommend a song to the user from the music library:
            //         {{$musicLibrary}}",
            //     functionName: "SuggestSong",
            //     description: "Recommend a song from the library"
            // );

            // kernel.Plugins.AddFromFunctions("SuggestSong", [songSuggesterFunction]);

            // // Automatically invoke function
            // string prompt = @"Can you recommend a song from the music library?";

            // var result = await kernel.InvokePromptAsync(prompt, new(settings));

            // Console.WriteLine(result);


            // Example 3: Automatically invoke function (final step is to call a function)
            string prompt = @"Add this song to the recently played songs list:  title: 'Touch', artist: 'Cat's Eye', genre: 'Pop'";

            var result = await kernel.InvokePromptAsync(prompt, new(settings));

            Console.WriteLine(result);

        }
    }
}
