module Display

open DMLib.String

// Displays a file list
let fileList title lst =
    match lst with
    | [] -> ()
    | l ->
        printfn "%s" title
        "".PadRight(30, '=') |> printfn "%s"
        l |> List.iter (printfn "%s")

let private bigBanner t =
    let b = "".PadRight(70, '*')
    printfn "%s" b
    printfn "%s" t
    printfn "%s" b

let smartassBanner () =
    """
Usage: drag and drop at least one folder or xml file 
to this program.

Try again."""
    |> trim
    |> bigBanner

let procesedItemBanner t =
    let p = printfn "%s"
    let b = "".PadRight(90, '*')

    p "\n"
    p b
    p $"Processing:\n{t}"
    p b
    p ""


open CmdLine

let logCmdLineArgs args (i: Parameters) =
    let log = i.logging.loggingFunction

    log "Starting parameters:"
    args |> sprintf "%A" |> log

    log ""

    log "Processed parameters:"
    i.optimization |> sprintf "%A" |> log
    i.logging |> sprintf "%A" |> log
    i.testing |> sprintf "%A" |> log
    i.input |> sprintf "%A" |> log
    i.output |> sprintf "%A" |> log

    log ""
    log ""
