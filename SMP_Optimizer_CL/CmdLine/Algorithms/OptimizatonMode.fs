module CmdLine.Implementations.OptimizationMode

open DMLib
open DMLib.String
open System.Text.RegularExpressions
open DMLib.IO
open System.IO
open CmdLine

type MediumBodyQuality =
    | TriangleBody
    | VertexBody

[<Literal>]
let Triangle = "per-triangle-shape"

[<Literal>]
let Vertex = "per-vertex-shape"

/// List of known physics bodies that don't follow the Virtual<n> convention.
let private knownBodies =
    Paths.physicsBodiesFile |> File.ReadAllLines |> Array.map dupFst |> Map.ofArray

let private (|IsPhysicsBody|_|) =
    function
    | StartsWithIC' "Virtual" body -> Some body
    | u -> knownBodies |> Map.tryFind u

let replaceAll from ``to`` log (filename: string, contents) =
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

let setMediumQuality bodyQuality log (filename: string, contents) =
    log "Changing to medium quality"
    let rx = Regex("(?s)<per-vertex-shape name=\"(.*?)\">.*?<\/per-vertex-shape>")

    // Sets some collision body to high quality
    let setAsTri collisionBody contents =
        let newQuality = collisionBody |> replace Vertex Triangle

        contents |> replace collisionBody newQuality

    let displayArmor = sprintf "armor (%s)"
    let displayBody = sprintf "physics body (%s)"
    let logUnchanged = sprintf "Found %s. Leave as vertex collision."
    let logChanged = sprintf "Found %s. Change collision to triangle."
    let id' _ = id

    let logBody, changeBody, logArmor, changeArmor =
        match bodyQuality with
        | TriangleBody -> displayBody >> logChanged, setAsTri, displayArmor >> logUnchanged, id'
        | VertexBody -> displayBody >> logUnchanged, id', displayArmor >> logChanged, setAsTri

    let lowQualityContents = contents |> replace Triangle Vertex

    let optimized =
        rx.Matches(lowQualityContents)
        |> Seq.map (fun m ->
            match m.Groups[1].Value with
            // Ground is always left as vertex
            | StartsWithIC' "VirtualGround" g ->
                g |> logUnchanged |> log
                id
            // Physics bodies
            | IsPhysicsBody body ->
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
