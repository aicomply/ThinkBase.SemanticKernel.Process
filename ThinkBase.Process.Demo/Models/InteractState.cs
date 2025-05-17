using Microsoft.SemanticKernel.ChatCompletion;

namespace ThinkBase.Process.Demo.Models
{
    public class InteractState
    {
        public ChatHistory? ChatHistory { get; set; }
        public bool Complete { get; set; } = false;
        public int Cycles { get; set; } = 0;
    }
}
