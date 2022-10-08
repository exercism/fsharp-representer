module Exercism.Representers.FSharp.Syntax

open Exercism.Representers.FSharp.Visitor
open System.IO
open System.Text.Json
open FSharp.Compiler.Syntax
open FSharp.Compiler.SyntaxTrivia
open FSharp.Compiler.Text
open FSharp.Compiler.Xml
open Fantomas.Core
open Fantomas.FCS.Parse

type RemoveComments() =
    inherit SyntaxVisitor()

    override this.VisitParsedImplFileInputTrivia(trivia: ParsedImplFileInputTrivia) =
        { trivia with CodeComments = [] }
        
    override this.VisitPreXmlDoc(_: PreXmlDoc) = PreXmlDoc.Empty

type RemoveImports() =
    inherit SyntaxVisitor()

    override this.VisitSynModuleOrNamespace(modOrNs: SynModuleOrNamespace): SynModuleOrNamespace =
        match modOrNs with
        | SynModuleOrNamespace(longIdent, isRecursive, isModule, decls, doc, attrs, access, range, trivia) ->
            let declsWithoutOpens =
                decls
                |> List.filter (function
                    | SynModuleDecl.Open _ -> false
                    | _ -> true)
                |> List.map this.VisitSynModuleDecl

            SynModuleOrNamespace
                (this.VisitLongIdent longIdent, isRecursive, isModule, declsWithoutOpens, doc,
                 attrs |> List.map this.VisitSynAttributeList, Option.map this.VisitSynAccess access, range, trivia)

type NormalizeIdentifiers() =
    inherit SyntaxVisitor()

    let mutable mapping: Map<string, string> = Map.empty

    let placeholderValue() = $"placeholder_%d{Map.count mapping + 1}"
    
    let placeholderKey (ident: Ident) = ident.idText

    member this.TryGetPlaceholder (synIdent: SynIdent) =
        let (SynIdent(ident, _)) = synIdent
        this.TryGetPlaceholder(ident)
        
    member this.TryGetPlaceholder (ident: Ident) =
        let key = placeholderKey ident
        Map.tryFind key mapping
        
    member this.TryAddPlaceholder (synLongIdent: SynLongIdent) =
        let (SynLongIdent(ident, _, _)) = synLongIdent
        this.TryAddPlaceholder(ident)
        
    member this.TryAddPlaceholder (longIdent: LongIdent) =
        longIdent |> List.iter this.TryAddPlaceholder

    member this.TryAddPlaceholder (synIdent: SynIdent) =
        let (SynIdent(ident, _)) = synIdent
        this.TryAddPlaceholder(ident)
        
    member this.TryAddPlaceholder (ident: Ident) =
        let newValue = placeholderValue()
        let key = placeholderKey ident

        let value = Map.tryFind key mapping |> Option.defaultValue newValue

        mapping <- Map.add key value mapping

    member this.Mapping = mapping

    override this.VisitSynModuleOrNamespace(modOrNs: SynModuleOrNamespace): SynModuleOrNamespace =
        match modOrNs with
        | SynModuleOrNamespace(longIdent, _, _, _, _, _, _, _, _) ->
            this.TryAddPlaceholder(longIdent)
            base.VisitSynModuleOrNamespace(modOrNs)

    override this.VisitSynPat(sp: SynPat): SynPat =
        match sp with
        | SynPat.Named(ident, _, _, _) ->
            this.TryAddPlaceholder(ident)
            base.VisitSynPat(sp)
        | SynPat.LongIdent(longDotId, ident, _, _, _, _) ->
            Option.iter (fun (ident: Ident) -> this.TryAddPlaceholder(ident)) ident
            this.TryAddPlaceholder(longDotId)
            base.VisitSynPat(sp)
        | _ -> base.VisitSynPat(sp)

    override this.VisitSynArgInfo(sai: SynArgInfo): SynArgInfo =
        match sai with
        | SynArgInfo(_, _, ident) ->
            ident |> Option.iter this.TryAddPlaceholder 
            base.VisitSynArgInfo(sai)

    override this.VisitIdent(ident: Ident): Ident =
        match this.TryGetPlaceholder ident with
        | Some placeholder -> Ident(placeholder, ident.idRange)
        | None -> ident

let parseFile file =
    let source = File.ReadAllText(file)
    let parsedInput, _diagnostics = parseFile false  (SourceText.ofString source) []
    Some (parsedInput, source) // TODO: use diagnostics to determine success

let toSimplifiedTreeAndMapping (tree, source) =
    let normalizeIdentifiers = NormalizeIdentifiers()

    let visitors: SyntaxVisitor list =
        [ RemoveComments()
          RemoveImports()
          normalizeIdentifiers ]

    let simplifiedTree = visitors |> List.fold (fun acc visitor -> visitor.VisitInput(acc)) tree

    let placeholdersToIdentifiers =
        normalizeIdentifiers.Mapping
        |> Map.toList
        |> List.map (fun (identifier, placeholder) -> (placeholder, identifier))
        |> Map.ofList

    (simplifiedTree, placeholdersToIdentifiers, source)

let private treeToRepresentation tree source =
    let config = { FormatConfig.FormatConfig.Default with KeepMaxNumberOfBlankLines = 1 }
    let code = CodeFormatter.FormatASTAsync(tree, source, config) |> Async.RunSynchronously
    CodeFormatter.FormatDocumentAsync(false, code, config) |> Async.RunSynchronously // The second pass is needed to remove empty lines

let private mappingToJson mapping = JsonSerializer.Serialize(mapping)

let writeToFile representationFile mappingFile (tree, mapping, source) =
    File.WriteAllText(representationFile, treeToRepresentation tree source)
    File.WriteAllText(mappingFile, mappingToJson mapping)
