using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;
using ThinkBase.Process.Demo.Models;
using ThinkBase.Process.Demo.Processes;

namespace ThinkBase.Process.Demo.Connectivity
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0080

    public class ChatClient : IChatClient
    {

        private readonly static ConcurrentDictionary<string, KernelProcess?> conversations = new(); //assumes single server. use cache otherwise
        private readonly IProcessFactory _proc;
        private readonly ILogger<ChatClient> _logger;
        private readonly IConfiguration _config;

        public ChatClient(IProcessFactory proc, ILogger<ChatClient> logger, IConfiguration config)
        {
            _proc = proc;
            _logger = logger;
            _config = config;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.ConversationId);
            if (messages.Count() > 0)
            {
                BotResponse botresponse = new();
                var UserMessage = new BotResponse { Content = messages.Last().Text, ContentType = BotResponseContentType.Text };
                if (!conversations.TryGetValue(options.ConversationId!, out KernelProcess? process))
                {
                    _logger.LogInformation($"New conversation started with new Id: {options.ConversationId}");
                    process = _proc.GetProcess();
                    conversations[options.ConversationId] = process; // Will reset for multiple members. problem?
                }
                botresponse = await _proc.InteractAsync(UserMessage, process);
                var resp = new ChatResponse { ConversationId = options.ConversationId, CreatedAt = DateTimeOffset.Now, FinishReason = ChatFinishReason.Stop };
                resp.Messages.Add(new ChatMessage(ChatRole.Assistant, botresponse.Content));
                return resp;
            }
            else //represents a new conversation
            {
                var process = _proc.GetProcess();
                conversations[options.ConversationId] = process; // Will reset for multiple members. problem?
                var resp = new ChatResponse { ConversationId = options.ConversationId, CreatedAt = DateTimeOffset.Now, FinishReason = ChatFinishReason.Stop };
                resp.Messages.Add(new ChatMessage(ChatRole.Assistant, _config["InitialText"]));
                _logger.LogInformation($"New conversation started with empty message list with Id: {options.ConversationId}, InitialText: {resp.Messages.Last().Text}");
                return resp;
            }
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore SKEXP0110
#pragma warning restore SKEXP0080
}
