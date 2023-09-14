#!/bin/bash

runtimes=("linux-x64" "linux-x86" "linux-arm64" "linux-arm")
rm -r bin/Release
rm -r obj/
rm /etc/KosmaPanel/webmanager/KosmaPanelWebManagerARM32
rm /etc/KosmaPanel/webmanager/KosmaPanelWebManagerARM64
rm /etc/KosmaPanel/webmanager/KosmaPanelWebManager64

for runtime in "${runtimes[@]}"; do
    echo "Publishing for runtime: $runtime"
    dotnet clean
    dotnet restore
    dotnet publish -c Release -r "$runtime" --self-contained true /p:PublishSingleFile=true -p:Version=1.0.0.1 
    echo "----------------------------------"
done
mv /etc/KosmaPanel/webmanager/bin/Release/net7.0/linux-arm/publish/KosmaPanelWebManager /etc/KosmaPanel/webmanager/
mv KosmaPanelWebManager KosmaPanelWebManagerARM32
mv /etc/KosmaPanel/webmanager/bin/Release/net7.0/linux-arm64/publish/KosmaPanelWebManager /etc/KosmaPanel/webmanager/
mv KosmaPanelWebManager KosmaPanelWebManagerARM64
mv /etc/KosmaPanel/webmanager/bin/Release/net7.0/linux-x64/publish/KosmaPanelWebManager /etc/KosmaPanel/webmanager/
mv KosmaPanelWebManager KosmaPanelWebManager64