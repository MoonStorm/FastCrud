$ErrorActionPreference = "Stop"

& "$PSScriptRoot\.nunit\nunit-console.exe" "$PSScriptRoot\Dapper.FastCRUD.Benchmarks\bin\Release\Dapper.FastCrud.Benchmarks.dll" /noshadow /nothread /framework:v4.5
if ($LASTEXITCODE -ne 0){
    throw "tests failed"
}

Write-Host "Check the README.md file to see the results"
Pause