<#
.SYNOPSIS
    Run all tests.
.DESCRIPTION
    Run all tests, verifying the behavior of the analyzer.
.PARAMETER UpdateExpected
    Update the expected representation files to the current output (optional).
.EXAMPLE
    The example below will run all tests
    PS C:\> ./test.ps1

    The example below will run all tests and update the expected representation files
    PS C:\> ./test.ps1 -UpdateExpected
.NOTES
    The UpdateExpected switch should only be used if a bulk update of the expected
    representation files is needed.
#>

param (
    [Parameter(Mandatory = $false)]
    [Switch]$UpdateExpected
)

function Update-Expected {
    $solutionsDir = Join-Path "test" "Exercism.Representers.FSharp.IntegrationTests" "Solutions"
    Generate-Solution-Representations $solutionsDir
    Move-Generated-Represenations-To-Expected
    Move-Generated-Mappings-To-Expected
}

function Generate-Solution-Representations ([string] $SolutionsDir) {
    ./generate-in-bulk.ps1 "fake" $solutionsDir
}

function Move-Generated-Represenations-To-Expected ([string] $SolutionsDir) {
    Get-ChildItem $solutionsDir "representation.txt" -Recurse | ForEach-Object { 
        Move-Item -Force $_.FullName $_.FullName.Replace("representation", "expected_representation")
    }
}

function Move-Generated-Mappings-To-Expected ([string] $SolutionsDir) {
    Get-ChildItem $solutionsDir "mapping.json" -Recurse | ForEach-Object { 
        Move-Item -Force $_.FullName $_.FullName.Replace("mapping", "expected_mapping")
    }
}

if ($UpdateExpected.IsPresent) {
    Update-Expected
}

dotnet test