open System
open DMLib.IO
open CmdLine

[<EntryPoint>]
let main (args) =
    try
        Console.Title <- "SMP Optimizer"

        let i = CmdLine.Core.processArgs args

        match i.showHelp with
        | HTML -> File.execute <| Paths.helpFile ()
        | NoHelp -> ()

        Display.logCmdLineArgs args i

        match i.input with
        | [||] -> Display.smartassBanner ()
        | a -> Optimize.start i a

        match i.calledFrom with
        | CommandLine -> Console.ReadLine() |> ignore
        | BatFile
        | GUIClient -> ()

        0
    with e ->
        printfn "%s" e.Message
        Console.ReadLine() |> ignore
        -1
