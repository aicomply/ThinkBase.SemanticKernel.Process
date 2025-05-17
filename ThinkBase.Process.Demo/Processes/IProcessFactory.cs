using Microsoft.SemanticKernel;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Processes
{
#pragma warning disable SKEXP0080
    public interface IProcessFactory
    {
        string DiscriminatorPrompt { get; }
        KernelProcess? GetProcess();
        ProcessBuilder GetProcessBuilder();
        Task<BotResponse> InteractAsync(BotResponse message, KernelProcess? chatProcess);
        Task<LocalKernelProcessContext> StartProcessAsync(KernelProcess kernelProcess, BotResponse initialText);

    }
#pragma warning restore SKEXP0080
}