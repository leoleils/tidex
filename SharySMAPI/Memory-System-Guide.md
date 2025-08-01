# Memory System Implementation Guide

## Overview
This document explains how to implement and enhance the memory system for the Shary NPC mod. The memory system allows Shary to remember previous conversations and events, providing context-aware responses.

## Current Implementation
The current implementation includes a basic memory system in `MemorySystem.cs` that stores and retrieves key-value pairs. However, a more sophisticated implementation would track conversation history, player preferences, and important events.

## Enhanced Implementation

### 1. Update MemorySystem.cs
Replace the current implementation with a more sophisticated version:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace SharySMAPI
{
    public class MemorySystem
    {
        private IModHelper Helper;
        private List<MemoryEntry> Memories;
        private const int MaxMemories = 50; // Limit to prevent memory bloat
        
        public MemorySystem(IModHelper helper)
        {
            this.Helper = helper;
            this.Memories = new List<MemoryEntry>();
            this.LoadMemories();
        }
        
        public void AddMemory(string category, string content, double importance = 1.0)
        {
            var memory = new MemoryEntry
            {
                Category = category,
                Content = content,
                Timestamp = DateTime.Now,
                Importance = importance
            }
            
            this.Memories.Add(memory);
            
            // Keep only the most important memories if we exceed the limit
            if (this.Memories.Count > MaxMemories)
            {
                this.Memories = this.Memories
                    .OrderByDescending(m => m.Importance)
                    .Take(MaxMemories)
                    .ToList();
            }
            
            this.SaveMemories();
        }
        
        public List<MemoryEntry> GetMemories(string category = null, int limit = 10)
        {
            var memories = category != null 
                ? this.Memories.Where(m => m.Category == category).ToList()
                : this.Memories;
                
            return memories
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .ToList();
        }
        
        public string GetContext()
        {
            var context = "Recent memories:\n";
            
            // Get the most recent memories from each category
            var categories = this.Memories.Select(m => m.Category).Distinct();
            foreach (var category in categories)
            {
                var recent = this.GetMemories(category, 3);
                if (recent.Any())
                {
                    context += $"{category}:\n";
                    foreach (var memory in recent)
                    {
                        context += $"  - {memory.Content} ({memory.Timestamp:yyyy-MM-dd HH:mm})\n";
                    }
                }
            }
            
            return context;
        }
        
        private void LoadMemories()
        {
            try
            {
                var data = this.Helper.Data.ReadJsonFile<List<MemoryEntry>>("memory.json") ?? new List<MemoryEntry>();
                this.Memories = data;
            }
            catch (Exception ex)
            {
                this.Helper.Monitor.Log($"Failed to load memories: {ex.Message}", LogLevel.Warn);
                this.Memories = new List<MemoryEntry>();
            }
        }
        
        private void SaveMemories()
        {
            try
            {
                this.Helper.Data.WriteJsonFile("memory.json", this.Memories);
            }
            catch (Exception ex)
            {
                this.Helper.Monitor.Log($"Failed to save memories: {ex.Message}", LogLevel.Warn);
            }
        }
        
        public void ClearMemories()
        {
            this.Memories.Clear();
            this.SaveMemories();
        }
    }
    
    public class MemoryEntry
    {
        public string Category { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public double Importance { get; set; }
    }
}
```

### 2. Update ModEntry.cs to use the enhanced memory system
Modify the interaction handling to store memories:

```csharp
private async void HandleSharyInteraction(NPC shary)
{
    // Get player's friendship level with Shary
    var friendshipLevel = Game1.player.friendshipData.TryGetValue("Shary", out var friendship) ? friendship.Points : 0;
    
    // Store this interaction in memory
    this.Memory.AddMemory("interaction", $"Player interacted with Shary at {Game1.currentLocation.Name}", 1.5);
    
    // Use Qwen to generate a response based on context and memory
    var context = this.Memory.GetContext();
    var response = this.Qwen.GenerateResponse(context, friendshipLevel);
    
    // Show dialogue
    Game1.drawDialogue(shary, response);
    
    // Store the response in memory
    this.Memory.AddMemory("dialogue", $"Shary said: {response}", 1.0);
}
```

### 3. Add Memory Categories
Use different memory categories for better organization:

```csharp
// In MemorySystem.cs or as constants in ModEntry.cs
public static class MemoryCategories
{
    public const string Interaction = "interaction";
    public const string Dialogue = "dialogue";
    public const string Gift = "gift";
    public const string Event = "event";
    public const string Location = "location";
    public const string Weather = "weather";
}
```

### 4. Memory Pruning
Implement a method to periodically clean up old memories:

```csharp
public void PruneOldMemories()
{
    var cutoff = DateTime.Now.AddDays(-30); // Keep memories for 30 days
    this.Memories.RemoveAll(m => m.Timestamp < cutoff);
    this.SaveMemories();
}
```

Call this method periodically, such as once per day in the `OnDayStarted` event:

```csharp
private void OnDayStarted(object sender, DayStartedEventArgs e)
{
    // Prune old memories
    this.Memory.PruneOldMemories();
    
    // Schedule Shary's special events based on time of day
    this.Helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
}
```

## Testing
1. Interact with Shary multiple times
2. Check the `memory.json` file in the mod's data folder
3. Verify that memories are being stored and retrieved correctly
4. Test memory pruning by advancing days in the game

## Performance Considerations
- Limit the number of memories to prevent performance issues
- Use efficient data structures for memory storage and retrieval
- Serialize memories to disk to persist between game sessions
- Periodically prune old memories to maintain performance