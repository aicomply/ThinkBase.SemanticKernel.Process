// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.
using Microsoft.SemanticKernel.ChatCompletion;

namespace AICompliance.ThinkBase.Process.Models
{
    public class TBInteractState
    {
        public ChatHistory? ChatHistory { get; set; }
        public bool Complete { get; set; } = false;
        public int Cycles { get; set; } = 0;
        public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    }
}
