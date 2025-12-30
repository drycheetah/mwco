#!/bin/bash

# MWCO Installation Script
# Copies mod files to My Winter Car BepInEx folder

set -e

echo "======================================"
echo "  MWCO - My Winter Car Online"
echo "  Mod Installation Script"
echo "======================================"
echo

# Check if game directory exists
GAME_DIR="$HOME/.local/share/Steam/steamapps/common/My Winter Car"
if [ ! -d "$GAME_DIR" ]; then
    echo "Error: My Winter Car not found at: $GAME_DIR"
    echo "Please install My Winter Car from Steam first."
    exit 1
fi

echo "Found My Winter Car at: $GAME_DIR"

# Check if BepInEx is installed
BEPINEX_DIR="$GAME_DIR/BepInEx"
if [ ! -d "$BEPINEX_DIR" ]; then
    echo
    echo "Error: BepInEx not found!"
    echo "Please install BepInEx 5.x first:"
    echo "  1. Download from: https://github.com/BepInEx/BepInEx/releases"
    echo "  2. Extract to: $GAME_DIR"
    echo "  3. Run the game once to generate folders"
    echo "  4. Run this script again"
    exit 1
fi

echo "Found BepInEx at: $BEPINEX_DIR"

# Create plugins directory if it doesn't exist
PLUGINS_DIR="$BEPINEX_DIR/plugins"
mkdir -p "$PLUGINS_DIR"

# Build the project
echo
echo "Building MWCO..."
dotnet build MWCO.slnx --configuration Release

# Copy DLLs
echo
echo "Installing MWCO mod files..."

cp -v "MWCO.Client/bin/Release/netstandard2.1/MWCO.Client.dll" "$PLUGINS_DIR/"
cp -v "MWCO.Shared/bin/Release/netstandard2.1/MWCO.Shared.dll" "$PLUGINS_DIR/"

# Copy Harmony if it exists
if [ -f "MWCO.Client/bin/Release/netstandard2.1/0Harmony.dll" ]; then
    cp -v "MWCO.Client/bin/Release/netstandard2.1/0Harmony.dll" "$PLUGINS_DIR/"
fi

echo
echo "======================================"
echo "  MWCO Mod Installed Successfully!"
echo "======================================"
echo
echo "Installed files:"
ls -lh "$PLUGINS_DIR"/MWCO*.dll "$PLUGINS_DIR"/0Harmony.dll 2>/dev/null || true
echo
echo "Next steps:"
echo "  1. Start the MWCO server: dotnet run --project MWCO.Server/MWCO.Server/MWCO.Server.csproj"
echo "  2. Launch My Winter Car"
echo "  3. Press F10 in-game to open MWCO menu"
echo "  4. Connect to server (default: 127.0.0.1:1999)"
echo
echo "For more information, see INSTALLATION.md"
