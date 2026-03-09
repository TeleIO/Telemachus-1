{
  description = "Telemachus-1 — KSP telemetry mod";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { nixpkgs, flake-utils, ... }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
      in {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            mono         # MSBuild + Mono runtime (.NET Framework 4.7.2)
            nuget        # NuGet package manager
            dotnet-sdk_8 # language server / IDE support
            curl
            unzip
            p7zip
            bun          # frontend runtime & package manager
            gitlint      # conventional commit linting
            git-cliff    # changelog generation
          ];

          shellHook = ''
            if [ -d .git ]; then
              mkdir -p .git/hooks
              cat > .git/hooks/commit-msg << 'HOOK'
#!/usr/bin/env bash
gitlint --msg-filename "$1"
HOOK
              chmod +x .git/hooks/commit-msg
            fi
            echo "Build:     msbuild Telemachus.sln /p:Configuration=Release"
            echo "Frontend: cd frontend && bun install && bun run dev"
            echo "Changelog: git cliff -o CHANGELOG.md"
          '';
        };
      });
}
