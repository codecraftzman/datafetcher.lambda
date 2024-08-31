using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class DataFetcher
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DataFetcher> _logger;

    public DataFetcher(ApiClient apiClient, ILogger<DataFetcher> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<string[]> FetchDataInParallel(List<string> endpoints, Dictionary<string, string> queryParams)
    {
        try
        {
            var tasks = endpoints.Select(endpoint => _apiClient.GetAsync(endpoint, queryParams)).ToList();
            return await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data in parallel");
            throw;
        }
    }

    public async Task<string[]> PostDataInParallel(List<string> endpoints, object body)
    {
        try
        {
            var tasks = endpoints.Select(endpoint => _apiClient.PostAsync(endpoint, body)).ToList();
            return await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while posting data in parallel");
            throw;
        }
    }

    public async Task<string[]> FetchSubsetDataInParallel(List<string> endpoints, Dictionary<string, string> queryParams, int subsetSize)
    {
        try
        {
            var tasks = new List<Task<string>>();
            for (int i = 0; i < endpoints.Count; i += subsetSize)
            {
                var subset = endpoints.Skip(i).Take(subsetSize);
                tasks.AddRange(subset.Select(endpoint => _apiClient.GetAsync(endpoint, queryParams)));
            }
            return await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching subset data in parallel");
            throw;
        }
    }
}
