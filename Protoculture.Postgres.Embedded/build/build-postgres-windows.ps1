# Hi Windows user.  Make sure you have vsredist installed.
# windows 11 vcruntime140.dll
# https://aka.ms/vs/17/release/vc_redist.x64.exe
#
param(
    $postgresVersion = "14.1-1"
)

$cpuArchitecture=$env:PROCESSOR_ARCHITECTURE
$postgresSource="https://get.enterprisedb.com/postgresql"
$scriptDir="$PSScriptRoot"
$buildDir="${scriptDir}\postgres-build"
$postgresBuildsDir="${scriptDir}\postgres"
$outputDir="${postgresBuildsDir}\windows\${cpuArchitecture}"
$postgresArchive="postgresql-${postgresVersion}-windows-x64-binaries.zip"

if ("${cpuArchitecture}" -ne "AMD64")
{
    throw "Unsupported CPU architecture."
}

$cpuArchitecture="x86_64"

New-Item -Type Directory -Path "${scriptDir}\postgres\windows\${cpuArchitecture}"
New-Item -Type Directory -Path "${buildDir}"

Set-Location -Path "${buildDir}"
Invoke-WebRequest -Uri "${postgresSource}\${postgresArchive}" -OutFile "${postgresArchive}"
Expand-Archive -Path "${postgresArchive}"
Set-Location -Path ".\postgresql-${postgresVersion}-windows-x64-binaries\pgsql"
Move-Item ".\bin" "${outputDir}"
Move-Item ".\lib" "${outputDir}"
Move-Item ".\share" "${outputDir}"

Set-Location -Path "${postgresBuildsDir}\windows"

Compress-Archive -Path "." -DestinationPath "${postgresBuildsDir}\windows-${cpuArchitecture}.zip"

Set-Location -Path "${scriptDir}"
