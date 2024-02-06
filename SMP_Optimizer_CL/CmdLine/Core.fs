module CmdLine.Core

open DMLib.String
open DMLib.IO

/// Translate command line parameters to app settings.
let processArgs args =
    let a =
        args
        |> Array.choose (function
            | StartsWith' Flags.startChar flag -> flag |> toLower |> Some // Normalize flags
            | IsWhiteSpaceStr -> None // Remove empty arguments
            | a -> Some a)

    let allFlags =
        a
        |> Array.choose (function
            | Flags.output -> None
            | StartsWith' Flags.startChar f -> Some f
            | _ -> None)
        |> Array.distinct

    let translateFlag flag exists notExists =
        match allFlags |> Array.tryFind (fun s -> s = flag) with
        | Some _ -> exists
        | None -> notExists

    let log = LogMode.get translateFlag
    let testingMode = TestingMode.get translateFlag
    let optimization = OptimizationMode.get translateFlag

    let outputA = a |> Array.except allFlags

    let output =
        match
            outputA
            |> Array.tryFindIndex (fun s -> s = Flags.output)
            |> Option.map (fun i ->
                let i' = i + 1
                if i' > outputA.Length - 1 then None else Some outputA[i'])
            |> Option.flatten
        with
        | Some x ->
            match x with
            | IsExtension ZipFileName.ext fn -> ToZip <| ZipFileName.ofStr fn
            | HasExtension _ -> failwith $"\"{x}\" is not a valid output folder/file."
            | IsEmptyStr -> Overwrite
            | dir -> ToDir <| DirName.ofStr dir
        | None -> Overwrite

    let input =
        outputA
        |> Array.except
            [| Flags.output
               match output with
               | Overwrite -> ""
               | ToDir d -> d.toStr
               | ToZip z -> z.toStr |] // All remaining stuff, except the output

    { logging = log
      testing = testingMode
      optimization = optimization
      output = output
      input = input }
