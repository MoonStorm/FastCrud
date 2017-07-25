$ErrorActionPreference = "Stop"

$packageVersion = ($env:PackageVersion)
$configuration = ($env:Configuration)
$msBuildExe = ($env:MsBuildExe)
$msBuildTarget = ($env:Targets)

#$packageVersion = "2.4.0-preview1"
#$configuration = "Release"
#$msBuildExe = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
#$msBuildTarget = "Build"

# restore dependencies
& "$PSScriptRoot\.nuget\nuget.exe" restore "$PSScriptRoot\Dapper.FastCRUD.sln"
if ($LASTEXITCODE -ne 0){
    throw "nuget restore failed"
}

# upgrade the version (no need myget will do that)
#& "$PSScriptRoot\.dnv\dnv.exe" --aivpat "$packageVersion" --write "$PSScriptRoot\Dapper.FastCrud\Properties\AssemblyInfo.cs" --what aiv
#if ($LASTEXITCODE -ne 0){
#    throw "AssemblyInfo.cs version update failed"
#}
#& "$PSScriptRoot\.dnv\dnv.exe" --avpat "$packageVersion" --write "$PSScriptRoot\Dapper.FastCrud\project.json" --what av
#if ($LASTEXITCODE -ne 0){
#    throw "project.json version update failed"
#}

# build the solution
& "$msBuildExe" "$PSScriptRoot\Dapper.FastCRUD.sln" /t:"$msBuildTarget" /p:Configuration="$configuration"
if ($LASTEXITCODE -ne 0){
    throw "build failed"
}

# & "$PSScriptRoot\.nunit\nunit-console.exe" "$PSScriptRoot\Dapper.FastCRUD.Tests\bin\$configuration\Dapper.FastCrud.Tests.dll" /exclude=ExternalDatabase /noshadow  /framework:v4.5
# if ($LASTEXITCODE -ne 0){
#     throw "tests failed"
# }

# create the NuGet packages
& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.nuspec" -OutputDirectory "$PSScriptRoot\Releases" -Version "$packageVersion" -Properties configuration="$configuration" -Verbosity detailed
if ($LASTEXITCODE -ne 0){
    throw "Nuget library packaging failed"
}

& "$PSScriptRoot\.nuget\nuget.exe" pack "$PSScriptRoot\NuGetSpecs\Dapper.FastCRUD.ModelGenerator.nuspec" -OutputDirectory "$PSScriptRoot\Releases" -Verbosity detailed  -Version "$packageVersion" -Properties configuration="$configuration"
if ($LASTEXITCODE -ne 0){
    throw "Nuget model generator packaging failed"
}