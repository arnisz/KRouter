#!/bin/bash
set -e

echo "Building KRouter CLI..."

dotnet restore src/Cli
dotnet build src/Cli -c Release

dotnet publish src/Cli -c Release -r linux-x64 --self-contained false /p:PublishSingleFile=true -o dist/linux
dotnet publish src/Cli -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true -o dist/windows
dotnet publish src/Cli -c Release -r osx-x64 --self-contained false /p:PublishSingleFile=true -o dist/macos

echo ""
echo "Build complete! Executables created in:"
echo "  Linux:   dist/linux/krouter"
echo "  Windows: dist/windows/krouter.exe"
echo "  macOS:   dist/macos/krouter"
echo ""
echo "To run immediately:"
echo "  dotnet run --project src/Cli -- route --in sample.dsn --out sample.ses"
echo ""
echo "Or use the compiled executable:"
echo "  ./dist/linux/krouter route --in sample.dsn --out sample.ses"
