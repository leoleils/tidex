# Custom NPC Mod for Stardew Valley

This mod adds a custom NPC named Aria to Stardew Valley who can interact with the player through dialogue.

## Features
- Adds a new NPC character to your farm
- Multiple dialogue options that change randomly
- Interactive dialogue system using the action button
- NPC can move around the farm autonomously
- Multiple emotional portraits that change based on dialogue content
- Animated character sprite with walking animations for all 4 directions
- JSON-based dialogue configuration for easy customization
- Clean, organized code structure

## Installation
1. Install [SMAPI](https://smapi.io/) following the official instructions
2. Download this mod and extract the folder into your `StardewValley/Mods` directory
3. Run the game using SMAPI

## Customization

### Dialogues
You can easily customize the NPC's dialogues by editing the `Dialogues/Aria.json` file.

To add new dialogue options, simply add new strings to the "dialogues" array:
```json
{
  "npcName": "Aria",
  "dialogues": [
    "Your new dialogue line here.",
    "Another dialogue option."
  ]
}
```

Each dialogue string can contain multiple parts separated by `#$b#` which creates a dialogue break in the game.

### Character Sprite Animation
The NPC has a character sprite with walking animations for all 4 directions:
- Down (0): First row of the sprite sheet
- Right (1): Second row of the sprite sheet
- Up (2): Third row of the sprite sheet
- Left (3): Fourth row of the sprite sheet

Each row should contain 4 frames of animation (16x32 pixels each), for a total sprite sheet size of 64x128 pixels.

To customize the character sprite, replace the image at `Characters/Aria.png` with your own sprite sheet.

### Portraits
The NPC has 7 different emotional portraits that change based on the content of the dialogue:
- Default: Neutral expression
- Happy: When dialogue contains words like "happy", "love", "perfect", or "wonderful"
- Angry: When dialogue contains words like "angry", "hate", "terrible", or "awful"
- Like: When dialogue contains words like "like", "enjoy", or "pleased"
- Laughing: When dialogue contains words like "laugh", "funny", or "hilarious"
- Sad: When dialogue contains words like "sad", "upset", or "depressed"
- Crying: When dialogue contains words like "cry", "tears", or "crying"

To customize any of these portraits, replace the corresponding image in the `Portraits` folder:
- `Aria_Default.png`
- `Aria_Happy.png`
- `Aria_Angry.png`
- `Aria_Like.png`
- `Aria_Laughing.png`
- `Aria_Sad.png`
- `Aria_Crying.png`

All portrait images should be 64x64 pixels.

### Movement
The NPC will automatically move around the farm at regular intervals. The movement pattern is randomized but will respect farm boundaries and avoid obstacles.

## Development
This mod is written in C# using the Stardew Modding API. To build it yourself:

1. Make sure you have the .NET 5 SDK installed
2. Update the `GamePath` in `CustomNPC.csproj` to point to your Stardew Valley installation
3. Run `dotnet build` in the project directory

## Credits
- Uses the [ModBuildConfig](https://github.com/Pathoschild/Stardew.ModBuildConfig) package for streamlined development
- Inspired by the official Stardew Valley modding documentation

## License
This mod is released under the MIT License. Feel free to use, modify, and distribute it as you see fit.