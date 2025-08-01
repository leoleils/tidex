# Shary NPC Mod - Quick Start Guide

## Project Structure
```
tidex/
├── SharyMod/              # Content Patcher mod (character definition)
│   ├── assets/            # Character sprites and portraits
│   ├── i18n/              # Dialogue files
│   ├── content.json       # Character data and assets
│   ├── manifest.json      # Mod metadata
│   └── README.md          # Mod description and usage
│
├── SharySMAPI/            # SMAPI mod (advanced features)
│   ├── Services/          # Service classes (memory, Qwen integration, etc.)
│   ├── Models/            # Data models
│   ├── ModEntry.cs        # Main mod class
│   ├── ModConfig.cs       # Configuration model
│   ├── manifest.json      # Mod metadata
│   ├── config.json        # Configuration file
│   ├── SharySMAPI.csproj  # Project file
│   └── README.md          # Technical documentation
│
├── README.md              # English main README
├── README_zh.md           # Chinese main README
└── package.sh             # Packaging script
```

## Installation

1. Make sure you have SMAPI and Content Patcher installed
2. Download or clone this repository
3. Copy both `SharyMod` and `SharySMAPI` folders to your `StardewValley/Mods` directory
4. Run the game using SMAPI

## Configuration

To enable the Qwen AI integration:

1. Get an API key from [DashScope](https://dashscope.aliyun.com/)
2. Enable the qwen3-235b-a22b-instruct-2507 model for your account
3. Edit `StardewValley/Mods/SharySMAPI/config.json`:
   ```json
   {
     "QwenApiKey": "your-api-key-here"
   }
   ```

## Features Overview

### Content Patcher Mod (SharyMod)
- Character definition and basic behaviors
- Schedule and dialogue
- Festival integration
- Visual assets

### SMAPI Mod (SharySMAPI)
- Qwen AI integration for dynamic conversations
- Memory system for context awareness
- Resource collection based on friendship
- Multi-turn dialogue support
- Interaction logging

## Development

To build from source:

1. Open `tidex.sln` in Visual Studio or VS Code
2. Set the `StardewPath` environment variable to your game's installation directory
3. Build the solution

## Documentation

- `SharyMod/README.md` - Character features and installation
- `SharySMAPI/README.md` - Technical implementation details
- `SharySMAPI/Qwen-Integration-Guide.md` - Qwen integration specifics
- `SharySMAPI/Memory-System-Guide.md` - Memory system implementation
- `SharySMAPI/Resource-Collection-Guide.md` - Resource collection implementation
- `Shary-Complete-Guide.md` - Complete project overview