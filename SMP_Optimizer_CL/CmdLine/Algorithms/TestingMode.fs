module CmdLine.Implementations.TestingMode

open DMLib.IO
open System.IO
open DMLib.IO.Path

/// Simulate writing.
let dontWrite log _ _ (filename: string, _: string) =
    filename
    |> getFileName
    |> sprintf "\"%s\" was not written (testing mode)."
    |> log

    Ok filename

/// Do the actual writing.
let doWrite log write setDisplayName (filename: string, contents: string) =
    try
        filename |> write contents |> sprintf "\"%s\"\nSuccesfully written." |> log
        filename |> setDisplayName |> Ok
    with e ->
        sprintf "\"%s\"\nCould not be written because of an exception." filename |> log
        Error e.Message
