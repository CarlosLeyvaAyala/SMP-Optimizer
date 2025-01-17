namespace Ops

open System.IO
open DMLib
open CmdLine
open DMLib.Combinators
open System.IO.Compression
open DMLib.String
open DMLib.IO

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
    let processDirWith (log: LoggingFunction) optimize write basePath =
        let r (s: string) = s.Replace(basePath, "")

        let ok, errors =
            getFolderFiles basePath
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

    let banner = Display.procesedItemBanner

    let errorMsg msg n =
        banner $"{n}\n\nCan not be processed because{msg}"

    let checkOutdirFile (i: Parameters) =
        match i.calledFrom, i.output with
        | BatFile, Overwrite ->
            let fn = Paths.batOutDirFile ()

            if (dont File.Exists fn) then
                File.Create fn |> ignore

            File.execute fn

            failwith
                """
    When running from a batch file (*.bat), this program needs 
    an output folder to where files will be written into.

    Please, write that output folder inside the "output_dir.txt"
    file that this program opened for you, then save it and drop
    whatever you want to optimize on the *.bat file once again."""
        | _ -> ()

    let packToZip log i =
        match i.output, i.testing with
        | Overwrite, _
        | ToDir _, _
        | ToZip _, Testing -> () // No need to pack
        | ToZip(d, z), DoWrite ->
            let dd = d.toStr
            let zz = z.toStr

            log "Packaging zip file"

            if File.Exists zz then
                File.Delete zz

            ZipFile.CreateFromDirectory(dd, zz, CompressionLevel.Optimal, false)
            Directory.Delete(dd, true)

            log "Zip file created"

    let optimizeInputs log i a =
        let doProcess operation n =
            let dirOutput = i.output.writingFunction
            let write = i.testing.writingFunction log dirOutput
            let optimize = i.optimization.optimizationFunction log
            banner n

            operation log optimize write n

        match i.testing with
        | Testing -> printfn "Running in testing mode.\nNo changes will be saved.\n"
        | DoWrite -> ()

        a
        |> Array.iter (fun input ->
            let onDir = doProcess processDirWith
            let onFile = doProcess processFileWith

            let onInvalidXml =
                errorMsg ":\nThis program will only process armor files; needs to be inside a \"\\Meshes\\\" folder."

            try
                mainLoop onDir onFile onInvalidXml errorMsg input
            with e ->
                errorMsg $":\n{e.Message}" input)
        |> ignore

module Optimize =
    let start i a =
        checkOutdirFile i

        let log = i.logging.loggingFunction

        optimizeInputs log i a
        packToZip log i

        printfn "\nSMP optimization finished"
