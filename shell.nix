{
  pkgs ? import <nixpkgs> { },
}:
let
  dotnet-sdk = pkgs.dotnet-sdk_9;
in
pkgs.mkShell {
  buildInputs = [
    dotnet-sdk
    pkgs.bun
  ];

  DOTNET_ROOT = "${dotnet-sdk}/share/dotnet";
}
