#!/bin/bash

LOG_FILE="/workspaces/Blazor.Tools/Database/openurls.log"

# Function to resolve Codespace name
codespace_resolve_name() {
    if [ -n "$CODESPACE_NAME" ]; then
        echo "$CODESPACE_NAME"
    else
        # Fallback logic if CODESPACE_NAME is not set
        local codespace_name
        codespace_name=$(printenv | grep -i 'CODESPACE' | grep -oP 'codespace-\K[^-]+')
        echo "$codespace_name"
    fi
}

{
    echo "Running openurls.sh"
    
    # Get Codespace name
    CODESPACE_NAME=$(codespace_resolve_name)

    # Construct URLs
    API_PORT="7040" 
    BLAZOR_PORT="7010" 

    API_URL="https://${CODESPACE_NAME}-${API_PORT}.app.github.dev/swagger/index.html"
    BLAZOR_URL="https://${CODESPACE_NAME}-${BLAZOR_PORT}.app.github.dev/"

    # Print the URLs
    echo "API URL: $API_URL"
    echo "Blazor URL: $BLAZOR_URL"

    # Open URLs using Python 3
    echo "Opening URLs in Codespaces built-in browser..."
    python3 -m webbrowser "$API_URL" &
    python3 -m webbrowser "$BLAZOR_URL" &

    echo "openurls.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
