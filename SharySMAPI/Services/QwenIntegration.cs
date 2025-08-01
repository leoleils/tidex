using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;

namespace SharySMAPI
{
    public class QwenIntegration
    {
        private IModHelper Helper;
        private IMonitor Monitor;
        private OpenAIService OpenAIService;
        private ModConfig Config;
        private MemorySystem Memory;
        
        public QwenIntegration(IModHelper helper, IMonitor monitor, MemorySystem memory)
        {
            this.Helper = helper;
            this.Monitor = monitor;
            this.Config = helper.ReadConfig<ModConfig>();
            this.Memory = memory;
            
            // Initialize OpenAI service if integration is enabled and API key is provided
            if (this.Config.EnableQwenIntegration && !string.IsNullOrEmpty(this.Config.QwenApiKey))
            {
                try
                {
                    var openAiSettings = new OpenAiOptions()
                    {
                        ApiKey = this.Config.QwenApiKey,
                        BaseDomain = this.Config.QwenApiEndpoint
                    };
                    
                    this.OpenAIService = new OpenAIService(openAiSettings);
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"Failed to initialize OpenAI service: {ex.Message}", LogLevel.Error);
                }
            }
        }
        
        public string GenerateResponse(string context, int friendshipLevel)
        {
            // Use async method but block for result to maintain API compatibility
            return this.GenerateResponseAsync(context, friendshipLevel).Result;
        }
        
        public async Task<string> GenerateResponseAsync(string context, int friendshipLevel)
        {
            try
            {
                // Check if Qwen integration is enabled
                if (!this.Config.EnableQwenIntegration || this.OpenAIService == null)
                {
                    return this.GetFallbackResponse(friendshipLevel);
                }
                
                // Validate configuration
                if (string.IsNullOrEmpty(this.Config.QwenApiKey))
                {
                    this.Monitor.Log("Qwen API key is not configured. Using fallback responses.", LogLevel.Warn);
                    return this.GetFallbackResponse(friendshipLevel);
                }
                
                // Prepare messages with conversation history
                var messages = this.PrepareMessages(context, friendshipLevel);
                
                // Create the chat completion request
                var chatCompletionRequest = new ChatCompletionCreateRequest
                {
                    Model = this.Config.Model,
                    Messages = messages,
                    MaxTokens = 200,
                    Temperature = 0.7f,
                    TopP = 0.8f
                };

                // Make the API call
                var response = await this.OpenAIService.ChatCompletion.CreateCompletion(chatCompletionRequest);
                
                if (response.Successful)
                {
                    return response.Choices[0].Message.Content ?? this.GetFallbackResponse(friendshipLevel);
                }
                else
                {
                    this.Monitor.Log($"Qwen API request failed: {response.Error?.Message}", LogLevel.Error);
                    return this.GetFallbackResponse(friendshipLevel);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error calling Qwen API: {ex.Message}", LogLevel.Error);
                return this.GetFallbackResponse(friendshipLevel);
            }
        }
        
        // This method is used for streaming output
        public async Task<string> GenerateStreamingResponse(string context, int friendshipLevel, Action<string> onTokenReceived)
        {
            try
            {
                // Check if Qwen integration is enabled
                if (!this.Config.EnableQwenIntegration || this.OpenAIService == null)
                {
                    var fallback = this.GetFallbackResponse(friendshipLevel);
                    onTokenReceived?.Invoke(fallback);
                    return fallback;
                }
                
                // Validate configuration
                if (string.IsNullOrEmpty(this.Config.QwenApiKey))
                {
                    this.Monitor.Log("Qwen API key is not configured. Using fallback responses.", LogLevel.Warn);
                    var fallback = this.GetFallbackResponse(friendshipLevel);
                    onTokenReceived?.Invoke(fallback);
                    return fallback;
                }
                
                // Prepare messages with conversation history
                var messages = this.PrepareMessages(context, friendshipLevel);
                
                // Create the chat completion request with streaming enabled
                var chatCompletionRequest = new ChatCompletionCreateRequest
                {
                    Model = this.Config.Model,
                    Messages = messages,
                    MaxTokens = 200,
                    Temperature = 0.7f,
                    TopP = 0.8f,
                    Stream = true
                };

                var result = "";
                
                // Make the streaming API call
                await this.OpenAIService.ChatCompletion.CreateCompletionAsStream(chatCompletionRequest,
                    (streamEvent) =>
                    {
                        if (streamEvent.Choices.FirstOrDefault()?.Delta?.Content != null)
                        {
                            var token = streamEvent.Choices.First().Delta.Content;
                            result += token;
                            onTokenReceived?.Invoke(token);
                        }
                    });
                
                return result;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error calling Qwen API for streaming: {ex.Message}", LogLevel.Error);
                var fallback = this.GetFallbackResponse(friendshipLevel);
                onTokenReceived?.Invoke(fallback);
                return fallback;
            }
        }
        
        private List<ChatMessage> PrepareMessages(string context, int friendshipLevel)
        {
            var messages = new List<ChatMessage>();
            
            // Add system message
            messages.Add(ChatMessage.FromSystem(this.Config.SystemPrompt));
            
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
            
            // Add current context and prompt
            var prompt = this.GeneratePrompt(context, friendshipLevel);
            messages.Add(ChatMessage.FromUser(prompt));
            
            return messages;
        }
        
        private string GeneratePrompt(string context, int friendshipLevel)
        {
            // Select the appropriate friendship note
            string friendshipNote;
            if (friendshipLevel < 1000)
            {
                friendshipNote = this.Config.FriendshipNotes.NewAcquaintance;
            }
            else if (friendshipLevel < 3000)
            {
                friendshipNote = this.Config.FriendshipNotes.DevelopingFriendship;
            }
            else if (friendshipLevel < 6000)
            {
                friendshipNote = this.Config.FriendshipNotes.GoodFriend;
            }
            else if (friendshipLevel < 9000)
            {
                friendshipNote = this.Config.FriendshipNotes.CloseFriend;
            }
            else
            {
                friendshipNote = this.Config.FriendshipNotes.Spouse;
            }
            
            // Format the prompt using the template
            return string.Format(this.Config.UserPromptTemplate, context, friendshipNote);
        }
        
        private string GetFallbackResponse(int friendshipLevel)
        {
            // Generate a response based on friendship level
            if (friendshipLevel < 1000)
            {
                return "Hello! It's nice to meet you. I'm Shary, the town librarian.";
            }
            else if (friendshipLevel < 3000)
            {
                return "Hello again! I've been organizing new books at the library. How are you doing today?";
            }
            else if (friendshipLevel < 6000)
            {
                return "Hi there! I was just thinking about you. Would you like to take a walk around town?";
            }
            else if (friendshipLevel < 9000)
            {
                return "It's so good to see you! I'd love to help you with those resources if you need. Just say the word!";
            }
            else
            {
                return "My dear friend! I'm always happy to help you with anything you need. What can I do for you today?";
            }
        }
    }
}