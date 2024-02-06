namespace CmdLine

open DMLib.String
open DMLib.IO
open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String
open DMLib.IO.Path

type ZipFileName = private ZipFileName of string
type DirName = private DirName of string

type LogMode =
    | Normal
    | Verbose

/// Run mockup operations?
type TestingMode =
    | DoWrite
    | Testing

/// Where to output processing results
type FileWritingMode =
    | Overwrite
    | ToDir of DirName
    | ToZip of ZipFileName

type OptimizationMode =
    /// Vertex on vertex collision.
    | Aggressive
    /// Triangle collision on body. Vertex on everything else.
    | MediumTBody
    /// Vertex collision on body. Triangle on everything else.
    | MediumVBody
    /// Triangle on triangle collision.
    | Expensive
    /// Will be transformed to default optimization level.
    | Unknown

type Parameters =
    { logging: LogMode
      testing: TestingMode
      output: FileWritingMode
      optimization: OptimizationMode
      input: string array }

[<RequireQualifiedAccess>]
module Flags =
    let startChar = "-"
    let testingMode = "-t"
    let logVerbose = "-v"
    let optAggresive = "-l2"
    let optMediumT = "-l1a"
    let optMediumV = "-l1b"
    let optExpensive = "-l0"

    [<Literal>]
    let output = "-o"

//████████╗██╗   ██╗██████╗ ███████╗
//╚══██╔══╝╚██╗ ██╔╝██╔══██╗██╔════╝
//   ██║    ╚████╔╝ ██████╔╝█████╗
//   ██║     ╚██╔╝  ██╔═══╝ ██╔══╝
//   ██║      ██║   ██║     ███████╗
//   ╚═╝      ╚═╝   ╚═╝     ╚══════╝

//███████╗██╗  ██╗████████╗███████╗███╗   ██╗███████╗██╗ ██████╗ ███╗   ██╗███████╗
//██╔════╝╚██╗██╔╝╚══██╔══╝██╔════╝████╗  ██║██╔════╝██║██╔═══██╗████╗  ██║██╔════╝
//█████╗   ╚███╔╝    ██║   █████╗  ██╔██╗ ██║███████╗██║██║   ██║██╔██╗ ██║███████╗
//██╔══╝   ██╔██╗    ██║   ██╔══╝  ██║╚██╗██║╚════██║██║██║   ██║██║╚██╗██║╚════██║
//███████╗██╔╝ ██╗   ██║   ███████╗██║ ╚████║███████║██║╚██████╔╝██║ ╚████║███████║
//╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═══╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝


type ZipFileName with

    static member ext = "zip"

    static member ofStr =
        function
        | IsExtension ZipFileName.ext fn -> ZipFileName fn
        | e -> failwith $"\"{e}\" has not a valid zip file extension."

    member t.toStr = let (ZipFileName v) = t in v

type DirName with

    static member ofStr s =
        match s with
        | HasExtension _ -> failwith $"\"{s}\" is a file path, not a directory path."
        | d -> DirName d

    member t.toStr = let (DirName v) = t in v


type WritingFunction = (string -> string) -> string * string -> Result<string, string>

type TestingMode with

    static member get translateFlag =
        translateFlag Flags.testingMode Testing DoWrite

    member t.writingFunction: (string -> unit) -> WritingFunction =
        match t with
        | Testing ->
            fun log _ (filename: string, _: string) ->
                filename
                |> getFileName
                |> sprintf "\"%s\" was not written (testing mode)"
                |> log

                Ok filename
        | DoWrite ->
            fun log r (filename: string, contents: string) ->
                let fn = getFileName filename

                try
                    File.WriteAllText(filename, contents)
                    sprintf "\"%s\" succesfully written." fn |> log
                    filename |> r |> Ok
                with e ->
                    sprintf "\"%s\" could not be written because of an exception." fn |> log
                    Error e.Message

type LogMode with

    static member get translateFlag =
        translateFlag Flags.logVerbose Verbose Normal

    member t.loggingFunction =
        match t with
        | Verbose -> printfn "%s"
        | Normal -> ignore

type OptimizationMode with

    static member get translateFlag =
        // TODO: Change to medium
        let Default = Aggressive

        let aggresive = translateFlag Flags.optAggresive Aggressive Unknown
        let mediumT = translateFlag Flags.optMediumT MediumTBody Unknown
        let mediumV = translateFlag Flags.optMediumV MediumTBody Unknown
        let expensive = translateFlag Flags.optExpensive Expensive Unknown

        match aggresive, mediumT, mediumV, expensive with
        | Unknown, Unknown, Unknown, Unknown -> Default
        | _, MediumTBody, _, _ -> MediumTBody
        | _, _, MediumVBody, _ -> MediumVBody
        | Aggressive, _, _, _ -> Aggressive
        | _, _, _, Expensive -> Expensive
        | _ -> Default

type FileWritingMode with

    static member get outputA =
        match
            outputA
            |> Array.tryFindIndex (fun s -> s = Flags.output)
            |> Option.map (fun i ->
                let i' = i + 1
                if i' > outputA.Length - 1 then None else Some outputA[i']) // The first argument after "-o"
            |> Option.flatten
        with
        | Some x ->
            match x with
            | IsExtension ZipFileName.ext fn -> ToZip <| ZipFileName.ofStr fn
            | HasExtension _ -> failwith $"\"{x}\" is not a valid output folder/file."
            | IsEmptyStr -> Overwrite
            | dir -> ToDir <| DirName.ofStr dir
        | None -> Overwrite
