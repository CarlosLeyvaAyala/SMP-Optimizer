module CmdLine.Core

open DMLib.String
open DMLib.IO

/// Translate command line parameters to app settings.
let processArgs args =
    let a =
        args
        |> Array.map (function
            | StartsWith' "-" flag -> flag |> toLower
            | a -> a) // Normalize flags

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

    let log = translateFlag "-v" Verbose Normal
    let testingMode = translateFlag "-t" Testing DoWrite

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

    let outputA = a |> Array.except allFlags

    let output =
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

    let input =
        outputA
        |> Array.except
            [| "-o"
               match output with
               | Overwrite
               | Invalid -> ""
               | ToDir d -> d.toStr
               | ToZip z -> z.toStr |]

    { logging = log
      testing = testingMode
      optimization = optimization
      output = output
      input = input }
