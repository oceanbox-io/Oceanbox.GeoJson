# Install just: https://github.com/casey/just
set dotenv-load

src := "src"
test := "tests"
dist := "dist"
pack := "packages"

# Default recipe - show available commands
default:
    @just --list

# Clean build artifacts
clean:
    rm -rf {{dist}}

# Build production bundle
bundle: clean
    dotnet publish -c Release -o {{dist}} {{src}}

# Build debug bundle
bundle-debug: clean
    dotnet publish -c Debug -o {{dist}} {{src}}

# Create NuGet package
pack: clean
    dotnet pack -c Release -o {{pack}} {{src}}

# Format code with Fantomas
format:
    fantomas {{src}} -r

# Run tests
test: clean
    #!/usr/bin/env bash
    if [ -d "{{test}}" ]; then
        dotnet run {{test}}
    fi

# Run (creates package)
run: pack