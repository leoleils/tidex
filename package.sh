#!/bin/bash
# Package script for Shary NPC mod

echo "Packaging Shary NPC mod..."

# Create distribution directory
mkdir -p dist

# Package Content Patcher mod
echo "Packaging Content Patcher mod..."
cd SharyMod
zip -r ../dist/Shary-NPC-Mod.zip ./*
cd ..

# Package SMAPI mod
echo "Packaging SMAPI mod..."
cd SharySMAPI
# Build the project
dotnet build --configuration Release
# Create a simple zip with the DLL and manifest
cd bin/Release/net6.0
zip -r ../../../../dist/SharySMAPI.zip ./*
cd ../../..
cp manifest.json ../dist/
cp config.json ../dist/ 2>/dev/null || true
cd ..

echo "Packaging complete!"
echo "Distribution files are in the 'dist' folder."