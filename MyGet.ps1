$ErrorActionPreference = "Stop"

$packageVersion = ($env:PackageVersion)
$configuration = ($env:Configuration)
$msBuildExe = ($env:MsBuildExe)
$msBuildTarget = ($env:Targets)

& "$PSScriptRoot\.nuget\nuget.exe" restore "$PSScriptRoot\Dapper.FastCRUD.sln"

& "$msBuildExe" "$PSScriptRoot\Dapper.FastCRUD.sln" /t:"$msBuildTarget" /p:Configuration="$configuration"

& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.nuspec" -OutputDirectory Releases -Version "$packageVersion" -Properties configuration="$configuration" -Verbosity detailed
& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.ModelGenerator.nuspec" -OutputDirectory Releases -Verbosity detailed  -Version "$packageVersion" -Properties configuration="$configuration"