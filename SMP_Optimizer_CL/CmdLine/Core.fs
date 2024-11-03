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
    let caller = CalledFrom.get translateFlag
    let help = ShowHelp.get translateFlag

    let notFlags = a |> Array.except allFlags
    let output = FileWritingMode.get notFlags

    let input =
        notFlags
        |> Array.except
            [| Flags.output
               match output with
               | Overwrite -> ""
               | ToDir d -> d.toStr
               | ToZip(_, z) -> z.toStr |] // All remaining stuff, except the output

    { logging = log
      testing = testingMode
      optimization = optimization
      output = output
      calledFrom = caller
      showHelp = help
      input = input }
