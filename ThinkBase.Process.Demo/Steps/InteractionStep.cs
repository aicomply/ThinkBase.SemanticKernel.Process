
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Steps
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050

    public class InteractionStep : KernelProcessStep
    {
        public static class Functions
        {
            public const string Interaction = nameof(Interaction);
        }

        [KernelFunction(Functions.Interaction)]
        public async ValueTask InteractionAsync(KernelProcessStepContext context, Kernel kernel, ILogger logger)
        {
            IChatHistoryProvider historyProvider = kernel.GetHistory();
            ChatHistory history = await historyProvider.GetHistoryAsync();
            history.AddSystemMessage(Prompts.SimpleInteractionInstructions);
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

                var simpleResponse = await chatCompletionService.GetChatMessageContentAsync(history, new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                }, kernel);

                history.Add(simpleResponse);

            logger.LogInformation($"*** InteractionStep responded with content");
            await context.EmitEventAsync(new() { Id = Events.Exit });
            await historyProvider.CommitAsync();
        }
    }

#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0050
}
