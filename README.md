# Exercism F# representer

A [representer][representer-introduction] creates a general representation of a submission, in order to automatically give feedback on similar code.

This repository contains the F# representer, which implements the [representer interface][representer-interface]. It uses [F# Compiler Services][fsharp-compiler-services] to parse the submission's source code into syntax trees, which are then normalized and output as a representation.

## Generate a representation for a solution

To create a representation of a solution, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./generate.ps1 <exercise> <input-directory> <output-directory>`. This script will generate a representation for the solution found in `<input-directory>`.
1. Once the script has completed, the representation will be written to `<output-directory>/representation.txt`.

## Generate a representation for a solution using Docker

To generate a representation for a solution using a Docker container, follow these steps:

1. Open a command prompt in the root directory.
1. Run `./generate-in-docker.ps1 <exercise> <input-directory> <output-directory>`. This script will:
   1. Build the representer Docker image (if necessary).
   1. Run the representer Docker image (as a container), passing the specified `exercise`, `input-directory` and `output-directory` arguments.
1. Once the script has completed, the representation can be found at `<output-directory>/representation.txt`.

## Source code formatting

This repository uses the [fantomas][fantomas] and [prettier][prettier] tools to format the source code. There are no custom rules; we just use the default formatting. You can format the code by running the `./format.ps1` command.

### Scripts

The scripts in this repository are written in PowerShell. As PowerShell is cross-platform nowadays, you can also install it on [Linux](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-linux?view=powershell-6) and [macOS](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-macos?view=powershell-6).

[representer-introduction]: https://github.com/exercism/automated-analysis/blob/master/docs/representers/introduction.md
[representer-interface]: https://github.com/exercism/automated-analysis/blob/master/docs/representers/interface.md
[fsharp-compiler-services]: https://fsharp.github.io/FSharp.Compiler.Service/
[fantomas]: https://github.com/fsprojects/fantomas
[prettier]: https://prettier.io/
