open System
open DMLib.IO
open CmdLine

let banner = Display.procesedItemBanner

let errorMsg msg n =
    banner $"\"{n}\"\n\nCan not be processed because{msg}."

[<EntryPoint>]
let main (args) =
    Console.Title <- "SMP Optimizer"
    let i = CmdLine.Core.processArgs args

    match i.input with
    | [||] -> Display.smartassBanner ()
    | a ->
        let doProcess op n =
            banner n
            op i.testing.writingFunction n

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
