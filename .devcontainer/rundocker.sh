#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/rundocker.log"

{
  echo "Running rundocker.sh"

  # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
  export DEBIAN_FRONTEND=noninteractive

  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=P@ssw0rd123" \
    -p 1433:1433 --name mssql \
    -v /var/opt/mssql/Database:/var/opt/mssql/Database \
    -d mcr.microsoft.com/mssql/server

  echo "rundocker.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
