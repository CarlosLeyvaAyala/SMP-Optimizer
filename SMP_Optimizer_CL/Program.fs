open System
open DMLib.IO
open DMLib.String
open CmdLine
open System.IO.Compression

// TODO: Ignore files from \SKSE\Plugins\
let banner = Display.procesedItemBanner

let errorMsg msg n =
    banner $"{n}\n\nCan not be processed because{msg}"

[<EntryPoint>]
let main (args) =
    try
        Console.Title <- "SMP Optimizer"
        let i = CmdLine.Core.processArgs args

        Display.logCmdLineArgs args i

        match i.input with
        | [||] -> Display.smartassBanner ()
        | a ->
            let log = i.logging.loggingFunction

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
                try
                    match input with
                    | IsDir d -> doProcess Optimize.directory d
                    // TODO: filter not in Meshes
                    | FileExists f & (IsExtension "xml" _) -> doProcess Optimize.singleFile f
                    | NotContainsIC' @"\meshes\" fn ->
                        errorMsg
                            ":\nThis program will only process armor files; needs to be inside a \"\\Meshes\\\" folder."
                            fn
                    | x -> errorMsg $" it is not an xml file or a folder." x
                with e ->
                    errorMsg $":\n{e.Message}" input)
            |> ignore

            // Pack to zip
            match i.output, i.testing with
            | Overwrite, _
            | ToDir _, _
            | ToZip _, Testing -> () // No need to pack
            | ToZip(d, z), DoWrite ->
                let dd = d.toStr
                let zz = z.toStr

                log "Packaging zip file"

                if IO.File.Exists zz then
                    IO.File.Delete zz

                ZipFile.CreateFromDirectory(dd, zz, CompressionLevel.Optimal, false)
                IO.Directory.Delete(dd, true)

                log "Zip file created"

            printfn "\nSMP optimization finished"

        0
    with e ->
        printfn "%s" e.Message
        -1
