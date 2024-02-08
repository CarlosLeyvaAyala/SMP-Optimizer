namespace CmdLine

open DMLib.String
open DMLib.IO
open System.Text.RegularExpressions
open System.IO
open DMLib

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
                filename |> sprintf "\"%s\"\nWas not written (testing mode)." |> log

                Ok filename
        | DoWrite ->
            fun log setDisplayName (filename: string, contents: string) ->
                try
                    File.WriteAllText(filename, contents)
                    sprintf "\"%s\"\nSuccesfully written." filename |> log
                    filename |> setDisplayName |> Ok
                with e ->
                    sprintf "\"%s\"\nCould not be written because of an exception." filename |> log
                    Error e.Message

type LogMode with

    static member get translateFlag =
        translateFlag Flags.logVerbose Verbose Normal

    member t.loggingFunction =
        match t with
        | Verbose -> printfn "%s"
        | Normal -> ignore

type OptimizationMode with

    static member Triangle = "per-triangle-shape"
    static member Vertex = "per-vertex-shape"

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

    static member private replaceAll from ``to`` log (filename: string, contents) =
        let didOptimize = sprintf "Optimization: \"%s\" was found. Replacing it for \"%s\""

        let didntOptimize =
            sprintf "Optimization: \"%s\" was not found. Nothing to process."

        match contents with
        | Contains from ->
            log <| didOptimize from ``to``
            Some(filename, contents |> replace from ``to``)
        | _ ->
            log <| didntOptimize from
            None

    static member private setMediumQuality hiQualityBody log (filename: string, contents) =
        log "Changing to medium quality"
        let rx = Regex("(?s)<per-vertex-shape name=\"(.*?)\">.*?<\/per-vertex-shape>")

        // Sets some collision body to high quality
        let setAsTri collisionBody contents =
            let newQuality =
                collisionBody |> replace OptimizationMode.Vertex OptimizationMode.Triangle

            contents |> replace collisionBody newQuality

        let logUnchanged = sprintf "Is %s. Leave as vertex collision."
        let logChanged = sprintf "Is %s. Change collision to triangle."
        let id' _ = id

        let logBody, changeBody, logArmor, changeArmor =
            if hiQualityBody then
                logChanged, setAsTri, logUnchanged, id'
            else
                logUnchanged, id', logChanged, setAsTri

        let lowQualityContents =
            contents |> replace OptimizationMode.Triangle OptimizationMode.Vertex

        let optimized =
            rx.Matches(lowQualityContents)
            |> Seq.map (fun m ->
                match m.Groups[1].Value with
                // Ground is always left as vertex
                | StartsWithIC' "VirtualGround" g ->
                    g |> logUnchanged |> log
                    id
                // Physics bodies
                | StartsWithIC' "Virtual" body ->
                    body |> logBody |> log
                    changeBody m.Value
                // Armor pieces
                | unknown ->
                    unknown |> logArmor |> log
                    changeArmor unknown)
            |> Seq.fold (fun acc f -> f acc) lowQualityContents

        if optimized = contents then
            log <| sprintf "\"%s\"\nWas already optimized." filename
            None
        else
            Some(filename, optimized)


    /// Returns a function that accepts a logging function, a (filename, contents) and
    /// returns an (filename, modifiedContents) option. <c>Ok</c> means the optimization was necessary.
    member t.optimizationFunction =
        match t with
        | Unknown
        | MediumTBody -> OptimizationMode.setMediumQuality true
        | Aggressive -> OptimizationMode.replaceAll OptimizationMode.Triangle OptimizationMode.Vertex
        | Expensive -> OptimizationMode.replaceAll OptimizationMode.Vertex OptimizationMode.Triangle
        | MediumVBody -> OptimizationMode.setMediumQuality false

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
