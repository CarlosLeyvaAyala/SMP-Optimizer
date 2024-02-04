open System
open DMLib.IO

let banner = Display.procesedItemBanner

let errorMsg msg n =
    banner $"\"{n}\"\n\nCan not be processed because{msg}."

let doProcess op n =
    banner n
    op n

[<EntryPoint>]
let main (args) =
    Console.Title <- "SMP Optimizer"

    match args with
    | [||] -> Display.smartassBanner ()
    | a ->
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

    printfn "\nPress any key to continue..."
    Console.ReadKey() |> ignore
    0
