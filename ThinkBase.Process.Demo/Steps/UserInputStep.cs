using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using ThinkBase.Process.Demo.Models;

namespace ThinkBase.Process.Demo.Steps;
#pragma warning disable SKEXP0080

/// <summary>
/// A step that elicits user input.
/// </summary>
public class UserInputStep : KernelProcessStep<InteractState>
{
    public static class Functions
    {
        public const string GetUserInput = nameof(GetUserInput);
        public const string WaitForUserInput = nameof(WaitForUserInput);
    }

    protected bool SuppressOutput { get; init; }

    private InteractState? _state;


    /// <summary>
    /// Activates the user input step by initializing the state object. This method is called when the process is started
    /// and before any of the KernelFunctions are invoked.
    /// </summary>
    /// <param name="state">The state object for the step.</param>
    /// <returns>A <see cref="ValueTask"/></returns>
    public override ValueTask ActivateAsync(KernelProcessStepState<InteractState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the user input.
    /// Could be overridden to customize the output events to be emitted
    /// </summary>
    /// <param name="context">An instance of <see cref="KernelProcessStepContext"/> which can be
    /// used to emit events from within a KernelFunction.</param>
    /// <returns>A <see cref="ValueTask"/></returns>
    [KernelFunction(Functions.GetUserInput)]
    public virtual async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        await context.EmitEventAsync(new() { Id = Events.UserInputReceived, Data = _state!.ChatHistory });
    }
    [KernelFunction(Functions.WaitForUserInput)]
    public virtual async ValueTask WaitForUserInputAsync(KernelProcessStepContext context, Kernel kernel, ChatHistory localhistory)
    {
        IChatHistoryProvider historyProvider = kernel.GetHistory();
        ChatHistory history = await historyProvider.GetHistoryAsync();
        history.Clear();
        history.AddRange(localhistory);
        await context.EmitEventAsync(new() { Id = Events.WaitForUserInput, Data = _state!.ChatHistory });
        await historyProvider.CommitAsync();
    }
}


#pragma warning restore SKEXP0080
