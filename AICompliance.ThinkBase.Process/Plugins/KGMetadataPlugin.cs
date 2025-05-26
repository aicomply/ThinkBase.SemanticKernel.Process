// Copyright (c) 2025 AI Compliance inc. Licensed under the MIT License.
using AICompliance.ThinkBase.Process.Models;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace AICompliance.ThinkBase.Process.Plugins
{
    /// <summary>
    /// Plugin to list the Knowledge graphs available and supply a description of each so that the LLM can match the request to the KG.
    /// </summary>
    public class KGMetadataPlugin
    {
        private readonly IConfiguration _config;
        private readonly GraphQLHttpClient _client;
        private readonly ILogger _logger;
        //temporary simple cache.  Passing in a memory cache is labarynthine, but the better solution.
        private static List<KGMetadata>? CachedMetadata = null;
        public KGMetadataPlugin(IConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _client = new GraphQLHttpClient(config["APIAddress"]!, new SystemTextJsonSerializer());
        }

        [KernelFunction("GetKGInfo")]
        [Description("Get Knowledge Graph information")]
        public async Task<List<KGMetadata>> GetKGInfoAsync()
        {
            _logger.LogInformation("Getting KGraph metadata");
            if (CachedMetadata is null)
            {
                //TODO cache this data
                string metadataquery =
                        """
                    query 
                    {
                        getKGraphMetaData(name: "$name")
                        {
                            name 
                            model
                            {
                                description 
                                initialText 
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
                if (responses.Errors != null && responses.Errors.Length > 0)
                {
                    _logger.LogError("Error getting KGraph list: {0}", responses.Errors[0].Message);
                    return metadata;
                }
                var teamResponses = responses.Data.kgraphs.Where(a => a.kgSource == "TEAM");
                _logger.LogInformation("Got KGraph list: {0}", teamResponses.Count());
                foreach (var kg in teamResponses)
                {
                    var name = kg.name;
                    try
                    {
                        var req2 = new GraphQLHttpThinkBaseRequest()
                        {
                            Query = metadataquery.Replace("$name", name),
                            apikey = _config["APIKey"]!
                        };
                        var responses2 = await _client.SendQueryAsync<KGMetadataResponse>(req2);
                        if (responses2.Errors != null && responses2.Errors.Length > 0)
                        {
                            _logger.LogError("Error getting KGraph metadata: {0}", responses2.Errors[0].Message);
                            return metadata;
                        }
                        if (responses2.Data != null && responses2.Data.getKGraphMetaData != null && !string.IsNullOrEmpty(responses2.Data.getKGraphMetaData!.model.description))
                        {
                            if (!metadata.Any(a => a.Name == name))
                            {
                                _logger.LogInformation("Got KGraph metadata for: {0}", name);
                                metadata.Add(new KGMetadata { Name = name, Description = responses2.Data.getKGraphMetaData!.model.description, InitialText = responses2.Data.getKGraphMetaData.model.initialText });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error getting KGraph metadata for: {0}, Exception: {1}", name, ex.Message);
                    }
                }
                CachedMetadata = metadata;
                return metadata;
            }
            return CachedMetadata;
        }
    }
}
