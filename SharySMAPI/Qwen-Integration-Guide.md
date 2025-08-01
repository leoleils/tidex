# Qwen Integration Implementation Guide

## Overview
This document explains how the Qwen integration is implemented for the Shary NPC mod, using the qwen3-235b-a22b-instruct-2507 model through DashScope's OpenAI-compatible API with multi-turn dialogue support.

## Prerequisites
1. Access to Qwen API through DashScope
2. API key for authentication
3. Internet connectivity from the player's machine
4. The qwen3-235b-a22b-instruct-2507 model enabled for your account

## Implementation Details

### 1. SDK Integration
The implementation uses the official OpenAI C# SDK with DashScope's OpenAI-compatible endpoint:

```csharp
var openAiSettings = new OpenAiOptions()
{
    ApiKey = this.Config.QwenApiKey,
    BaseDomain = this.Config.QwenApiEndpoint
};

this.OpenAIService = new OpenAIService(openAiSettings);
```

### 2. Configuration
The integration is configured through the `config.json` file with several customizable options:

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

### 3. Multi-turn Dialogue Support
The implementation now supports multi-turn dialogues by maintaining conversation history:

1. **Dialogue History Storage**: The memory system stores the last 20 exchanges between player and Shary
2. **Context Preparation**: When preparing messages for the API, previous exchanges are included
3. **Role Differentiation**: Player messages are marked as "user" role, Shary's responses as "assistant" role

```csharp
// Add previous conversation history
var dialogueHistory = this.Memory.GetDialogueHistory(10); // Get last 10 exchanges
foreach (var entry in dialogueHistory)
{
    if (entry.Speaker == "Player")
    {
        messages.Add(ChatMessage.FromUser(entry.Message));
    }
    else if (entry.Speaker == "Shary")
    {
        messages.Add(ChatMessage.FromAssistant(entry.Message));
    }
}
```

### 4. API Communication
The integration handles both standard and streaming API calls using the OpenAI SDK:

#### Standard Requests
```csharp
var response = await this.OpenAIService.ChatCompletion.CreateCompletion(chatCompletionRequest);
```

#### Streaming Requests
```csharp
await this.OpenAIService.ChatCompletion.CreateCompletionAsStream(chatCompletionRequest, (streamEvent) => {
    // Handle streaming events
});
```

### 5. Prompt Engineering
The system constructs context-aware prompts that include:
- Conversation history from the memory system
- Player's friendship level with contextual notes
- Character personality and background
- Game-appropriate response formatting

The prompts are fully configurable:
- `SystemPrompt`: Defines the character's personality and role
- `UserPromptTemplate`: Controls how context information is presented
- `FriendshipNotes`: Defines how responses vary by relationship level

### 6. Error Handling
The implementation includes comprehensive error handling:
- Configuration validation
- Network error handling
- API error response parsing
- Fallback to template responses when API is unavailable

### 7. Streaming Output
The mod implements streaming output for a more natural conversation experience:
- Creates a dialogue box that updates in real-time
- Processes streaming events from the API
- Updates the dialogue box with each token as it arrives

## API Integration Flow

1. Player interacts with Shary NPC
2. Mod retrieves conversation context and history from memory system
3. Mod constructs a prompt with context and character information using configurable templates
4. Mod prepares message history including previous exchanges
5. Mod sends request to Qwen API via OpenAI SDK
6. API processes the request using qwen3-235b-a22b-instruct-2507 with full context
7. For streaming, tokens are received and displayed in real-time
8. For standard requests, the complete response is displayed
9. Conversation is stored in memory and dialogue history for future context

## Customization Examples

### Changing Shary's Personality
To make Shary more playful and energetic:
```json
{
  "SystemPrompt": "You are Shary, a 25-year-old female librarian in Stardew Valley. You live in the town and work at the library. You are energetic, playful, and love sharing interesting facts from books. Respond in a way that fits the game's tone and style."
}
```

### Adjusting Friendship Notes
To make friendship progression more nuanced:
```json
{
  "FriendshipNotes": {
    "NewAcquaintance": "This is a new acquaintance. Be courteous and professional.",
    "DevelopingFriendship": "This is a developing friendship. Show genuine interest in their activities.",
    "GoodFriend": "This is a good friend. Be supportive and share personal thoughts.",
    "CloseFriend": "This is a close friend with romantic potential. Show affection and care.",
    "Spouse": "This is a spouse. Be deeply loving and intimate in your conversation."
  }
}
```

### Modifying the Prompt Template
To include more information in the prompt:
```json
{
  "UserPromptTemplate": "Context: {0}\n\nFriendship Note: {1}\n\nCurrent Location: {2}\n\nCurrent Time: {3}\n\nPlease respond as Shary in a single, natural sentence that fits the Stardew Valley game style:"
}
```

Note: If you modify the template, you'll need to update the `GeneratePrompt` method in `QwenIntegration.cs` to provide the additional parameters.

## Testing Multi-turn Dialogue
1. Configure the API key in `config.json`
2. Run the game with SMAPI
3. Have multiple conversations with Shary in succession
4. Observe how context from previous conversations influences responses
5. Check the SMAPI console for any error messages

## Troubleshooting
- Ensure the API key is valid and has access to the Qwen model
- Check that the endpoint URL is correct
- Verify internet connectivity
- Check the SMAPI log for detailed error messages
- Confirm the qwen3-235b-a22b-instruct-2507 model is enabled in your DashScope account

## Performance Considerations
- API calls have a delay, so consider caching responses for common interactions
- Implement a timeout for API requests to prevent hanging
- Consider rate limiting to avoid exceeding API quotas
- Streaming responses provide a better user experience but require more processing
- Keeping too much dialogue history may impact performance and token usage