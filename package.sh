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
dotnet build --configuration Release
cd ..

echo "Packaging complete!"
echo "Distribution files are in the 'dist' folder."