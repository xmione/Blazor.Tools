# Use the .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
LABEL maintainer="solomiosisante@gmail.com"
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy the .csproj and restore dependencies
COPY Blazor.Tools.BlazorBundler.csproj .
RUN dotnet restore "Blazor.Tools.BlazorBundler.csproj"

# Copy the project files and build
COPY . .
RUN dotnet build "Blazor.Tools.BlazorBundler.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Publish the library
RUN dotnet publish "Blazor.Tools.BlazorBundler.csproj" -c $BUILD_CONFIGURATION -o /app/publish 

# Build the runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
LABEL maintainer="solomiosisante@gmail.com"
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /app/build ./

# Set the entry point
ENTRYPOINT ["dotnet", "Blazor.Tools.BlazorBundler.dll"]
