# Shary NPC Mod - Complete Implementation Overview

## Introduction
This document provides a complete overview of the Shary NPC mod for Stardew Valley, including all components and how they work together to create a rich, interactive character experience.

## Mod Structure
The mod consists of two main components:

1. **Content Patcher Mod** (`SharyMod` directory)
   - Defines Shary's character data, sprites, and basic behaviors
   - Handles dialogue, schedules, and festival appearances
   - Provides the visual and basic interactive elements

2. **SMAPI Mod** (`SharySMAPI` directory)
   - Implements advanced features like Qwen integration, memory system, and resource collection
   - Handles complex behaviors and timed events
   - Provides the "intelligence" behind Shary's interactions

## Component Integration

### Character Definition (Content Patcher)
The Content Patcher mod defines Shary through several key files:

1. `manifest.json` - Mod metadata
2. `content.json` - Character data, gift tastes, animations, and asset loading
3. `assets/Shary.json` - Daily schedule
4. `i18n/default.json` - Dialogue strings for different situations
5. Sprite and portrait assets

### Advanced Features (SMAPI)
The SMAPI mod enhances Shary with several advanced systems:

1. **Qwen Integration**
   - Provides dynamic, context-aware dialogue using the qwen3-235b-a22b-instruct-2507 model
   - Implements streaming output for natural conversation flow via OpenAI-compatible API
   - Uses friendship level and memory context to customize responses
   - Fully configurable prompts for personality customization
   - Supports multi-turn dialogue with conversation history

2. **Memory System**
   - Tracks conversation history and important events
   - Provides context to the Qwen integration
   - Persists between game sessions

3. **Resource Collection**
   - Allows Shary to help collect farm resources
   - Controlled by friendship level
   - Delivers items to player or farmhouse

4. **Scheduled Behaviors**
   - Enhances Shary's daily schedule with special events
   - Triggers behaviors at specific times of day
   - Integrates with the game's time system

5. **Interaction Logging**
   - Records all interactions with Shary
   - Organizes logs by game date
   - Provides console commands for viewing logs

## Implementation Flow

### Game Startup
1. SMAPI loads both mods
2. Content Patcher registers Shary as an NPC
3. SMAPI mod initializes systems (memory, Qwen integration, etc.)

### Daily Routine
1. Shary follows her schedule defined in `assets/Shary.json`
2. SMAPI mod enhances scheduled events with additional behaviors
3. Special events trigger at specific times (afternoon walk, evening return home)

### Player Interaction
1. Player approaches and interacts with Shary
2. SMAPI mod intercepts interaction:
   - Retrieves friendship level
   - Gets context and dialogue history from memory system
   - Generates response using Qwen integration with streaming output
   - Displays dialogue with real-time token updates
3. Conversation is stored in memory and dialogue history for future context
4. Interaction is logged with timestamp and game context

### Resource Collection
1. Player requests resource collection (via key combination or dialogue)
2. SMAPI mod checks friendship level against threshold
3. If approved:
   - Collects eligible items from farm
   - Delivers to player inventory or farmhouse
   - Updates memory with event
4. If denied:
   - Shows appropriate message
   - Updates memory with event
5. Request and result are logged

## Technical Details

### Dependencies
- SMAPI 3.0+
- Content Patcher
- Harmony (for code patching)
- Newtonsoft.Json (for serialization)
- OpenAI C# SDK (for Qwen integration)

### Data Persistence
- Dialogue memories: Stored in `memory.json`
- Dialogue history: Stored in `dialogue_history.json`
- Interaction logs: Stored in `interaction_logs.json`
- Friendship data: Handled by the game
- Configuration: Stored in `config.json`

### Performance Considerations
- Memory system limits stored entries
- Resource collection has item limits
- API calls are asynchronous to prevent game freezing
- Efficient algorithms for finding items and free spots

## Multi-turn Dialogue Implementation

The mod implements multi-turn dialogue through:

1. **Dialogue History Storage**:
   - Maintains the last 20 exchanges between player and Shary
   - Stores messages separately for player and Shary
   - Persists history between game sessions

2. **Context-aware Responses**:
   - Includes previous dialogue in API requests
   - Uses role-based message formatting (user/assistant)
   - Maintains conversation coherence over multiple interactions

3. **Memory Integration**:
   - Combines dialogue history with other contextual memories
   - Provides rich context to the Qwen model
   - Enables more natural and personalized conversations

Note: Due to Stardew Valley's input system, direct player text input is not captured. However, the sequence of interactions provides context that enables multi-turn dialogue.

## Interaction Logging

The mod implements comprehensive interaction logging:

1. **Log Organization**:
   - Logs are organized by game date (e.g., "s01" for Spring 1st)
   - Each entry includes timestamp, speaker, message, and event type
   - Separate storage for different event types (dialogue, location, events, etc.)

2. **Automatic Logging**:
   - All interactions with Shary are automatically logged
   - Scheduled events are recorded
   - Resource collection requests and results are logged
   - Conversation start/end events are tracked

3. **Log Viewing**:
   - Console command `shary_logs` to view logs
   - Support for viewing current date, specific date, or all logs
   - Formatted output for easy reading

## Extensibility
The mod is designed to be extensible:

1. **Adding New Dialogue**
   - Update `i18n/default.json` with new entries
   - No code changes required for simple dialogue

2. **Enhancing Schedule**
   - Modify `assets/Shary.json` to add new activities
   - Add corresponding SMAPI event handlers if needed

3. **Adding New Features**
   - Create new service classes in the `Services` directory
   - Register event handlers in `ModEntry.cs`
   - Update configuration as needed

4. **Customizing Personality**
   - Modify prompts in `config.json` to change Shary's personality
   - No code changes required for personality adjustments

## Configuration Options

The mod supports extensive configuration through the `config.json` file:

### Qwen Integration
- API endpoint and key (using OpenAI-compatible interface)
- Model selection (qwen3-235b-a22b-instruct-2507)
- System prompt defining character personality
- User prompt template for context formatting
- Friendship notes for relationship-based responses

### Gameplay Features
- Minimum friendship level for resource collection
- Enable/disable switches for different features

## Console Commands

The mod provides the following console commands for viewing interaction logs:

- `shary_logs` - View logs for the current game date
- `shary_logs s01` - View logs for a specific date (Spring 1st in this example)
- `shary_logs all` - View all logs across all dates

## Future Improvements
1. **Enhanced Qwen Integration**
   - Add more sophisticated prompt engineering
   - Implement personality customization options
   - Add better multi-turn conversation support with improved context management

2. **Advanced Memory System**
   - Implement semantic memory clustering
   - Add long-term memory consolidation
   - Include player preference tracking

3. **Expanded Resource Collection**
   - Add fishing and foraging assistance
   - Implement item delivery scheduling
   - Add collection notifications

4. **Additional Features**
   - Custom books that Shary recommends
   - Library events and activities
   - Seasonal outfit changes

5. **Enhanced Logging**
   - Add log filtering by event type
   - Implement log export functionality
   - Add web-based log viewer

## Conclusion
The Shary NPC mod provides a comprehensive example of how to create a rich, interactive character in Stardew Valley. By combining Content Patcher for basic character definition with SMAPI for advanced features, it demonstrates best practices for mod development and creates an engaging gameplay experience. The integration with the Qwen model (qwen3-235b-a22b-instruct-2507) through DashScope's OpenAI-compatible API adds a layer of dynamic, context-aware dialogue that makes interactions with Shary feel more natural and engaging. With fully configurable prompts, SDK-based implementation, multi-turn dialogue support, and comprehensive interaction logging organized by game date, users can easily customize Shary's personality to their preferences and review all interactions through the console commands.