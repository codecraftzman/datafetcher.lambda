using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class EndpointProcessor
{
    private readonly DataFetcher _dataFetcher;
    //private readonly S3Uploader? _s3Uploader;
    private readonly ILogger<EndpointProcessor> _logger;

    public EndpointProcessor(DataFetcher dataFetcher, ILogger<EndpointProcessor> logger)
    {
        _dataFetcher = dataFetcher;
        _logger = logger;
        //_s3Uploader = s3Uploader;
    }

    //public EndpointProcessor(DataFetcher dataFetcher, ILogger<EndpointProcessor> logger, S3Uploader s3Uploader = null!)
    //{
    //    _dataFetcher = dataFetcher;
    //    _logger = logger;
    //    _s3Uploader = s3Uploader;
    //}

    public async Task ProcessEndpointsAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var endpoints = JsonSerializer.Deserialize<List<EndpointConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true  });

            if (endpoints == null)
            {
                _logger.LogError("No endpoints found in the configuration file.");
                return;
            }

            var getEndpoints = new List<string>();
            var postEndpoints = new List<string>();
            var getParams = new Dictionary<string, string>();
            var postBodies = new List<object>();

            foreach (var endpoint in endpoints)
            {
                var resolvedEndpoint = ResolveUrlParameters(endpoint.Endpoint, endpoint.UrlParameters);

                if (endpoint.Batch)
                {
                    if (endpoint.Type.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        getEndpoints.Add(resolvedEndpoint);
                        foreach (var param in endpoint.Parameters)
                        {
                            getParams[param.Key] = param.Value.ToString()!;
                        }
                    }
                    else if (endpoint.Type.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        postEndpoints.Add(resolvedEndpoint);
                        postBodies.Add(endpoint.Parameters);
                    }
                }
                else
                {
                    if (endpoint.Type.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        var result = await _dataFetcher.FetchDataInParallel(new List<string> { resolvedEndpoint }, endpoint.Parameters.ToDictionary(k => k.Key, v => v.Value.ToString()!));
                        Console.WriteLine(string.Join("\n", result));
                    }
                    else if (endpoint.Type.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        var result = await _dataFetcher.PostDataInParallel(new List<string> { resolvedEndpoint }, endpoint.Parameters);
                        Console.WriteLine(string.Join("\n", result));
                    }
                }
            }

            if (getEndpoints.Count > 0)
            {
                var getResults = await _dataFetcher.FetchDataInParallel(getEndpoints, getParams);
                Console.WriteLine(string.Join("\n", getResults));
            }

            if (postEndpoints.Count > 0)
            {
                var postResults = await _dataFetcher.PostDataInParallel(postEndpoints, postBodies);
                Console.WriteLine(string.Join("\n", postResults));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing endpoints");
        }
    }

    //public async Task ProcessEndpointsAsync(string filePath, string bucketName, string keyPrefix)
    //{
    //    try
    //    {
    //        var json = await File.ReadAllTextAsync(filePath);
    //        var endpoints = JsonSerializer.Deserialize<List<EndpointConfig>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //        if (endpoints == null)
    //        {
    //            _logger.LogError("No endpoints found in the configuration file.");
    //            return;
    //        }

    //        var getEndpoints = new List<string>();
    //        var postEndpoints = new List<string>();
    //        var getParams = new Dictionary<string, string>();
    //        var postBodies = new List<object>();

    //        foreach (var endpoint in endpoints)
    //        {
    //            var resolvedEndpoint = ResolveUrlParameters(endpoint.Endpoint, endpoint.UrlParameters);

    //            if (endpoint.Batch)
    //            {
    //                if (endpoint.Type.Equals("GET", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    getEndpoints.Add(resolvedEndpoint);
    //                    foreach (var param in endpoint.Parameters)
    //                    {
    //                        getParams[param.Key] = param.Value.ToString()!;
    //                    }
    //                }
    //                else if (endpoint.Type.Equals("POST", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    postEndpoints.Add(resolvedEndpoint);
    //                    postBodies.Add(endpoint.Parameters);
    //                }
    //            }
    //            else
    //            {
    //                if (endpoint.Type.Equals("GET", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    var result = await _dataFetcher.FetchDataInParallel(new List<string> { resolvedEndpoint }, endpoint.Parameters.ToDictionary(k => k.Key, v => v.Value.ToString()!));
    //                    await UploadToS3(bucketName, $"{keyPrefix}/{Guid.NewGuid()}.txt", result);
    //                }
    //                else if (endpoint.Type.Equals("POST", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    var result = await _dataFetcher.PostDataInParallel(new List<string> { resolvedEndpoint }, endpoint.Parameters);
    //                    await UploadToS3(bucketName, $"{keyPrefix}/{Guid.NewGuid()}.txt", result);
    //                }
    //            }
    //        }

    //        if (getEndpoints.Count > 0)
    //        {
    //            var getResults = await _dataFetcher.FetchDataInParallel(getEndpoints, getParams);
    //            await UploadToS3(bucketName, $"{keyPrefix}/getResults.txt", getResults);
    //        }

    //        if (postEndpoints.Count > 0)
    //        {
    //            var postResults = await _dataFetcher.PostDataInParallel(postEndpoints, postBodies);
    //            await UploadToS3(bucketName, $"{keyPrefix}/postResults.txt", postResults);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred while processing endpoints");
    //    }
    //}

    private string ResolveUrlParameters(string endpoint, Dictionary<string, string> urlParameters)
    {
        foreach (var param in urlParameters)
        {
            endpoint = endpoint.Replace($"{{{param.Key}}}", param.Value);
        }
        return endpoint;
    }

    //private async Task UploadToS3(string bucketName, string key, IEnumerable<string> data)
    //{
    //    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", data))))
    //    {
    //        await _s3Uploader!.UploadToS3Async(bucketName, key, stream);
    //    }
    //}
}
