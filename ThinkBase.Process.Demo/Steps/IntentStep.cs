// Copyright (c) Microsoft. All rights reserved.
using AICompliance.ThinkBase.Process.Models;
using AICompliance.ThinkBase.Process.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using System.ComponentModel;
using System.Text.Json;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Steps;
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010

/// <summary>
/// This steps defines actions for the primary agent.  This agent is responsible forinteracting with
/// the user as well as as delegating to a group of agents.
/// </summary>
public class IntentStep : KernelProcessStep<IntentStepState>
{
    public const string providerServiceKey = $"{nameof(IntentStep)}:{nameof(providerServiceKey)}";


    public static class Functions
    {
        public const string InvokeAgent = nameof(InvokeAgent);
        public const string ResetStatus = nameof(ResetStatus);
        public const string SetNextStep = nameof(SetNextStep);
    }

    private IntentStepState? _state;

    public override ValueTask ActivateAsync(KernelProcessStepState<IntentStepState> state)
    {
        _state = state.State ?? new();
        return ValueTask.CompletedTask;
    }

    [KernelFunction(Functions.InvokeAgent)]
    public async Task InvokeAgentAsync(KernelProcessStepContext context, Kernel kernel, ILogger logger, BotResponse userInput)
    {
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();
        history.Add(new ChatMessageContent(AuthorRole.User, userInput.Content));
        await historyProvider.CommitAsync();

        if (_state!.InSubProcess)
        {
            await context.EmitEventAsync(new() { Id = _state.NextEvent!, Data = new TBMessage { History = history, GraphName = _state.GraphName, InitialPrompt = _state.InitialText } });
            logger.LogInformation($"*** IntentStep is in a sub-process, handing over to {_state.NextEvent!} and passing history.");
            return;
        }

        var localChatHistory = new ChatHistory
        {
            new ChatMessageContent(AuthorRole.User, userInput.Content),
            new ChatMessageContent(AuthorRole.System, Prompts.IntentInstructions)
        };
        OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings();
        settings.ResponseFormat = typeof(IntentResponse);
        settings.FunctionChoiceBehavior = FunctionChoiceBehavior.Auto();
        settings.Temperature = 0.0f;
        // Add the KGMetaData plugin
        var config = kernel.GetRequiredService<IConfiguration>();
        var kgplugin = new KGMetadataPlugin(config, logger);
        kernel.ImportPluginFromObject(kgplugin, nameof(KGMetadataPlugin));
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var intentResponse = await chatCompletionService.GetChatMessageContentAsync(localChatHistory, executionSettings: settings, kernel: kernel);
        var formattedResponse = JsonSerializer.Deserialize<IntentResponse>(intentResponse.Content!.ToString());
        //remove the plugin
        //Some of these next steps are possibly multi  pass. Where this is true, set the appropriate event to return to them if required in _state.
        if (formattedResponse!.Interaction)
        {
            logger.LogInformation($"*** IntentStep inferred {userInput.Content} was a request for Interaction handling");
            await context.EmitEventAsync(new() { Id = Events.InteractionRequest });
        }
        else if (formattedResponse.KnowledgeGraphDescribe)
        {
            logger.LogInformation($"*** IntentStep inferred {userInput.Content} was a request for KG info");
            await context.EmitEventAsync(new() { Id = Events.DescribeKGraphRequest });
        }
        else if (formattedResponse!.KnowledgeGraphFunction)
        {
            _state.InSubProcess = true;
            _state.NextEvent = ThinkBaseStepEvents.ThinkBaseInteract;
            logger.LogInformation($"*** IntentStep inferred {userInput.Content} was a request for a KG to run.");
            //extract the name and initial text and pass on.
            _state.GraphName = formattedResponse.Name;
            _state.InitialText = formattedResponse.InitialText;
            await context.EmitEventAsync(new() { Id = ThinkBaseStepEvents.ThinkBaseInteract, Data = new TBMessage { History = history, GraphName = formattedResponse.Name, InitialPrompt = formattedResponse.InitialText } });
        }
        kernel.Plugins.Remove(kernel.Plugins[nameof(KGMetadataPlugin)]);

    }


    private class IntentResponse
    {
        [Description("Specifies that the user's input is a simple interaction such as hello. ")]
        public bool Interaction { get; set; }

        /// <summary>
        /// Specifies that the user's input is a request to describe the available knowledge graphs.
        /// </summary>
        public bool KnowledgeGraphDescribe { get; set; } = false;

        [Description("Specifies that the user's input is a request for one of the knowledge graphs to run. ")]
        public bool KnowledgeGraphFunction { get; set; }

        [Description("Specifies the name of the Knowledge graph to use. ")]
        public string Name { get; set; } = string.Empty;

        [Description("Specifies the initial text to send to the Knowledge Graph. ")]
        public string InitialText { get; set; } = string.Empty;

    }



    [KernelFunction(Functions.ResetStatus)]
    public async Task ResetStatusAsync(KernelProcessStepContext context, Kernel kernel, ILogger logger, ChatHistory localhistory)
    {
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();
        history.Clear();
        history.AddRange(localhistory);
        logger.LogInformation($"*** IntentStep: Sub-process completed. Reverting to top level.");
        _state!.InSubProcess = false;
        await historyProvider.CommitAsync();
    }


}
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0010
