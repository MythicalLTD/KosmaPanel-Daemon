#!/bin/bash

runtimes=("linux-x64" "linux-x86" "linux-arm64" "linux-arm")
rm -r bin/Release
rm -r obj/
rm /etc/KosmaPanel/KosmaPanelARM32
rm /etc/KosmaPanel/KosmaPanelARM64
rm /etc/KosmaPanel/KosmaPanel64

for runtime in "${runtimes[@]}"; do
    echo "Publishing for runtime: $runtime"
    dotnet clean
    dotnet restore
    dotnet publish -c Release -r "$runtime" --self-contained true /p:PublishSingleFile=true -p:Version=1.0.0.1 
    echo "----------------------------------"
done
mv /etc/KosmaPanel/bin/Release/net7.0/linux-arm/publish/KosmaPanel /etc/KosmaPanel/
mv KosmaPanel KosmaPanelARM32
mv /etc/KosmaPanel/bin/Release/net7.0/linux-arm64/publish/KosmaPanel /etc/KosmaPanel/
mv KosmaPanel KosmaPanelARM64
mv /etc/KosmaPanel/bin/Release/net7.0/linux-x64/publish/KosmaPanel /etc/KosmaPanel/
mv KosmaPanel KosmaPanel64