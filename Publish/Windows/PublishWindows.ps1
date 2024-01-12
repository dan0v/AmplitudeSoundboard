$loc = $(Get-Location)
cd -Path "$loc\..\..\"
dotnet publish -r win-x64 -c Release -p:SelfContained=True -p:IncludeAllContentForSelfExtract=True -p:PublishSingleFile=True -o bin/Release/net8.0/publishWin
cd -Path "$loc"
