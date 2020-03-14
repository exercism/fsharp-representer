module Tests

open Xunit
open System.IO
open Exercism.Representers.FSharp.Program

type TestSolutionRepresentation =
    { Expected: string
      Actual: string }

type TestSolution =
    { Slug: string
      Directory: string
      DirectoryName: string }

open Xunit.Sdk

type TestSolutionsDataAttribute() =
    inherit DataAttribute()
    
    override __.GetData(_) = 
        Directory.GetDirectories("Solutions")
        |> Seq.collect Directory.GetDirectories
        |> Seq.map (fun dir -> [| { Slug = "fake"; Directory = Path.GetFullPath(dir); DirectoryName = dir } |])


let readRepresentation solution =
      let readFile fileName = File.ReadAllText(Path.Combine(solution.Directory, fileName))
      let actual = readFile "representation.txt"
      let expected = readFile "expected_representation.txt"
      
      { Expected = expected; Actual = actual }

let runRepresenter (solution: TestSolution) =
    main [| solution.Slug; solution.Directory; solution.Directory |]

[<Theory>]
[<TestSolutionsData>]
let ``Solution is represented correctly`` (solution: TestSolution) =
    runRepresenter solution |> ignore
    
    let representation = readRepresentation solution
    
    Assert.Equal(representation.Expected, representation.Actual)
