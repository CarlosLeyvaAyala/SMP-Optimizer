[<AutoOpen>]
module CmdLine.Globals

open DMLib.String
open DMLib.IO
open System.IO
open DMLib

[<Literal>]
let Triangle = "per-triangle-shape"

[<Literal>]
let Vertex = "per-vertex-shape"

/// List of known physics bodies that don't follow the Virtual<n> convention.
let private knownBodies =
    Paths.physicsBodiesFile ()
    |> File.ReadAllLines
    |> Array.map TupleCommon.dupFst
    |> Map.ofArray

let (|IsPhysicsBody|_|) =
    function
    | StartsWithIC' "Virtual" body -> Some body
    | StartsWithIC' "Box" body -> Some body
    | ContainsIC' "Collision" body -> Some body
    | u -> knownBodies |> Map.tryFind u

let (|IsVirtualGround|_|) =
    function
    | StartsWithIC' "VirtualGround" g -> Some g
    | _ -> None

let (|IsSmpConfigFile|_|) =
    function
    | FileExists f & (IsExtension "xml" _) & ContainsIC @"\meshes\" -> Some f
    | _ -> None

/// Finds files SMP xml files in some folder
let getFolderFiles path =
    Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories)
    |> Array.choose (|IsSmpConfigFile|_|)

/// Does something according to the input
let mainLoop onDir onFile onInvalidXml onInvalidFile input =
    match input with
    | IsDir d -> onDir d
    | IsSmpConfigFile f -> onFile f
    | NotContainsIC' @"\meshes\" fn -> onInvalidXml fn
    | x -> onInvalidFile $" is not an xml file or a folder." x
