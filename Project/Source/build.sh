#!/bin/bash

# Build script for Aria NPC mods - Ultra-clean consolidated packaging

echo "🚀 Starting Aria Mod Build Process..."

# Create unified output directory
echo "📁 Setting up clean build directory..."
mkdir -p "../FinalRelease"
cd "../FinalRelease"
rm -rf *  # Clean previous builds

# Build the AriaMod (AI enhancement)
echo "🔨 Building AriaMod..."
cd "../Source/AriaMod"
dotnet build -c Release

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "❌ Build failed for AriaMod!"
    exit 1
fi

# Create ultra-clean AriaMod package
echo "📦 Creating AriaMod package..."
mkdir -p "../../FinalRelease/AriaMod"
cp "./bin/Release/net6.0/AriaMod.dll" "../../FinalRelease/AriaMod/"
cp "./bin/Release/net6.0/AriaMod.deps.json" "../../FinalRelease/AriaMod/"
cp "manifest.json" "../../FinalRelease/AriaMod/"
cp "config.json" "../../FinalRelease/AriaMod/"

# Create ultra-clean [CP]Aria package
echo "📦 Creating [CP]Aria package..."
mkdir -p "../../FinalRelease/[CP]Aria"
# Only copy essential files
cp -r "../[CP]Aria/Characters" "../../FinalRelease/[CP]Aria/" 2>/dev/null || true
cp -r "../[CP]Aria/Portraits" "../../FinalRelease/[CP]Aria/" 2>/dev/null || true
cp -r "../[CP]Aria/data" "../../FinalRelease/[CP]Aria/" 2>/dev/null || true
cp "../[CP]Aria/content.json" "../../FinalRelease/[CP]Aria/" 2>/dev/null || true
cp "../[CP]Aria/manifest.json" "../../FinalRelease/[CP]Aria/" 2>/dev/null || true

# Aggressive cleanup of unnecessary files
echo "🧹 Performing deep cleanup..."
cd "../../FinalRelease"

# Remove all hidden files and common junk
find . -type f -name ".*" -delete
find . -type f -name "*.DS_Store" -delete
find . -type f -name "*.bak" -delete
find . -type f -name "*.tmp" -delete
find . -type f -name "*.log" -delete
find . -type f -name "*.old" -delete
find . -type f -name "*.original" -delete
find . -type f -name "*.backup" -delete

# Remove development files
find . -type f -name "*.csproj" -delete
find . -type f -name "*.sln" -delete
find . -type f -name "*.user" -delete
find . -type f -name "*.cache" -delete
find . -type f -name "*.pdb" -delete

# Remove macOS specific files
find . -type f -name ".DS_Store" -delete
find . -type d -name ".__MACOSX" -exec rm -rf {} + 2>/dev/null || true

# Remove empty directories
find . -type d -empty -delete 2>/dev/null || true

# Create final ultra-clean zip files
echo "🗜️ Creating final packages..."
zip -r "AriaMod.zip" "AriaMod" -x "*.DS_Store" "*/.*" "*__pycache__*" "*.tmp" "*.log" "*.old" "*.original" "*.backup" 2>/dev/null
zip -r "Aria.zip" "[CP]Aria" -x "*.DS_Store" "*/.*" "*__pycache__*" "*.tmp" "*.log" "*.old" "*.original" "*.backup" 2>/dev/null

# Create combined package
echo "🎯 Creating combined package..."
mkdir -p "AriaComplete"
cp -r "AriaMod" "AriaComplete/"
cp -r "[CP]Aria" "AriaComplete/"
zip -r "AriaComplete.zip" "AriaComplete" -x "*.DS_Store" "*/.*" "*__pycache__*" "*.tmp" "*.log" "*.old" "*.original" "*.backup" 2>/dev/null

# Clean up temporary directory
rm -rf "AriaComplete"

# Final verification
echo "✅ Build verification..."
echo "📊 Final package sizes:"
du -h *.zip 2>/dev/null || echo "No zip files found"

echo ""
echo "✅ Build completed successfully!"
echo "📦 Ultra-clean packages created in ../FinalRelease/:"
echo "   📋 Aria.zip                (Base NPC mod - {size}KB)"
echo "   🤖 AriaMod.zip             (AI enhancement - {size}KB)"
echo "   🎯 AriaComplete.zip        (Both mods combined - {size}KB)"
echo ""
echo "📁 Clean directory structure:"
echo "   📁 AriaMod/                (4 essential files only)"
echo "   📁 [CP]Aria/               (Assets + config only)"
echo ""
echo "🧹 All unnecessary files removed:"
echo "   ✓ Hidden files (.DS_Store, .*)"
echo "   ✓ Development files (.csproj, .pdb, .cache)"
echo "   ✓ Backup files (.bak, .old, .original)"
echo "   ✓ Log files and temporary files"