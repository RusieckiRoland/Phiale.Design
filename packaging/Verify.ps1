param([switch]$UsePackages)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$output = Join-Path $root ('artifacts/verification/' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Force -Path $output
$properties = @()
if ($UsePackages) {
    $properties = @('-p:DesignUsePackages=true', "-p:RestoreAdditionalProjectSources=$(Join-Path $root 'artifacts/packages')", "-p:RestorePackagesPath=$(Join-Path $output 'packages')")
}
dotnet test (Join-Path $root 'tests/Avalonia/Phiale.Design.Avalonia.Tests.csproj') -c Release @properties --nologo
if ($LASTEXITCODE -ne 0) { throw 'Avalonia tests failed.' }
$oldResult = $env:PHIALE_DESIGN_RESULT
$env:PHIALE_DESIGN_RESULT = $output
try {
    foreach ($case in @(
        @{ Folder='Wpf'; Id='Wpf'; Paths=@('net48','net8.0-windows10.0.19041.0'); Reports=@('wpf-4.txt','wpf-8.txt') },
        @{ Folder='Avalonia'; Id='Avalonia'; Paths=@('net8.0'); Reports=@('avalonia.txt') },
        @{ Folder='WinUI3'; Id='WinUI'; Paths=@('net8.0-windows10.0.19041.0/win-x64'); Reports=@('winui.txt') }
    )) {
        $folder = Join-Path $root "tests/Foundation/$($case.Folder)"
        dotnet build (Join-Path $folder "Foundation.$($case.Id).csproj") -c Release -p:Platform=x64 @properties --nologo
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $($case.Id)" }
        for ($index = 0; $index -lt $case.Paths.Count; $index++) {
            $exe = Join-Path $folder "bin/x64/Release/$($case.Paths[$index])/Foundation.$($case.Id).exe"
            $process = Start-Process -FilePath $exe -PassThru -WindowStyle Hidden
            if (-not $process.WaitForExit(40000)) { Stop-Process -Id $process.Id; throw "Timed out: $exe" }
            $report = Join-Path $output $case.Reports[$index]
            if (-not (Test-Path -LiteralPath $report)) { throw "Missing result: $report" }
            $result = Get-Content -LiteralPath $report -Raw
            if ($process.ExitCode -ne 0 -or $result -notmatch '^PASS ') { throw $result }
            Write-Output $result.Trim()
        }
    }
} finally { $env:PHIALE_DESIGN_RESULT = $oldResult }
Write-Output "Results: $output"
