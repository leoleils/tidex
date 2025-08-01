# Shary NPC Advanced Features - SMAPI Mod

## Description
This SMAPI mod enhances the Shary NPC by adding advanced features including:
- Qwen AI integration for dynamic conversations using the qwen3-235b-a22b-instruct-2507 model via OpenAI-compatible API
- Memory system to maintain conversation context and multi-turn dialogue history
- Resource collection based on friendship level
- Scheduled events and behaviors
- Interaction logging organized by game date

## Requirements
- Stardew Valley (latest version)
- SMAPI (latest version)
- Shary NPC Content Patcher mod (included in the main mod package)

## Features
### Qwen Integration
Shary uses the Qwen model (qwen3-235b-a22b-instruct-2507) for natural conversations with streaming output support. The integration provides context-aware responses based on:
- Current conversation context
- Player's friendship level with Shary
- Time of day and location
- Previous conversation history (multi-turn dialogue support)

The implementation uses the official OpenAI C# SDK with DashScope's OpenAI-compatible endpoint.

### Memory System with Multi-turn Dialogue
The mod maintains a memory of previous conversations and events to provide context-aware responses. It now supports:
- Multi-turn dialogue history tracking (last 20 exchanges)
- Contextual memory storage
- Persistent storage between game sessions
- Separate storage for player and Shary messages

### Interaction Logging
All interactions with Shary are logged and organized by game date:
- Dialogue exchanges
- Location visits
- Scheduled events
- Resource collection requests
- Conversation start/end events

Logs are stored in `interaction_logs.json` and can be viewed through SMAPI console commands.

### Resource Collection
Based on your friendship level with Shary, she can help collect resources from your farm and bring them to your house.

### Scheduled Behaviors
The mod enhances Shary's schedule with additional timed events and behaviors.

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

To use the Qwen integration:
1. Get an API key from [DashScope](https://dashscope.aliyun.com/)
2. Enable the qwen3-235b-a22b-instruct-2507 model for your account
3. Replace "your-api-key-here" with your actual API key

### Multi-turn Dialogue
The mod supports multi-turn dialogue by maintaining conversation history between player and Shary. Each interaction is stored and provided as context for future conversations, allowing for more natural and coherent conversations over time.

Note: Due to Stardew Valley's input system limitations, direct player text input is not captured. However, the conversation context is maintained through the sequence of interactions.

### Interaction Logging
All interactions with Shary are automatically logged with timestamps and game context. Logs are organized by game date (e.g., "s01" for Spring 1st) and include:
- Dialogue exchanges
- Location visits
- Scheduled events
- Resource collection requests
- Conversation start/end events

### Viewing Logs
Use the SMAPI console command `shary_logs` to view interaction logs:
- `shary_logs` - View logs for the current game date
- `shary_logs s01` - View logs for a specific date (Spring 1st in this example)
- `shary_logs all` - View all logs across all dates

### Customizing Shary's Personality
You can customize Shary's personality by modifying the prompts:

- `SystemPrompt`: Defines Shary's core personality and role
- `UserPromptTemplate`: Controls how context and friendship information are presented to the model
- `FriendshipNotes`: Defines how Shary's responses should vary based on friendship level

This allows you to completely change Shary's personality without modifying the code.

## Building from Source
To build this mod from source:

1. Clone or download the source code
2. Open the solution in Visual Studio or VS Code
3. Set the `StardewPath` environment variable to your game's installation directory
4. Build the project (the OpenAI package will be automatically downloaded)

## Troubleshooting
If you encounter any issues:
1. Ensure all dependencies are installed
2. Check the SMAPI log for error messages
3. Verify the configuration settings
4. Make sure your API key is valid and the model is enabled
5. Report any bugs to the mod's maintainer

## Credits
- Created by: YourName
- Uses Harmony for code patching
- Integrates with the Qwen model via OpenAI-compatible API
- Uses the official OpenAI C# SDK
