#!/bin/bash

LOG_FILE="/workspaces/Blazor.Tools/Database/attachmdf.log"
{
    echo "Running attachmdf.sh"
    
    # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
    export DEBIAN_FRONTEND=noninteractive

    # Check if 'nc' (Netcat) command is available
    if ! command -v nc &>/dev/null; then
        echo "Error: 'nc' command not found. Please install Netcat (nc) before running this script."
        exit 1
    fi

    SQL_SERVER="localhost"
    PORT="1433"
    SQL_USER="sa"
    SQL_PASSWORD="P@ssw0rd123"
    DATABASE_NAME="AccSol"
    MDF_FILE="/var/opt/mssql/Database/AccSol.mdf"
    LDF_FILE="/var/opt/mssql/Database/AccSol_log.ldf"
    SQL_SCRIPT="CREATE DATABASE [${DATABASE_NAME}] ON (FILENAME = '${MDF_FILE}'), (FILENAME = '${LDF_FILE}') FOR ATTACH;"

    export PATH="$PATH:/opt/mssql-tools/bin"

    # Check if port 1433 is available
    echo "Checking if port 1433 is available..."
    while ! nc -z localhost "$PORT"; do
        echo "Error: Port 1433 is not available. Ensure that the SQL Server service is running and listening on port 1433. Retrying..."
        sleep 1
    done    

    # Run the SQL script to attach the database
    echo "Attaching the database..."
    /opt/mssql-tools/bin/sqlcmd -S "${SQL_SERVER},${PORT}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -Q "${SQL_SCRIPT}" || {
        echo "Error: Failed to execute SQL script."
        exit 1
    }

    ## Run the projects
    #/bin/bash /workspaces/AccSol/runprojects.sh

    ## Start services and wait for them to be available
    #for port in 7040 7010; do
    #    echo "Waiting for service on port $port to be available..."
    #    while ! nc -z localhost $port; do
    #        sleep 1
    #    done
    #    echo "Service on port $port is now available."
    #done

    ## Open URLs
    #/bin/bash /workspaces/AccSol/Database/openurls.sh

    echo "attachmdf.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
