// Copyright (c) Microsoft. All rights reserved.
using AICompliance.ThinkBase.Process.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
            await context.EmitEventAsync(new() { Id = _state.NextEvent!, Data = history});
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

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var proofreadResponse = await chatCompletionService.GetChatMessageContentAsync(localChatHistory, executionSettings: settings);
        var formattedResponse = JsonSerializer.Deserialize<IntentResponse>(proofreadResponse.Content!.ToString());
        //Some of these next steps are possibly multi  pass. Where this is true, set the appropriate event to return to them if required in _state.
        if (formattedResponse!.Interaction)
        {
            logger.LogInformation($"*** IntentStep inferred {userInput.Content} was a request for Interaction handling");
            await context.EmitEventAsync(new() { Id = Events.InteractionRequest });
        }
        else if (formattedResponse!.Compliance)
        {
            _state.InSubProcess = true;
            _state.NextEvent = ThinkBaseStepEvents.ThinkBaseInteract;
            logger.LogInformation($"*** IntentStep inferred {userInput.Content} was a request for Compliance info");
            await context.EmitEventAsync(new() { Id = ThinkBaseStepEvents.ThinkBaseInteract, Data = history });
        }
    }


    private class IntentResponse
    {
        [Description("Specifies that the user's input is a simple interaction such as hello. ")]
        public bool Interaction { get; set; }
        [Description("Specifies that the user's input is a request for general information. ")]
        public bool Information { get; set; }
        [Description("Specifies that the user's input is a request to access email services. ")]
        public bool Email { get; set; }
        [Description("Specifies that the user's input is a request to access calendar and event services. ")]
        public bool Calendar { get; set; }
        [Description("Specifies that the user's input is a request to access task list services. ")]
        public bool TaskList { get; set; }
        [Description("Specifies that the user's input is a request to obtain an audit of an AI solution. ")]
        public bool Audit { get; set; }
        [Description("Specifies that the user's input is a request to obtain Compliance information. ")]
        public bool Compliance{ get; set; }
    }



    [KernelFunction(Functions.ResetStatus)]
    public void ResetStatus(KernelProcessStepContext context, Kernel kernel, ILogger logger)
    {
        logger.LogInformation($"*** IntentStep: Sub-process completed. Reverting to top level.");
        _state!.InSubProcess = false;
    }


}
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0010
