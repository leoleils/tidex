# Custom NPC Mod for Stardew Valley

This project contains all the source code and assets needed to build a custom NPC mod for Stardew Valley.

## Project Structure

```
Project/
├── Source/                 # Source code and assets
│   ├── Assets/             # Generic assets folder
│   ├── Characters/         # NPC character sprites (walking animations)
│   ├── Dialogues/          # JSON files containing NPC dialogues
│   ├── Portraits/          # NPC portrait files for Content Patcher
│   ├── build.sh            # Build script
│   ├── content.json        # Content Patcher configuration for main mod
│   ├── CustomNPC.csproj    # C# project file
│   ├── manifest.json       # Main mod manifest
│   ├── ModEntry.cs         # Main mod code
│   ├── README.md           # Developer documentation
│   └── LICENSE.txt         # License information
│
├── Packaged/               # Packaged mod ready for distribution
│   ├── CustomNPC/          # Main mod folder
│   └── CustomNPC_Portraits/ # Content Patcher content pack for portraits
│
└── README.md              # This file
```

## Building the Mod

1. Navigate to the `Source` directory
2. Run `./build.sh` to compile the mod
3. The packaged mod will be in the `Packaged` directory

## Installation

1. Install SMAPI and Content Patcher
2. Extract the contents of the `Packaged` directory to your Stardew Valley Mods folder
3. You should have two folders: `CustomNPC` and `CustomNPC_Portraits`
4. Run the game through SMAPI

## Features

- Custom NPC with animated sprites
- Multiple dialogue options
- Interactive dialogue system
- Emotional portrait system using Content Patcher
- NPC movement around the farm