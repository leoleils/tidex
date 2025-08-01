using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;

namespace SharySMAPI
{
    public class MemorySystem
    {
        private IModHelper Helper;
        private IMonitor Monitor;
        private List<MemoryEntry> Memories;
        private List<DialogueEntry> DialogueHistory;
        private Dictionary<string, List<InteractionLogEntry>> InteractionLogs; // Key: Game date (e01, s02, etc.)
        private const int MaxMemories = 50; // Limit to prevent memory bloat
        private const int MaxDialogueHistory = 20; // Keep last 20 exchanges
        
        public MemorySystem(IModHelper helper, IMonitor monitor)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.Memories = new List<MemoryEntry>();
            this.DialogueHistory = new List<DialogueEntry>();
            this.InteractionLogs = new Dictionary<string, List<InteractionLogEntry>>();
            this.LoadMemories();
            this.LoadDialogueHistory();
            this.LoadInteractionLogs();
        }
        
        public void AddMemory(string category, string content, double importance = 1.0)
        {
            var memory = new MemoryEntry
            {
                Category = category,
                Content = content,
                Timestamp = DateTime.Now,
                Importance = importance
            };
            
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
        
        public void AddDialogueEntry(string speaker, string message)
        {
            var entry = new DialogueEntry
            {
                Speaker = speaker,
                Message = message,
                Timestamp = DateTime.Now
            };
            
            this.DialogueHistory.Add(entry);
            
            // Keep only the most recent entries
            if (this.DialogueHistory.Count > MaxDialogueHistory)
            {
                this.DialogueHistory.RemoveAt(0);
            }
            
            // Also log to interaction logs
            this.AddInteractionLogEntry(speaker, message, "dialogue");
            
            this.SaveDialogueHistory();
        }
        
        public void AddInteractionLogEntry(string speaker, string message, string type)
        {
            // Get current game date in format "s01", "e02", etc. (season + day)
            var gameDate = this.GetCurrentGameDate();
            
            if (!this.InteractionLogs.ContainsKey(gameDate))
            {
                this.InteractionLogs[gameDate] = new List<InteractionLogEntry>();
            }
            
            var entry = new InteractionLogEntry
            {
                Speaker = speaker,
                Message = message,
                Type = type,
                GameTime = this.GetCurrentGameTime(),
                Timestamp = DateTime.Now
            };
            
            this.InteractionLogs[gameDate].Add(entry);
            
            // Save the interaction logs
            this.SaveInteractionLogs();
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
        
        public List<DialogueEntry> GetDialogueHistory(int limit = 10)
        {
            return this.DialogueHistory
                .OrderByDescending(d => d.Timestamp)
                .Take(limit)
                .Reverse() // Show in chronological order
                .ToList();
        }
        
        public Dictionary<string, List<InteractionLogEntry>> GetInteractionLogs()
        {
            return this.InteractionLogs;
        }
        
        public List<InteractionLogEntry> GetInteractionLogsForDate(string date)
        {
            if (this.InteractionLogs.ContainsKey(date))
            {
                return this.InteractionLogs[date];
            }
            return new List<InteractionLogEntry>();
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
            
            // Add recent dialogue history
            var recentDialogues = this.GetDialogueHistory(10);
            if (recentDialogues.Any())
            {
                context += "\nRecent conversation:\n";
                foreach (var dialogue in recentDialogues)
                {
                    context += $"{dialogue.Speaker}: {dialogue.Message}\n";
                }
            }
            
            return context;
        }
        
        private string GetCurrentGameDate()
        {
            try
            {
                // This will be implemented in the ModEntry where Game1 is available
                return DateTime.Now.ToString("MMdd");
            }
            catch
            {
                // Fallback to current date if game date is not available
                return DateTime.Now.ToString("MMdd");
            }
        }
        
        private string GetCurrentGameTime()
        {
            try
            {
                // This will be implemented in the ModEntry where Game1 is available
                return DateTime.Now.ToString("HHmm");
            }
            catch
            {
                // Fallback to current time if game time is not available
                return DateTime.Now.ToString("HHmm");
            }
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
                this.Monitor.Log($"Failed to load memories: {ex.Message}", LogLevel.Warn);
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
                this.Monitor.Log($"Failed to save memories: {ex.Message}", LogLevel.Warn);
            }
        }
        
        private void LoadDialogueHistory()
        {
            try
            {
                var data = this.Helper.Data.ReadJsonFile<List<DialogueEntry>>("dialogue_history.json") ?? new List<DialogueEntry>();
                this.DialogueHistory = data;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to load dialogue history: {ex.Message}", LogLevel.Warn);
                this.DialogueHistory = new List<DialogueEntry>();
            }
        }
        
        private void SaveDialogueHistory()
        {
            try
            {
                this.Helper.Data.WriteJsonFile("dialogue_history.json", this.DialogueHistory);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to save dialogue history: {ex.Message}", LogLevel.Warn);
            }
        }
        
        private void LoadInteractionLogs()
        {
            try
            {
                var data = this.Helper.Data.ReadJsonFile<Dictionary<string, List<InteractionLogEntry>>>("interaction_logs.json") ?? 
                          new Dictionary<string, List<InteractionLogEntry>>();
                this.InteractionLogs = data;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to load interaction logs: {ex.Message}", LogLevel.Warn);
                this.InteractionLogs = new Dictionary<string, List<InteractionLogEntry>>();
            }
        }
        
        private void SaveInteractionLogs()
        {
            try
            {
                this.Helper.Data.WriteJsonFile("interaction_logs.json", this.InteractionLogs);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to save interaction logs: {ex.Message}", LogLevel.Warn);
            }
        }
        
        public void ClearMemories()
        {
            this.Memories.Clear();
            this.DialogueHistory.Clear();
            this.InteractionLogs.Clear();
            this.SaveMemories();
            this.SaveDialogueHistory();
            this.SaveInteractionLogs();
        }
    }
    
    public class MemoryEntry
    {
        public string Category { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public double Importance { get; set; }
    }
    
    public class DialogueEntry
    {
        public string Speaker { get; set; } // "Player" or "Shary"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class InteractionLogEntry
    {
        public string Speaker { get; set; } // "Player" or "Shary"
        public string Message { get; set; }
        public string Type { get; set; } // "dialogue", "interaction", "event", etc.
        public string GameTime { get; set; } // Game time in format "0900"
        public DateTime Timestamp { get; set; }
    }
}