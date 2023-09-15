#!/bin/bash

arch=$(uname -m)

if [ "$arch" == "aarch64" ]; then
    echo "Host architecture is arm64."
    mv KosmaPanelARM64 KosmaPanel 
elif [ "$arch" == "armv7l" ]; then
    echo "Host architecture is arm32."
    mv KosmaPanelARM32 KosmaPanel
elif [ "$arch" == "x86_64" ]; then
    echo "Host architecture is amd64."
    mv KosmaPanel64 KosmaPanel
else
    echo "Unsupported architecture: $arch"
    exit 1
fi
