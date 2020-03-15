module Tests

open Xunit
open Xunit.Sdk
open System.IO

type TestSolutionsDataAttribute() =
    inherit DataAttribute()

    override __.GetData(_) =
        Directory.GetDirectories("Solutions")
        |> Seq.collect Directory.GetDirectories
        |> Seq.map (fun dir -> [| dir |])

type TestSolutionRepresentation =
    { Expected: string
      Actual: string }

let private readRepresentation (directory: string) =
    let normalize (representation: string) = representation.Replace("\r\n", "\n").Trim()
    let readFile fileName = File.ReadAllText(Path.Combine(directory, fileName)) |> normalize

    { Expected = readFile "expected_representation.txt"
      Actual = readFile "representation.txt" }

let private runRepresenter (directory: string) =
    let argv = [| "fake"; directory; directory |]
    Exercism.Representers.FSharp.Program.main argv

[<Theory>]
[<TestSolutionsData>]
let ``Solution is represented correctly`` (directory: string) =
    runRepresenter directory |> ignore

    let representation = readRepresentation directory

    Assert.Equal(representation.Expected, representation.Actual)
