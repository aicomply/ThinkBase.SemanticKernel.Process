﻿using AICompliance.ThinkBase.Process.Models;
using AICompliance.ThinkBase.Process.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using ThinkBase.Process.Demo.Models;
using ThinkBase.Process.Demo.Steps;

namespace ThinkBase.Process.Demo.Processes
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public class IntentProcess : AbstractProcess
    {

        private readonly ILogger<IntentProcess> _logger;
        private readonly Kernel _kernel;
        private readonly IConfiguration _config;
        private readonly ChatHistorySummarizationReducer _historyReducer;

        public IntentProcess(ILogger<IntentProcess> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            ChatHistory history = [];
            IKernelBuilder builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(config["AzureOpenAI:DeploymentName"]!, config["AzureOpenAI:Endpoint"]!, config["AzureOpenAI:ApiKey"]!);
            builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));
            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddSingleton<IChatHistoryProvider>(new ChatHistoryProvider(history));
            _kernel = builder.Build();
            _historyReducer = new ChatHistorySummarizationReducer(_kernel.GetRequiredService<IChatCompletionService>(), 3, 3);
        }


        public override ProcessBuilder GetProcessBuilder()
        {
            ProcessBuilder process = new("intent");
            var intentStep = process.AddStepFromType<IntentStep>();
            var thinkbaseStep = process.AddStepFromType<ThinkBaseStep>();
            var userInputStep = process.AddStepFromType<UserInputStep>();
            var interactStep = process.AddStepFromType<InteractionStep>();
            var KGinfoStep = process.AddStepFromType<DescribeKGraphsStep>();

            process.OnInputEvent(Events.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(intentStep, IntentStep.Functions.InvokeAgent));

            AttachErrorStep(
                intentStep,
                IntentStep.Functions.InvokeAgent);

            intentStep
                 .OnEvent(Events.InteractionRequest)
                 .SendEventTo(new ProcessFunctionTargetBuilder(interactStep));

            intentStep
                .OnEvent(ThinkBaseStepEvents.ThinkBaseInteract)
                .SendEventTo(new ProcessFunctionTargetBuilder(thinkbaseStep));

            intentStep
                .OnEvent(Events.DescribeKGraphRequest)
                .SendEventTo(new ProcessFunctionTargetBuilder(KGinfoStep));

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

            KGinfoStep
                .OnEvent(Events.Exit)
                .StopProcess();


            void AttachErrorStep(ProcessStepBuilder step, params string[] functionNames)
            {
                foreach (string functionName in functionNames)
                {
                    step
                        .OnFunctionError(functionName)
                        .StopProcess();
                }
            }

            return process;
        }
        public override async Task<List<BotResponse>> InteractAsync(BotResponse message, KernelProcess? chatProcess)
        {
            var respList = new List<BotResponse>();
            var rep = await StartProcessAsync(chatProcess!, message);
            var history = await _kernel.Services.GetRequiredService<IChatHistoryProvider>().GetHistoryAsync();
            var assistantMessages = GetLastAssistantMessages(history);
            foreach (var assistantMessage in assistantMessages)
            {
                var bresp = new BotResponse
                {
                    Content = assistantMessage.Content!,
                    ContentType = BotResponseContentType.Text
                };
                respList.Add(bresp);
            }
            var reducedMessages = await _historyReducer!.ReduceAsync(history);
            if (reducedMessages is not null)
            {
                history.Clear();
                history.AddRange(reducedMessages);
            }
            return respList;
        }

        /// <summary>
        /// Get the last assistant messages from the chat history.
        /// <remarks>Often several assistant messages are emitted as a block. Isolate that block.</remarks>
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        private IList<ChatMessageContent> GetLastAssistantMessages(ChatHistory history)
        {
            var list = new List<ChatMessageContent>();
            foreach (var message in history.Reverse())
            {
                if (message is ChatMessageContent chatMessage && chatMessage.Role == AuthorRole.Assistant)
                {
                    list.Add(chatMessage);
                }
                else
                {
                    break;
                }
            }
            list.Reverse();
            return list;
        }

        public override async Task<LocalKernelProcessContext> StartProcessAsync(KernelProcess kernelProcess, BotResponse initialText)
        {
            var initialEvent = new KernelProcessEvent { Id = Events.StartProcess, Data = initialText };
            return await kernelProcess.StartAsync(_kernel, initialEvent);
        }
    
    }
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0110
#pragma warning restore SKEXP0001
}
