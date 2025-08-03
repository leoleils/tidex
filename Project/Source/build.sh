#!/bin/bash

# Define base directory
BASE_DIR="/Users/chenleiyu/Desktop/tidex/Project/Source"

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf "$BASE_DIR/zip/[CP]Aria.zip"

# Create temporary directory for packaging
TEMP_DIR="$BASE_DIR/temp_packaging"
rm -rf "$TEMP_DIR"
mkdir -p "$TEMP_DIR/[CP]Aria/data"
mkdir -p "$TEMP_DIR/[CP]Aria/Characters"
mkdir -p "$TEMP_DIR/[CP]Aria/Portraits"

# Create necessary directories
echo "Creating directories..."
mkdir -p "$BASE_DIR/zip"

# Copy assets
echo "Copying assets..."
cp -r "$BASE_DIR/Characters/"* "$TEMP_DIR/[CP]Aria/Characters/"
cp -r "$BASE_DIR/Portraits/"* "$TEMP_DIR/[CP]Aria/Portraits/"
cp -r "$BASE_DIR/data/"* "$TEMP_DIR/[CP]Aria/data/"
cp "$BASE_DIR/manifest.json" "$TEMP_DIR/[CP]Aria/"
cp "$BASE_DIR/content.json" "$TEMP_DIR/[CP]Aria/"

# Create zip package
echo "Creating zip package..."
cd "$TEMP_DIR"
zip -r "$BASE_DIR/zip/[CP]Aria.zip" "[CP]Aria"

# Clean up temporary directory
rm -rf "$TEMP_DIR"

echo "Build completed successfully!"
echo "The packaged mod is located in the zip/ directory as [CP]Aria.zip"
echo "To install, extract the zip file to your Stardew Valley Mods directory"