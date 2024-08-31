using System.Collections.Generic;

public class EndpointConfig
{
    public bool Batch { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public Dictionary<string, string> UrlParameters { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}
