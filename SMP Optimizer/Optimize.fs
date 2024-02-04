[<AutoOpen>]
module Ops

open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String

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

    // Displays a file list
    let displayFileList title lst =
        match lst with
        | [] -> ()
        | l ->
            printfn "%s" title
            "".PadRight(30, '=') |> printfn "%s"
            l |> List.iter (printfn "%s")

[<AutoOpen>]
module private Core =
    let private change from ``to`` (filename: string, contents) =
        if contents |> contains from then
            Some <| (filename, contents |> replace from ``to``)
        else
            None

    let optimization = change "per-triangle-shape" "per-vertex-shape"
    let revert = change "per-vertex-shape" "per-triangle-shape"

    let read fn = fn |> Tuple.dupMapSnd File.ReadAllText

    let write r (filename: string, contents: string) =
        try
            File.WriteAllText(filename, contents)
            filename |> r |> Ok
        with e ->
            Error e.Message

    /// Process a single file
    let processFileWith op filename =
        match filename |> read |> op |> Option.map (write id) with
        | None -> printfn "No optimizations were needed"
        | Some(Ok _) -> printfn "Optimization was successful"
        | Some(Error e) -> printfn "Could not be optimized:\n%s" e

    // Process all files inside a directory
    let processDirWith op basePath =
        let r (s: string) = s.Replace(basePath, "")

        let ok, errors =
            Directory.GetFiles(basePath, "*.xml", SearchOption.AllDirectories) // Find files
            |> Array.map read // Get file contents
            |> Array.Parallel.choose op // Replacement step
            |> Array.map (write r) // Save updated files
            |> splitByOkAndErrors

        match ok, errors with
        | [], [] -> printfn "No optimizations were needed"
        | _, _ ->
            ok |> displayFileList "The following files were successfully optimized"
            errors |> displayFileList "Errors found while optimizing"

module Optimize =
    let singleFile = processFileWith optimization
    let directory = processDirWith optimization

module Revert =
    let singleFile = processFileWith revert
    let directory = processDirWith revert
