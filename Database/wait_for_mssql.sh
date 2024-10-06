#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/Database/wait_for_mssql.log"

{
    echo "Checking if the SQL Server Docker container is running..."

    # Define the name or ID of your Docker container
    CONTAINER_NAME="mssql"

    # Maximum number of attempts to check if the container is running
    MAX_ATTEMPTS=10
    # Initial interval between attempts in seconds
    INITIAL_INTERVAL=5
    # Counter for attempts
    attempt=1
    # Initial interval for sleep
    interval=$INITIAL_INTERVAL

    # Check if the Docker container is running
    while [ $attempt -le $MAX_ATTEMPTS ]; do
        if [ "$(docker ps -q -f name=${CONTAINER_NAME})" ]; then
            echo "SQL Server Docker container is already running."
            break
        else
            echo "SQL Server Docker container is not running. Starting the container..."
            docker start "${CONTAINER_NAME}"
            sleep $interval
            attempt=$((attempt + 1))
            interval=$((interval + 5))
        fi
    done

    # Check if the maximum number of attempts is reached
    if [ $attempt -gt $MAX_ATTEMPTS ]; then
        echo "Maximum number of attempts reached. Exiting."
        exit 1
    fi

    echo "Waiting for SQL Server to be available..."

    # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
    export DEBIAN_FRONTEND=noninteractive

    SQL_SERVER="localhost"
    PORT="1433"

    while ! /opt/mssql-tools/bin/sqlcmd -S "${SQL_SERVER},${PORT}" -U "sa" -P "P@ssw0rd123" -Q "SELECT 1" &>/dev/null; do
        echo "SQL Server is not yet available. Retrying in 5 seconds..."
        sleep 5
    done

    echo "SQL Server is up and running."
} 2>&1 | tee -a "$LOG_FILE"
