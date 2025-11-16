<#
.SYNOPSIS
    Deletes all 'bin' and 'obj' folders recursively under a specified path.

.PARAMETER Path
    The root directory to search for 'bin' and 'obj' folders.

.EXAMPLE
    .\Clean-BuildFolders.ps1 -Path "C:\Repos"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

# Verify the path exists
if (-not (Test-Path $Path)) {
    Write-Host "The path '$Path' does not exist." -ForegroundColor Red
    exit
}

Write-Host "Scanning for 'bin' and 'obj' folders under: $Path" -ForegroundColor Cyan

# Find all directories named 'bin' or 'obj' recursively
$folders = Get-ChildItem -Path $Path -Directory -Recurse -ErrorAction SilentlyContinue |
           Where-Object { $_.Name -in @('bin', 'obj') }

if ($folders.Count -eq 0) {
    Write-Host "No 'bin' or 'obj' folders found." -ForegroundColor Yellow
    exit
}

foreach ($folder in $folders) {
    try {
        Write-Host "Deleting: $($folder.FullName)" -ForegroundColor Yellow
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
        Write-Host "Deleted: $($folder.FullName)" -ForegroundColor Green
    } catch {
        Write-Host "Failed to delete $($folder.FullName): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Cleanup complete." -ForegroundColor Cyan