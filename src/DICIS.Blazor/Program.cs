using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DICIS.Blazor;
using DICIS.Blazor.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => 
{
    var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7000") };
    // Configure timeout to prevent hanging on startup if API is not available
    httpClient.Timeout = TimeSpan.FromSeconds(30);
    return httpClient;
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApplicationService>();

await builder.Build().RunAsync();
