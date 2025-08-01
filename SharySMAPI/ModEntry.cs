using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Threading.Tasks;

namespace SharySMAPI
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private IMonitor CustomMonitor;
        private IModHelper CustomHelper;
        private MemorySystem Memory;
        private QwenIntegration Qwen;
        private bool IsInConversationWithShary = false;
        
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            this.CustomMonitor = this.Monitor;
            this.CustomHelper = helper;
            this.Memory = new MemorySystem(helper, this.Monitor);
            this.Qwen = new QwenIntegration(helper, this.Monitor, this.Memory);
            
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            
            // Register command to view interaction logs
            helper.ConsoleCommands.Add("shary_logs", "View Shary interaction logs. Usage: shary_logs [date] or shary_logs all", this.HandleSharyLogsCommand);
        }
        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Shary SMAPI mod loaded successfully!", LogLevel.Info);
        }
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Log the start of a new day
            this.Memory.AddInteractionLogEntry("System", $"New day started: {Game1.currentSeason} {Game1.dayOfMonth}", "day_start");
            
            // Schedule Shary's special events based on time of day
            this.CustomHelper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        }
        
        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            // Check if it's time for Shary to perform special actions
            if (e.NewTime == 1300) // 1:00 PM
            {
                // Trigger afternoon walk
                this.TriggerAfternoonWalk();
            }
            else if (e.NewTime == 1900) // 7:00 PM
            {
                // Trigger evening return home
                this.TriggerEveningReturn();
            }
        }
        
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // Check if player entered the library
            if (e.NewLocation.Name.Equals("Library"))
            {
                // Check if Shary is in the library
                var shary = e.NewLocation.characters.FirstOrDefault(c => c.Name == "Shary");
                if (shary != null)
                {
                    this.Monitor.Log("Player entered the library where Shary is present.", LogLevel.Info);
                    this.Memory.AddInteractionLogEntry("Player", "Entered library where Shary is present", "location");
                }
            }
        }
        
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Check for interaction with Shary
            if (e.Button == SButton.MouseLeft || e.Button == SButton.ControllerA)
            {
                var npc = this.GetShary();
                if (npc != null && Game1.player.currentLocation.characters.Contains(npc))
                {
                    // Check if player is close enough to interact
                    var distance = Utility.distance(Game1.player.Position.X, Game1.player.Position.Y, npc.Position.X, npc.Position.Y);
                    if (distance < 100f)
                    {
                        // Log the interaction
                        this.Memory.AddInteractionLogEntry("Player", $"Interacted with Shary at {Game1.player.currentLocation.Name}", "interaction");
                        
                        // Handle interaction with Qwen integration
                        this.IsInConversationWithShary = true;
                        this.HandleSharyInteraction(npc);
                    }
                }
            }
        }
        
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            // Detect when dialogue is closed to end conversation
            if (this.IsInConversationWithShary && Game1.activeClickableMenu == null)
            {
                this.IsInConversationWithShary = false;
                this.Memory.AddInteractionLogEntry("System", "Conversation with Shary ended", "conversation_end");
            }
        }
        
        private NPC GetShary()
        {
            foreach (var location in Game1.locations)
            {
                var shary = location.characters.FirstOrDefault(n => n.Name == "Shary");
                if (shary != null)
                    return shary;
            }
            return null;
        }
        
        private async void HandleSharyInteraction(NPC shary)
        {
            // Get player's friendship level with Shary
            var friendshipLevel = Game1.player.friendshipData.TryGetValue("Shary", out var friendship) ? friendship.Points : 0;
            
            // Store this interaction in memory
            this.Memory.AddMemory("interaction", $"Player interacted with Shary at {Game1.player.currentLocation.Name}", 1.5);
            
            // Use Qwen to generate a response based on context and memory
            var context = this.Memory.GetContext();
            
            // Create a dialogue box for streaming output
            var dialogueBox = new DialogueBox("");
            Game1.activeClickableMenu = dialogueBox;
            
            // Generate streaming response
            var response = await this.Qwen.GenerateStreamingResponse(context, friendshipLevel, (token) => {
                // Update the dialogue box with each token
                var currentText = dialogueBox.getCurrentString() ?? "";
                dialogueBox.dialogues[0] = currentText + token;
            });
            
            // Store the response in memory and dialogue history
            this.Memory.AddMemory("dialogue", $"Shary said: {response}", 1.0);
            this.Memory.AddDialogueEntry("Shary", response);
        }
        
        private void HandleSharyLogsCommand(string command, string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    // Show logs for current date
                    var currentDate = this.GetCurrentGameDate();
                    this.ShowLogsForDate(currentDate);
                }
                else if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    // Show all logs
                    this.ShowAllLogs();
                }
                else
                {
                    // Show logs for specified date
                    this.ShowLogsForDate(args[0]);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error handling shary_logs command: {ex.Message}", LogLevel.Error);
            }
        }
        
        private void ShowLogsForDate(string date)
        {
            var logs = this.Memory.GetInteractionLogsForDate(date);
            if (logs.Any())
            {
                this.Monitor.Log($"=== Interaction Logs for {date} ===", LogLevel.Info);
                foreach (var log in logs)
                {
                    this.Monitor.Log($"{log.GameTime} [{log.Type}] {log.Speaker}: {log.Message}", LogLevel.Info);
                }
            }
            else
            {
                this.Monitor.Log($"No interaction logs found for {date}", LogLevel.Info);
            }
        }
        
        private void ShowAllLogs()
        {
            var allLogs = this.Memory.GetInteractionLogs();
            if (allLogs.Any())
            {
                this.Monitor.Log("=== All Interaction Logs ===", LogLevel.Info);
                foreach (var kvp in allLogs.OrderBy(k => k.Key))
                {
                    this.Monitor.Log($"\n--- {kvp.Key} ---", LogLevel.Info);
                    foreach (var log in kvp.Value)
                    {
                        this.Monitor.Log($"{log.GameTime} [{log.Type}] {log.Speaker}: {log.Message}", LogLevel.Info);
                    }
                }
            }
            else
            {
                this.Monitor.Log("No interaction logs found", LogLevel.Info);
            }
        }
        
        private string GetCurrentGameDate()
        {
            try
            {
                // Format: Season + Day (e.g., "s01" for Spring 1st, "w15" for Winter 15th)
                var season = Game1.currentSeason.ToLower().Substring(0, 1); // First letter of season
                var day = Game1.dayOfMonth.ToString("D2"); // Day with leading zero
                return $"{season}{day}";
            }
            catch
            {
                // Fallback to current date if game date is not available
                return DateTime.Now.ToString("MMdd");
            }
        }
        
        private void TriggerAfternoonWalk()
        {
            // This would implement Shary's afternoon walk behavior
            this.Monitor.Log("Shary is going for her afternoon walk.", LogLevel.Info);
            this.Memory.AddInteractionLogEntry("System", "Shary is going for her afternoon walk", "scheduled_event");
        }
        
        private void TriggerEveningReturn()
        {
            // This would implement Shary's evening return home behavior
            this.Monitor.Log("Shary is returning home for the evening.", LogLevel.Info);
            this.Memory.AddInteractionLogEntry("System", "Shary is returning home for the evening", "scheduled_event");
        }
    }
}
