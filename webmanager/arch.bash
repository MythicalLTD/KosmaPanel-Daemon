#!/bin/bash

arch=$(uname -m)

if [ "$arch" == "aarch64" ]; then
    echo "Host architecture is arm64."
    mv KosmaPanelWebManagerARM64 KosmaPanelWebManager 
elif [ "$arch" == "armv7l" ]; then
    echo "Host architecture is arm32."
    mv KosmaPanelWebManagerARM32 KosmaPanelWebManager
elif [ "$arch" == "x86_64" ]; then
    echo "Host architecture is amd64."
    mv KosmaPanelWebManager64 KosmaPanelWebManager
else
    echo "Unsupported architecture: $arch"
    exit 1
fi
