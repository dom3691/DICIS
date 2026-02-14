using DICIS.Core.DTOs;
using System.Net.Http.Json;

namespace DICIS.Blazor.Services;

public class LocationService
{
    private readonly HttpClient _httpClient;

    public LocationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<string>> GetCountriesAsync()
    {
        var response = await _httpClient.GetAsync("api/location/countries");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LocationResponse>();
        return result?.Countries ?? new List<string>();
    }

    public async Task<List<string>> GetStatesAsync(string country)
    {
        var response = await _httpClient.GetAsync($"api/location/states?country={Uri.EscapeDataString(country)}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LocationResponse>();
        return result?.States ?? new List<string>();
    }

    public async Task<List<string>> GetLocalGovernmentsAsync(string country, string state)
    {
        var response = await _httpClient.GetAsync($"api/location/local-governments?country={Uri.EscapeDataString(country)}&state={Uri.EscapeDataString(state)}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LocationResponse>();
        return result?.LocalGovernments ?? new List<string>();
    }
}
