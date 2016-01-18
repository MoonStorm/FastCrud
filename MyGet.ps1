$ErrorActionPreference = "Stop"

$packageVersion = ($env:PackageVersion)
$configuration = ($env:Configuration)
$msBuildExe = ($env:MsBuildExe)
$msBuildTarget = ($env:Targets)

& "$PSScriptRoot\.nuget\nuget.exe" restore "$PSScriptRoot\Dapper.FastCRUD.sln"
if ($LASTEXITCODE -ne 0){
    throw "nuget restore failed"
}

& "$msBuildExe" "$PSScriptRoot\Dapper.FastCRUD.sln" /t:"$msBuildTarget" /p:Configuration="$configuration"
if ($LASTEXITCODE -ne 0){
    throw "sbuild failed"
}

# & "$PSScriptRoot\.nunit\nunit-console.exe" "$PSScriptRoot\Dapper.FastCRUD.Tests\bin\$configuration\Dapper.FastCrud.Tests.dll" /exclude=ExternalDatabase /noshadow  /framework:v4.5
# if ($LASTEXITCODE -ne 0){
    # throw "tests failed"
# }

& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.nuspec" -OutputDirectory Releases -Version "$packageVersion" -Properties configuration="$configuration" -Verbosity detailed
if ($LASTEXITCODE -ne 0){
    throw "packing failed"
}

& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.ModelGenerator.nuspec" -OutputDirectory Releases -Verbosity detailed  -Version "$packageVersion" -Properties configuration="$configuration"
if ($LASTEXITCODE -ne 0){
    throw "symbol packing failed"
}
