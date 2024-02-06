[<AutoOpen>]
module Ops

open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String
open CmdLine
open DMLib.Combinators

[<AutoOpen>]
module private Helpers =
    /// Splits the list of errors and successes into two different lists
    let splitByOkAndErrors xs =
        let oks = List<'T>()
        let errors = List<'V>()

        for x in xs do
            match x with
            | Ok v -> oks.Add v
            | Error e -> errors.Add e

        let l x = x |> seq |> List.ofSeq

        (l oks, l errors)

[<AutoOpen>]
module private Core =
    let private change log from ``to`` (filename: string, contents) =
        if contents |> contains from then
            log <| sprintf "\"%s\" was found. Replacing it for \"%s\"" from ``to``

            Some <| (filename, contents |> replace from ``to``)
        else
            log <| sprintf "\"%s\" was not found. Nothing to process." from
            None

    let optimization log =
        change log "per-triangle-shape" "per-vertex-shape"

    let revert log =
        change log "per-vertex-shape" "per-triangle-shape"

    let read fn = fn |> Tuple.dupMapSnd File.ReadAllText

    /// Process a single file
    let processFileWith op (log: string -> unit) (write: WritingFunction) filename =
        // TODO: Integrate logging with the op function
        match filename |> read |> (op log) |> Option.map (write id) with
        | None -> printfn "No optimizations were needed"
        | Some(Ok _) -> printfn "Optimization was successful"
        | Some(Error e) -> printfn "Could not be optimized:\n%s" e

    // Process all files inside a directory
    let processDirWith op (log: string -> unit) write basePath =
        let r (s: string) = s.Replace(basePath, "")

        let ok, errors =
            Directory.GetFiles(basePath, "*.xml", SearchOption.AllDirectories) // Find files
            |> tee (sprintf "*.xml files found:\n%A\n" >> log)
            |> Array.map read // Get file contents
            // TODO: Integrate logging with the op function
            |> Array.Parallel.choose (op log) // Replacement step
            |> tap (log "")
            |> Array.map (write r) // Save updated files
            |> tap (log "")
            |> splitByOkAndErrors

        match ok, errors with
        | [], [] -> printfn "No optimizations were needed"
        | _, _ ->
            ok |> Display.fileList "The following files were successfully optimized"
            errors |> Display.fileList "Errors found while optimizing"

module Optimize =
    let singleFile = processFileWith optimization
    let directory = processDirWith optimization

module Revert =
    let singleFile = processFileWith revert
    let directory = processDirWith revert
