using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AICompliance.ThinkBase.Process.Models
{
    public  class TBMessage
    {
        public ChatHistory History { get; set; } = new();
        public string? InitialPrompt { get; set; } = string.Empty;
        public string? GraphName { get; set; } = string.Empty;
    }
}
