var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Blazor_Tools>("blazor-tools");

builder.AddProject<Projects.Blazor_Tools_BlazorBundler>("blazor-tools-blazorbundler");

builder.Build().Run();
