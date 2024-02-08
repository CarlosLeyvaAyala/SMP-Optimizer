open System
open DMLib.IO
open DMLib.String
open CmdLine

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
            let doProcess operation n =
                let log = i.logging.loggingFunction
                let write = i.testing.writingFunction log
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
                    | FileExists f & (IsExtension "xml" _) -> doProcess Optimize.singleFile f
                    | x -> errorMsg $" it is not an xml file or a folder." x
                with e ->
                    errorMsg $":\n{e.Message}" input)
            |> ignore

            printfn "\nSMP optimization finished"

        0
    with e ->
        printfn "%s" e.Message
        -1
