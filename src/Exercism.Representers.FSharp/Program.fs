module Exercism.Representers.FSharp.Program

open System.IO
open CommandLine
open Humanizer
open Fantomas
open FSharp.Compiler.SourceCodeServices

type Options =
    { [<Value(0, Required = true, HelpText = "The solution's exercise")>]
      Slug: string
      [<Value(1, Required = true, HelpText = "The directory containing the solution")>]
      InputDirectory: string
      [<Value(2, Required = true, HelpText = "The directory to which the results will be written")>]
      OutputDirectory: string }

let checker = FSharpChecker.Create()

let getUntypedTree file source =
    let parsingOptions = { FSharpParsingOptions.Default with SourceFiles = [| file |] }
    let parseFileResults =
        checker.ParseFile(file, FSharp.Compiler.Text.SourceText.ofString source, parsingOptions)
        |> Async.RunSynchronously

    parseFileResults.ParseTree

let writeToFile options representation =
    let filePath = Path.Combine(options.OutputDirectory, "representation.txt")
    File.WriteAllText(filePath, representation)

let formatTree file tree =
    CodeFormatter.FormatASTAsync(tree, file, [], None, FormatConfig.FormatConfig.Default) |> Async.RunSynchronously

let inputFile options =
    let fileName = sprintf "%s.fs" (options.Slug.Dehumanize().Pascalize())
    Path.Combine(options.InputDirectory, fileName)

let createRepresentation options =
    let file = inputFile options

    if File.Exists file then
        File.ReadAllText(file)
        |> getUntypedTree file
        |> Option.map (formatTree file)
    else
        None

let writeRepresentationToFile options representation =
    let filePath = Path.Combine(options.OutputDirectory, "representation.txt")
    File.WriteAllText(filePath, representation)

let onParseSuccess options =
    match createRepresentation options with
    | Some representation ->
        writeRepresentationToFile options representation
        0
    | None -> 1

let onParseFailure = 1

[<EntryPoint>]
let main argv =
    let parserResult = CommandLine.Parser.Default.ParseArguments<Options>(argv)
    match parserResult with
    | :? (Parsed<Options>) as options -> onParseSuccess options.Value
    | _ -> onParseFailure
