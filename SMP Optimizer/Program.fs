open System
open DMLib.String
open DMLib.IO
open DMLib.IO.Path

[<EntryPoint>]
let main (args) =
    Console.Title <- "SMP Optimizer"

    match args with
    | [||] ->
        let smartassBanner t =
            let b = "".PadRight(70, '*')
            printfn "%s" b
            printfn "%s" t
            printfn "%s" b

        """
Usage: drag and drop at least one folder to this program.

Try again."""
        |> trim
        |> smartassBanner
    | a ->
        let p = printfn "%s"

        let banner t =
            let b = "".PadRight(90, '*')
            p "\n"
            p b
            p $"Processing:\n{t}"
            p b
            p ""

        a
        |> Array.iter (fun dir ->
            match dir with
            | IsDir d ->
                banner d
                Optimize.directory d
            | FileExists f & (IsExtension "xml" _) ->
                banner f
                Optimize.singleFile f
            | x -> banner $"{x} can not be processed.")
        |> ignore

    printfn "\nPress any key to continue..."
    Console.ReadKey() |> ignore
    0
