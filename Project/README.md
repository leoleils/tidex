# Aria NPC Mod for Stardew Valley

This mod adds a new NPC named Aria to Stardew Valley. Aria is a town librarian who can be found in various locations around Pelican Town.

## Features

- Adds Aria, a unique NPC character to the game
- Custom sprite and portrait artwork
- Daily dialogue lines for different days of the week
- Location schedule for all seasons
- Integration with Content Patcher framework
- **AI-powered dialogue system** - Aria can now have dynamic conversations with the player using a large language model

## Installation

1. Install [SMAPI](https://smapi.io/) - the mod loader for Stardew Valley
2. Install [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) by Pathoschild
3. Download this mod and unzip it into your Stardew Valley Mods folder
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

- **Spring**: Starts at the Archaeology House, then visits various locations in town during the day
- **Summer**: Similar pattern with different dialogue and locations
- **Fall**: Different routine with unique dialogue
- **Winter**: Indoor-focused schedule due to weather
- **Festival Days**: Special schedule for events like the Egg Festival

Aria now has a special appointment at 1400 (2pm) each day where she will engage in AI-powered dialogue with the player.

## Dialogue

Aria has unique dialogue for:
- Initial introduction
- Each day of the week
- Special interactions when the player is nearby
- **AI-powered conversations** at 2pm daily

Sample dialogue includes topics about:
- Library work and book cataloging
- Gardening and farming techniques
- Local folklore and legends
- Seasonal activities and observations

## AI Dialogue System

This mod includes an experimental AI dialogue system that allows Aria to have more dynamic conversations with the player. The system uses a large language model to generate contextual responses based on:

- Time of day
- Season
- Player name
- Location
- Conversation history (limited)

The AI dialogue is triggered automatically when visiting Aria at the library at 2pm, or when interacting with her during special events.

## Configuration

The mod can be configured through the `config.json` file:

- `Url`: The API endpoint for the language model (default: DashScope)
- `Apikey`: Your API key for the language model service
- `Model`: The specific model to use for generating responses

## Dependencies

- Stardew Valley
- SMAPI
- Content Patcher

## Credits

- Created by leo
- Uses Content Patcher framework for integration with the game
- AI dialogue system powered by large language models