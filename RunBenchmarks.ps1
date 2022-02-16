$ErrorActionPreference = "Stop"

dotnet test "$PSScriptRoot\Dapper.FastCRUD.Benchmarks\bin\Release\net6.0\Dapper.FastCrud.Benchmarks.dll" -l "console;verbosity=normal"
if ($LASTEXITCODE -ne 0){
    throw "tests failed"
}

Write-Host "Check the README.md file to see the results"
Pause