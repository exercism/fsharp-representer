<#
.SYNOPSIS
    Bulk generate representations for the solutions in a directory.
.DESCRIPTION
    Bulk generate representations for the solutions in a directory.
    Each child directory of the specified directory will be assumed to
    contain a solution.
.PARAMETER Exercise
    The slug of the exercise for which a representation should be generated.
.PARAMETER Directory
    The directory in which the solutions can be found.
.EXAMPLE
    The example below will generate a representation for the two-fer solutions
    in the "~/exercism/two-fer" directory. Note: the representation file will
    be written to the solution's directory.
    PS C:\> ./generate-in-bulk.ps1 two-fer ~/exercism/two-fer
#>

param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Exercise, 

    [Parameter(Position = 1, Mandatory = $true)]
    [string]$Directory
)

dotnet run --project ./src/Exercism.Representers.FSharp.Bulk/ $Exercise $Directory