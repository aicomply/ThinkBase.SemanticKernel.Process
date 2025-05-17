using AICompliance.ThinkBase.Process.Models;
using AICompliance.ThinkBase.Process.Steps;
using Microsoft.SemanticKernel;
using ThinkBase.Process.Demo.Models;
using ThinkBase.Process.Demo.Steps;

namespace ThinkBase.Process.Demo.Processes
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public class IntentProcess : AbstractProcess
    {
        public override ProcessBuilder GetProcessBuilder()
        {
            ProcessBuilder process = new("intent");
            var intentStep = process.AddStepFromType<IntentStep>();
            var thinkbaseStep = process.AddStepFromType<ThinkBaseStep>();
            var userInputStep = process.AddStepFromType<UserInputStep>();
            var interactStep = process.AddStepFromType<InteractionStep>();

            process.OnInputEvent(Events.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(intentStep, IntentStep.Functions.InvokeAgent));



            thinkbaseStep
                .OnEvent(ThinkBaseStepEvents.Exit)
                .SendEventTo(new ProcessFunctionTargetBuilder(intentStep, IntentStep.Functions.ResetStatus))
                .StopProcess();

            thinkbaseStep
                .OnEvent(ThinkBaseStepEvents.ThinkBaseInteract)
                .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep, UserInputStep.Functions.WaitForUserInput));

            interactStep
            .OnEvent(Events.Exit)
            .StopProcess();


            return process;
        }
        public override Task<BotResponse> InteractAsync(BotResponse message, KernelProcess? chatProcess)
        {
            throw new NotImplementedException();
        }
        public override Task<LocalKernelProcessContext> StartProcessAsync(KernelProcess kernelProcess, BotResponse initialText)
        {
            throw new NotImplementedException();
        }
    
    }
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0110
#pragma warning restore SKEXP0001
}
