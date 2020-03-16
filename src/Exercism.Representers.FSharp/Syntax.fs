module Exercism.Representers.FSharp.Syntax

open Exercism.Representers.FSharp.Visitor
open System.IO
open System.Text.Json
open FSharp.Compiler.Ast
open FSharp.Compiler.Text
open FSharp.Compiler.SourceCodeServices
open Fantomas

type RemoveImports() =
    inherit SyntaxVisitor()

    override this.VisitSynModuleOrNamespace(modOrNs: SynModuleOrNamespace): SynModuleOrNamespace =
        match modOrNs with
        | SynModuleOrNamespace(longIdent, isRecursive, isModule, decls, doc, attrs, access, range) ->
            let declsWithoutOpens =
                decls
                |> List.filter (function
                    | SynModuleDecl.Open(_, _) -> false
                    | _ -> true)
                |> List.map this.VisitSynModuleDecl

            // TODO: use base
            SynModuleOrNamespace
                (this.VisitLongIdent longIdent, isRecursive, isModule, declsWithoutOpens, doc,
                 attrs |> List.map this.VisitSynAttributeList, Option.map this.VisitSynAccess access, range)

type NormalizeIdentifiers() =
    inherit SyntaxVisitor()

    let mutable mapping: Map<string, string> = Map.empty

    let placeholderKey (ident: Ident) = ident.idText

    let placeholderValue() = sprintf "PLACEHOLDER_%d" (Map.count mapping + 1)

    let tryAddPlaceholder (ident: Ident) =
        let newValue = placeholderValue()
        let key = placeholderKey ident

        let value = Map.tryFind key mapping |> Option.defaultValue newValue

        mapping <- Map.add key value mapping

    let tryGetPlaceholder (ident: Ident) =
        let key = placeholderKey ident
        Map.tryFind key mapping

    member this.Mapping = mapping

    override __.VisitIdent(ident: Ident): Ident =
        match tryGetPlaceholder ident with
        | Some placeholder -> Ident(placeholder, ident.idRange)
        | None -> ident

    override __.VisitSynPat(sp: SynPat): SynPat =
        match sp with
        | SynPat.Named(_, ident, _, _, _) ->
            tryAddPlaceholder ident
            base.VisitSynPat(sp)
        | _ -> base.VisitSynPat(sp)

    override __.VisitSynArgInfo(sai: SynArgInfo): SynArgInfo =
        match sai with
        | SynArgInfo(_, _, ident) ->
            Option.iter tryAddPlaceholder ident
            base.VisitSynArgInfo(sai)

let private checker = FSharpChecker.Create()

let private parseTree file =
    let parsingOptions = { FSharpParsingOptions.Default with SourceFiles = [| file |] }
    let parseFileResults =
        checker.ParseFile(file, File.ReadAllText(file) |> SourceText.ofString, parsingOptions) |> Async.RunSynchronously

    parseFileResults.ParseTree

let parseFile file =
    if File.Exists file then parseTree file else None

let toSimplifiedTreeAndMapping tree =
    let normalizeIdentifiers = NormalizeIdentifiers()

    let visitors: SyntaxVisitor list =
        [ RemoveImports()
          normalizeIdentifiers ]

    let simplifiedTree = visitors |> List.fold (fun acc visitor -> visitor.VisitInput(acc)) tree

    let placeholdersToIdentifiers =
        normalizeIdentifiers.Mapping
        |> Map.toList
        |> List.map (fun (identifier, placeholder) -> (placeholder, identifier))
        |> Map.ofList

    (simplifiedTree, placeholdersToIdentifiers)

let private treeToRepresentation tree =
    CodeFormatter.FormatASTAsync(tree, "", [], None, FormatConfig.FormatConfig.Default) |> Async.RunSynchronously

let private mappingToJson mapping = JsonSerializer.Serialize(mapping)

let writeToFile representationFile mappingFile (tree, mapping) =
    File.WriteAllText(representationFile, treeToRepresentation tree)
    File.WriteAllText(mappingFile, mappingToJson mapping)
