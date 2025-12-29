{
  deps,
  pkgs,
  dotnet-sdk,
  nix-gitignore,
  dotnet-runtime,
  buildDotnetModule,
  version,
}:
let
  name = "Oceanbox.GeoJson";
in
buildDotnetModule {
  pname = name;
  inherit dotnet-sdk dotnet-runtime version;

  src = nix-gitignore.gitignoreSource [ ] ../.;
  projectFile = "src/Oceanbox.GeoJson.fsproj";
  dotnetRestoreFlags = "--force-evaluate";

  nugetDeps = deps {
    inherit pkgs name;
    lockfiles = [ ./packages.lock.json ];
  };

  packNupkg = true;
  doCheck = false;
}
