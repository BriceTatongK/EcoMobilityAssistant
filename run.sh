#!/bin/bash

# -------------------------------
# run-cli.sh
# -------------------------------
# Build and run the EcoMob.Cli project
# Usage: ./run-cli.sh
# -------------------------------

# Stop on first error
set -e

# Navigate to the solution root (adjust if needed)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.." || exit 1

# Optional: Clean previous builds
dotnet clean ./src/EcoMob.Cli/EcoMob.Cli.csproj

# Build the CLI project
dotnet build ./src/EcoMob.Cli/EcoMob.Cli.csproj -c Release

# Run the CLI
dotnet run --project ./src/EcoMob.Cli/EcoMob.Cli.csproj --configuration Release