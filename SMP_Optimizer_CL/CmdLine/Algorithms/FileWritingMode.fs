module CmdLine.Implementations.FileWritingMode

open System.IO
open DMLib.String
open DMLib.IO
open DMLib.IO.Path

/// Takes a full path and shortens it to the "Meshes\..." path
let pathFromMeshes outputPath originalName =
    match originalName with
    | Regex @"(?i).*\\(meshes\\.*)" l -> combine2 outputPath l[0]
    | _ -> failwith $"{originalName} is not an armor file. This entry should have been filtered by the main function."

/// Writes a file to a different dir. Used to create the zip file and the manager overwrite.
let writeToAnotherDir transformFilename outputPath contents filename =
    let f = filename |> transformFilename outputPath
    getDir f |> forceDir
    File.WriteAllText(f, contents)
    f

/// Writes an output in a form that a mod manager can recognize as overwrite.
let writeForModManager = writeToAnotherDir pathFromMeshes
