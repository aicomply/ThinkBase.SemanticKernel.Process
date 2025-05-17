using AICompliance.ThinkBase.Process.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using GraphQL.Client.Http;
using GraphQL;
using System.Reflection;
using Azure.Storage.Blobs;
using Orionsoft.MarkdownToPdfLib;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Text;

namespace AICompliance.ThinkBase.Process.Steps
{
#pragma warning disable SKEXP0080
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
    public class ThinkBaseStep : KernelProcessStep<TBInteractState>
    {

        private TBInteractState _state = new();
        GraphQLHttpClient? client = null;
        private const int MaxCycles = 50;

        public static class Functions
        {
            public const string Interact = nameof(InteractFunctionAsync);
        }

        override public ValueTask ActivateAsync(KernelProcessStepState<TBInteractState> state)
        {
            _state = state.State!;
            _state.ChatHistory ??= new ChatHistory();

            return base.ActivateAsync(state);
        }

        [KernelFunction(Functions.Interact)]
        protected async ValueTask InteractFunctionAsync(KernelProcessStepContext context, Kernel kernel, ILogger logger, ChatHistory history)
        {
            try { 
                string text = string.Empty;
                var config = kernel.GetRequiredService<IConfiguration>();
                if (_state.ChatHistory!.Count == 0)
                {
                    text = config["GraphPrompt"]!;
                    logger.LogInformation($"Started a new ThinkBaseStep. id: {_state.ConversationId}");
                }
                else
                {
                    text = history.Last().ToString();
                    _state.ChatHistory.AddUserMessage(text);
                    logger.LogInformation($"In a ThinkBaseStep iteration. id: {_state.ConversationId}, text: {text}");

                }
                if (client == null)
                {
                    client = new GraphQLHttpClient(config["APIAddress"]!, new SystemTextJsonSerializer());
                }
                var modelName = config["Graph"];

                string query =
                    $$"""
                    {
                      interactKnowledgeGraph(kgModelName: "{{modelName}}", conversationId: "{{_state.ConversationId}}", conversationData: 
                        {name: "" value: "{{text}}" dataType: textual})
                      { 
                        response 
                        { 
                          value 
                          dataType 
                          approximate 
                          categories
                          {
                            name
                            value
                          } 
                        } 
                      } 
                    }             
                    """;

                var req = new GraphQLHttpThinkBaseRequest()
                {
                    Query = query,
                    apikey = config["APIKey"]!
                };
                var responses = await client.SendQueryAsync<ResponseEnvelope>(req);
                if (responses.Errors != null && responses.Errors.Length > 0)
                {
                    logger.LogError($"In ThinkBaseStep. GraphQL error: {responses.Errors[0].Message}");
                    throw new Exception(responses.Errors[0].Message);
                }

                foreach (var response in responses.Data.interactKnowledgeGraph)
                {
                    var outText = await CreateActivity(response, config);
                    if(!_state.Complete)
                    {
                        _state.ChatHistory!.AddAssistantMessage(outText);
                    }
                    history.AddAssistantMessage( outText);
                    logger.LogInformation($"In ThinkBaseStep. Interaction response: {outText} ");
                }
                if (_state.Complete || _state.Cycles > MaxCycles)
                {
                    logger.LogInformation($"In ThinkBaseStep. Interactions completed. ");
                    await context.EmitEventAsync(new() { Id = ThinkBaseStepEvents.Exit, Data = history });
                    _state.ChatHistory.Clear();
                    _state.Complete = false;
                    _state.Cycles = 0;
                }
                else
                {
                    _state.Cycles++;
                    logger.LogInformation($"In ThinkBaseStep. Interactions ongoing. ");
                    await context.EmitEventAsync(new() { Id = ThinkBaseStepEvents.ThinkBaseInteract, Data = history }); 
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in InteractFunctionAsync");
                history.AddAssistantMessage("Error in InteractFunctionAsync: " + ex.Message);
                await context.EmitEventAsync(new() { Id = ThinkBaseStepEvents.Exit, Data = history });
            }

        }

        /// <summary>
        /// Creates an adaptive card for the catehories
        /// </summary>
        /// <param name="r">The response to format</param>
        /// <returns>the json for the card</returns>
        private static string CreateCategoricalCard(InteractResponse r)
        {
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("AICompliance.ThinkBase.Process.Templates.Categories.json")!);
            var source = str.ReadToEnd();

            var sb = new StringBuilder();
            foreach (var item in r.response.categories)
            {
                sb.AppendLine($"{{\"type\": \"Action.Submit\",\"title\": \"{item.name}\"}},");
            }
            sb.Remove(sb.Length - 1, 1);
            source = source.Replace("**CategoryButtons**", sb.ToString());
            source = source.Replace("**text**", r.response.value);
            return source;
        }

        private string CreateFeedbackCard(string link)
        {
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("AICompliance.ThinkBase.Process.Templates.FeedbackCard.json")!);
            var source = str.ReadToEnd();
            source = source.Replace("$link", link);
            return source;
        }


        private async Task<string> CreateActivity(InteractResponse r, IConfiguration config)
        {
            string m = "internal error";
            switch (r.response.dataType)
            {
                case DarlVar.DataType.categorical:
                    m = CreateCategoricalCard(r);
                    break;

                case DarlVar.DataType.link:
                    m = $"[{r.response.value}]({r.response.value})";
                    break;

                case DarlVar.DataType.complete:
                    {
                        if (!string.IsNullOrEmpty(r.response.value))
                        {
                            var link = await CreatePDF(r.response.value, config);
                            m = CreateFeedbackCard(link);
                        }
                        _state.Complete = true;
                    }
                    break;


                default:
                    if (!string.IsNullOrEmpty(r.response.value))
                    {
                        m = r.response.value;
                    }
                    break;
            }
            return m;
        }

        public async Task<string> CreatePDF(string source, IConfiguration config)
        {
            //create a pdf
            var pdf = new MarkdownToPdf();
            var header = string.Empty;
            var footer = string.Empty;
            try
            {
                header = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("AICompliance.ThinkBase.Process.MarkDown.header.md")!).ReadToEnd();
                footer = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("AICompliance.ThinkBase.Process.MarkDown.footer.md")!).ReadToEnd();
            }
            catch { }
            header = header.Replace("$title", config["HeaderTitle"] ?? "Report");
            // insert the source
            pdf.Add(header);
            pdf.Add(source);
            pdf.Add(footer);
            using MemoryStream ms = new();
            pdf.Save(ms);
            ms.Position = 0;
            var name = Guid.NewGuid().ToString() + ".pdf";
            BlobContainerClient container = new(config["StorageConnectionString"], config["ReportContainer"]);
            var b = container.GetBlobClient(name);
            await b.UploadAsync(ms, true);
            ms.Dispose();
            return config["StaticBlobSite"] + name;
        }
    }
#pragma warning restore SKEXP0080
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0050
}
