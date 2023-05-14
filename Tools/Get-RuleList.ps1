<#
.SYNOPSIS
Read global .editorconfig rule set files and output as a TSV table.
.PARAMETER SdkVersion
Specify SDK version to read global .editorconfig rule set files.
If not specified, latest version will be used.
.PARAMETER IncludesOldVersion
If specified, old version of global .editorconfig rule set files will be included.
#>
<#
Copyright (c) FUJIWARA, Yusuke and all contributors.
This file is licensed under MIT license.
See the LICENSE in the project root for more information.
#>

#Requires -Version 7.0

using namespace System.Collections.Generic
using namespace System.IO
using namespace System.Text.RegularExpressions

[CmdLetBinding(PositionalBinding = $false)]
param(
    [string]$SdkVersion = $null,
    [switch]$IncludesOldVersion
)

Set-StrictMode -Version latest

$ErrorActionPreference = 'Break'
$InformationPreference = 'Continue'
# $VerbosePreference = 'Continue'

New-Variable -Name 'BaseSdkPath' -Value "${env:programfiles}\dotnet\sdk" -Option Constant
[string[]]$RuleLevels = @('default', 'minimum', 'recommended', 'all')

function Get-LatestSdkVersion {
    [OutputType([String])]
    param()

    return Get-ChildItem -Path $BaseSdkPath -Directory | Sort-Object -Property Name -Descending | Select-Object -First 1 -ExpandProperty Name
}

function Get-TargetFiles {
    [OutputType([String[]])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$SdkVersion,
        [Parameter(Mandatory)][bool]$IncludesOldVersion
    )

    if ([String]::IsNullOrEmpty($SdkVersion)) {
        $SdkVersion = Get-LatestSdkVersion
        Write-Verbose "Detected latest SDK version: ${SdkVersion}"
    }

    [String]$ConfigPath = "${BaseSdkPath}\${SdkVersion}\Sdks\Microsoft.NET.Sdk\analyzers\build\config"
    [Version]$SdkVersionInfo = [Version]::Parse($SdkVersion)

    # Make versions array
    [List[int]]$MajorVersions = [List[int]]::new()
    if ($IncludesOldVersion) {
        for ($i = 5; $i -le $SdkVersionInfo.Major; $i++) {
            $MajorVersions.Add($i)
        }
    }
    else {
        $MajorVersions.Add($SdkVersionInfo.Major)
    }

    # Returns latest -all rules last.
    foreach ($MajorVersion in $MajorVersions) {
        foreach ($RuleLevel in $RuleLevels) {
            [string]$Path = "${ConfigPath}\analysislevel_${MajorVersion}_${RuleLevel}.editorconfig"
            if (Test-Path -Path $Path -PathType Leaf) {
                Write-Verbose -Message "Found: ${Path}"
                $Path
            }
        }
    }
}


[regex]$CACommentPattern = '^\s*#\s*(?<ID>CA\d+):\s*(?<Name>.*)$'
[regex]$CARulePattern = '^\s*dotnet_diagnostic\.(?<ID>CA\d+)\.severity\s*=\s*(?<Value>.*)$'


# Made from https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/overview#enabled-rules
[hashtable]$DefaultRulesMaster = @{
    'CA1416' = 'Platform compatibility analyzer'
    'CA1417' = 'Do not use OutAttribute on string parameters for P/Invokes'
    'CA1418' = 'Use valid platform string'
    'CA1420' = "Using features that require runtime marshalling when it's disabled will result in run-time exceptions"
    'CA1422' = 'Validate platform compatibility'
    'CA1831' = 'Use AsSpan instead of range-based indexers for string when appropriate'
    'CA2013' = 'Do not use ReferenceEquals with value types'
    'CA2014' = 'Do not use stackalloc in loops'
    'CA2015' = 'Do not define finalizers for types derived from MemoryManager<T>'
    'CA2017' = 'Parameter count mismatch'
    'CA2018' = 'The count argument to Buffer.BlockCopy should specify the number of bytes to copy'
    'CA2200' = 'Rethrow to preserve stack details'
    'CA2247' = 'Argument passed to TaskCompletionSource constructor should be TaskCreationOptions enum instead of TaskContinuationOptions'
    'CA2252' = 'Opt in to preview features'
    'CA2255' = 'The ModuleInitializer attribute should not be used in libraries'
    'CA2256' = 'All members declared in parent interfaces must have an implementation in a DynamicInterfaceCastableImplementation-attributed interface'
    'CA2257' = 'Members defined on an interface with the DynamicInterfaceCastableImplementationAttribute should be static'
    'CA2258' = 'Providing a DynamicInterfaceCastableImplementation interface in Visual Basic is unsupported'
    'CA2259' = 'ThreadStatic only affects static fields'
    'CA2260' = 'Use correct type parameter'
}

function Get-AnalysisRulesMaster {
    [OutputType([Dictionary[string, string]])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$RuleFile
    )

    Write-Verbose "Use master rule file: ${RuleFile}"

    [Dictionary[string, string]]$Rules = [Dictionary[string, string]]::new()
    # Default rules could not be included in the .editorconfig file, so add them manually first.
    foreach ($Id in $DefaultRulesMaster.Keys) {
        $Rules.Add($Id, $DefaultRulesMaster[$Id])
    }

    [string]$CommentId = $null
    [string]$CommentName = $null
    foreach ($line in [File]::ReadLines($RuleFile)) {
        [Match]$CommentMatch = $CACommentPattern.Match($line)
        if ($CommentMatch.Success) {
            $CommentId = $CommentMatch.Groups['ID'].Value
            $CommentName = $CommentMatch.Groups['Name'].Value
            continue
        }

        [Match]$RuleMatch = $CARulePattern.Match($line)
        if ($RuleMatch.Success -and $RuleMatch.Groups['ID'].Value -ieq $CommentId) {
            Write-Verbose "Found rule: ${CommentId} - ${CommentName}"
            $Rules[$CommentId] = $CommentName
            continue
        }
    }

    return $Rules
}

function Get-AnalysisRules {
    [OutputType([Dictionary[string, string]])]
    param([Parameter(Mandatory)][string]$RuleFile)

    [Dictionary[string, string]]$Rules = [Dictionary[string, string]]::new()
    foreach ($line in [File]::ReadLines($RuleFile)) {
        [Match]$RuleMatch = $CARulePattern.Match($line)
        if ($RuleMatch.Success) {
            $Rules.Add($RuleMatch.Groups['ID'].Value, $RuleMatch.Groups['Value'].Value)
            continue
        }
    }

    return $Rules
}

# Made from https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/overview#enabled-rules
[hashtable]$DefaultRules = @{
    '6' = @{
        'CA1416' = 'warning'
        'CA1417' = 'warning'
        'CA1418' = 'warning'
        'CA1831' = 'warning'
        'CA2013' = 'warning'
        'CA2014' = 'warning'
        'CA2015' = 'warning'
        'CA2017' = 'warning'
        'CA2018' = 'warning'
        'CA2200' = 'warning'
        'CA2247' = 'warning'
        'CA2252' = 'error'
        'CA2255' = 'warning'
        'CA2256' = 'warning'
        'CA2257' = 'warning'
        'CA2258' = 'warning'
    }
    '7' = @{
        'CA1416' = 'warning'
        'CA1417' = 'warning'
        'CA1418' = 'warning'
        'CA1420' = 'warning'
        'CA1422' = 'warning'
        'CA1831' = 'warning'
        'CA2013' = 'warning'
        'CA2014' = 'warning'
        'CA2015' = 'warning'
        'CA2017' = 'warning'
        'CA2018' = 'warning'
        'CA2200' = 'warning'
        'CA2247' = 'warning'
        'CA2252' = 'error'
        'CA2255' = 'warning'
        'CA2256' = 'warning'
        'CA2257' = 'warning'
        'CA2258' = 'warning'
        'CA2259' = 'warning'
        'CA2260' = 'warning'
    }
}

[regex]$FilePatternRegex = 'analysislevel_(?<Version>\d+)_(?<Level>\w+)\.editorconfig'
function Get-AnalysisRulesTableFromFiles {
    [OutputType([Dictionary[string, Dictionary[string, Dictionary[string, string]]]])]
    param (
        [Parameter(Mandatory)][string[]]$ConfigFiles
    )
    
    [Dictionary[string, Dictionary[string, Dictionary[string, string]]]]$Rules = [Dictionary[string, Dictionary[string, Dictionary[string, string]]]]::new()

    foreach ($ConfigFile in $ConfigFiles) {
        [Match]$FileMatch = $FilePatternRegex.Match($ConfigFile)
        if ($FileMatch.Success) {
            [string]$Version = $FileMatch.Groups['Version'].Value
            [string]$Level = $FileMatch.Groups['Level'].Value
            [Dictionary[string, string]]$Rule = Get-AnalysisRules -RuleFile $ConfigFile

            if (-not $Rules.ContainsKey($Version)) {
                $Rules.Add($Version, [Dictionary[string, Dictionary[string, string]]]::new())
            }

            if (-not $Rules[$Version].ContainsKey($Level)) {
                $Rules[$Version].Add($Level, [Dictionary[string, string]]::new())
            }

            if ($DefaultRules.ContainsKey($Version)) {
                # Default rules could not be included in the .editorconfig file, so add them manually first.
                foreach ($Id in $DefaultRules[$Version].Keys) {
                    $Rules[$Version][$Level].Add($Id, $DefaultRules[$Version][$Id])
                }
            }

            foreach ($Id in $Rule.Keys) {
                $Rules[$Version][$Level][$Id] = $Rule[$Id]
            }

            Write-Verbose "Found rules: ${Version} - ${Level}: $($Rules[$Version][$Level].Count)"
        }
    }

    return $Rules
}

function Get-Header {
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][Dictionary[string, Dictionary[string, Dictionary[string, string]]]]$Rules
    )

    [StringWriter]$writer = [StringWriter]::new()
    $writer.Write("Category`tId`tName")
    foreach ($Version in $Rules.Keys | Sort-Object) {
        foreach ($Level in $RuleLevels) {
            $writer.Write("`t${Version}-${Level}")
        }
    }

    return $writer.ToString()
}

[HashSet[string]]$UsageCategories = [HashSet[string]]::new()
[void]$UsageCategories.Add('CA1801')
[void]$UsageCategories.Add('CA1816')
[hashtable]$Categories = @{
    'CA10' = 'Design';
    'CA12' = 'Documentation';
    'CA13' = 'Globalization';
    'CA14' = 'Interoperability';
    'CA15' = 'Maintainability';
    'CA17' = 'Naming';
    'CA18' = 'Performance';
    'CA20' = 'Reliability';
    'CA21' = 'Security';
    'CA22' = 'Usage';
    'CA23' = 'Security';
    'CA30' = 'Security';
    'CA31' = 'Security';
    'CA53' = 'Security';
    'CA54' = 'Security';
    'IDE0' = 'Style';
    'IDE1' = 'Style';
    'IL30' = 'SingleFile'
}

function Get-Category {
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string]$Id
    )

    [string]$Prefix = $Id.Remove(4)
    if ($UsageCategories.Contains($Prefix)) {
        return 'Usage'
    }

    if ($Categories.ContainsKey($Prefix)) {
        return $Categories[$Prefix]
    }

    return 'Unknown'
}


[string[]]$ConfigFiles = Get-TargetFiles -SdkVersion $SdkVersion -IncludesOldVersion $IncludesOldVersion

[Dictionary[string, string]]$RuleMaster = Get-AnalysisRulesMaster -RuleFile ($ConfigFiles | Select-Object -Last 1)
# {"Version": {"level": {"rule1": "level", "rule2": "level", ...}...}}
[Dictionary[string, Dictionary[string, Dictionary[string, string]]]]$Rules = Get-AnalysisRulesTableFromFiles -ConfigFiles $ConfigFiles

# Make table as TSV
# Headers: Id Name Version1-Level1 Version1-Level2 ...
Get-Header -Rules $Rules

# Body: Id Name Value1 Value2 ...
foreach ($Id in $RuleMaster.Keys | Sort-Object) {
    [StringWriter]$writer = [StringWriter]::new()
    [string]$Category = Get-Category -Id $Id
    $writer.Write("${Category}`t${Id}`t$($RuleMaster[$Id])")
    foreach ($Version in $Rules.Keys | Sort-Object) {
        foreach ($Level in $RuleLevels) {
            if ($Rules[$Version][$Level].ContainsKey($Id)) {
                $writer.Write("`t$($Rules[$Version][$Level][$Id])")
            }
            else {
                $writer.Write("`t")
            }
        }
    }

    $writer.ToString()
}
