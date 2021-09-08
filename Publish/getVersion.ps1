param(
    [string]$TargetDir
)

$Version = (Get-Command ${TargetDir}amplitude_soundboard.exe).FileVersionInfo.FileVersion