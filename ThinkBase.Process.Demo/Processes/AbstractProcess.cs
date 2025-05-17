using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Process.Tools;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Processes
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001
    public abstract class AbstractProcess : IProcessFactory
    {
        public string DiscriminatorPrompt => string.Empty;

        public virtual KernelProcess? GetProcess()
        {
            var process = GetProcessBuilder();
            KernelProcess kernelProcess = process.Build();

            var diag = process.ToMermaid();
            Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
            Console.WriteLine(diag);
            Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

            return kernelProcess;
        }

        public virtual ProcessBuilder GetProcessBuilder()
        {
            throw new NotImplementedException();
        }

        public virtual Task<BotResponse> InteractAsync(BotResponse message, KernelProcess? chatProcess)
        {
            throw new NotImplementedException();
        }

        public virtual Task<LocalKernelProcessContext> StartProcessAsync(KernelProcess kernelProcess, BotResponse initialText)
        {
            throw new NotImplementedException();
        }


    }
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0110
#pragma warning restore SKEXP0001
}
