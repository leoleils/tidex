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
using Microsoft.Xna.Framework;
using xTile.Dimensions;

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
        private int _interactionCount = 0; // Track number of interactions per day
        private List<string> _playerResponseChoices = new List<string>(); // Player response choices
        private bool _waitingForPlayerResponse = false; // Track if we're waiting for player input
        private int _lastApproachTime = -1; // Track last time Aria approached the player
        private int _approachCooldown = 1800; // 30 second cooldown between approaches (30 * 60 ticks)
        private NPC _ariaNPC; // Reference to Aria NPC object
        private bool _isPathfindingActive = false; // Track if Aria is currently pathfinding to player
        private Vector2 _targetPlayerPosition; // Target position for pathfinding
        private int _pathfindingStartTime = -1; // When pathfinding started
        private const int MaxPathfindingDuration = 3000; // Maximum time to attempt pathfinding (50 seconds)
        private Dictionary<string, int> _playerBehaviorPatterns = new Dictionary<string, int>(); // Track player behavior patterns
        private List<string> _dailyInteractionLog = new List<string>(); // Log interactions for the day

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
            _ariaAI = new AriaAI(_config.Url, _config.Apikey, _config.Model, _config);
            
            // Hook into events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += OnPlayerWarped;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            
            this.Monitor.Log("Aria AI mod loaded successfully!", LogLevel.Info);
        }
        
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
                _interactionCount = 0; // Reset interaction count
                _lastApproachTime = -1; // Reset approach time
                _dailyInteractionLog.Clear(); // Clear daily interaction log
                _isPathfindingActive = false; // Reset pathfinding state
            }
            
            // Update pathfinding if active
            if (_isPathfindingActive)
            {
                UpdatePathfinding();
            }
            
            // Enhanced AI interaction based on player patterns
            CheckDynamicInteractions();
        }

        /// <summary>Raised when the time changes in-game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnTimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            // Check for special interaction times based on player behavior
            CheckBehaviorBasedInteractions(e.NewTime);
        }

        /// <summary>Raised when a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnDayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            // Reset daily counters and logs
            _interactionCount = 0;
            _dailyInteractionLog.Clear();
            _isPathfindingActive = false;
            
            // Analyze player behavior from previous days
            AnalyzePlayerBehavior();
            
            // Schedule Aria's day based on player patterns
            ScheduleAriaDay();
        }

        /// <summary>Raised after the player moves to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnPlayerWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            // Check if Aria is in the same location as the player
            if (e.NewLocation != null && e.Player == Game1.player)
            {
                TryLocationBasedInteraction(e.NewLocation);
            }
        }

        /// <summary>Raised when a button is pressed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            // Handle number key presses for dialogue choices
            if (Game1.currentSpeaker != null && 
                Game1.currentSpeaker.Name.Equals("Aria", StringComparison.OrdinalIgnoreCase) &&
                _waitingForPlayerResponse)
            {
                string playerResponse = null;
                
                switch (e.Button)
                {
                    case SButton.NumPad1:
                    case SButton.D1:
                        if (_playerResponseChoices.Count > 0)
                            playerResponse = _playerResponseChoices[0];
                        break;
                    case SButton.NumPad2:
                    case SButton.D2:
                        if (_playerResponseChoices.Count > 1)
                            playerResponse = _playerResponseChoices[1];
                        break;
                    case SButton.NumPad3:
                    case SButton.D3:
                        if (_playerResponseChoices.Count > 2)
                            playerResponse = _playerResponseChoices[2];
                        break;
                    case SButton.NumPad4:
                    case SButton.D4:
                        if (_playerResponseChoices.Count > 3)
                            playerResponse = _playerResponseChoices[3];
                        break;
                }
                
                if (playerResponse != null)
                {
                    // Send the player's choice to the AI
                    _waitingForPlayerResponse = false;
                    _isGeneratingResponse = true;
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    _ = GenerateAIResponse(playerResponse, _cancellationTokenSource.Token);
                }
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

        /// <summary>Try to have Aria approach the player with pathfinding.</summary>
        private async void TryApproachPlayer()
        {
            try
            {
                this.Monitor.Log($"[Aria AI Debug] TryApproachPlayer called - isGenerating: {_isGeneratingResponse}, cooldown: {Game1.ticks - _lastApproachTime}, cooldownNeeded: {_approachCooldown}", LogLevel.Debug);
                
                // Check if we're already generating a response to prevent overlapping
                if (_isGeneratingResponse)
                {
                    this.Monitor.Log($"[Aria AI Debug] Approach denied - response generation in progress", LogLevel.Debug);
                    return;
                }
                    
                // Check if enough time has passed since last approach
                int cooldownRemaining = Game1.ticks - _lastApproachTime;
                if (cooldownRemaining < _approachCooldown)
                {
                    this.Monitor.Log($"[Aria AI Debug] Approach denied - cooldown active ({_approachCooldown - cooldownRemaining} ticks remaining)", LogLevel.Debug);
                    return;
                }
                    
                // Check if player is in a valid location for Aria to approach
                if (Game1.player.currentLocation != null && Game1.currentSpeaker == null)
                {
                    this.Monitor.Log($"[Aria AI Debug] Player location valid: {Game1.player.currentLocation.Name}, no speaker active", LogLevel.Debug);
                    
                    // Find Aria in the current location
                    _ariaNPC = Game1.player.currentLocation.characters.FirstOrDefault(npc => 
                        npc.Name.Equals("Aria", StringComparison.OrdinalIgnoreCase));
                    
                    this.Monitor.Log($"[Aria AI Debug] Aria found in location: {_ariaNPC != null}, pathfinding active: {_isPathfindingActive}", LogLevel.Debug);
                    
                    if (_ariaNPC != null && !_isPathfindingActive)
                    {
                        // Calculate distance to player
                        float distance = Vector2.Distance(
                            _ariaNPC.Tile, 
                            Game1.player.Tile
                        );
                        
                        this.Monitor.Log($"[Aria AI Debug] Distance to player: {distance} tiles", LogLevel.Debug);
                        
                        // If Aria is far from player, initiate pathfinding
                        if (distance > 3f) // More than 3 tiles away
                        {
                            this.Monitor.Log($"[Aria AI Debug] Starting pathfinding to player", LogLevel.Info);
                            StartPathfindingToPlayer();
                        }
                        else
                        {
                            this.Monitor.Log($"[Aria AI Debug] Aria close enough, starting conversation", LogLevel.Info);
                            // Aria is close enough, start conversation
                            await InitiateProactiveConversation();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error in TryApproachPlayer: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>Start pathfinding to bring Aria to the player's location.</summary>
        private void StartPathfindingToPlayer()
        {
            if (_ariaNPC == null) 
            {
                this.Monitor.Log($"[Aria AI Debug] Cannot start pathfinding - Aria NPC is null", LogLevel.Debug);
                return;
            }
            
            _isPathfindingActive = true;
            _targetPlayerPosition = Game1.player.Tile;
            _pathfindingStartTime = Game1.ticks;
            _lastApproachTime = Game1.ticks; // Reset cooldown
            
            this.Monitor.Log($"[Aria AI Debug] Aria starting pathfinding to player at position {_targetPlayerPosition}", LogLevel.Info);
        }

        /// <summary>Update Aria's pathfinding movement.</summary>
        private void UpdatePathfinding()
        {
            if (!_isPathfindingActive || _ariaNPC == null) 
            {
                this.Monitor.Log($"[Aria AI Debug] Pathfinding update skipped - active: {_isPathfindingActive}, Aria: {_ariaNPC != null}", LogLevel.Debug);
                return;
            }
            
            try
            {
                // Check if pathfinding has timed out
                int elapsedTicks = Game1.ticks - _pathfindingStartTime;
                if (elapsedTicks > MaxPathfindingDuration)
                {
                    _isPathfindingActive = false;
                    this.Monitor.Log($"[Aria AI Debug] Pathfinding timed out after {elapsedTicks} ticks", LogLevel.Info);
                    return;
                }
                
                // Update target position if player moved
                _targetPlayerPosition = Game1.player.Tile;
                
                // Calculate current distance
                float distance = Vector2.Distance(
                    _ariaNPC.Tile, 
                    _targetPlayerPosition
                );
                
                this.Monitor.Log($"[Aria AI Debug] Pathfinding - distance to player: {distance} tiles", LogLevel.Debug);
                
                // If close enough, stop pathfinding and initiate conversation
                if (distance <= 2f)
                {
                    this.Monitor.Log($"[Aria AI Debug] Pathfinding complete - starting conversation", LogLevel.Info);
                    _isPathfindingActive = false;
                    _ = InitiateProactiveConversation();
                    return;
                }
                
                // Move Aria towards player (simple pathfinding)
                this.Monitor.Log($"[Aria AI Debug] Moving Aria towards player", LogLevel.Debug);
                MoveAriaTowardsPlayer();
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error in UpdatePathfinding: {ex.Message}", LogLevel.Error);
                _isPathfindingActive = false;
            }
        }

        /// <summary>Move Aria towards the player's position.</summary>
        private void MoveAriaTowardsPlayer()
        {
            if (_ariaNPC == null) return;
            
            Vector2 ariaPos = _ariaNPC.Tile;
            Vector2 playerPos = Game1.player.Tile;
            
            // Calculate direction
            Vector2 direction = playerPos - ariaPos;
            direction.Normalize();
            
            // Round to cardinal directions for simpler movement
            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                direction.Y = 0;
                direction.X = Math.Sign(direction.X);
            }
            else
            {
                direction.X = 0;
                direction.Y = Math.Sign(direction.Y);
            }
            
            // Check if the next position is walkable
            Vector2 nextPos = ariaPos + direction;
            try
            {
                // Simplified movement check - just check if tile is within bounds
                if (nextPos.X >= 0 && nextPos.Y >= 0 && 
                    nextPos.X < _ariaNPC.currentLocation.map.DisplayWidth && 
                    nextPos.Y < _ariaNPC.currentLocation.map.DisplayHeight)
                {
                    _ariaNPC.setTileLocation(nextPos);
                    
                    // Update facing direction
                    if (direction.X > 0) _ariaNPC.FacingDirection = 1; // Right
                    else if (direction.X < 0) _ariaNPC.FacingDirection = 3; // Left
                    else if (direction.Y > 0) _ariaNPC.FacingDirection = 2; // Down
                    else if (direction.Y < 0) _ariaNPC.FacingDirection = 0; // Up
                }
            }
            catch (Exception)
            {
                // If any error occurs, just skip movement
            }
        }

        /// <summary>Initiate a proactive conversation with the player.</summary>
        private async Task InitiateProactiveConversation()
        {
            try
            {
                this.Monitor.Log($"[Aria AI Debug] Initiating proactive conversation", LogLevel.Info);
                
                // Ensure Aria is facing the player
                if (_ariaNPC != null)
                {
                    _ariaNPC.faceGeneralDirection(Game1.player.Tile);
                    this.Monitor.Log($"[Aria AI Debug] Aria facing player", LogLevel.Debug);
                }
                
                // Build context information
                var context = new Dictionary<string, string>
                {
                    ["Time"] = Game1.timeOfDay.ToString(),
                    ["Season"] = Game1.currentSeason,
                    ["DayOfWeek"] = Game1.dayOfMonth.ToString(),
                    ["PlayerName"] = Game1.player.Name,
                    ["Location"] = Game1.player.currentLocation?.Name ?? "Unknown",
                    ["InteractionType"] = "ProactiveApproach",
                    ["PlayerBehavior"] = GetPlayerBehaviorSummary()
                };

                this.Monitor.Log($"[Aria AI Debug] Context: {string.Join(", ", context.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}", LogLevel.Debug);

                // Generate contextual greeting from AI
                string prompt = GenerateProactivePrompt(context);
                this.Monitor.Log($"[Aria AI Debug] Generated prompt: {prompt}", LogLevel.Debug);
                
                var response = await _ariaAI.GenerateResponse(prompt, context);
                
                // Show the dialogue to the player
                Game1.drawDialogueNoTyping(response.Dialogue);
                this.Monitor.Log($"Aria proactively approached player with: {response.Dialogue}", LogLevel.Info);
                
                // Log interaction
                _dailyInteractionLog.Add($"{Game1.timeOfDay}: Proactive approach - {response.Dialogue}");
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error in InitiateProactiveConversation: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>Try location-based interaction when player warps to a new location.</summary>
        /// <param name="location">The location the player warped to.</param>
        private async void TryLocationBasedInteraction(GameLocation location)
        {
            try
            {
                this.Monitor.Log($"[Aria AI Debug] Checking location-based interaction - Location: {location?.Name}, Count: {_interactionCount}, LastApproach: {_lastApproachTime}, CurrentTicks: {Game1.ticks}", LogLevel.Debug);
                
                // Only interact if we haven't reached the daily limit (max 2 interactions per day)
                if (_interactionCount >= 2)
                {
                    this.Monitor.Log($"[Aria AI Debug] Location interaction denied - daily limit reached ({_interactionCount}/2)", LogLevel.Debug);
                    return;
                }
                    
                // Add cooldown between location-based interactions
                int cooldownRemaining = Game1.ticks - _lastApproachTime;
                if (Game1.ticks - _lastApproachTime < 9000) // 150 second cooldown
                {
                    this.Monitor.Log($"[Aria AI Debug] Location interaction denied - cooldown active ({9000 - cooldownRemaining} ticks remaining)", LogLevel.Debug);
                    return;
                }
                    
                // Check if Aria is in the same location
                _ariaNPC = location.characters.FirstOrDefault(npc => npc.Name.Equals("Aria", StringComparison.OrdinalIgnoreCase));
                
                this.Monitor.Log($"[Aria AI Debug] Aria found in location: {_ariaNPC != null} (Location: {location?.Name})", LogLevel.Debug);
                
                if (_ariaNPC != null)
                {
                    // Reduced chance for location-based interactions
                    int baseChance = 15; // Reduced from 30
                    string locationKey = $"Visited_{location.Name}";
                    
                    if (_playerBehaviorPatterns.ContainsKey(locationKey))
                    {
                        // Reduce chance for frequently visited locations
                        baseChance = Math.Max(5, baseChance - (_playerBehaviorPatterns[locationKey] * 3));
                    }
                    
                    int roll = new Random().Next(100);
                    bool shouldInteract = roll < baseChance;
                    
                    this.Monitor.Log($"[Aria AI Debug] Location interaction chance: {baseChance}%, roll: {roll}, interact: {shouldInteract}", LogLevel.Debug);
                    
                    if (shouldInteract)
                    {
                        this.Monitor.Log($"[Aria AI Debug] Initiating location-based interaction at {location.Name}", LogLevel.Info);
                        
                        // Build enhanced context information
                        var context = new Dictionary<string, string>
                        {
                            ["Time"] = Game1.timeOfDay.ToString(),
                            ["Season"] = Game1.currentSeason,
                            ["DayOfWeek"] = Game1.dayOfMonth.ToString(),
                            ["PlayerName"] = Game1.player.Name,
                            ["Location"] = location.Name,
                            ["InteractionType"] = "LocationBased",
                            ["VisitCount"] = _playerBehaviorPatterns.ContainsKey(locationKey) ? _playerBehaviorPatterns[locationKey].ToString() : "1",
                            ["PlayerBehavior"] = GetPlayerBehaviorSummary()
                        };

                        // Generate location-appropriate greeting from AI
                        string prompt = $"I'm at {location.Name} and I see {Game1.player.Name} has arrived. This is their {(_playerBehaviorPatterns.ContainsKey(locationKey) ? _playerBehaviorPatterns[locationKey] + 1 : 1)} time here today. I'll greet them appropriately.";
                        var response = await _ariaAI.GenerateResponse(prompt, context);
                        
                        // Show the dialogue to the player
                        Game1.drawDialogueNoTyping(response.Dialogue);
                        this.Monitor.Log($"Aria initiated location-based interaction: {response.Dialogue}", LogLevel.Info);
                        _interactionCount++;
                        _lastApproachTime = Game1.ticks; // Update cooldown
                        
                        // Log interaction
                        _dailyInteractionLog.Add($"{Game1.timeOfDay}: Location-based at {location.Name} - {response.Dialogue}");
                        
                        // Update behavior tracking
                        if (_playerBehaviorPatterns.ContainsKey(locationKey))
                            _playerBehaviorPatterns[locationKey]++;
                        else
                            _playerBehaviorPatterns[locationKey] = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error in TryLocationBasedInteraction: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>Check for dynamic interactions based on player behavior patterns.</summary>
        private void CheckDynamicInteractions()
        {
            this.Monitor.Log($"[Aria AI Debug] Checking dynamic interactions - Count: {_interactionCount}, LastApproach: {_lastApproachTime}, CurrentTicks: {Game1.ticks}, Time: {Game1.timeOfDay}", LogLevel.Debug);
            
            // Check for various interaction triggers based on time and player behavior
            if (_interactionCount >= 3)
            {
                this.Monitor.Log($"[Aria AI Debug] Daily limit reached ({_interactionCount}/3)", LogLevel.Debug);
                return;
            }
            
            // Add cooldown between interactions
            int cooldownRemaining = Game1.ticks - _lastApproachTime;
            if (Game1.ticks - _lastApproachTime < 6000) // 100 second cooldown
            {
                this.Monitor.Log($"[Aria AI Debug] Cooldown active ({6000 - cooldownRemaining} ticks remaining)", LogLevel.Debug);
                return;
            }
                
            // Morning interaction (8:30-9:30 AM only)
            if (Game1.timeOfDay >= 830 && Game1.timeOfDay <= 930)
            {
                bool shouldInteract = ShouldInitiateMorningInteraction();
                this.Monitor.Log($"[Aria AI Debug] Morning interaction check: {shouldInteract} (time: {Game1.timeOfDay})", LogLevel.Debug);
                if (shouldInteract)
                {
                    this.Monitor.Log($"[Aria AI Debug] Initiating morning interaction", LogLevel.Info);
                    TryApproachPlayer();
                }
            }
            
            // Evening interaction (5:00-6:00 PM only)
            if (Game1.timeOfDay >= 1700 && Game1.timeOfDay <= 1800)
            {
                bool shouldInteract = ShouldInitiateEveningInteraction();
                this.Monitor.Log($"[Aria AI Debug] Evening interaction check: {shouldInteract} (time: {Game1.timeOfDay})", LogLevel.Debug);
                if (shouldInteract)
                {
                    this.Monitor.Log($"[Aria AI Debug] Initiating evening interaction", LogLevel.Info);
                    TryApproachPlayer();
                }
            }
        }

        /// <summary>Check behavior-based interactions at specific times.</summary>
        /// <param name="newTime">The new game time.</param>
        private void CheckBehaviorBasedInteractions(int newTime)
        {
            // Track player time-based patterns
            string timeKey = $"Active_{newTime / 100}00"; // Group by hour
            if (_playerBehaviorPatterns.ContainsKey(timeKey))
                _playerBehaviorPatterns[timeKey]++;
            else
                _playerBehaviorPatterns[timeKey] = 1;
        }

        /// <summary>Generate a proactive prompt based on context.</summary>
        /// <param name="context">The interaction context.</param>
        /// <returns>Appropriate AI prompt.</returns>
        private string GenerateProactivePrompt(Dictionary<string, string> context)
        {
            string basePrompt;
            
            // Use configured proactive prompt if available
            if (_config?.PromptProfiles?.ContainsKey(_config.ActivePromptProfile) == true &&
                !string.IsNullOrEmpty(_config.PromptProfiles[_config.ActivePromptProfile].Proactive))
            {
                basePrompt = _config.PromptProfiles[_config.ActivePromptProfile].Proactive;
            }
            else
            {
                basePrompt = "I noticed you're here at {time} in {location}. What brings you here today?";
            }
            
            // Replace placeholders with actual values
            string time = context.ContainsKey("Time") ? context["Time"] : "this time";
            string location = context.ContainsKey("Location") ? context["Location"] : "here";
            string season = context.ContainsKey("Season") ? context["Season"] : "this season";
            
            basePrompt = basePrompt
                .Replace("{time}", time)
                .Replace("{location}", location)
                .Replace("{season}", season);
            
            return basePrompt;
        }

        /// <summary>Analyze player behavior patterns to inform interactions.</summary>
        private void AnalyzePlayerBehavior()
        {
            // This would typically load from persistent storage
            // For now, we'll use the in-memory tracking
            this.Monitor.Log("Analyzing player behavior patterns...", LogLevel.Debug);
        }

        /// <summary>Schedule Aria's day based on player patterns.</summary>
        private void ScheduleAriaDay()
        {
            // Dynamic scheduling based on player behavior
            string mostActiveTime = GetMostActivePlayerTime();
            this.Monitor.Log($"Player most active at: {mostActiveTime}", LogLevel.Debug);
            
            // Adjust Aria's schedule to be more likely to interact during player active times
            // This is a placeholder for more sophisticated scheduling
        }

        /// <summary>Get summary of player behavior for AI context.</summary>
        /// <returns>Behavior summary string.</returns>
        private string GetPlayerBehaviorSummary()
        {
            if (_playerBehaviorPatterns.Count == 0)
                return "New to the area";
            
            var topLocations = _playerBehaviorPatterns
                .Where(kvp => kvp.Key.StartsWith("Visited_"))
                .OrderByDescending(kvp => kvp.Value)
                .Take(3)
                .Select(kvp => kvp.Key.Replace("Visited_", ""))
                .ToList();
            
            if (topLocations.Any())
                return $"Frequent visitor to: {string.Join(", ", topLocations)}";
            
            return "Enjoys exploring different areas";
        }

        /// <summary>Check if should initiate morning interaction.</summary>
        private bool ShouldInitiateMorningInteraction()
        {
            // Enhanced morning interaction logic with lower chance
            return Game1.timeOfDay >= 830 && Game1.timeOfDay <= 930 && 
                   new Random().Next(100) < 5; // 5% chance during morning window
        }

        /// <summary>Check if should initiate evening interaction.</summary>
        private bool ShouldInitiateEveningInteraction()
        {
            // Enhanced evening interaction logic with lower chance
            return Game1.timeOfDay >= 1700 && Game1.timeOfDay <= 1800 && 
                   new Random().Next(100) < 8; // 8% chance during evening window
        }

        /// <summary>Get the most active time for the player.</summary>
        private string GetMostActivePlayerTime()
        {
            var activeTimes = _playerBehaviorPatterns
                .Where(kvp => kvp.Key.StartsWith("Active_"))
                .OrderByDescending(kvp => kvp.Value)
                .FirstOrDefault();
            
            return activeTimes.Key ?? "Varied";
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
                // Only generate a response if we haven't already done so today or if it's a new interaction
                if (_interactionCount < 2)
                {
                    _isSpeakingWithAria = true;
                    _hasGeneratedResponse = true; // Mark that we've generated a response
                    _interactionCount++; // Increment interaction count
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
                _playerResponseChoices.Clear(); // Clear response choices when dialogue ends
                _waitingForPlayerResponse = false; // Reset waiting flag
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
                    ["Location"] = Game1.player.currentLocation?.Name ?? "Unknown",
                    ["InteractionCount"] = _interactionCount.ToString()
                };
                
                // Add conversation history if available
                if (_conversationHistory.Any())
                {
                    context["ConversationHistory"] = string.Join("; ", _conversationHistory.Select(kv => $"{kv.Key}: {kv.Value}"));
                }
                
                // Generate response from AI
                var response = await _ariaAI.GenerateResponse(playerMessage, context);
                _currentAriaResponse = response.Dialogue;
                
                // Store player response choices
                _playerResponseChoices = response.Choices.ToList();
                
                // Create dialogue text with response choices in Stardew Valley format
                string dialogueWithChoices = _currentAriaResponse + GenerateStardewChoicesText();
                
                // Set waiting flag to true as we're expecting player input
                _waitingForPlayerResponse = true;
                
                // Update the dialogue box with the AI response
                UpdateDialogueBoxText(dialogueWithChoices);
                
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
                _waitingForPlayerResponse = true; // Now we're waiting for player response
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
        /// Generate text displaying response choices for the player in Stardew Valley format
        /// </summary>
        /// <returns>Formatted string with response choices</returns>
        private string GenerateStardewChoicesText()
        {
            if (_playerResponseChoices == null || !_playerResponseChoices.Any())
                return "";
                
            string choicesText = "";
            for (int i = 0; i < _playerResponseChoices.Count && i < 4; i++)
            {
                choicesText += $"#{i + 1}# {_playerResponseChoices[i]}";
            }
            return choicesText;
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
                Game1.drawDialogueNoTyping(newText);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error updating dialogue box text: {ex.Message}", LogLevel.Error);
            }
        }
    }
    
}