[<AutoOpen>]
module Ops

open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String
open CmdLine
open DMLib.Combinators

[<AutoOpen>]
module private Core =
    let read fn = fn |> Tuple.dupMapSnd File.ReadAllText

    /// Process a single file
    let processFileWith _ optimize (write: WritingFunction) filename =
        match filename |> read |> optimize |> Option.map (write id) with
        | None -> printfn "No optimizations were needed"
        | Some(Ok _) -> printfn "Optimization was successful"
        | Some(Error e) -> printfn "Could not be optimized:\n%s" e

    // Process all files inside a directory
    let processDirWith (log: string -> unit) optimize write basePath =
        let r (s: string) = s.Replace(basePath, "")

        // TODO: filter not in Meshes
        let ok, errors =
            Directory.GetFiles(basePath, "*.xml", SearchOption.AllDirectories) // Find files
            |> tee (sprintf "*.xml files found:\n%A\n" >> log)
            |> Array.map read // Get file contents
            |> Array.Parallel.choose optimize // Replacement step
            |> tap (log "")
            |> Array.map (write r) // Save updated files
            |> tap (log "")
            |> List.ofArray
            |> List.partitionResult

        // Display results
        match ok, errors with
        | [], [] -> printfn "No optimizations were needed"
        | _, _ ->
            ok |> Display.fileList "The following files were successfully optimized"
            errors |> Display.fileList "Errors found while optimizing"

module Optimize =
    let singleFile = processFileWith
    let directory = processDirWith
