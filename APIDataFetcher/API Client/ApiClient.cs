using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, IOptions<ApiClientOptions> options, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = options.Value.ApiKey;
        _logger = logger;
    }

    public async Task<string> GetAsync(string schema, Dictionary<string, string> queryParams = null!)
    {
        try
        {
            var query = queryParams != null ? "&" + string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}")) : string.Empty;
            //var response = await _httpClient.GetAsync($"api/generate.json?key={_apiKey}&schema={schema}{query}");
            var response = await _httpClient.GetAsync($"{schema}?key={_apiKey}&{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while making GET request to {Schema}", schema);
            throw;
        }
    }

    public async Task<string> PostAsync(string schema, object body)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
            //var response = await _httpClient.PostAsync($"api/generate.json?key={_apiKey}&schema={schema}", content);
            var response = await _httpClient.PostAsync($"{schema}?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while making POST request to {Schema}", schema);
            throw;
        }
    }
}

public class ApiClientOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
