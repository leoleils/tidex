# Resource Collection System Implementation Guide

## Overview
This document explains how to implement and enhance the resource collection system for the Shary NPC mod. This system allows Shary to help players collect resources from their farm based on friendship level.

## Current Implementation
The current implementation in `ResourceCollectionSystem.cs` provides a basic framework for collecting resources, but it can be enhanced with more sophisticated logic and better integration with the game's systems.

## Enhanced Implementation

### 1. Update ResourceCollectionSystem.cs
Replace the current implementation with a more robust version:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace SharySMAPI
{
    public class ResourceCollectionSystem
    {
        private IModHelper Helper;
        private IMonitor Monitor;
        private ModConfig Config;
        
        public ResourceCollectionSystem(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.Config = config;
        }
        
        public bool CollectResources(int friendshipLevel)
        {
            // Check if friendship level is high enough
            if (friendshipLevel < this.Config.MinimumFriendshipForHelp)
            {
                return false;
            }
            
            // Get the farm location
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) 
            {
                this.Monitor.Log("Could not find farm location", LogLevel.Warn);
                return false;
            }
            
            // Find collectible items
            var itemsToCollect = new List<CollectedItem>();
            
            // Check for fruit trees with fruit
            foreach (var terrainFeature in farm.terrainFeatures.Values)
            {
                if (terrainFeature is FruitTree tree && tree.fruit.Count > 0)
                {
                    for (int i = 0; i < tree.fruit.Count; i++)
                    {
                        itemsToCollect.Add(new CollectedItem
                        {
                            Item = tree.fruit[i].getOne(),
                            Position = tree.currentTileLocation,
                            Source = "Fruit Tree"
                        });
                    }
                    tree.fruit.Clear();
                }
            }
            
            // Check for crops that are ready to harvest
            foreach (var pair in farm.Objects.Pairs)
            {
                var position = pair.Key;
                var obj = pair.Value;
                
                // Check if it's a crop that's ready to harvest
                if (obj is HoeDirt dirt && dirt.crop != null && dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1)
                {
                    var cropId = dirt.crop.indexOfHarvest.Value;
                    var crop = new SObject(cropId, 1);
                    
                    itemsToCollect.Add(new CollectedItem
                    {
                        Item = crop,
                        Position = position,
                        Source = "Crop"
                    });
                    
                    // Remove the crop
                    dirt.crop = null;
                }
            }
            
            // Check for forageable items
            var forageables = farm.Objects.Values
                .Where(obj => obj.IsSpawnedObject && obj.Name != "Stone" && obj.Name != "Twig")
                .ToList();
                
            foreach (var forageable in forageables)
            {
                itemsToCollect.Add(new CollectedItem
                {
                    Item = forageable.getOne(),
                    Position = forageable.TileLocation,
                    Source = "Forageable"
                });
                
                // Remove the forageable from the farm
                farm.Objects.Remove(forageable.TileLocation);
            }
            
            // Deliver items to player or farmhouse
            this.DeliverItems(itemsToCollect);
            
            return true;
        }
        
        private void DeliverItems(List<CollectedItem> items)
        {
            if (items.Count == 0) return;
            
            // Try to add items to player's inventory first
            var itemsAdded = 0;
            foreach (var item in items)
            {
                if (Game1.player.addItemToInventoryBool(item.Item))
                {
                    itemsAdded++;
                }
            }
            
            // If there are still items left, deliver them to the farmhouse
            if (itemsAdded < items.Count)
            {
                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                if (house != null)
                {
                    // Find a free spot in the house to place a chest
                    var chestPosition = this.FindFreeSpotInHouse(house);
                    if (chestPosition != null)
                    {
                        var remainingItems = items.Skip(itemsAdded).Select(i => i.Item).ToList();
                        var chest = new Chest(true, remainingItems, chestPosition.Value)
                        {
                            Name = "Shary's Collection"
                        };
                        
                        house.Objects.Add(chestPosition.Value, chest);
                        
                        // Notify player
                        Game1.addHUDMessage(new HUDMessage($"Shary left {remainingItems.Count} items in a chest for you!", 1));
                    }
                }
            }
            else
            {
                // Notify player
                Game1.addHUDMessage(new HUDMessage($"Shary collected {items.Count} items for you!", 1));
            }
        }
        
        private Vector2? FindFreeSpotInHouse(FarmHouse house)
        {
            // Look for a free spot near the kitchen
            for (int x = 10; x < 15; x++)
            {
                for (int y = 5; y < 10; y++)
                {
                    var position = new Vector2(x, y);
                    if (!house.Objects.ContainsKey(position) && 
                        house.getTileIndexAt(x, y, "Buildings") == -1 &&
                        house.getTileIndexAt(x, y, "Back") != 338) // Not water
                    {
                        return position;
                    }
                }
            }
            
            return null;
        }
    }
    
    public class CollectedItem
    {
        public Item Item { get; set; }
        public Vector2 Position { get; set; }
        public string Source { get; set; }
    }
}
```

### 2. Update ModEntry.cs to use the enhanced system
Modify the mod entry to integrate the resource collection:

```csharp
// Add a method to handle resource collection requests
private void HandleResourceCollectionRequest()
{
    // Get player's friendship level with Shary
    var friendshipLevel = Game1.player.friendshipData.TryGetValue("Shary", out var friendship) ? friendship.Points : 0;
    
    // Create resource collection system
    var resourceSystem = new ResourceCollectionSystem(this.CustomHelper, this.CustomMonitor, this.Config);
    
    // Attempt to collect resources
    var success = resourceSystem.CollectResources(friendshipLevel);
    
    if (success)
    {
        // Add a positive memory
        this.Memory.AddMemory(MemoryCategories.Event, "Player asked Shary to collect resources. She agreed and helped.", 2.0);
        
        // Show a message
        Game1.addHUDMessage(new HUDMessage("Shary is collecting resources from your farm!", 1));
    }
    else
    {
        // Add a negative memory
        this.Memory.AddMemory(MemoryCategories.Event, "Player asked Shary to collect resources. She declined due to low friendship.", 1.0);
        
        // Show a message
        Game1.addHUDMessage(new HUDMessage("Shary isn't comfortable helping with that yet. Try building your friendship.", 1));
    }
}
```

### 3. Add Resource Collection Triggers
Add ways for players to request resource collection:

```csharp
// In the button press handler
private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
{
    // Existing interaction code...
    
    // Add a special key combination for resource collection
    if (e.Button == SButton.R && Game1.player.isKeyDown(SButton.LeftShift))
    {
        // Check if Shary is nearby
        var shary = this.GetShary();
        if (shary != null && Game1.player.currentLocation.characters.Contains(shary))
        {
            var distance = Utility.distance(Game1.player.Position, shary.Position);
            if (distance < 100f)
            {
                this.HandleResourceCollectionRequest();
            }
        }
    }
}
```

Alternatively, you could add a dialogue option when talking to Shary:

```csharp
// In the dialogue handling
private async void HandleSharyInteraction(NPC shary)
{
    // Get player's friendship level with Shary
    var friendshipLevel = Game1.player.friendshipData.TryGetValue("Shary", out var friendship) ? friendship.Points : 0;
    
    // Create dialogue options
    var responses = new List<Response>();
    
    // Add standard dialogue option
    responses.Add(new Response("chat", "Chat with Shary"));
    
    // Add resource collection option if friendship is high enough
    if (friendshipLevel >= this.Config.MinimumFriendshipForHelp)
    {
        responses.Add(new Response("collect", "Ask Shary to collect resources"));
    }
    
    // Show dialogue with options
    var dialogue = new Dialogue("What would you like to do?", shary);
    Game1.currentSpeaker = shary;
    Game1.dialogueUp = true;
    Game1.dialogueTyping = false;
    Game1.questionChoices = responses.ToArray();
}
```

### 4. Add Configuration Options
Enhance the configuration with more resource collection options:

```csharp
public class ModConfig
{
    // Existing options...
    
    /// <summary>
    /// Whether Shary can collect crops from the farm.
    /// </summary>
    public bool CollectCrops { get; set; } = true;
    
    /// <summary>
    /// Whether Shary can collect fruit from trees.
    /// </summary>
    public bool CollectFruit { get; set; } = true;
    
    /// <summary>
    /// Whether Shary can collect forageable items.
    /// </summary>
    public bool CollectForageables { get; set; } = true;
    
    /// <summary>
    /// Maximum number of items Shary will collect in one trip.
    /// </summary>
    public int MaxItemsPerCollection { get; set; } = 50;
    
    /// <summary>
    /// Time of day when Shary can collect resources (24-hour format).
    /// </summary>
    public int CollectionStartTime { get; set; } = 900; // 9:00 AM
    
    /// <summary>
    /// Time of day when Shary stops collecting resources (24-hour format).
    /// </summary>
    public int CollectionEndTime { get; set; } = 1700; // 5:00 PM
}
```

## Testing
1. Build friendship with Shary to the required level
2. Press the resource collection key combination (Shift+R by default)
3. Verify that resources are collected from the farm
4. Check that items are delivered to the player's inventory or farmhouse
5. Test during different times of day to verify time restrictions

## Performance Considerations
- Limit the number of items collected in one trip to prevent performance issues
- Cache references to locations to avoid repeated lookups
- Use efficient algorithms for finding items and free spots
- Consider implementing a cooldown period between collections