module CmdLine

open DMLib.IO

type ZipFileName = private ZipFileName of string
type DirName = private DirName of string

type LogMode =
    | Normal
    | Verbose

type OutputMode =
    | Normal
    | Nothing

type WritingMode =
    | Overwrite
    | ToDir of DirName
    | ToZip of ZipFileName
    | Invalid

type OptimizationMode =
    | Aggressive
    | Medium
    | Expensive
    | Unknown


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
