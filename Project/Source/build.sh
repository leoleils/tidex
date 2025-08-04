#!/bin/bash

# Build script for Aria AI mod

# Navigate to the mod directory
cd "$(dirname "$0")/AriaMod"

# Set the mod name
MOD_NAME="AriaMod"

# Create output directory
mkdir -p "../zip"

# Build the project
echo "Building $MOD_NAME..."
dotnet build -c Release

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

# Create the mod package
echo "Creating mod package..."

# Create directory structure
mkdir -p "../zip/$MOD_NAME"

# Copy built files (fixing the path issue)
if [ -d "./bin/Release/net6.0/" ]; then
    cp -r "./bin/Release/net6.0/"* "../zip/$MOD_NAME/"
else
    echo "Error: Could not find built files in ./bin/Release/net6.0/"
    exit 1
fi

# Copy content files
cp -r "./Characters" "../zip/$MOD_NAME/"
cp -r "./data" "../zip/$MOD_NAME/"
cp -r "./Portraits" "../zip/$MOD_NAME/"

# Copy manifest and content files
cp "manifest.json" "../zip/$MOD_NAME/"
cp "content.json" "../zip/$MOD_NAME/"
cp "config.json" "../zip/$MOD_NAME/"

# Create the final zip file
cd "../zip"
zip -r "${MOD_NAME}.zip" "$MOD_NAME"

echo "Build completed successfully! Mod package created: zip/${MOD_NAME}.zip"