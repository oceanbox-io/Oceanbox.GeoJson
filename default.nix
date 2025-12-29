{
  sources ? import ./nix,
  system ? builtins.currentSystem,
  pkgs ? import sources.nixpkgs { inherit system; },
  nix-utils ? import sources.nix-utils { },
}:
let
  version =
    let
      clean = pkgs.lib.removeSuffix "\n";
      version = builtins.readFile ./VERSION;
    in
    clean version;

  dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
  dotnet-runtime = pkgs.dotnetCorePackages.runtime_10_0;

  geojson = pkgs.callPackage ./src {
    inherit (nix-utils.output.lib.nuget) deps;
    inherit
      dotnet-sdk
      dotnet-runtime
      version
      ;
  };

  packages = {
    inherit geojson;
  };

in
{
  default = geojson;

  inherit
    packages
    ;

  shell = pkgs.mkShell {
    packages = with pkgs; [
      just
      bun
      npins
      fantomas
      fsautocomplete
      dotnet-sdk
    ];

    NPINS_DIRECTORY = "nix";
  };
}
