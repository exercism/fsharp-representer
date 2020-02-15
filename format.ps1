<#
.SYNOPSIS
    Format the source code.
.DESCRIPTION
    Formats the .NET source code, as well as all markdown files.
.EXAMPLE
    The example below will format all source code
    PS C:\> ./format.ps1
.NOTES
    The formatting of markdown files is done through prettier. This means
    that NPM has to be installed for this functionality to work.
#>

dotnet fantomas --recurse src
dotnet fantomas test\Exercism.Representers.FSharp.IntegrationTests

npx prettier@1.19.1 --write "**/*.md"