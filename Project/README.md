# Aria NPC Mod for Stardew Valley

This project contains two separate mods for Stardew Valley:

1. **[CP]Aria** - The base NPC mod that adds Aria to the game
2. **AriaMod** - The AI enhancement mod that provides dynamic dialogue for Aria

## [CP]Aria - Base NPC Mod

This mod adds a new NPC named Aria to Stardew Valley. Aria is a town librarian who can be found in various locations around Pelican Town.

### Features

- Adds Aria, a unique NPC character to the game
- Custom sprite and portrait artwork
- Daily dialogue lines for different days of the week
- Location schedule for all seasons
- Integration with Content Patcher framework

## AriaMod - AI Dialogue Enhancement

This mod enhances the Aria NPC with AI-powered dialogue, allowing for dynamic conversations based on context.

### Features

- AI-powered dialogue system using DashScope API
- Context-aware responses based on time, season, location, and conversation history
- Aria actively seeks out the player in the morning (8:30am-9:30am)
- Location-based interactions when the player enters areas where Aria is present
- Up to 3 interactions per day to prevent spam
- Streaming or direct text response modes

## Installation

1. Install [SMAPI](https://smapi.io/) - the mod loader for Stardew Valley
2. Install [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) by Pathoschild
3. Download both mods and unzip them into your Stardew Valley Mods folder
4. Run the game using SMAPI

## Character Information

- **Name**: Aria
- **Occupation**: Town Librarian
- **Birthday**: Spring 15th
- **Home Region**: Town
- **Gender**: Female
- **Age**: Adult
- **Personality**: Neutral manner, social anxiety, and optimism

## Schedule

Aria follows a daily schedule that varies by season:

- **Spring**: Starts at the Archaeology House, then visits various locations in town, beach, and forest
- **Summer**: Similar pattern with different dialogue and locations
- **Fall**: Different routine with unique dialogue
- **Winter**: Indoor-focused schedule due to weather
- **Festival Days**: Special schedule for events like the Egg Festival

## Dialogue

Aria has unique dialogue for:
- Initial introduction
- Each day of the week
- Special interactions when the player is nearby
- Location-specific interactions

Sample dialogue includes topics about:
- Library work and book cataloging
- Gardening and farming techniques
- Local folklore and legends
- Seasonal activities and observations

## AI Dialogue System

This mod includes an AI dialogue system that allows Aria to have more dynamic conversations with the player. The system uses a large language model to generate contextual responses based on:

- Time of day
- Season
- Player name
- Location
- Previous conversation history (limited)
- Interaction type (approach, location-based, regular dialogue)

The AI dialogue is triggered:
1. Automatically when visiting Aria at the library at 8:30am-9:30am
2. When entering locations where Aria is present (30% chance, up to 3 times per day)
3. When interacting with her during special events

## Configuration

The mod can be configured through the `config.json` file in the AriaMod folder:

- `Url`: The API endpoint for the language model (default: DashScope)
- `Apikey`: Your API key for the language model service
- `Model`: The specific model to use for generating responses

## Dependencies

- Stardew Valley
- SMAPI
- Content Patcher
- Aria NPC (base mod)

## Credits

- Created by leo
- Uses Content Patcher framework for integration with the game
- AI dialogue system powered by large language models