try {
    Push-Location $PSScriptRoot > $null
    Write-Debug "moved to path $($PSScriptRoot)"    
}
catch {
    Write-Error "unable to move to script path, aborting."
    return -1;
}

try {
    Set-Location .. > $null
    Write-Debug "running cleanup from $(Get-Location)..."

    $folders=@("bin","obj",".artifacts")

    Write-Host "cleaning up $folders folders..." -ForegroundColor DarkYellow
    Get-ChildItem src/** -include $folders -Recurse | ForEach-Object ($_) { Write-Host "removing $($_.fullname)..." -ForegroundColor DarkYellow; Remove-Item $_.fullname -Force -Recurse }

    Write-Host "cleaning up $folders folders..." -ForegroundColor DarkYellow
    Get-ChildItem tests/** -include $folders -Recurse | ForEach-Object ($_) { Write-Host "removing $($_.fullname)..." -ForegroundColor DarkYellow; Remove-Item $_.fullname -Force -Recurse }

    Write-Host "restoring dotnet packages..." -ForegroundColor DarkBlue
    return (dotnet restore)
}
finally {    
    Pop-Location > $null
    Write-Host "cleanup complete, restored original path $(Get-Location)."
}


