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

let private parseInputToTree (options: Options) = Syntax.parseFile options.InputFile

let private simplifyTree tree = Syntax.simplifyTree tree

let private treeToRepresentation tree = Syntax.treeToString tree

let private writeRepresentation (options: Options) representation =
    File.WriteAllText(options.RepresentationFile, representation)

let private parseSuccess options =
    parseInputToTree options
    |> Option.map simplifyTree
    |> Option.map treeToRepresentation
    |> Option.map (writeRepresentation options)

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
