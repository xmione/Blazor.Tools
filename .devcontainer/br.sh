#!/bin/bash
# The line above makes this script a bash script

# Your original br.sh commands
cd /workspaces/Blazor.Tools/Blazor.Tools/Blazor.Tools
dotnet restore
dotnet build
dotnet run
cd /workspaces/Blazor.Tools
