using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Locations;
using Microsoft.Xna.Framework;

namespace SharySMAPI
{
    public class ResourceCollectionSystem
    {
        private IModHelper Helper;
        private IMonitor Monitor;
        
        public ResourceCollectionSystem(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
        }
        
        public bool CollectResources(int friendshipLevel, int minimumFriendship)
        {
            // Check if friendship level is high enough
            if (friendshipLevel < minimumFriendship)
            {
                return false;
            }
            
            // Find resources on the farm
            var farm = Game1.getLocationFromName("Farm") as Farm;
            if (farm == null) return false;
            
            // Collect items (simplified implementation)
            var itemsToCollect = new List<Item>();
            var objectsToRemove = new List<Vector2>();
            
            foreach (var pair in farm.Objects.Pairs)
            {
                var position = pair.Key;
                var obj = pair.Value;
                
                // Check if it's a collectible resource (fruits, vegetables, etc.)
                if (obj is FruitTree || obj is HoeDirt || obj.Name.Contains("Cauliflower") || 
                    obj.Name.Contains("Potato") || obj.Name.Contains("Blueberry") ||
                    obj.Name.Contains("Corn") || obj.Name.Contains("Tomato") ||
                    obj.Name.Contains("Wheat") || obj.Name.Contains("Carrot") ||
                    obj.Name.Contains("Cabbage") || obj.Name.Contains("Beet"))
                {
                    itemsToCollect.Add(obj.getOne());
                    objectsToRemove.Add(position);
                }
            }
            
            // Remove collected objects
            foreach (var pos in objectsToRemove)
            {
                farm.Objects.Remove(pos);
            }
            
            // Add items to player's inventory or house
            foreach (var item in itemsToCollect)
            {
                // Try to add to inventory first
                if (!Game1.player.addItemToInventoryBool(item))
                {
                    // If inventory is full, place in house
                    var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                    if (house != null)
                    {
                        // Place item on the floor of the house
                        var position = new Vector2(10, 10); // Simplified position
                        house.Objects.Add(position, new Chest(true, new List<Item> { item }, position));
                    }
                }
            }
            
            return true;
        }
    }
}