// See https://aka.ms/new-console-template for more information
using APIDataFetcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;


// Set up dependency injection and logging
var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddHttpClient<ApiClient>((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>();
                    client.BaseAddress = new Uri("https://my.api.mockaroo.com/"); //Replace with your actual Base address
                })
                .Services
                .Configure<ApiClientOptions>(options =>
                {
                    options.ApiKey = "7e066420"; // Replace with your actual API key
                })
                .AddSingleton<DataFetcher>()
                .AddSingleton<EndpointProcessor>()
                .AddSingleton<S3Uploader>()
                .AddSingleton<LambdaHandler>()
                .BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<Program>>();
logger?.LogInformation("Starting AWS Lambda data fetcher application");


// Dealing with endpoints configuration file
#region snippet_EndpointProcessor
try
{
    var endpointProcessor = serviceProvider.GetService<EndpointProcessor>();
    await endpointProcessor!.ProcessEndpointsAsync("endpoints.json");
}
catch (Exception ex)
{
    logger?.LogError(ex, "An error occurred while processing the request");
}

#endregion

// Dealing with S3 events

#region snippet_LambdaHandler
/*
try
{
    var lambdaHandler = serviceProvider.GetService<LambdaHandler>();

    // Simulate Lambda event
    var s3Event = new Amazon.Lambda.S3Events.S3Event();
    var context = new TestLambdaContext();
    await lambdaHandler!.HandleRequest(s3Event, context, "endpoints.json");
}
catch (Exception ex)
{
    logger?.LogError(ex, "An error occurred while processing the request");
}
*/
#endregion

logger?.LogInformation("Application finished");

///Without DI

/*
var apiClient = new ApiClient("https://my.api.mockaroo.com/");
var dataFetcher = new DataFetcher(apiClient);
var s3Client = new Amazon.S3.AmazonS3Client();
var s3Uploader = new S3Uploader(s3Client);
var lambdaHandler = new LambdaHandler(dataFetcher, s3Uploader);

// Simulate Lambda event
var s3Event = new Amazon.Lambda.S3Events.S3Event();
var context = new TestLambdaContext();
await lambdaHandler.HandleRequest(s3Event, context);
*/

