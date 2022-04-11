open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext()

let srcPath  = Path.getFullName "src"
let testPath = Path.getFullName "test"
let libPath  = Some srcPath

let deployPath  = Path.getFullName "deploy"
let packPath    = Path.getFullName "packages"
let versionFile = Path.getFullName ".version"

Target.create "Clean" (fun _ -> Shell.cleanDir deployPath)

Target.create "InstallClient" (fun _ ->
    run npm "install" "."
    run dotnet "tool restore" "."
)

Target.create "Bundle" (fun _ ->
    run dotnet $"publish -c Release -o \"{deployPath}\"" srcPath
)

Target.create "BundleDebug" (fun _ ->
    run dotnet $"publish -c Debug -o \"{deployPath}\"" srcPath
)

Target.create "Pack" (fun _ ->
    match libPath with
    | Some p -> run dotnet $"pack -c Release -o \"{packPath}\"" p
    | None -> ()
)


Target.create "Format" (fun _ ->
    run dotnet "fantomas . -r" "src"
)

Target.create "Test" (fun _ ->
    if System.IO.Directory.Exists testPath then
        run dotnet "run" testPath
    else ()
)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean"
        ==> "InstallClient"
        ==> "Bundle"

    "Clean"
        ==> "BundleDebug"

    "Clean"
        ==> "Test"


    "Clean"
        ==> "Pack"
]

[<EntryPoint>]
let main args = runOrDefault args
