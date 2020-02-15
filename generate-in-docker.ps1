<#
.SYNOPSIS
    Generate a representation for a solution using the Docker representer image.
.DESCRIPTION
    Solutions on the website are stored in a normalized, representation format
    using the Docker representer image.
    This script allows one to verify that this Docker image correctly
    generates a valid representation of a solution.
.PARAMETER Exercise
    The slug of the exercise for which a representation should be generated.
.PARAMETER InputDirectory
    The directory in which the solution can be found.
.PARAMETER OutputDirectory
    The directory to which the representation file will be written.
.EXAMPLE
    The example below will generate a representation for the two-fer solution 
    in the "~/exercism/two-fer" directory and write the results to "~/exercism/results/"
    PS C:\> ./generate-in-docker.ps1 two-fer ~/exercism/two-fer ~/exercism/results/
#>

param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]$Exercise,

    [Parameter(Position = 1, Mandatory = $true)]
    [string]$InputDirectory,

    [Parameter(Position = 2, Mandatory = $true)]
    [string]$OutputDirectory
)

docker build -t exercism/fsharp-representer .
docker run -v ${InputDirectory}:/input -v ${OutputDirectory}:/output exercism/fsharp-representer $Exercise /input /output