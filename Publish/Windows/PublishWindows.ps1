cd -Path "$(Get-Location)\..\..\"
dotnet publish -r win-x64 -c Release -p:SelfContained=True -p:IncludeAllContentForSelfExtract=True -p:PublishSingleFile=True -o bin/Release/net5.0/publishWin
