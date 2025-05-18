using AICompliance.ThinkBase.Process.Models;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.Common;

namespace AICompliance.ThinkBase.Process.Plugins
{
    public class KGMetadataPlugin
    {
        private readonly IConfiguration _config;
        private readonly GraphQLHttpClient _client;
        public KGMetadataPlugin(IConfiguration config)
        {
            _config = config;
            _client = new GraphQLHttpClient(config["APIAddress"]!, new SystemTextJsonSerializer());
        }

        [KernelFunction("GetKGInfo")]
        [Description("Get Knowledge Graph information")]
        public async Task<List<KGMetadata>> GetKGInfoAsync()
        {
            string metadataquery =
                    """
                    query ($name: String! )
                    {
                        getKGraphMetaData(name: $name)
                        {
                            name 
                            model
                            {
                                author 
                                copyright 
                                description 
                                initialText 
                                licenseUrl 
                                defaultTarget
                               }
                            }
                        }         
                    """;
            var kgsquery = """"
                query 
                {
                    kgraphs
                    {
                        name 
                        kgSource
                    }
                }

                """";

            var metadata = new List<KGMetadata>();

            var req = new GraphQLHttpThinkBaseRequest()
            {
                Query = kgsquery,
                apikey = _config["APIKey"]!
            };
            var responses = await _client.SendQueryAsync<KGraphsResponse>(req);
            foreach (var kg in responses.Data.kgraphs.Where( a => a.kgSource == "TEAM"))
            {
                var req2 = new GraphQLHttpThinkBaseRequest()
                {
                    Variables = new { name = kg.name },
                    Query = metadataquery,
                    apikey = _config["APIKey"]!
                };
                var responses2 = await _client.SendQueryAsync<KGMetadataResponse>(req2);
                if (responses2.Data != null)
                {
                    metadata.Add( new KGMetadata { Name = kg.name, Description = responses2.Data.getKGraphMetaData!.model.description, InitialText = responses2.Data.getKGraphMetaData.model.initialText });
                }
            }
            return metadata;
        }
    }
}
