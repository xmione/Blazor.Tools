#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/runprojects.log"
PID_FILE="/workspaces/Blazor.Tools/Database/service_pids.log"

{
  echo "Running runprojects.sh"

  # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
  export DEBIAN_FRONTEND=noninteractive

 # Running projects in the background and storing their PIDs
    
    # echo "Running AccSol.API Web API project."
    # cd /workspaces/AccSol/AccSol.API/
    # nohup dotnet watch run > /dev/null 2>&1 &
    # echo $! >> $PID_FILE

    echo "Running Blazor project."
    cd /workspaces/Blazor.Tools/Blazor.Tools/
    nohup dotnet watch run > /dev/null 2>&1 &
    echo $! >> $PID_FILE
     
  echo "runprojects.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
