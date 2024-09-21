#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/Database/clearports.log"
PID_FILE="/workspaces/Blazor.Tools/Database/service_pids.log"

{
    echo "Running clearports.sh"

    if [ -f "$PID_FILE" ]; then
        while IFS= read -r pid; do
            echo "Stopping process with PID: $pid"
            kill -9 "$pid"
        done < "$PID_FILE"
        rm "$PID_FILE"
        echo "All services stopped."
    else
        echo "No PID file found. Are the services running?"
    fi
} 2>&1 | tee -a "$LOG_FILE"
