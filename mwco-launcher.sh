#!/bin/bash

# MWCO Launcher Script
# Quick way to run the launcher

cd "$(dirname "$0")"

echo "Starting MWCO Launcher..."
dotnet run --project MWCO.Launcher/MWCO.Launcher.csproj
