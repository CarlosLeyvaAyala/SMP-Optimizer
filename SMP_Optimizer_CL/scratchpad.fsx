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

let rx = """<(per-triangle-shape|per-vertex-shape).*name="(?<n>.*)">""" |> Regex

let analyze fn =
    let allNames =
        File.ReadAllText fn
        |> rx.Matches
        |> Seq.choose (fun m ->
            match m.Groups["n"].Value with
            | IsVirtualGround _ -> None
            | s -> Some s)
        |> Seq.toList

    match allNames |> List.choose (|IsPhysicsBody|_|) |> List.length with
    | 0 -> Error(fn :: allNames)
    | _ -> Ok fn

fn |> List.map analyze

let allXML = @"F:\Skyrim SE\MO2\mods\" |> getFolderFiles

let ok, errors =
    allXML
    |> (if allXML.Length > 300 then
            Array.Parallel.map
        else
            Array.map)
        analyze
    |> List.ofArray
    |> List.partitionResult

let trueErrors, special =
    errors
    |> List.map (function
        | filename :: [] -> Error filename
        | filename :: errors ->
            let e = errors |> List.fold smartNl ""

            Ok $"{filename}\n{e}\n"
        | [] -> Error "")
    |> List.partitionResult



let writeToOutput writeFn title outList =
    let outFn = @"C:\Users\Osrail\Documents\GitHub\SMP Optimizer\errors.txt"

    let banner =
        let s = '*'
        let t = $"{s} {title} {s}"
        let sep = "".PadRight(t.Length, s)
        [ sep; t |> toUpper; sep; "" ]

    outList |> List.append banner |> List.toArray |> setFst outFn |> writeFn

let rewriteFile = writeToOutput File.WriteAllLines
let appendFile = writeToOutput File.AppendAllLines

rewriteFile "files with no known bodies" trueErrors
appendFile "special(?) files" (special @ [ "" ])
appendFile "files with no problems" ok
