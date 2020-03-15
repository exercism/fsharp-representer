module Exercism.Representers.FSharp.Bulk.Program

open CommandLine
open System.IO

type Options =
    { [<Value(0, Required = true, HelpText = "The solution's exercise")>]
      Slug: string
      [<Value(1, Required = true, HelpText = "The directory containing the solutions")>]
      Directory: string }

let private getSolutionDirectories (directory: string) =
    let isLeafDirectory (directory: string) = Directory.EnumerateDirectories(directory) |> Seq.isEmpty
    let isNotHidden (directory: string) = not (directory.StartsWith("."))

    Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories)
    |> Seq.filter isLeafDirectory
    |> Seq.filter isNotHidden
    |> Seq.sort

let private generateRepresentation options directory =
    let argv = [| options.Slug; directory; directory |]
    Exercism.Representers.FSharp.Program.main argv |> ignore

let private parseSuccess options =
    getSolutionDirectories options.Directory |> Seq.iter (generateRepresentation options)

let private parseOptions argv =
    let parserResult = CommandLine.Parser.Default.ParseArguments<Options>(argv)
    match parserResult with
    | :? (Parsed<Options>) as options -> Some options.Value
    | _ -> None

[<EntryPoint>]
let main argv =
    match parseOptions argv |> Option.map parseSuccess with
    | Some _ -> 0
    | None -> 1
