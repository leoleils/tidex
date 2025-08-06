using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace AriaMod
{
    /// <summary>
    /// Handles communication with the AI model for generating Aria's dialogue.
    /// </summary>
    public class AriaAI : IDisposable
    {
        private readonly string _apiUrl;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;
        private readonly ModConfig _config;

        public AriaAI(string apiUrl, string apiKey, string model, ModConfig config)
        {
            _apiUrl = apiUrl;
            _apiKey = apiKey;
            _model = model;
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }

        /// <summary>
        /// Generates a response from Aria based on the player's message and context.
        /// </summary>
        /// <param name="playerMessage">The player's message to Aria</param>
        /// <param name="context">Additional context like time of day, season, etc.</param>
        /// <returns>Aria's response</returns>
        public async Task<AriaResponse> GenerateResponse(string playerMessage, Dictionary<string, string> context)
        {
            try
            {
                // Prepare the prompt for the AI model
                var prompt = BuildPrompt(playerMessage, context);

                // Prepare the request body
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = GetSystemPrompt() },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 150,
                    stream = false,
                    enable_thinking = false
                };

                // Convert to JSON
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                // Send the request
                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API request failed with status {response.StatusCode}: {responseContent}");
                }

                // Parse the response
                var responseJson = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
                var aiMessage = responseJson.choices[0].message.content.Trim();

                // Try to parse choices from the response, or generate default ones
                List<string> choices = ParseChoicesFromResponse(aiMessage);
                if (choices.Count == 0)
                {
                    choices = GenerateDefaultChoices();
                }

                return new AriaResponse
                {
                    Dialogue = CleanDialogueText(aiMessage),
                    Choices = choices
                };
            }
            catch (Exception ex)
            {
                // Return a default response in case of error
                return new AriaResponse
                {
                    Dialogue = $"I'm sorry, I didn't quite catch that. Could you repeat it? (Error: {ex.Message})",
                    Choices = GenerateDefaultChoices()
                };
            }
        }

        /// <summary>
        /// Generates a streaming response from Aria based on the player's message and context.
        /// This method allows for real-time display of the AI's response.
        /// </summary>
        /// <param name="playerMessage">The player's message to Aria</param>
        /// <param name="context">Additional context like time of day, season, etc.</param>
        /// <param name="onPartialResponse">Callback function to handle partial responses</param>
        /// <param name="cancellationToken">Cancellation token to stop the streaming</param>
        public async Task StreamResponse(
            string playerMessage, 
            Dictionary<string, string> context, 
            Action<string> onPartialResponse,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Prepare the prompt for the AI model
                var prompt = BuildPrompt(playerMessage, context);

                // Prepare the request body with streaming enabled
                var requestBody = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = GetSystemPrompt() },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 150,
                    stream = true
                };

                // Convert to JSON
                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add authorization header
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                // Send the request
                var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl)
                {
                    Content = content
                };

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                // Process the streaming response
                string accumulatedResponse = "";
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Handle SSE format
                    if (line.StartsWith("data: "))
                    {
                        var data = line.Substring(6);
                        if (data == "[DONE]")
                            break;

                        try
                        {
                            var responseJson = JsonConvert.DeserializeObject<StreamResponse>(data);
                            if (responseJson.choices?.Length > 0 && 
                                responseJson.choices[0].delta?.content != null)
                            {
                                accumulatedResponse += responseJson.choices[0].delta.content;
                                onPartialResponse(accumulatedResponse);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            // Ignore parsing errors for individual lines, but log them for debugging
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // In case of error, provide a default response
                onPartialResponse($"I'm sorry, I didn't quite catch that. Could you repeat it? (Error: {ex.Message})");
            }
        }

        /// <summary>
        /// Builds a prompt for the AI model with context information.
        /// </summary>
        private string BuildPrompt(string playerMessage, Dictionary<string, string> context)
        {
            var promptBuilder = new StringBuilder();
            
            // Use configured prompt template if available
            string promptTemplate = null;
            if (_config?.PromptProfiles?.ContainsKey(_config.ActivePromptProfile) == true)
            {
                promptTemplate = _config.PromptProfiles[_config.ActivePromptProfile].Default;
            }

            // Replace placeholders in prompt template
            if (!string.IsNullOrEmpty(promptTemplate))
            {
                promptTemplate = promptTemplate
                    .Replace("{time}", context.ContainsKey("Time") ? context["Time"] : "this time")
                    .Replace("{location}", context.ContainsKey("Location") ? context["Location"] : "here")
                    .Replace("{season}", context.ContainsKey("Season") ? context["Season"] : "this season");
            }

            promptBuilder.AppendLine($"Player says to Aria: \"{playerMessage}\"");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Context information:");
            
            foreach (var kvp in context)
            {
                promptBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            
            promptBuilder.AppendLine();
            
            if (!string.IsNullOrEmpty(promptTemplate))
            {
                promptBuilder.AppendLine($"Aria's personality guidance: {promptTemplate}");
            }
            
            promptBuilder.AppendLine("Please respond as Aria with a brief dialogue (1-2 sentences) in the appropriate style.");
            promptBuilder.AppendLine("Keep the response natural and in character based on the personality guidance above.");
            promptBuilder.AppendLine("At the end of your response, please provide 2-4 response options for the player, each on its own line and prefixed with 'Choice:'. For example:");
            promptBuilder.AppendLine("Choice: That's very interesting!");
            promptBuilder.AppendLine("Choice: I've been thinking about that too.");
            promptBuilder.AppendLine("Choice: Could you tell me more about that?");
            promptBuilder.AppendLine("Important: Format your response so it works with Stardew Valley's dialogue system. The response options will be automatically converted to clickable choices in the game.");
            
            return promptBuilder.ToString();
        }

        /// <summary>
        /// Gets the system prompt that defines Aria's character and response style.
        /// </summary>
        private string GetSystemPrompt()
        {
            if (_config?.PromptProfiles?.ContainsKey(_config.ActivePromptProfile) == true)
            {
                return _config.PromptProfiles[_config.ActivePromptProfile].System;
            }
            
            // Fallback to default if profile not found
            return @"You are Aria, a friendly and knowledgeable town librarian in Stardew Valley. 
Key characteristics:
- You love books, gardening, and local folklore
- You're well-read and curious about the world
- You're helpful but not overly familiar
- You speak in a warm, intelligent manner
- You have a particular interest in seasonal activities and nature

Response guidelines:
- Keep responses brief (1-2 sentences)
- Use simple, natural language
- Stay in character at all times
- Reference the context provided when relevant
- Avoid modern slang or complex vocabulary
- Respond as if in a casual conversation in a small town
- Include Stardew Valley-appropriate topics like farming, seasons, local events, and nature
- At the end of your response, provide 2-4 response options for the player, each on its own line and prefixed with 'Choice:'

Example responses:
- ""These books on ancient farming are fascinating. I've learned so much about soil composition.""
- ""The spring blossoms are beautiful this year. Have you planted anything new?""
- ""I'm researching local folklore. Did you know there are legends about the forest spirits?""

Example choices:
Choice: That sounds fascinating!
Choice: I prefer modern farming methods.
Choice: Could you recommend a book on that?

Do not:
- Break character
- Mention anything outside the Stardew Valley universe
- Use complex vocabulary or long responses
- Make assumptions about the player's actions or thoughts
- Forget to include 2-4 response options at the end of your response
- Use markdown or special formatting
- Include line breaks in the middle of your main response";
        }

        /// <summary>
        /// Parse player response choices from the AI's response
        /// </summary>
        /// <param name="response">The full AI response</param>
        /// <returns>List of player response choices</returns>
        private List<string> ParseChoicesFromResponse(string response)
        {
            var choices = new List<string>();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("Choice:"))
                {
                    var choice = line.Substring(7).Trim(); // Remove "Choice:" prefix
                    if (!string.IsNullOrEmpty(choice))
                    {
                        choices.Add(choice);
                    }
                }
            }
            
            return choices;
        }
        
        /// <summary>
        /// Generate default player response choices
        /// </summary>
        /// <returns>List of default response choices</returns>
        private List<string> GenerateDefaultChoices()
        {
            return new List<string>
            {
                "That's very interesting!",
                "I've been thinking about that too.",
                "Could you tell me more about that?",
                "I have a different opinion on that."
            };
        }
        
        /// <summary>
        /// Clean dialogue text by removing choice lines
        /// </summary>
        /// <param name="response">The full AI response</param>
        /// <returns>Cleaned dialogue text</returns>
        private string CleanDialogueText(string response)
        {
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var cleanLines = new List<string>();
            
            foreach (var line in lines)
            {
                if (!line.StartsWith("Choice:"))
                {
                    cleanLines.Add(line);
                }
            }
            
            return string.Join("\n", cleanLines);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Represents a response from Aria including her dialogue and player choices.
    /// </summary>
    public class AriaResponse
    {
        public string Dialogue { get; set; } = "";
        public List<string> Choices { get; set; } = new();
    }

    /// <summary>
    /// Represents the structure of the API response from the model.
    /// </summary>
    public class ApiResponse
    {
        public Choice[] choices { get; set; } = Array.Empty<Choice>();
    }

    public class Choice
    {
        public Message message { get; set; } = new();
    }

    public class Message
    {
        public string content { get; set; } = "";
    }

    /// <summary>
    /// Represents the structure of a streaming response from the model.
    /// </summary>
    public class StreamResponse
    {
        public StreamChoice[] choices { get; set; } = Array.Empty<StreamChoice>();
    }

    public class StreamChoice
    {
        public StreamDelta delta { get; set; } = new();
    }

    public class StreamDelta
    {
        public string content { get; set; } = "";
    }
}