module Ops.Analize

open System.IO
open System.Text.RegularExpressions
open DMLib
open DMLib.String
open DMLib.IO
open DMLib.IO.Path
open CmdLine
open CmdLine.Core

type XmlFileName = string

type PhysBodyError =
    { xmlFile: XmlFileName
      bodies: string list }

type BodyAnalysisResult =
    | AllOk of XmlFileName
    | MissingBody of PhysBodyError
    | SpecialFile of XmlFileName
    | InvalidFile of XmlFileName

let private analyzeBodies fn =
    let rx = $"""<({Triangle}|{Vertex}).*name="(?<n>.*)">""" |> Regex

    let allNames =
        File.ReadAllText fn
        |> rx.Matches
        |> Seq.choose (fun m ->
            match m.Groups["n"].Value with
            | IsVirtualGround _ -> None
            | s -> Some s)
        |> Seq.toList

    match allNames |> List.choose (|IsPhysicsBody|_|) |> List.length with
    | 0 ->
        match allNames with
        | [] -> SpecialFile fn
        | bodies -> MissingBody { xmlFile = fn; bodies = bodies }
    | _ -> AllOk fn

let private processDir d =
    let fs = d |> getFolderFiles
    fs |> (if fs.Length > 300 then Array.Parallel.map else Array.map) analyzeBodies

let private processFile = analyzeBodies

let private splitResults results =
    let mutable ok = []
    let mutable missing = []
    let mutable special = []
    let mutable invalid = []

    results
    |> Array.iter (fun r ->
        match r with
        | AllOk fn -> ok <- ok @ [ fn ]
        | SpecialFile fn -> special <- special @ [ fn ]
        | InvalidFile fn -> invalid <- invalid @ [ fn ]
        | MissingBody d -> missing <- missing @ [ d ])

    ok, missing, special, invalid

type AnalysisResult =
    | NoIssues
    | BodyHasIssues

let start i a =
    let mutable r = [||]

    let (ok, missing, special, invalid) =
        a
        |> Array.map (fun input ->
            let onDir = processDir
            let onFile = processFile >> Array.create 1
            let onInvalidXml fn = [| InvalidFile fn |]

            let onInvalidFile msg fn =
                [| sprintf "%s%s" msg fn |> InvalidFile |]

            try
                mainLoop onDir onFile onInvalidXml onInvalidFile input
            with e ->
                onInvalidXml $"{e.Message} ({input})")
        |> Array.collect id
        |> splitResults

    if missing.Length > 1 then BodyHasIssues else NoIssues
