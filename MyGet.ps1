$ErrorActionPreference = "Stop"

param(
    [string]$packageVersion,
    [string]$configuration
)

if(-not $packageVersion){
    $packageVersion = ($env:PackageVersion)
}

if(-not $configuration){
    $configuration = ($env:Configuration)
}

if(-not $packageVersion){
    throw "invalid package version"
}
if(-not $configuration){
    throw "invalid configuration"
}

"$PSScriptRoot\.nuget\nuget.exe" restore "$PSScriptRoot\Dapper.FastCRUD.sln"

"$(${env:ProgramFiles(x86)})\MSBuild\14.0\Bin\MsBuild.exe" "$PSScriptRoot\Dapper.FastCRUD.sln" /t:Rebuild /p:Configuration="$configuration"

"$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.nuspec" -OutputDirectory Releases -Version "$packageVersion" -Properties configuration=$configuration -Verbosity detailed
"$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.ModelGenerator.nuspec" -OutputDirectory Releases -Verbosity detailed  -Version "$packageVersion" -Properties configuration=$configuration