module Optimize

open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String

let private splitByOkAndErrors xs =
    let oks = List<'T>()
    let errors = List<'V>()

    for x in xs do
        match x with
        | Ok v -> oks.Add v
        | Error e -> errors.Add e

    (oks |> seq |> List.ofSeq, errors |> seq |> List.ofSeq)

let private displayFileList title lst =
    match lst with
    | [] -> ()
    | l ->
        printfn "%s" title
        "".PadRight(30, '=') |> printfn "%s"
        l |> List.iter (printfn "%s")

let private replace (filename, contents) =
    if contents |> contains "per-triangle-shape" then
        Some <| (filename, contents |> replace "per-triangle-shape" "per-vertex-shape")
    else
        None

let private read fn = fn |> Tuple.dupMapSnd File.ReadAllText

let private write r (filename: string, contents: string) =
    try
        File.WriteAllText(filename, contents)
        filename |> r |> Ok
    with e ->
        Error e.Message

let singleFile filename =
    match filename |> read |> replace |> Option.map (write id) with
    | None -> printfn "No optimizations were needed"
    | Some(Ok _) -> printfn "Optimization was successful"
    | Some(Error e) -> printfn "Could not be optimized: %s" e

let directory basePath =
    let r (s: string) = s.Replace(basePath, "")

    let ok, errors =
        Directory.GetFiles(basePath, "*.xml", SearchOption.AllDirectories) // Find files
        |> Array.map read // Get file contents
        |> Array.Parallel.choose replace // Replacement step
        |> Array.map (write r) // Save updated files
        |> splitByOkAndErrors

    match ok, errors with
    | [], [] -> printfn "No optimizations were needed"
    | _, _ ->
        ok |> displayFileList "The following files were successfully optimized"
        errors |> displayFileList "Errors found while optimizing"
