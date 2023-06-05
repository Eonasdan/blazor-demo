﻿namespace BlazorDemo.Shared.DownstreamApi;

public class DownstreamApiConfiguration
{
    public string? ClientId { get; set; }
    public string? BaseUrl { get; set; }
    
    public string[]? Scopes { get; set; }
}