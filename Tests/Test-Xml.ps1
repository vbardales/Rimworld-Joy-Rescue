$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$failures = 0
$checks = 0
function Check([string]$Name, [scriptblock]$Body) {
    $script:checks++
    try { & $Body; Write-Output "PASS $Name" }
    catch { $script:failures++; Write-Output "FAIL ${Name}: $_" }
}
function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}
function Read-SafeXml([string]$Path) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $reader = [System.Xml.XmlReader]::Create($Path, $settings)
    try {
        $doc = [System.Xml.XmlDocument]::new()
        $doc.Load($reader)
        return ,$doc
    } finally { $reader.Dispose() }
}
$xmlFiles = @(Get-ChildItem -LiteralPath "$repo/Mod" -Filter '*.xml' -Recurse)
Check 'XML files exist' { Require ($xmlFiles.Count -gt 0) 'No XML files found' }
foreach ($file in $xmlFiles) {
    Check "Parse $($file.Name) [$($file.Directory.Parent.Name)]" {
        $doc = Read-SafeXml $file.FullName
        $expected = if ($file.Name -eq 'About.xml') { 'ModMetaData' }
            elseif ($file.FullName -match '[\\/]Defs[\\/]') { 'Defs' }
            else { 'LanguageData' }
        Require ($doc.DocumentElement.LocalName -ceq $expected) "Expected root $expected"
    }
}
Check 'About identity, version and Harmony dependency' {
    $about = (Read-SafeXml "$repo/Mod/About/About.xml").ModMetaData
    Require ($about.name -ceq 'Joy Rescue') 'Unexpected title'
    Require ($about.packageId -ceq 'nelim.joyrescue') 'Unexpected package ID'
    Require (@($about.supportedVersions.li) -contains '1.6') 'Missing RimWorld 1.6 support'
    Require (@($about.modDependencies.li.packageId) -contains 'brrainz.harmony') 'Missing Harmony dependency'
    Require (@($about.loadAfter.li) -contains 'brrainz.harmony') 'Missing Harmony load order'
}
Check 'GitHub URL in metadata AND visible description' {
    $about = (Read-SafeXml "$repo/Mod/About/About.xml").ModMetaData
    $url = 'https://github.com/vbardales/Rimworld-Joy-Rescue'
    Require ($about.url -ceq $url) 'Incorrect metadata URL'
    Require ($about.description.TrimEnd().EndsWith("[url=$url]Source code on GitHub[/url]")) 'Required final GitHub link missing'
}
$catalogs = @{}
foreach ($language in @('English', 'French')) {
    Check "$language keys unique and nonempty" {
        $catalog = [System.Collections.Generic.Dictionary[string,string]]::new([System.StringComparer]::Ordinal)
        foreach ($file in Get-ChildItem -LiteralPath "$repo/Mod/Languages/$language/Keyed" -Filter '*.xml') {
            $doc = Read-SafeXml $file.FullName
            foreach ($node in $doc.DocumentElement.ChildNodes) {
                if ($node.NodeType -ne [System.Xml.XmlNodeType]::Element) { continue }
                Require (-not $catalog.ContainsKey($node.Name)) "Duplicate key $($node.Name)"
                Require (-not [string]::IsNullOrWhiteSpace($node.InnerText)) "Empty key $($node.Name)"
                $catalog.Add($node.Name, $node.InnerText)
            }
        }
        Require ($catalog.Count -gt 0) "No keys in $language"
        $catalogs[$language] = $catalog
    }
}
Check 'French and English key parity' {
    Require ($catalogs.Count -eq 2) 'Catalog loading failed'
    $difference = @(Compare-Object @($catalogs.English.Keys) @($catalogs.French.Keys) -CaseSensitive)
    Require ($difference.Count -eq 0) "Key differences: $($difference.InputObject -join ', ')"
}
Check 'Translation placeholders and format strings' {
    Require ($catalogs.Count -eq 2) 'Catalog loading failed'
    foreach ($key in $catalogs.English.Keys) {
        Require ($catalogs.French.ContainsKey($key)) "Missing French key $key"
        $en = $catalogs.English[$key]
        $fr = $catalogs.French[$key]
        $enSlots = @([regex]::Matches($en, '\{(\d+)(?:[^{}]*)\}') | ForEach-Object { [int]$_.Groups[1].Value } | Sort-Object -Unique)
        $frSlots = @([regex]::Matches($fr, '\{(\d+)(?:[^{}]*)\}') | ForEach-Object { [int]$_.Groups[1].Value } | Sort-Object -Unique)
        Require (($enSlots -join ',') -ceq ($frSlots -join ',')) "Placeholder mismatch in $key"
        $max = if ($enSlots.Count) { ($enSlots | Measure-Object -Maximum).Maximum } else { -1 }
        $values = [object[]]::new([int]$max + 1)
        for ($i = 0; $i -lt $values.Length; $i++) { $values[$i] = "argument$i" }
        $null = [string]::Format($en, $values)
        $null = [string]::Format($fr, $values)
    }
}
Check 'Literal translation keys used by C# exist in both languages' {
    $found = 0
    foreach ($file in Get-ChildItem -LiteralPath "$repo/Source" -Filter '*.cs' -Recurse) {
        $source = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($match in [regex]::Matches($source, '"(JoyRescue\.[^"]+)"\s*\.Translate\(')) {
            $key = $match.Groups[1].Value
            $found++
            foreach ($language in @('English', 'French')) {
                Require ($catalogs[$language].ContainsKey($key)) "$language missing $key"
            }
        }
    }
    Require ($found -gt 0) 'No translation calls inspected'
}
Check 'Translation call arguments satisfy resource placeholders' {
    foreach ($file in Get-ChildItem -LiteralPath "$repo/Source" -Filter '*.cs' -Recurse) {
        $source = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($match in [regex]::Matches($source, '"(JoyRescue\.[^"]+)"\s*\.Translate\(')) {
            $key = $match.Groups[1].Value
            $depth = 1; $commas = 0; $hasArgument = $false; $quoted = $false; $escaped = $false
            for ($i = $match.Index + $match.Length; $i -lt $source.Length; $i++) {
                $ch = $source[$i]
                if ($quoted) {
                    if ($escaped) { $escaped = $false }
                    elseif ($ch -eq '\') { $escaped = $true }
                    elseif ($ch -eq '"') { $quoted = $false }
                    continue
                }
                if ($ch -eq '"') { $quoted = $true; $hasArgument = $true; continue }
                if ($ch -eq '(') { $depth++; $hasArgument = $true; continue }
                if ($ch -eq ')') { $depth--; if ($depth -eq 0) { break }; continue }
                if ($ch -eq ',' -and $depth -eq 1) { $commas++ }
                if (-not [char]::IsWhiteSpace($ch)) { $hasArgument = $true }
            }
            Require ($depth -eq 0) "Unclosed Translate call: $key"
            $arity = if ($hasArgument) { $commas + 1 } else { 0 }
            foreach ($language in @('English', 'French')) {
                foreach ($slot in [regex]::Matches($catalogs[$language][$key], '\{(\d+)(?:[^{}]*)\}')) {
                    Require ([int]$slot.Groups[1].Value -lt $arity) "$language $key needs argument $($slot.Groups[1].Value), call supplies $arity"
                }
            }
        }
    }
}
Check 'The activity tooltip is the activity name alone in both languages' {
    Require ($catalogs.Count -eq 2) 'Catalog loading failed'
    foreach ($language in @('English', 'French')) {
        Require ($catalogs[$language]['JoyRescue.Settings.GiverTip'] -ceq '{0}') "$language GiverTip must be exactly {0}: the row tooltip is the activity name, nothing else"
    }
}
Check 'Optional settings shortcut and native French injection paths' {
    $button = (Read-SafeXml "$repo/Mod/Defs/MainButtonDefs/JoyRescue.xml").Defs.MainButtonDef
    Require ($button.defName -ceq 'JoyRescue_Settings') 'Unexpected shortcut identity'
    Require ($button.buttonVisible -ceq 'false') 'Shortcut must be hidden, not disabled'
    Require ($button.validWithoutMap -ceq 'true') 'Shortcut should also support world view'
    Require ($button.workerClass -ceq 'JoyRescue.MainButtonWorker_JoyRescueSettings') 'Wrong shortcut worker'
    $fr = (Read-SafeXml "$repo/Mod/Languages/French/DefInjected/MainButtonDef/JoyRescue.xml").LanguageData
    foreach ($field in @('label', 'description')) {
        Require (-not [string]::IsNullOrWhiteSpace($button.$field)) "English source $field is empty"
        Require (-not [string]::IsNullOrWhiteSpace($fr."JoyRescue_Settings.$field")) "French $field missing"
    }
}
Check 'Distribution documentation matches repository notices' {
    foreach ($name in @('LICENSE', 'ATTRIBUTION.md', 'CHANGELOG.md')) {
        Require (Test-Path "$repo/$name") "Missing $name"
        Require (Test-Path "$repo/Mod/$name") "Missing distributed $name"
        Require ((Get-FileHash "$repo/$name").Hash -eq (Get-FileHash "$repo/Mod/$name").Hash) "Distributed $name is stale"
    }
}
Write-Output "$($checks - $failures)/$checks passed; $failures failed."
if ($failures) { exit 1 }
exit 0
