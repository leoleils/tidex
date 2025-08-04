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

        public AriaAI(string apiUrl, string apiKey, string model)
        {
            _apiUrl = apiUrl;
            _apiKey = apiKey;
            _model = model;
            _httpClient = new HttpClient();
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
                        new { role = "system", content = "You are Aria, a friendly and knowledgeable town librarian in Stardew Valley. You love books, gardening, and local folklore. Respond naturally as Aria would, keeping responses brief and in the style of Stardew Valley dialogue. Format your response as a single line of dialogue, without any prefixes or special formatting." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 100,
                    stream = false,
                    // Add DashScope specific parameter
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

                // For this implementation, we'll just return the AI message as dialogue
                // A more advanced implementation would parse choices from the response
                return new AriaResponse
                {
                    Dialogue = aiMessage,
                    Choices = new List<string> { "That's interesting.", "Tell me more.", "I should go now." }
                };
            }
            catch (Exception ex)
            {
                // Return a default response in case of error
                return new AriaResponse
                {
                    Dialogue = $"I'm sorry, I didn't quite catch that. Could you repeat it? (Error: {ex.Message})",
                    Choices = new List<string> { "Sure!", "Never mind." }
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
                        new { role = "system", content = "You are Aria, a friendly and knowledgeable town librarian in Stardew Valley. You love books, gardening, and local folklore. Respond naturally as Aria would, keeping responses brief and in the style of Stardew Valley dialogue. Format your response as a single line of dialogue, without any prefixes or special formatting." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 100,
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
            promptBuilder.AppendLine($"Player says to Aria: \"{playerMessage}\"");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Context information:");
            
            foreach (var kvp in context)
            {
                promptBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Please respond as Aria with a brief dialogue (1 sentence) in the style of Stardew Valley.");
            
            return promptBuilder.ToString();
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