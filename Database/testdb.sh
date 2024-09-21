#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/Database/testdb.log"

{
	echo "Running testdb.sh"

	# Set DEBIAN_FRONTEND to noninteractive to prevent prompts
	export DEBIAN_FRONTEND=noninteractive

	# Test the database connection and query
	#/opt/mssql-tools/bin/sqlcmd -S "localhost,1433" -U "sa" -P "P@ssw0rd123" -Q "select * from [master].[sys].[objects]"
	/opt/mssql-tools/bin/sqlcmd -S "localhost,1433" -U "sa" -P "P@ssw0rd123" -Q "select * from [AccSol].[dbo].[Client]"

	echo "testdb.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
