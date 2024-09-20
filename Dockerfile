# Use the .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
LABEL maintainer="solomiosisante@gmail.com"
ARG BUILD_CONFIGURATION=Release
ARG NUGET_API_KEY

WORKDIR /src
COPY ["Blazor.Tools.BlazorBundler/Blazor.Tools.BlazorBundler.csproj", "Blazor.Tools.BlazorBundler/"]
COPY ["Blazor.Tools.BlazorBundler.Entities/Blazor.Tools.BlazorBundler.Entities.csproj", "Blazor.Tools.BlazorBundler.Entities/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data/Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data.csproj", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models/Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models.csproj", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels/Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels.csproj", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels/"]
COPY ["Blazor.Tools.BlazorBundler.Extensions/Blazor.Tools.BlazorBundler.Extensions.csproj", "Blazor.Tools.BlazorBundler.Extensions/"]
COPY ["Blazor.Tools.BlazorBundler.Factories/Blazor.Tools.BlazorBundler.Factories.csproj", "Blazor.Tools.BlazorBundler.Factories/"]
COPY ["Blazor.Tools.BlazorBundler.Interfaces/Blazor.Tools.BlazorBundler.Interfaces.csproj", "Blazor.Tools.BlazorBundler.Interfaces/"]
COPY ["Blazor.Tools.BlazorBundler.Models/Blazor.Tools.BlazorBundler.Models.csproj", "Blazor.Tools.BlazorBundler.Models/"]
COPY ["Blazor.Tools.BlazorBundler.SessionManagement/Blazor.Tools.BlazorBundler.SessionManagement.csproj", "Blazor.Tools.BlazorBundler.SessionManagement/"]
COPY ["Blazor.Tools.BlazorBundler.Utilities/Blazor.Tools.BlazorBundler.Utilities.csproj", "Blazor.Tools.BlazorBundler.Utilities/"]

RUN dotnet restore "Blazor.Tools.BlazorBundler/Blazor.Tools.BlazorBundler.csproj"

# Copy the project files and build
COPY ["Blazor.Tools.BlazorBundler/.", "Blazor.Tools.BlazorBundler/"]
COPY ["Blazor.Tools.BlazorBundler.Entities/.", "Blazor.Tools.BlazorBundler.Entities/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data/.", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Data/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models/.", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models/"]
COPY ["Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels/.", "Blazor.Tools.BlazorBundler.Entities.SampleObjects.ViewModels/"]
COPY ["Blazor.Tools.BlazorBundler.Extensions/.", "Blazor.Tools.BlazorBundler.Extensions/"]
COPY ["Blazor.Tools.BlazorBundler.Factories/.", "Blazor.Tools.BlazorBundler.Factories/"]
COPY ["Blazor.Tools.BlazorBundler.Interfaces/.", "Blazor.Tools.BlazorBundler.Interfaces/"]
COPY ["Blazor.Tools.BlazorBundler.Models/.", "Blazor.Tools.BlazorBundler.Models/"]
COPY ["Blazor.Tools.BlazorBundler.SessionManagement/.", "Blazor.Tools.BlazorBundler.SessionManagement/"]
COPY ["Blazor.Tools.BlazorBundler.Utilities/.", "Blazor.Tools.BlazorBundler.Utilities/"]

WORKDIR "/src/Blazor.Tools.BlazorBundler"
RUN dotnet build "Blazor.Tools.BlazorBundler.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Publish the library
RUN dotnet publish "Blazor.Tools.BlazorBundler.csproj" -c $BUILD_CONFIGURATION -o /app/publish 

# Build the runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
LABEL maintainer="solomiosisante@gmail.com"
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/build ./

# Set the entry point
ENTRYPOINT ["dotnet", "Blazor.Tools.BlazorBundler.dll"]
