#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/setup.log"

{
    echo "Running setup.sh"
    
    # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
    export DEBIAN_FRONTEND=noninteractive
    export ACCEPT_EULA=Y

    # Update package lists and install necessary packages
    apt-get update
    apt-get install -y mssql-tools unixodbc-dev

    # Install python3 for opening browsers
    apt-get install -y python3
    # Install Netcat (nc)
    apt-get install -y netcat-traditional

    # Install xdg-utils for xdg-open
    apt-get install -y xdg-utils

    # Add mssql-tools to PATH
    echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
    source ~/.bashrc

    # Generate certificates
    ./gencerts.sh

    # Run Docker setup script
    ./rundocker.sh

    # Build API and Blazor projects
    # cd /workspaces/AccSol/AccSol/AccSol.API/ 
    # dotnet build

    cd /workspaces/Blazor.Tools/Blazor.Tools/Blazor.Tools/ 
    dotnet build

    echo "setup.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
