#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/Database/testattachmdf.log"

{
    echo "Running testattachmdf.sh"

    # Set DEBIAN_FRONTEND to noninteractive to prevent prompts
    export DEBIAN_FRONTEND=noninteractive

    SQL_SERVER="localhost"
    PORT="1433"
    SQL_USER="sa"
    SQL_PASSWORD="P@ssw0rd123"
    DATABASE_NAME="AccSol"
    MDF_FILE="/var/opt/mssql/Database/AccSol.mdf"
    LDF_FILE="/var/opt/mssql/Database/AccSol_log.ldf"

    SQL_SCRIPT="CREATE DATABASE [${DATABASE_NAME}] ON \
                (FILENAME = '${MDF_FILE}'), \
                (FILENAME = '${LDF_FILE}') \
                FOR ATTACH;"

    export PATH="$PATH:/opt/mssql-tools/bin"
       

    # Run the SQL script to attach the database
    /opt/mssql-tools/bin/sqlcmd -S "${SQL_SERVER},${PORT}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -Q "${SQL_SCRIPT}"

    ./testdb.sh

    echo "testattachmdf.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
