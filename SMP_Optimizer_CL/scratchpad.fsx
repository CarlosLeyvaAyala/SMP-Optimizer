#r "nuget: TextCopy"

// DMLib includes must be deleted once nuget works again
#I "..\..\DMLib-FSharp"
#load "Combinators.fs"
#load "MathL.fs"
#load "Result.fs"
#load "Option.fs"
#load "Tuples.fs"
#load "Array.fs"
#load "String.fs"
#load "List.fs"
#load "Map.fs"
#load "Dictionary.fs"
#load "Objects.fs"
#load "Collections.fs"
#load "Files.fs"
#load "IO\IO.Path.fs"
#load "IO\File.fs"
#load "IO\Directory.fs"
#load "Json.fs"
#load "Misc.fs"
#load "Types\NonEmptyString.fs"
#load "Types\RecordId.fs"
#load "Types\MemoryAddress.fs"
#load "Types\CanvasPoint.fs"
#load "Types\Chance.fs"
#load "Types\Skyrim\EDID.fs"
#load "Types\Skyrim\Weight.fs"
#load "Types\Skyrim\EspFileName.fs"
#load "Types\Skyrim\UniqueId.fs"

// App
#I "CmdLine"
#I "CmdLine\Algorithms"
#load "Paths.fs"
#load "Globals.fs"
#load "TestingMode.fs"
#load "OptimizatonMode.fs"
#load "FileWritingMode.fs"
#load "Decls.fs"
#load "Core.fs"
#load "Display.fs"
#load "Optimize.fs"

//
#time "on"

open System.IO
open System.Text.RegularExpressions
open DMLib
open DMLib.String
open DMLib.IO
open DMLib.IO.Path
open CmdLine
open CmdLine.Core

let loadDecls =
    getScriptLoadDeclarations
        @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\SMP_Optimizer_CL\scratchpad.fsx"
        @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\SMP_Optimizer_CL\SMP_Optimizer_CL.fsproj"

loadDecls @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\SMP_Optimizer_CL\"
|> TextCopy.ClipboardService.SetText

open System.Text.RegularExpressions


let fn =
    [ @"F:\Skyrim SE\MO2\mods\[Christine] Queen Barbarian\meshes\SunJeongStuff\Queen Barbarian\upper.xml"
      @"F:\Skyrim SE\MO2\mods\[COCO] Chun Li Qipao - CBBE 3BA\meshes\Coco_Cloths\CHUNLI_v1\coco_chunli.xml" ]

type XmlFileName = string

type PhysBodyError =
    { xmlFile: XmlFileName
      bodies: string list }

type BodyAnalysisResult =
    | AllOk of XmlFileName
    | MissingBody of PhysBodyError
    | SpecialFile of XmlFileName

let analyzeBodies fn =
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

fn |> List.map analyzeBodies

let allXML = @"F:\Skyrim SE\MO2\mods\" |> getFolderFiles

let processed =
    allXML
    |> (if allXML.Length > 300 then
            Array.Parallel.map
        else
            Array.map)
        analyzeBodies

let (ok, missing, special) =
    let mutable ok = []
    let mutable missing = []
    let mutable special = []

    processed
    |> Array.iter (fun r ->
        match r with
        | AllOk fn -> ok <- ok @ [ fn ]
        | SpecialFile fn -> special <- special @ [ fn ]
        | MissingBody d -> missing <- missing @ [ d ])

    ok, missing, special

//let trueErrors, special =
//    errors
//    |> List.map (function
//        | filename :: [] -> Error filename
//        | filename :: errors ->
//            let e = errors |> List.fold smartNl ""

//            Ok $"{filename}\n{e}\n"
//        | [] -> Error "")
//    |> List.partitionResult


let writeToOutput (writeFn: string * string array -> unit) (title: string) outList =
    let outFn = @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\errors.txt"

    let banner =
        let s = '*'
        let t = $"{s} {title} {s}"
        let sep = "".PadRight(t.Length, s)
        [ sep; t |> toUpper; sep; "" ]

    outList |> List.append banner |> List.toArray |> setFst outFn |> writeFn

let rewriteFile = writeToOutput File.WriteAllLines
let appendFile = writeToOutput File.AppendAllLines

missing
|> List.map (fun m -> m.xmlFile :: m.bodies @ [ "" ])
|> List.collect id
|> rewriteFile "files with no known bodies"

missing
|> List.map _.bodies
|> List.collect id
|> List.distinct
|> List.sortWith compareICase
|> appendFile "Missing bodies summary"
//|> List.toArray
//|> setFst @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\a - bodies.txt"
//|> File.WriteAllLines

appendFile "special(?) files" (special @ [ "" ])
appendFile "files with no problems" ok
