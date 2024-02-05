// DMLib includes must be deleted once nuget works again
#load "..\..\DMLib-FSharp\Combinators.fs"
#load "..\..\DMLib-FSharp\MathL.fs"
#load "..\..\DMLib-FSharp\Result.fs"
#load "..\..\DMLib-FSharp\Option.fs"
#load "..\..\DMLib-FSharp\Tuples.fs"
#load "..\..\DMLib-FSharp\Array.fs"
#load "..\..\DMLib-FSharp\String.fs"
#load "..\..\DMLib-FSharp\List.fs"
#load "..\..\DMLib-FSharp\Map.fs"
#load "..\..\DMLib-FSharp\Dictionary.fs"
#load "..\..\DMLib-FSharp\Objects.fs"
#load "..\..\DMLib-FSharp\Collections.fs"
#load "..\..\DMLib-FSharp\Files.fs"
#load "..\..\DMLib-FSharp\IO\IO.Path.fs"
#load "..\..\DMLib-FSharp\IO\File.fs"
#load "..\..\DMLib-FSharp\IO\Directory.fs"
#load "..\..\DMLib-FSharp\Json.fs"
#load "..\..\DMLib-FSharp\Misc.fs"
#load "..\..\DMLib-FSharp\Types\NonEmptyString.fs"
#load "..\..\DMLib-FSharp\Types\RecordId.fs"
#load "..\..\DMLib-FSharp\Types\MemoryAddress.fs"
#load "..\..\DMLib-FSharp\Types\CanvasPoint.fs"
#load "..\..\DMLib-FSharp\Types\Chance.fs"
#load "..\..\DMLib-FSharp\Types\Skyrim\EDID.fs"
#load "..\..\DMLib-FSharp\Types\Skyrim\Weight.fs"
#load "..\..\DMLib-FSharp\Types\Skyrim\EspFileName.fs"
#load "..\..\DMLib-FSharp\Types\Skyrim\UniqueId.fs"

// App
#load "./CmdLine.fs"

open System.IO
open DMLib.String
open DMLib.IO
open CmdLine

// Get output file name
Directory.GetFiles(@"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer", "*.xml", SearchOption.AllDirectories)
|> Array.choose (fun xxx ->
    match xxx with
    | Regex @".*\\(meshes\\.*)" l -> Some l[0]
    | _ -> None)


let processArgs a = a |> Array.filter (startsWith "-")

let a =
    [| "-V"
       @"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer"
       "-t"
       "-l0"
       "-l2"
       @"F:\Skyrim SE\MO2\mods"
       "-o"
       "-t" |]
    |> Array.map (function
        | StartsWith' "-" flag -> flag |> toLower
        | a -> a)
//let a =
//    [| "-V"
//       @"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer"
//       "-t"
//       "-o"
//       @"F:\Skyrim SE\MO2\mods"
//       "-l0"
//       "-l2" |]

let allFlags =
    a
    |> Array.choose (fun s ->
        match toLower s with
        | "-o" -> None
        | StartsWith' "-" f -> Some f
        | _ -> None)
    |> Array.distinct

let translateFlag flag exists notExists =
    match allFlags |> Array.tryFind (fun s -> s = flag) with
    | Some _ -> exists
    | None -> notExists

let log = translateFlag "-v" Verbose LogMode.Normal
let testingMode = translateFlag "-t" Nothing Normal

let optimization =
    let defaultOptimization = Aggressive // TODO: Change to medium
    let aggresive = translateFlag "-l2" Aggressive Unknown
    let medium = translateFlag "-l1" Medium Unknown
    let expensive = translateFlag "-l0" Expensive Unknown

    match aggresive, medium, expensive with
    | Unknown, Unknown, Unknown -> defaultOptimization
    | _, Medium, _ -> Medium
    | Aggressive, _, _ -> Aggressive
    | _, _, Expensive -> Expensive
    | _ -> defaultOptimization

log, testingMode, optimization

let outputA = a |> Array.except allFlags

match
    outputA
    |> Array.tryFindIndex (fun s -> s = "-o")
    |> Option.map (fun i ->
        let i' = i + 1
        if i' > outputA.Length - 1 then None else Some outputA[i'])
    |> Option.flatten
with
| Some x ->
    match x with
    | IsExtension ZipFileName.ext fn -> ToZip <| ZipFileName.ofStr fn
    | HasExtension _ -> Invalid
    | IsEmptyStr -> Overwrite
    | dir -> ToDir <| DirName.ofStr dir
| None -> Overwrite
