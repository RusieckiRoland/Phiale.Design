param([string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output = Join-Path $root 'artifacts/packages'
foreach ($project in @(Get-ChildItem (Join-Path $root 'Platforms') -Recurse -Filter *.csproj)) {
    dotnet pack $project.FullName -c $Configuration --nologo -o $output -p:GeneratePackageOnBuild=false
    if ($LASTEXITCODE -ne 0) { throw "Packing failed: $($project.Name)" }
}
Write-Output "Packages: $output"
