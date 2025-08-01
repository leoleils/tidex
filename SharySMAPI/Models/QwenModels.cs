using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharySMAPI.Models
{
    public class QwenRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; }
        
        [JsonProperty("input")]
        public QwenInput Input { get; set; }
        
        [JsonProperty("parameters")]
        public QwenParameters Parameters { get; set; }
    }

    public class QwenInput
    {
        [JsonProperty("messages")]
        public List<QwenMessage> Messages { get; set; }
    }

    public class QwenMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class QwenParameters
    {
        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; }
        
        [JsonProperty("temperature")]
        public float Temperature { get; set; }
        
        [JsonProperty("top_p")]
        public float TopP { get; set; }
        
        [JsonProperty("result_format")]
        public string ResultFormat { get; set; }
        
        [JsonProperty("streaming")]
        public bool Streaming { get; set; }
        
        [JsonProperty("seed")]
        public int? Seed { get; set; }
        
        [JsonProperty("repetition_penalty")]
        public float? RepetitionPenalty { get; set; }
    }

    public class QwenResponse
    {
        [JsonProperty("output")]
        public QwenOutput Output { get; set; }
        
        [JsonProperty("usage")]
        public QwenUsage Usage { get; set; }
        
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
    }

    public class QwenOutput
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
        
        [JsonProperty("choices")]
        public List<QwenChoice> Choices { get; set; }
    }

    public class QwenChoice
    {
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
        
        [JsonProperty("message")]
        public QwenMessage Message { get; set; }
    }

    public class QwenUsage
    {
        [JsonProperty("input_tokens")]
        public int InputTokens { get; set; }
        
        [JsonProperty("output_tokens")]
        public int OutputTokens { get; set; }
        
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class QwenStreamResponse
    {
        [JsonProperty("output")]
        public QwenStreamOutput Output { get; set; }
        
        [JsonProperty("usage")]
        public QwenUsage Usage { get; set; }
        
        [JsonProperty("request_id")]
        public string RequestId { get; set; }
    }

    public class QwenStreamOutput
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }
}