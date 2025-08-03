using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SObject = StardewValley.Object;

namespace CustomNPC
{
    public class ModEntry : Mod
    {
        private string npcName = "Aria";
        private NPC customNPC;
        private List<string> dialogues;
        private Random random = new Random();
        private int moveTimer = 0;
        private int moveInterval = 300; // Move every 5 seconds approximately
        private Stack<Point> movementPath = new Stack<Point>();
        private int animationFrame = 0;
        private int animationTimer = 0;
        private int animationSpeed = 10; // Frames between animation updates
        
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            
            // Load dialogues from file
            LoadDialogues();
        }

        private void LoadDialogues()
        {
            // Use relative path instead of full path
            string dialoguesPath = "Dialogues/Aria.json";
            
            try
            {
                var dialogueData = Helper.Data.ReadJsonFile<Dictionary<string, object>>(dialoguesPath);
                if (dialogueData != null && dialogueData.ContainsKey("dialogues"))
                {
                    // Properly cast JArray to List<string>
                    var jArray = (JArray)dialogueData["dialogues"];
                    dialogues = jArray.ToObject<List<string>>();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to load dialogues: {ex.Message}", LogLevel.Error);
            }
            
            // Fallback to default dialogues if file not found or failed to load
            if (dialogues == null || dialogues.Count == 0)
            {
                dialogues = new List<string>
                {
                    "Hello there, farmer! Welcome to my little corner of the valley.#$b#I've been working on some new recipes with seasonal ingredients.",
                    "The weather is perfect for growing crops today!#$b#I planted some new experimental seeds this morning.",
                    "Have you visited the local market recently?#$b#They got in some fresh seafood from the coast.",
                    "I love this time of year when the flowers are blooming.#$b#It reminds me of my grandmother's garden.",
                    "Working the land is so peaceful.#$b#It helps me think and reflect on life."
                };
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Add NPC to the game
            AddCustomNPC();
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Add NPC when player enters a location
            if (e.NewLocation is Farm)
            {
                AddCustomNPC();
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (customNPC == null || !Context.IsWorldReady)
                return;

            // Handle NPC movement
            moveTimer++;
            if (moveTimer >= moveInterval)
            {
                // Only move if not already moving
                if (movementPath.Count == 0)
                {
                    CalculateMovementPath();
                }
                MoveNPC();
                moveTimer = 0;
            }
            
            // Handle animation
            animationTimer++;
            if (animationTimer >= animationSpeed)
            {
                animationFrame = (animationFrame + 1) % 4; // Cycle through 4 frames
                if (customNPC != null)
                {
                    customNPC.Sprite.currentFrame = customNPC.FacingDirection * 4 + animationFrame;
                }
                animationTimer = 0;
            }
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            // This could be used for additional rendering if needed
        }

        private void CalculateMovementPath()
        {
            if (customNPC == null || Game1.currentLocation != Game1.getFarm())
                return;

            Farm farm = Game1.getFarm();
            Point currentTile = new Point((int)(customNPC.Position.X / 64), (int)(customNPC.Position.Y / 64));
            
            // Generate a random target position within the farm bounds
            Point targetTile = new Point(
                random.Next(5, farm.map.Layers[0].LayerWidth - 5),
                random.Next(5, farm.map.Layers[0].LayerHeight - 5)
            );
            
            // Simple pathfinding - move directly toward target
            int dx = targetTile.X - currentTile.X;
            int dy = targetTile.Y - currentTile.Y;
            
            // Create a simple path by moving in the dominant direction first
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            for (int i = 0; i < steps; i++)
            {
                int x = currentTile.X;
                int y = currentTile.Y;
                
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    x += Math.Sign(dx);
                }
                else
                {
                    y += Math.Sign(dy);
                }
                
                // Check if the tile is walkable
                if (IsTileWalkable(x, y))
                {
                    movementPath.Push(new Point(x, y));
                }
                
                currentTile = new Point(x, y);
                dx = targetTile.X - currentTile.X;
                dy = targetTile.Y - currentTile.Y;
            }
        }

        private bool IsTileWalkable(int x, int y)
        {
            Farm farm = Game1.getFarm();
            
            // Check if tile is within bounds
            if (x < 0 || y < 0 || x >= farm.map.Layers[0].LayerWidth || y >= farm.map.Layers[0].LayerHeight)
                return false;
                
            // Check if there are any colliding objects
            foreach (var obj in farm.objects.Values)
            {
                if (obj.TileLocation.X == x && obj.TileLocation.Y == y)
                    return false;
            }
            
            // Check if there are any terrain features
            Vector2 tilePos = new Vector2(x, y);
            if (farm.terrainFeatures.ContainsKey(tilePos))
                return false;
                
            // Check tile properties
            string tileProp = farm.doesTileHaveProperty(x, y, "Water", "Back");
            if (tileProp != null)
                return false;
                
            return true;
        }

        private void MoveNPC()
        {
            if (customNPC == null || Game1.currentLocation != Game1.getFarm() || movementPath.Count == 0)
                return;

            Point nextTile = movementPath.Pop();
            Vector2 nextPosition = new Vector2(nextTile.X * 64, nextTile.Y * 64);
            
            // Determine facing direction
            Vector2 direction = nextPosition - customNPC.Position;
            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                customNPC.FacingDirection = direction.X > 0 ? 1 : 3; // Right or Left
            }
            else
            {
                customNPC.FacingDirection = direction.Y > 0 ? 2 : 0; // Down or Up
            }
            
            // Move to the next position
            customNPC.Position = nextPosition;
            
            // Reset animation frame when changing direction
            animationFrame = 0;
            if (customNPC != null)
            {
                customNPC.Sprite.currentFrame = customNPC.FacingDirection * 4 + animationFrame;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Check if the player is pressing the action button
            if (e.Button == SButton.MouseLeft || e.Button == SButton.ControllerA)
            {
                // Check if player is close to the NPC
                if (customNPC != null && Utility.tileWithinRadiusOfPlayer(
                        (int)(customNPC.Position.X / 64), 
                        (int)(customNPC.Position.Y / 64), 
                        4, // Increased radius for easier interaction
                        Game1.player))
                {
                    // Show dialogue
                    ShowDialogue();
                }
            }
        }

        private void ShowDialogue()
        {
            if (dialogues != null && dialogues.Count > 0)
            {
                // Pick a random dialogue
                string dialogue = dialogues[random.Next(dialogues.Count)];
                
                // Show dialogue above the NPC's head
                customNPC.showTextAboveHead(dialogue);
            }
        }

        private void AddCustomNPC()
        {
            // Check if NPC already exists
            NPC existingNPC = Game1.getCharacterFromName(npcName);
            if (existingNPC != null)
            {
                customNPC = existingNPC;
                return;
            }

            // Create a new NPC using the proper constructor
            // Place the NPC in a more visible location near the player's start position
            // Using hardcoded path to the character sprite
            customNPC = new NPC(
                new AnimatedSprite("Characters\\Aria", 0, 16, 32), // Path to your custom sprite
                new Vector2(64, 15) * 4,  // More visible position near the farmhouse
                2,  // Facing direction (down)
                npcName  // Name
            )
            {
                // Set additional properties
                DefaultMap = "Farm"
            };

            // Add the NPC to the game
            Game1.getFarm().addCharacter(customNPC);
            
            Monitor.Log($"{npcName} added to the farm at position {customNPC.Position}.", LogLevel.Info);
        }
    }
}