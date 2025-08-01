using StardewModdingAPI;

namespace SharySMAPI
{
    public class ModConfig
    {
        /// <summary>
        /// Whether to enable Qwen integration for conversations.
        /// </summary>
        public bool EnableQwenIntegration { get; set; } = true;
        
        /// <summary>
        /// The minimum friendship level required for Shary to help with resource collection.
        /// </summary>
        public int MinimumFriendshipForHelp { get; set; } = 2000;
        
        /// <summary>
        /// The Qwen API endpoint (OpenAI compatible).
        /// </summary>
        public string QwenApiEndpoint { get; set; } = "https://dashscope.aliyuncs.com/compatible-mode/v1";
        
        /// <summary>
        /// The API key for accessing Qwen.
        /// </summary>
        public string QwenApiKey { get; set; } = "";
        
        /// <summary>
        /// The model to use for generating responses.
        /// </summary>
        public string Model { get; set; } = "qwen3-235b-a22b-instruct-2507";
        
        /// <summary>
        /// System prompt that defines Shary's personality and role.
        /// </summary>
        public string SystemPrompt { get; set; } = "You are Shary, a 25-year-old female librarian in Stardew Valley. You live in the town and work at the library. You are polite, intelligent, and enjoy books and nature. Respond in a way that fits the game's tone and style.";
        
        /// <summary>
        /// Prompt template for user messages. {0} is replaced with context, {1} with friendship note.
        /// </summary>
        public string UserPromptTemplate { get; set; } = "Context: {0}\n\nNote: {1}\n\nPlease respond as Shary in a single, natural sentence that fits the Stardew Valley game style:";
        
        /// <summary>
        /// Notes for different friendship levels.
        /// </summary>
        public FriendshipNotes FriendshipNotes { get; set; } = new FriendshipNotes();
    }
    
    public class FriendshipNotes
    {
        public string NewAcquaintance { get; set; } = "This is a new acquaintance. Keep the conversation polite but reserved.";
        public string DevelopingFriendship { get; set; } = "This is a developing friendship. Be friendly and interested in their activities.";
        public string GoodFriend { get; set; } = "This is a good friend. Be warm and open in your conversation.";
        public string CloseFriend { get; set; } = "This is a close friend with romantic potential. Be affectionate but respectful.";
        public string Spouse { get; set; } = "This is a spouse. Be loving and intimate in your conversation.";
    }
}