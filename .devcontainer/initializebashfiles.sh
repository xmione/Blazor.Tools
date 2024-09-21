#!/bin/bash
LOG_FILE="/workspaces/AccSol/initializebashfiles.log"

{
  echo "Running initializebashfiles.sh"

  chmod +x /workspaces/Blazor.Tools/.devContainers/setup.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/rundocker.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/runprojects.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/wait_for_mssql.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/testattachmdf.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/attachmdf.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/openurls.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/testdb.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/Database/clearports.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/br.sh
  chmod +x /workspaces/Blazor.Tools/.devContainers/gencerts.sh

  ./setup.sh

  echo "initializebashfiles completed"
} 2>&1 | tee -a "$LOG_FILE"
