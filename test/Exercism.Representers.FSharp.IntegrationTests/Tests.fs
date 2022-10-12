module Tests

open System.Collections.Generic
open Xunit
open Xunit.Sdk
open System.IO
open System.Text.Json

type SolutionDirectoriesDataAttribute() =
    inherit DataAttribute()

    override _.GetData(_) =
        Directory.GetDirectories("Solutions")
        |> Seq.collect Directory.GetDirectories
        |> Seq.map (fun dir -> [| dir |])

type AssertionData =
    { Expected: string
      Actual: string }

let private normalizeWhitespace (str: string) = str.Replace("\r\n", "\n").Trim()

let private readMapping (directory: string) =
    let readFile fileName =
        File.ReadAllText(Path.Combine(directory, fileName))
        |> JsonSerializer.Deserialize<Dictionary<string, string>>
        |> JsonSerializer.Serialize
        |> normalizeWhitespace

    { Expected = readFile "expected_mapping.json"
      Actual = readFile "mapping.json" }

let private readRepresentation (directory: string) =
    let readFile fileName = File.ReadAllText(Path.Combine(directory, fileName)) |> normalizeWhitespace

    { Expected = readFile "expected_representation.txt"
      Actual = readFile "representation.txt" }

let private runRepresenter (directory: string) =
    let argv = [| "fake"; directory; directory |]
    Exercism.Representers.FSharp.Program.main argv

[<Theory>]
[<SolutionDirectoriesData>]
let ``Solution is represented correctly`` (directory: string) =
    runRepresenter directory |> ignore

    let representation = readRepresentation directory
    let mapping = readMapping directory

    Assert.Equal(representation.Expected, representation.Actual)
    Assert.Equal(mapping.Expected, mapping.Actual)
