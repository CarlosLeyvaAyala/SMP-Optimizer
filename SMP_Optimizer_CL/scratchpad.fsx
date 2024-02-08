#r "nuget: TextCopy"

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
#load "./CmdLine/Decls.fs"
#load "./CmdLine/Core.fs"
//
#time "on"

open System.IO
open DMLib
open DMLib.String
open DMLib.IO
open CmdLine
open CmdLine.Core
open System.Text.RegularExpressions

// Get output file name
Directory.GetFiles(@"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer", "*.xml", SearchOption.AllDirectories)
|> Array.choose (fun xxx ->
    match xxx with
    | Regex @".*\\(meshes\\.*)" l -> Some l[0]
    | _ -> None)




let f = OptimizationMode.Aggressive.optimizationFunction ignore

//let ff =
//    Directory.GetFiles(@"F:\Skyrim SE\MO2\mods", "*.xml", SearchOption.AllDirectories)

//let fff = ff |> Array.Parallel.map (Tuple.dupMapSnd File.ReadAllText)

//let rx = Regex("<per-\\w+-shape name=\"(.*?)\">")

//// Get most used tags
//let x =
//    fff
//    |> Array.Parallel.choose (fun (fn, c) ->
//        let m = rx.Matches(c)

//        if m.Count > 0 then
//            (seq { fn }, m |> Seq.map _.Groups[1].Value)
//            ||> Seq.allPairs
//            |> Seq.toArray
//            |> Some
//        else
//            None)
//    |> Array.collect id

//x
//|> Array.groupBy snd
//|> Array.map (Tuple.mapSnd (Array.distinct >> Array.map fst))
//|> Array.filter (snd >> (fun a -> a.Length > 1))
//|> Array.sortByDescending (snd >> _.Length)
//|> Tuple.dupMapFst (fun a -> a |> Array.map fst |> Array.fold smartNl "")
//|> fun (tagList, a) ->
//    let fullList =
//        a
//        |> Array.map (fun (tag, files) ->
//            tag
//            + "\n"
//            + (files |> Array.map (enclose "\t\t" "") |> Array.fold smartNl "")
//            + "\n\n")
//        |> Array.fold smartNl ""

//    tagList + "\n\n" + fullList
//|> setFst @"F:\Skyrim SE\MO2\mods\DM-Dynamic-Armors\known smp tags.txt"
//|> File.WriteAllText


// params

let contents =
    "F:\Skyrim SE\MO2\mods\Fantasia Outfit\meshes\SunJeongStuff\Fantasia\cuirass.xml"
    |> File.ReadAllText

let log = printfn "%s"
let hiQualityBody = true

// func
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


//|> TextCopy.ClipboardService.SetText
