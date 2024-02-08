module CmdLine.Implementations.TestingMode

open DMLib.IO
open System.IO

let dontWrite log _ (filename: string, _: string) =
    filename |> sprintf "\"%s\"\nWas not written (testing mode)." |> log

    Ok filename


let doWrite log setDisplayName (filename: string, contents: string) =
    try
        File.WriteAllText(filename, contents)
        sprintf "\"%s\"\nSuccesfully written." filename |> log
        filename |> setDisplayName |> Ok
    with e ->
        sprintf "\"%s\"\nCould not be written because of an exception." filename |> log
        Error e.Message
