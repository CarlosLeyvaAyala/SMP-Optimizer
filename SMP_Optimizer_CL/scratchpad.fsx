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

open System.IO
open DMLib.String
open DMLib.IO
open CmdLine
open CmdLine.Core

// Get output file name
Directory.GetFiles(@"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer", "*.xml", SearchOption.AllDirectories)
|> Array.choose (fun xxx ->
    match xxx with
    | Regex @".*\\(meshes\\.*)" l -> Some l[0]
    | _ -> None)


let args =
    [| ""
       @"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer"
       "-t"
       "-l0"
       @"F:\Skyrim SE\MO2\mods\[Christine] Companion Moon"
       "-l2"
       "-o"
       @"F:\Skyrim SE\MO2\mods\Optimized SMP.zip"
       "-t" |]
    |> processArgs

//let a =
//    [| "-V"
//       @"F:\Skyrim SE\MO2\mods\[Christine] Flirty Summer"
//       "-t"
//       "-o"
//       @"F:\Skyrim SE\MO2\mods"
//       "-l0"
//       "-l2" |]
