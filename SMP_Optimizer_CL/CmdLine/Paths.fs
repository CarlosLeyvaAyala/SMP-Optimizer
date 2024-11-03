module CmdLine.Paths

open DMLib
open DMLib.String
open System.Text.RegularExpressions
open DMLib.IO

let getAppDir () =
    System.Reflection.Assembly.GetExecutingAssembly().Location
    |> System.IO.Path.GetDirectoryName

/// Path to the file used to get where output files will be written to when app was called from *.bat file.
let batOutDirFile = getAppDir () |> Path.combine2' "output_dir.txt"

/// Path to the file with known tags for physics bodies for armors.
let physicsBodiesFile = getAppDir () |> Path.combine2' "Physics_Bodies.txt"

/// Path to the help file.
let helpFile = getAppDir () |> Path.combine2' "Help.html"
