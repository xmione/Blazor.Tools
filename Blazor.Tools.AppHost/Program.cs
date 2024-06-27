var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Blazor_Tools>("blazor-tools");

builder.Build().Run();
