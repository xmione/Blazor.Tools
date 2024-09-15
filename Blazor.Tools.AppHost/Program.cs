using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Blazor_Tools>("blazor-tools");

// Register the 'webfrontend' resource
builder.Services.AddHttpClient("webfrontend", client =>
{
    client.BaseAddress = new Uri("https://localhost:7031/");  // Or your actual frontend URL
});
builder.Build().Run();
