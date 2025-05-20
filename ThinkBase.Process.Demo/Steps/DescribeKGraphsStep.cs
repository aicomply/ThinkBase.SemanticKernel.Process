using AICompliance.ThinkBase.Process.Models;
using AICompliance.ThinkBase.Process.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Steps
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050

    public class DescribeKGraphsStep : KernelProcessStep
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
            history.AddSystemMessage(Prompts.KGraphInfoInstructions);
            IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var config = kernel.GetRequiredService<IConfiguration>();

            var kgplugin = new KGMetadataPlugin(config, logger);
            kernel.ImportPluginFromObject(kgplugin, nameof(KGMetadataPlugin));

            var simpleResponse = await chatCompletionService.GetChatMessageContentAsync(history, new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            }, kernel);

            history.Add(simpleResponse);

            logger.LogInformation($"*** DescribeKGraphsStep responded with content");
            await context.EmitEventAsync(new() { Id = Events.Exit });
            await historyProvider.CommitAsync();
            kernel.Plugins.Remove(kernel.Plugins[nameof(KGMetadataPlugin)]);
        }
    }

#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0050
}
