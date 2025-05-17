using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThinkBase.Process.Demo.Models
{
#pragma warning disable SKEXP0080

    public enum BotResponseContentType
    {
        [JsonPropertyName("text")]
        Text,

        [JsonPropertyName("adaptive-card")]
        AdaptiveCard
    }
    public class BotResponse
    {
        [JsonPropertyName("contentType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BotResponseContentType ContentType { get; set; }

        [JsonPropertyName("content")]
        [Description("The content of the response, may be plain text, or JSON based adaptive card but must be a string.")]
        [Required]
        public string Content { get; set; }
        public KernelProcess? KernelProcess { get; set; }
    }
#pragma warning restore SKEXP0080

}
