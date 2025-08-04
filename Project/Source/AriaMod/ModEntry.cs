using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;

namespace AriaMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private AriaAI _ariaAI;
        private ModConfig _config;
        private string _currentAriaResponse = "Hello, @! What brings you to the library today?";
        private bool _isSpeakingWithAria = false;
        private Dictionary<string, string> _conversationHistory = new Dictionary<string, string>();
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isGeneratingResponse = false;
        private int _lastUpdateDay = -1;
        private bool _hasGeneratedResponse = false; // Prevent infinite loops

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Load config
            _config = helper.ReadConfig<ModConfig>();
            
            // Log config for debugging
            this.Monitor.Log($"API URL: {_config.Url}", LogLevel.Debug);
            this.Monitor.Log($"API Key length: {_config.Apikey?.Length ?? 0}", LogLevel.Debug);
            this.Monitor.Log($"Model: {_config.Model}", LogLevel.Debug);
            
            // Initialize AI
            _ariaAI = new AriaAI(_config.Url, _config.Apikey, _config.Model);
            
            // Hook into events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            
            this.Monitor.Log("Aria AI mod loaded successfully!", LogLevel.Info);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the game is launched, right before the first update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // Register a command to test the AI
            this.Helper.ConsoleCommands.Add("aria_test", "Test the Aria AI dialogue system. Usage: aria_test [message]", TestAICommand);
        }

        /// <summary>Raised once per game update tick.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnUpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            // Only run once per second (every 60 ticks)
            if (e.Ticks % 60 != 0)
                return;
                
            // Check if it's a new day to reset our update flag
            if (Game1.Date.TotalDays != _lastUpdateDay)
            {
                _lastUpdateDay = Game1.Date.TotalDays;
                _hasGeneratedResponse = false; // Reset the flag each day
            }
            
            // Check if Aria should approach the player (8:30am to 9:30am)
            if (IsAIInteractionTime())
            {
                TryApproachPlayer();
            }
        }

        /// <summary>Test command for the AI system.</summary>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        private void TestAICommand(string command, string[] args)
        {
            string message = args.Length > 0 ? string.Join(" ", args) : "Hello Aria!";
            this.Monitor.Log($"Testing Aria AI with message: {message}", LogLevel.Info);
            _ = TestAIResponse(message);
        }

        /// <summary>Test the AI response generation.</summary>
        /// <param name="message">The message to send to the AI.</param>
        private async Task TestAIResponse(string message)
        {
            try
            {
                this.Monitor.Log($"Sending message to Aria AI: {message}", LogLevel.Info);
                
                // Build context information
                var context = new Dictionary<string, string>
                {
                    ["Time"] = Game1.timeOfDay.ToString(),
                    ["Season"] = Game1.currentSeason,
                    ["DayOfWeek"] = Game1.dayOfMonth.ToString(),
                    ["PlayerName"] = Game1.player.Name,
                    ["Location"] = Game1.player.currentLocation?.Name ?? "Unknown"
                };
                
                this.Monitor.Log("Context information:", LogLevel.Debug);
                foreach (var kvp in context)
                {
                    this.Monitor.Log($"  {kvp.Key}: {kvp.Value}", LogLevel.Debug);
                }
                
                // Generate response from AI
                this.Monitor.Log("Calling AI API...", LogLevel.Info);
                var response = await _ariaAI.GenerateResponse(message, context);
                this.Monitor.Log($"Aria's response: {response.Dialogue}", LogLevel.Info);
                this.Monitor.Log($"Response choices: {string.Join(", ", response.Choices)}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error testing AI response: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>Try to have Aria approach the player.</summary>
        private async void TryApproachPlayer()
        {
            try
            {
                // Check if player is outside and it's a suitable location for Aria to approach
                if (Game1.player.currentLocation != null && 
                    Game1.player.currentLocation.IsOutdoors && 
                    Game1.currentSpeaker == null)
                {
                    // 25% chance each second during the time window to approach the player
                    if (new Random().Next(100) < 25)
                    {
                        // Build context information
                        var context = new Dictionary<string, string>
                        {
                            ["Time"] = Game1.timeOfDay.ToString(),
                            ["Season"] = Game1.currentSeason,
                            ["DayOfWeek"] = Game1.dayOfMonth.ToString(),
                            ["PlayerName"] = Game1.player.Name,
                            ["Location"] = Game1.player.currentLocation.Name
                        };

                        // Generate a greeting from AI
                        var response = await _ariaAI.GenerateResponse(
                            "I'm out for a morning walk. I see the player nearby. I'll go say hello.", 
                            context
                        );
                        
                        // Show the dialogue to the player
                        Game1.drawDialogueNoTyping(response.Dialogue);
                        this.Monitor.Log($"Aria approached player with: {response.Dialogue}", LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error in TryApproachPlayer: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>Raised after a game menu is opened or closed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnMenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            // Check if we're opening a dialogue box with Aria
            if (e.NewMenu is StardewValley.Menus.DialogueBox dialogueBox && 
                Game1.currentSpeaker != null && 
                Game1.currentSpeaker.Name.Equals("Aria", StringComparison.OrdinalIgnoreCase))
            {
                // Only generate a response if we haven't already done so today
                if (!_hasGeneratedResponse)
                {
                    _isSpeakingWithAria = true;
                    _hasGeneratedResponse = true; // Mark that we've generated a response
                    this.Monitor.Log("Player is speaking with Aria", LogLevel.Info);
                    
                    // Cancel any previous response generation
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    
                    // Generate an AI response
                    _isGeneratingResponse = true;
                    _ = GenerateAIResponse("Hello Aria!", _cancellationTokenSource.Token);
                }
            }
            else if (e.OldMenu is StardewValley.Menus.DialogueBox)
            {
                _isSpeakingWithAria = false;
                _isGeneratingResponse = false;
                _cancellationTokenSource?.Cancel();
            }
        }
        
        /// <summary>
        /// Check if it's time for the AI interaction (8:30am to 9:30am).
        /// </summary>
        /// <returns>True if it's time for AI interaction.</returns>
        private bool IsAIInteractionTime()
        {
            // Check if it's between 8:30am (0830) and 9:30am (0930)
            return Game1.timeOfDay >= 830 && Game1.timeOfDay <= 930;
        }
        
        /// <summary>
        /// Generate an AI response for Aria based on the player's message and context.
        /// </summary>
        /// <param name="playerMessage">The player's message</param>
        /// <param name="cancellationToken">Cancellation token to stop the generation</param>
        private async Task GenerateAIResponse(string playerMessage, CancellationToken cancellationToken)
        {
            try
            {
                // Build context information
                var context = new Dictionary<string, string>
                {
                    ["Time"] = Game1.timeOfDay.ToString(),
                    ["Season"] = Game1.currentSeason,
                    ["DayOfWeek"] = Game1.dayOfMonth.ToString(),
                    ["PlayerName"] = Game1.player.Name,
                    ["Location"] = Game1.player.currentLocation?.Name ?? "Unknown"
                };
                
                // Add conversation history if available
                if (_conversationHistory.Any())
                {
                    context["ConversationHistory"] = string.Join("; ", _conversationHistory.Select(kv => $"{kv.Key}: {kv.Value}"));
                }
                
                // Generate response from AI
                var response = await _ariaAI.GenerateResponse(playerMessage, context);
                _currentAriaResponse = response.Dialogue;
                
                // Update the dialogue box with the AI response
                UpdateDialogueBoxText(_currentAriaResponse);
                
                // Store final response in conversation history
                _conversationHistory[DateTime.Now.ToString()] = $"Player: {playerMessage} | Aria: {response.Dialogue}";
                
                // Limit conversation history size
                if (_conversationHistory.Count > 5)
                {
                    var firstKey = _conversationHistory.Keys.First();
                    _conversationHistory.Remove(firstKey);
                }
                
                this.Monitor.Log($"Aria says: {response.Dialogue}", LogLevel.Info);
                _isGeneratingResponse = false;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error generating AI response: {ex.Message}", LogLevel.Error);
                this.Monitor.Log($"Stack trace: {ex.StackTrace}", LogLevel.Error);
                _isGeneratingResponse = false;
                
                // Show an error message to the player
                UpdateDialogueBoxText("I'm sorry, I didn't quite catch that. Could you repeat it?");
            }
        }
        
        /// <summary>
        /// Update the text in the current dialogue box.
        /// </summary>
        /// <param name="newText">The new text to display</param>
        private void UpdateDialogueBoxText(string newText)
        {
            try
            {
                if (Game1.currentSpeaker == null) 
                    return;
                
                // Close the current dialogue box
                if (Game1.activeClickableMenu is DialogueBox)
                {
                    Game1.exitActiveMenu();
                }
                
                // Show the new dialogue text
                Game1.drawDialogueNoTyping(newText + "$h#$b#");
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error updating dialogue box text: {ex.Message}", LogLevel.Error);
            }
        }
    }
    
    /// <summary>The mod configuration.</summary>
    public class ModConfig
    {
        public string Url { get; set; } = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
        public string Apikey { get; set; } = "";
        public string Model { get; set; } = "qwen3-30b-a3b";
    }
}