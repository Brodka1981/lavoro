param([string]$Path="./appsettings.json")
 
$appsettings_obj=Get-Content $Path -Raw | ConvertFrom-Json

Write-Verbose "converting $Path..."

Function Flatten-Object {                                       # https://powersnippets.com/flatten-object/
    [CmdletBinding()] Param (                                    # Version 02.00.16, by iRon
        [Parameter(ValueFromPipeLine = $True)] [Object[]] $Objects,
        [String] $Separator = ".", [ValidateSet("", 0, 1)] $Base = 1, [Int] $Depth = 5, [Int] $Uncut = 1,
        [String[]] $ToString = ([String], [DateTime], [TimeSpan], [Version], [Enum]), [String[]] $Path = @()
    )
    $PipeLine = $Input | ForEach {$_}; If ($PipeLine) {$Objects = $PipeLine}
    If (@(Get-PSCallStack)[1].Command -eq $MyInvocation.MyCommand.Name -or @(Get-PSCallStack)[1].Command -eq "<position>") {
        $Object = @($Objects)[0]; $Iterate = New-Object System.Collections.Specialized.OrderedDictionary
        If ($ToString | Where {$Object -is $_}) {$Object = $Object.ToString()}
        ElseIf ($Depth) {$Depth--
            If ($Object.GetEnumerator.OverloadDefinitions -match "[\W]IDictionaryEnumerator[\W]") {
                $Iterate = $Object
            } ElseIf ($Object.GetEnumerator.OverloadDefinitions -match "[\W]IEnumerator[\W]") {
                $Object.GetEnumerator() | ForEach -Begin {$i = $Base} {$Iterate.($i) = $_; $i += 1}
            } Else {
                $Names = If ($Uncut) {$Uncut--} Else {$Object.PSStandardMembers.DefaultDisplayPropertySet.ReferencedPropertyNames}
                If (!$Names) {$Names = $Object.PSObject.Properties | Where {$_.IsGettable} | Select -Expand Name}
                If ($Names) {$Names | ForEach {$Iterate.$_ = $Object.$_}}
            }
        }
        If (@($Iterate.Keys).Count) {
            $Iterate.Keys | ForEach {
                Flatten-Object @(,$Iterate.$_) $Separator $Base $Depth $Uncut $ToString ($Path + $_)
            }
        }  Else {$Property.(($Path | Where {$_}) -Join $Separator) = $Object}
    } ElseIf ($Objects -ne $Null) {
        @($Objects) | ForEach -Begin {$Output = @(); $Names = @()} {
            New-Variable -Force -Option AllScope -Name Property -Value (New-Object System.Collections.Specialized.OrderedDictionary)
            Flatten-Object @(,$_) $Separator $Base $Depth $Uncut $ToString $Path
            $Output += New-Object PSObject -Property $Property
            $Names += $Output[-1].PSObject.Properties | Select -Expand Name
        }
        $Output | Select ([String[]]($Names | Select -Unique))
    }
};

function Get-KubernetesConfigMapFragments { 
    param (
        [Parameter(ValueFromPipeLine = $True)] [object] $object
    )

    $object | Format-List

    $props = $object `
        | Get-Member -MemberType Properties `
        | Sort-Object -Property @{ Expression = { 
                $n = [Regex]::Replace($_.Name.Replace("__", "") , '\b.', { $args[0].Value.TolowerInvariant() })

                if( $n -match "\d+" ) {
                    $digits = [string]$matches[0]
                    $n = $n.TrimEnd($digits)

                    $sortBy = $n + $digits.PadLeft(128, "0")
                    $sortBy
                }
                else {
                    $n
                }
            }
        }

    Write-Host "--- config map key refs ---"
    Write-Host

    foreach($prop in $props) {
        $key = $prop.Name
        $configMapKey = [Regex]::Replace($key.Replace("__", "") , '\b.', { $args[0].Value.TolowerInvariant() })
        $value = $object.($prop.Name)

        Write-Host @"     
        - name: $key
          valueFrom:
            configMapKeyRef:
              name: rename-me-config-map
              key: $configMapKey
"@
    }

    Write-Host
    Write-Host "--- config map values ---"
    Write-Host

    foreach($prop in $props) {
        $key = $prop.Name
        $configMapKey =  [Regex]::Replace($key.Replace("__", "") , '\b.', { $args[0].Value.TolowerInvariant() })
        $value = $object.($prop.Name)

        $out = $null
        $valuestr = switch($true) {
            ( [bool]::TryParse($value, [ref]$out) ) { if($out) { "true" } else { "false" } }
            default                                 { $value }
        }

        Write-Host @"     
  $($configMapKey): $valuestr
"@
    }

}

Write-Host "--- appsettings vars ---"
Write-Host
 
$appsettings_obj `
    | Flatten-Object -Separator "__" -Base 0 `
    | Get-KubernetesConfigMapFragments