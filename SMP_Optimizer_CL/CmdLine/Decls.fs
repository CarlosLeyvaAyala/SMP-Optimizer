namespace CmdLine

open DMLib.String
open DMLib.IO
open DMLib.IO.Path
open System.IO
open DMLib

type ZipFileName = private ZipFileName of string
type DirName = private DirName of string
type TempDirName = DirName

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
    | ToZip of TempDirName * ZipFileName

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

open CmdLine.Implementations

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

/// Converts a file name to a displayable version.
type FileNameDisplayFunction = string -> string
/// Writes an optimized file. Tells if optimization was needed.
type WritingFunction = FileNameDisplayFunction -> string * string -> Result<string, string>
/// Accepts a string and logs it.
type LoggingFunction = string -> unit
/// Transforms a file name, writes the file and returns the new file name. signature: contents -> filename -> createdFileName
type OutputWritingFunction = string -> string -> string

type TestingMode with

    static member get translateFlag =
        translateFlag Flags.testingMode Testing DoWrite

    member t.writingFunction: LoggingFunction -> OutputWritingFunction -> WritingFunction =
        match t with
        | Testing -> TestingMode.dontWrite
        | DoWrite -> TestingMode.doWrite

type LogMode with

    static member get translateFlag =
        translateFlag Flags.logVerbose Verbose Normal

    member t.loggingFunction =
        match t with
        | Verbose -> printfn "%s"
        | Normal -> ignore

type OptimizationMode with

    static member get translateFlag =
        let Default = MediumTBody

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


    /// Returns a function that accepts a logging function, a (filename, contents) and
    /// returns an (filename, modifiedContents) option. <c>Ok</c> means the optimization was necessary.
    member t.optimizationFunction =
        match t with
        | Unknown
        | MediumTBody -> OptimizationMode.setMediumQuality OptimizationMode.TriangleBody
        | Aggressive -> OptimizationMode.replaceAll OptimizationMode.Triangle OptimizationMode.Vertex
        | Expensive -> OptimizationMode.replaceAll OptimizationMode.Vertex OptimizationMode.Triangle
        | MediumVBody -> OptimizationMode.setMediumQuality OptimizationMode.VertexBody

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
            | IsExtension ZipFileName.ext fn ->
                let zip = ZipFileName.ofStr fn

                let tmp =
                    Path.GetTempPath()
                    |> combine2' ("SMP Optimizer - " + System.Guid.NewGuid().ToString())
                    |> TempDirName.ofStr

                ToZip(tmp, zip)
            | HasExtension _ -> failwith $"\"{x}\" is not a valid output folder/file."
            | IsEmptyStr -> Overwrite
            | dir -> ToDir <| DirName.ofStr dir
        | None -> Overwrite

    /// Returns a function with signature: contents -> filename -> createdFileName
    member t.writingFunction: OutputWritingFunction =
        match t with
        | ToDir d -> FileWritingMode.writeForModManager d.toStr
        | ToZip(d, _) -> FileWritingMode.writeForModManager d.toStr
        | Overwrite ->
            fun c fn ->
                File.WriteAllText(fn, c)
                fn
