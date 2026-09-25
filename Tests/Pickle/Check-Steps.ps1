<#
.SYNOPSIS
  Before a ticket is filed: every step pattern of this suite compiles with Pickle's own expression engine,
  none is declared twice, and every step line of every feature of this suite matches exactly ONE expression
  among this suite's, Pickle's and the shared tools' and other suites'. No game, a few seconds.

.DESCRIPTION
  A run that meets an invalid pattern plays zero scenarios, and a line two expressions match fails a healthy
  scenario ("Ambiguous step"), each after hours in the queue. Adapted from PickleTools/ResearchSteps/Check-Steps.ps1.
  A line that matches nothing is reported too: it would fail at run time as an undefined step.

.EXAMPLE
  powershell.exe -ExecutionPolicy Bypass -File JoyRescue/Tests/Pickle/Check-Steps.ps1
#>
param(
    [string]$PickleAssemblies = 'C:\Program Files (x86)\Steam\steamapps\workshop\content\294100\3791648678\Assemblies',
    [string]$Cecil = "$env:USERPROFILE\.nuget\packages\mono.cecil\0.11.5\lib\net40\Mono.Cecil.dll"
)
$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repoRoot = Split-Path (Split-Path (Split-Path $here -Parent) -Parent) -Parent      # ...\rimworld

foreach ($dll in 'CucumberExpressions.dll', 'RimWorks.Pickle.Core.dll') {
    $path = Join-Path $PickleAssemblies $dll
    if (-not (Test-Path $path)) { throw "$dll not found under $PickleAssemblies" }
    [Reflection.Assembly]::LoadFrom($path) | Out-Null
}
Add-Type -Path $Cecil
$core = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'RimWorks.Pickle.Core' }
$registryType = $core.GetType('RimWorks.Pickle.Core.Steps.PickleParameterTypeRegistry')
if (-not $registryType) { throw 'PickleParameterTypeRegistry no longer exists: Pickle renamed it, update this script.' }
$registry = [Activator]::CreateInstance($registryType)
function New-Expr($pattern) { New-Object CucumberExpressions.CucumberExpression($pattern, $registry) }

$attr = '\[(?:Given|When|Then)\("((?:[^"\\]|\\.)*)"'
function Read-Patterns($dir, $source) {
    foreach ($f in Get-ChildItem -LiteralPath $dir -Filter *.cs -ErrorAction SilentlyContinue) {
        $text = [IO.File]::ReadAllText($f.FullName)
        foreach ($m in [regex]::Matches($text, $attr)) {
            [pscustomobject]@{ Source = $source; File = $f.Name; Pattern = ($m.Groups[1].Value -replace '\\\\', '\' -replace '\\"', '"') }
        }
    }
}

$bad = 0
$mine = @(Read-Patterns (Join-Path $here 'Source') 'this suite')
if ($mine.Count -eq 0) { throw "no step patterns under $here\Source" }
foreach ($g in ($mine | Group-Object Pattern | Where-Object { $_.Count -gt 1 })) {
    Write-Host "DUPLICATE  $($g.Name)  (declared $($g.Count) times)" -ForegroundColor Red; $bad++
}
$expressions = @()
foreach ($d in $mine) {
    try { $expressions += [pscustomobject]@{ Source = $d.Source; Pattern = $d.Pattern; Regex = (New-Expr $d.Pattern).Regex; Mine = $true } }
    catch {
        $e = $_.Exception; while ($e.InnerException) { $e = $e.InnerException }
        Write-Host "INVALID  $($d.File): $($d.Pattern)`n         $($e.Message.Split("`n")[0])" -ForegroundColor Red; $bad++
    }
}

# Pickle's own vocabulary: the runner and the vanilla steps.
foreach ($name in 'RimWorks.Pickle.Vanilla.dll', 'RimWorks.Pickle.dll') {
    $asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $PickleAssemblies $name))
    foreach ($t in $asm.MainModule.GetTypes()) {
        foreach ($m in $t.Methods) {
            foreach ($a in $m.CustomAttributes | Where-Object { $_.AttributeType.Name -in 'GivenAttribute', 'WhenAttribute', 'ThenAttribute' }) {
                $p = [string]$a.ConstructorArguments[0].Value
                try { $expressions += [pscustomobject]@{ Source = 'pickle'; Pattern = $p; Regex = (New-Expr $p).Regex; Mine = $false } } catch { }
            }
        }
    }
}
foreach ($p in 'the save {string} is loaded') { $expressions += [pscustomobject]@{ Source = 'pickle-engine'; Pattern = $p; Regex = (New-Expr $p).Regex; Mine = $false } }

# The shared tools a pass may stage, and the other suites of the repository.
$dirs = @(Get-ChildItem -LiteralPath (Join-Path $repoRoot 'PickleTools') -Directory | ForEach-Object { Join-Path $_.FullName 'Source' })
foreach ($top in Get-ChildItem -LiteralPath $repoRoot -Directory) {
    if ($top.Attributes -band [IO.FileAttributes]::ReparsePoint) { continue }
    if ($top.FullName -eq (Split-Path (Split-Path $here -Parent) -Parent)) { continue }
    $dirs += Join-Path $top.FullName 'Tests\Pickle\Source'
}
foreach ($dir in $dirs | Sort-Object -Unique) {
    if (-not (Test-Path -LiteralPath $dir)) { continue }
    foreach ($o in Read-Patterns $dir ('other:' + (Split-Path (Split-Path $dir -Parent) -Leaf))) {
        try { $expressions += [pscustomobject]@{ Source = $o.Source; Pattern = $o.Pattern; Regex = (New-Expr $o.Pattern).Regex; Mine = $false } } catch { }
    }
}

# Every step line of every feature of this suite, wherever its mod folder is.
$featureFiles = @(Get-ChildItem -LiteralPath $here -Filter *.feature -Recurse | Where-Object { $_.FullName -notmatch '\\Evidence\\' })
$lines = 0
foreach ($file in $featureFiles) {
    $n = 0
    foreach ($raw in [IO.File]::ReadAllLines($file.FullName)) {
        $n++
        if ($raw.Trim() -notmatch '^(Given|When|Then|And|But)\s+(.+)$') { continue }
        $step = $Matches[2].Trim(); $lines++
        $hits = @($expressions | Where-Object { $_.Regex.IsMatch($step) })
        if ($hits.Count -eq 0) {
            Write-Host "UNDEFINED  $($file.Name):$n  $step" -ForegroundColor Red; $bad++
        } elseif ($hits.Count -gt 1) {
            Write-Host "AMBIGUOUS  $($file.Name):$n  $step`n           " + (($hits | ForEach-Object { "$($_.Source) `"$($_.Pattern)`"" }) -join ' AND ') -ForegroundColor Red; $bad++
        }
    }
}

Write-Host ''
Write-Host "$($mine.Count) patterns of this suite, $($expressions.Count) expressions in all. $lines step lines in $($featureFiles.Count) feature files."
if ($bad -gt 0) {
    Write-Host "$bad PROBLEM(S). An invalid pattern makes a run play zero scenarios; an ambiguous or undefined line fails a healthy scenario." -ForegroundColor Red
    exit 1
}
Write-Host 'ALL PATTERNS COMPILE, NONE DECLARED TWICE, EVERY STEP LINE MATCHES EXACTLY ONE EXPRESSION' -ForegroundColor Green
exit 0
