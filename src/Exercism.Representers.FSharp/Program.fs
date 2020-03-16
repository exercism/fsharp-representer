module Exercism.Representers.FSharp.Program

open System.IO
open CommandLine
open Exercism.Representers.FSharp
open Humanizer

type Options =
    { [<Value(0, Required = true, HelpText = "The solution's exercise")>]
      Slug: string
      [<Value(1, Required = true, HelpText = "The directory containing the solution")>]
      InputDirectory: string
      [<Value(2, Required = true, HelpText = "The directory to which the results will be written")>]
      OutputDirectory: string }
    member this.InputFile = Path.Combine(this.InputDirectory, sprintf "%s.fs" (this.Slug.Dehumanize().Pascalize()))
    member this.RepresentationFile = Path.Combine(this.OutputDirectory, "representation.txt")
    member this.MappingFile = Path.Combine(this.OutputDirectory, "mapping.json")

let private parseSuccess (options: Options) =
    Syntax.parseFile options.InputFile
    |> Option.map Syntax.toSimplifiedTreeAndMapping
    |> Option.map (Syntax.writeToFile options.RepresentationFile options.MappingFile)

let private parseOptions argv =
    let parserResult = CommandLine.Parser.Default.ParseArguments<Options>(argv)
    match parserResult with
    | :? (Parsed<Options>) as options -> Some options.Value
    | _ -> None

[<EntryPoint>]
let main argv =
    match parseOptions argv |> Option.bind parseSuccess with
    | Some _ -> 0
    | None -> 1
