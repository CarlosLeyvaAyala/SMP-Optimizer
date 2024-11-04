open System
open DMLib.IO
open DMLib.String
open CmdLine
open System.IO.Compression
open System.IO
open DMLib.Combinators
open System.Diagnostics
open System.Threading

// TODO: Ignore files from \SKSE\Plugins\
let banner = Display.procesedItemBanner

let errorMsg msg n =
    banner $"{n}\n\nCan not be processed because{msg}"

let packToZip log i =
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
        try
            match input with
            | IsDir d -> doProcess Optimize.directory d
            | IsSmpConfigFile f -> doProcess Optimize.singleFile f
            | NotContainsIC' @"\meshes\" fn ->
                errorMsg
                    ":\nThis program will only process armor files; needs to be inside a \"\\Meshes\\\" folder."
                    fn
            | x -> errorMsg $" it is not an xml file or a folder." x
        with e ->
            errorMsg $":\n{e.Message}" input)
    |> ignore

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

let processData i =
    checkOutdirFile i

    match i.input with
    | [||] -> Display.smartassBanner ()
    | a ->
        let log = i.logging.loggingFunction

        optimizeInputs log i a
        packToZip log i

        printfn "\nSMP optimization finished"

[<EntryPoint>]
let main (args) =
    try
        Console.Title <- "SMP Optimizer"

        let i = CmdLine.Core.processArgs args

        match i.showHelp with
        | HTML -> File.execute <| Paths.helpFile ()
        | NoHelp -> ()

        Display.logCmdLineArgs args i
        processData i

        match i.calledFrom with
        | CommandLine -> Console.ReadLine() |> ignore
        | BatFile
        | GUIClient -> ()

        0
    with e ->
        printfn "%s" e.Message
        Console.ReadLine() |> ignore
        -1
