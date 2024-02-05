namespace CmdLine

open DMLib.String
open DMLib.IO
open System.Collections.Generic
open System.IO
open DMLib
open DMLib.String

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
    | Invalid

type OptimizationMode =
    | Aggressive
    | Medium
    | Expensive
    | Unknown

type Parameters =
    { logging: LogMode
      testing: TestingMode
      output: FileWritingMode
      optimization: OptimizationMode
      input: string array }


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
        | e -> failwith $"\"{e}\" has not a valid zip file extension    "

    member t.toStr = let (ZipFileName v) = t in v

type DirName with

    static member ofStr s =
        match s with
        | HasExtension _ -> failwith $"\"{s}\" is a file path, not a directory path."
        | d -> DirName d

    member t.toStr = let (DirName v) = t in v


type WritingFunction = (string -> string) -> string * string -> Result<string, string>

type TestingMode with

    member t.writingFunction: WritingFunction =
        match t with
        | Testing ->
            fun _ (filename: string, _: string) ->
                printfn "\"%s\"\nWas not be written because app is running in testing mode\n" filename
                Ok filename
        | DoWrite ->
            fun r (filename: string, contents: string) ->
                try
                    File.WriteAllText(filename, contents)
                    filename |> r |> Ok
                with e ->
                    Error e.Message
