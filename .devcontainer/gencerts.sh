#!/bin/bash
LOG_FILE="/workspaces/Blazor.Tools/gencerts.log"

{
    echo "Running gencerts.sh"
    
    # Define certificate details
    DOMAIN="localhost"
    CERT_DIR="/workspaces/Blazor.Tools/certificates"
    CERT_FILE="${CERT_DIR}/localhost.crt"
    KEY_FILE="${CERT_DIR}/localhost.key"

    # Create the certificates directory if it doesn't exist
    mkdir -p ${CERT_DIR}

    # Generate a private key
    openssl genpkey -algorithm RSA -out ${KEY_FILE} -pkeyopt rsa_keygen_bits:2048

    # Generate a self-signed certificate
    openssl req -new -x509 -key ${KEY_FILE} -out ${CERT_FILE} -days 365 -subj "/CN=${DOMAIN}"

    echo "Certificate and key have been generated at ${CERT_DIR}"
    echo "gencerts.sh completed"
} 2>&1 | tee -a "$LOG_FILE"
