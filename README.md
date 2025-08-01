# Shary NPC Mod for Stardew Valley

## Description
This mod adds Shary, a 25-year-old librarian NPC to Stardew Valley. She can be found in the town, works at the library, and has a full schedule of activities throughout the day. Players can build a friendship with her, date her, attend festivals together, and even marry her.

## Features
- Fully voiced NPC with unique dialogue based on friendship level
- Daily schedule that includes working at the library, walking around town, and returning home
- Special dialogue for different seasons, weather conditions, and days of the week
- Integration with festivals and special events
- Marriageable with a custom spouse room
- Resource collection feature that activates based on friendship level
- Qwen AI integration for dynamic conversations (using qwen3-235b-a22b-instruct-2507 model)
- Memory system to maintain conversation context
- Multi-turn dialogue support
- Interaction logging organized by game date
- Streaming output for natural conversation flow

## Installation
1. Install the latest version of [SMAPI](https://smapi.io/)
2. Install [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915)
3. Download this mod and unzip it into StardewValley/Mods
4. Run the game using SMAPI

## Character Details
- **Name**: Shary (雪莉)
- **Age**: 25
- **Occupation**: Librarian
- **Location**: Town
- **Schedule**:
  - Morning (8am-12pm): Working at the library
  - Afternoon (1pm-5pm): Walking around town
  - Evening (7pm-10pm): At home
  - Night (10pm-12am): Sleeping

## Friendship Progression
As you build friendship with Shary through gifts, conversations, and spending time together, you'll unlock:
- Special dialogue at different friendship levels
- Date opportunities
- Dance invitations at festivals
- Resource collection assistance

## Resource Collection
Once you reach a certain friendship level with Shary, you can ask her to help collect resources from your farm. She will gather items and bring them back to your house.

## Qwen Integration
Shary uses the Qwen model (qwen3-235b-a22b-instruct-2507) for dynamic conversations via DashScope's OpenAI-compatible API. The integration provides:
- Context-aware responses based on conversation history and friendship level
- Streaming output for natural conversation flow
- Fully configurable prompts for personality customization
- Multi-turn dialogue support

To use this feature:
1. Get an API key from [DashScope](https://dashscope.aliyun.com/)
2. Enable the qwen3-235b-a22b-instruct-2507 model for your account
3. Add your API key to the config.json file

## Configuration
You can customize the mod behavior by editing the `config.json` file:

```json
{
  "EnableQwenIntegration": true,
  "MinimumFriendshipForHelp": 2000,
  "QwenApiEndpoint": "https://dashscope.aliyuncs.com/compatible-mode/v1",
  "QwenApiKey": "your-api-key-here",
  "Model": "qwen3-235b-a22b-instruct-2507",
  "SystemPrompt": "You are Shary, a 25-year-old female librarian in Stardew Valley. You live in the town and work at the library. You are polite, intelligent, and enjoy books and nature. Respond in a way that fits the game's tone and style.",
  "UserPromptTemplate": "Context: {0}\n\nNote: {1}\n\nPlease respond as Shary in a single, natural sentence that fits the Stardew Valley game style:",
  "FriendshipNotes": {
    "NewAcquaintance": "This is a new acquaintance. Keep the conversation polite but reserved.",
    "DevelopingFriendship": "This is a developing friendship. Be friendly and interested in their activities.",
    "GoodFriend": "This is a good friend. Be warm and open in your conversation.",
    "CloseFriend": "This is a close friend with romantic potential. Be affectionate but respectful.",
    "Spouse": "This is a spouse. Be loving and intimate in your conversation."
  }
}
```

## Interaction Logging
All interactions with Shary are automatically logged with timestamps and game context. Logs are organized by game date (e.g., "s01" for Spring 1st) and include:
- Dialogue exchanges
- Location visits
- Scheduled events
- Resource collection requests
- Conversation start/end events

Use the SMAPI console command `shary_logs` to view interaction logs:
- `shary_logs` - View logs for the current game date
- `shary_logs s01` - View logs for a specific date (Spring 1st in this example)
- `shary_logs all` - View all logs across all dates

## Troubleshooting
If you encounter any issues:
1. Ensure you have the latest versions of SMAPI and Content Patcher
2. Check that all files are in the correct locations
3. Verify the configuration settings
4. Check the SMAPI log for error messages
5. Report any bugs on the mod's Nexus page

## Credits
- Created by: YourName
- Inspired by the wonderful world of Stardew Valley
- Uses the official OpenAI C# SDK for Qwen integration