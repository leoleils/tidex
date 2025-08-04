#!/bin/bash

# Build script for Aria NPC mods

# Create output directory
mkdir -p "./zip"

# Build the AriaMod (AI enhancement)
echo "Building AriaMod..."
cd "./AriaMod"
dotnet build -c Release

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed for AriaMod!"
    exit 1
fi

# Create the AriaMod package
echo "Creating AriaMod package..."

# Create directory structure
mkdir -p "../zip/AriaMod"

# Copy built files
cp -r "./bin/Release/net6.0/"* "../zip/AriaMod/"

# Copy manifest and config files
cp "manifest.json" "../zip/AriaMod/"
cp "config.json" "../zip/AriaMod/"

# Go back to Source directory
cd ".."

# Copy the base [CP]Aria mod
echo "Copying [CP]Aria base mod..."
cp -r "./[CP]Aria" "../zip/"

# Create the final zip files
cd "../zip"
if [ -d "AriaMod" ]; then
    zip -r "AriaMod.zip" "AriaMod"
fi

if [ -d "[CP]Aria" ]; then
    zip -r "Aria.zip" "[CP]Aria"
fi

echo "Build completed successfully!"
echo "Created packages:"
if [ -f "AriaMod.zip" ]; then
    echo "  - zip/AriaMod.zip (AI enhancement mod)"
fi
if [ -f "Aria.zip" ]; then
    echo "  - zip/Aria.zip (Base NPC mod)"
fi